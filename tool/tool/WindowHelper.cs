using tool;

class WindowHelper
{
    /// Thử lấy kích thước cửa sổ Royal Revolt 2 (nếu đang mở)
    /// </summary>
    public static bool TryGetRoyalRevoltWindow(out RECT rect)
    {
        rect = new RECT();
        var gameName = GameApplication.AppName;
        IntPtr hWnd = SystemUtil.FindWindow(null, GameApplication.AppName);
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

    /// <summary>
    /// Kiểm tra xem Royal Revolt 2 có đang mở và hiển thị không
    /// </summary>
    public static bool IsRoyalRevoltRunning()
    {
        var gameName = GameApplication.AppName;
        IntPtr hWnd = SystemUtil.FindWindow(null, gameName);

        if (hWnd == IntPtr.Zero)
        {
            Console.WriteLine($"[CHECK] {gameName} not open or close.");
            return false;
        }

        // Nếu bị minimize
        if (SystemUtil.IsIconic(hWnd))
        {
            Console.WriteLine($"[CHECK] {gameName} is minimized.");
            return false;
        }

        // Nếu không phải cửa sổ foreground
        if (SystemUtil.GetForegroundWindow() != hWnd)
        {
            Console.WriteLine($"[CHECK] {gameName} not actived windown.");
            return false;
        }

        return true;
    }

    // ------------------- CLICK -------------------
    public static bool Click(int x, int y, string actionName = "unknown", int numClicks = 2)
    {
        var isClick = false;
        TemplateHelper.CaptureItem(x, y, actionName);
        while (TemplateHelper.IsTemplateVisible(x, y, actionName))
        {
            for (int i = 0; i < numClicks; i++)
            {
                SystemUtil.SetCursorPos(x, y);
                SystemUtil.mouse_event(Util.MOUSEEVENTF_LEFTDOWN, x, y, 0, UIntPtr.Zero);
                Thread.Sleep(50);
                SystemUtil.mouse_event(Util.MOUSEEVENTF_LEFTUP, x, y, 0, UIntPtr.Zero);
                Console.WriteLine($"[CLICK] {actionName} at ({x},{y})");
                Thread.Sleep(1000);
            }
            isClick = true;
        }
        return isClick;
    }
}
