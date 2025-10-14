using tool;

class Program
{
    // =====================
    // MAIN
    // =====================
    static void Main()
    {
        Console.WriteLine("=== AUTO START ==="); 
       
        while (true)
        {
            if (WindowHelper.IsRoyalRevoltRunning())
            {
                if (WindowHelper.TryGetRoyalRevoltWindow(out var rect))
                {
                    Util.w = rect.Right - rect.Left;
                    Util.h = rect.Bottom - rect.Top;

                    Console.WriteLine($"[INFO] Kích thước cửa sổ game: {Util.w}x{Util.h}");
                    StartScriptJoinBattle();
                }
            }
            else
            {
                Console.WriteLine("[WARN] Không lấy được kích thước cửa sổ game, fallback về full screen.");
                Thread.Sleep(1000);
            }
        }
    }
    static void StartScriptJoinBattle()
    {
        //if (ClickBattleIcon())
        //{
        //    if (ClickAttackIcon())
        //    {
        //        // ClickCollectIcon();
        //        ThreadHolder.StartBattle();
        //        ClickContinueIcon();
        //        ClickChests();
        //    }
        //}
        ClickChests();
    }
    static bool ClickBattleIcon() => WindowHelper.Click((int)(Util.w * 0.92), (int)(Util.h * 0.88), "battle", 2);
    static bool ClickAttackIcon() => WindowHelper.Click((int)(Util.w * 0.74), (int)(Util.h * 0.83), "attack");
    static bool ClickContinueIcon() => WindowHelper.Click((int)(Util.w * 0.75), (int)(Util.h * 0.85), "continue");
    static bool ClickCollectIcon() => WindowHelper.Click((int)(Util.w * 0.6), (int)(Util.h * 0.7), "collect");
    static void ClickChests()
    {
        // Các vị trí cố định (đáy rương)
        double[] xRatios = { 0.25, 0.5, 0.75 };
        double[] yRatios = { 0.55, 0.75 };

        var rnd = new Random();

        // Tạo danh sách tất cả vị trí có thể click (6 vị trí)
        var positions = new List<(double x, double y)>();
        foreach (var y in yRatios)
            foreach (var x in xRatios)
                positions.Add((x, y));

        // Trộn ngẫu nhiên danh sách và chỉ chọn 3 vị trí để click
        var selected = positions.OrderBy(_ => rnd.Next()).Take(3);

        foreach (var (x, y) in selected)
        {
            // Thêm lệch nhẹ (±2%) để giả lập hành vi tự nhiên
            double jitterX = x + (rnd.NextDouble() - 0.5) * 0.02;
            double jitterY = y + (rnd.NextDouble() - 0.5) * 0.02;

            int clickX = (int)(Util.w * Math.Clamp(jitterX, 0.05, 0.95));
            int clickY = (int)(Util.h * Math.Clamp(jitterY, 0.05, 0.95));

            WindowHelper.Click(clickX, clickY, $"chest");
            Thread.Sleep(rnd.Next(250, 500));
            if (AfterSelectChest())
            {
                Thread.Sleep(rnd.Next(250, 500));
            }
        }

        Console.WriteLine("[CHEST] Clicked 3 random chests");
    }
    static bool AfterSelectChest()
    {
       return WindowHelper.Click((int)(Util.w * 0.3), (int)(Util.h * 0.7), "getit") ||
        WindowHelper.Click((int)(Util.w * 0.6), (int)(Util.h * 0.7), "sell");
    }
}
