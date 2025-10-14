 using System.Runtime.InteropServices;
using tool;

class WindowHelper
{
    [DllImport("user32.dll")]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    static extern bool IsIconic(IntPtr hWnd); // kiểm tra xem có bị minimize không

    [DllImport("user32.dll")]
    static extern bool GetClientRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")] static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")] static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private const string WindowTitle = "Royal Revolt 2";

    /// <summary>
    /// Thử lấy kích thước cửa sổ Royal Revolt 2 (nếu đang mở)
    /// </summary>
    public static bool TryGetRoyalRevoltWindow(out RECT rect)
    {
        rect = new RECT();

        IntPtr hWnd = FindWindow(null, WindowTitle);
        if (hWnd == IntPtr.Zero)
        {
            Console.WriteLine("[ERR] Không tìm thấy cửa sổ Royal Revolt 2.");
            return false;
        }

        if (!GetWindowRect(hWnd, out rect))
        {
            Console.WriteLine("[ERR] Không thể lấy kích thước cửa sổ.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Kiểm tra xem Royal Revolt 2 có đang mở và hiển thị không
    /// </summary>
    public static bool IsRoyalRevoltRunning()
    {
        IntPtr hWnd = FindWindow(null, WindowTitle);

        if (hWnd == IntPtr.Zero)
        {
            Console.WriteLine("[CHECK] Royal Revolt 2 chưa mở hoặc đã đóng.");
            return false;
        }

        // Nếu bị minimize
        if (IsIconic(hWnd))
        {
            Console.WriteLine("[CHECK] Royal Revolt 2 đang bị minimize.");
            return false;
        }

        // Nếu không phải cửa sổ foreground
        if (GetForegroundWindow() != hWnd)
        {
            Console.WriteLine("[CHECK] Royal Revolt 2 không phải cửa sổ đang active.");
            return false;
        }

        return true;
    }

    // ------------------- CLICK -------------------
    public static bool Click(int x, int y, string actionName = "unknown", int numClicks = 2)
    {
        var isClick = false;
        while (TemplateChecker.IsTemplateVisible(x,y,actionName))
        {
            for (int i = 0; i < numClicks; i++)
            {
                SetCursorPos(x, y);
                mouse_event(Util.MOUSEEVENTF_LEFTDOWN, x, y, 0, UIntPtr.Zero);
                Thread.Sleep(50);
                mouse_event(Util.MOUSEEVENTF_LEFTUP, x, y, 0, UIntPtr.Zero);
                Console.WriteLine($"[CLICK] {actionName} at ({x},{y})");
                Thread.Sleep(1000);
            }
            isClick = true;
        }
        return isClick;
    }
}
