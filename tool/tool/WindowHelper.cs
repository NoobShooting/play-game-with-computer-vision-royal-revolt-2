using tool;

class WindowHelper
{
    public static bool TryGetRoyalRevoltWindow(out RECT rect)
    {
        rect = new RECT();
        var gameName = GameApplication.AppName;
        IntPtr hWnd = SystemUtil.FindWindow(null, gameName);
        if (hWnd == IntPtr.Zero)
        {
            Console.WriteLine($"[ERR] Can not find {gameName}.");
            return false;
        }

        if (!SystemUtil.GetWindowRect(hWnd, out rect))
        {
            Console.WriteLine($"[ERR] Can not get {gameName} resolution.");
            return false;
        }

        return true;
    }
    public static bool IsRoyalRevoltRunning()
    {
        var gameName = GameApplication.AppName;
        IntPtr hWnd = SystemUtil.FindWindow(null, gameName);

        if (hWnd == IntPtr.Zero)
        {
            Console.WriteLine($"[CHECK] {gameName} not open or close.");
            return false;
        }

        if (SystemUtil.IsIconic(hWnd))
        {
            Console.WriteLine($"[CHECK] {gameName} is minimized.");
            return false;
        }

        if (SystemUtil.GetForegroundWindow() != hWnd)
        {
            Console.WriteLine($"[CHECK] {gameName} not actived windown.");
            return false;
        }

        return true;
    }

    public static bool Click(GameAction action, int numClicks = 2)
    {
        var actionName = action.GetName();
        var (ix, iy) = GameApplication.ActionCoordinate.GetCoordinate(action);
        var isClick = false;
        TemplateHelper.CaptureItem(action);
        while (TemplateHelper.IsTemplateVisible(action))
        {
            for (int i = 0; i < numClicks; i++)
            {
                SystemUtil.SetCursorPos(ix, iy);
                SystemUtil.mouse_event(Util.MOUSEEVENTF_LEFTDOWN, ix, iy, 0, UIntPtr.Zero);
                Thread.Sleep(50);
                SystemUtil.mouse_event(Util.MOUSEEVENTF_LEFTUP, ix, iy, 0, UIntPtr.Zero);
                Console.WriteLine($"[CLICK] {actionName} at ({ix},{iy})");
                Thread.Sleep(1000);
            }
            isClick = true;
        }
        return isClick;
    }
}
