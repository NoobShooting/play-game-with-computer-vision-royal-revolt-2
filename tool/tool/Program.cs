using tool;

class Program
{
    // =====================
    // MAIN
    // =====================
    static void Main()
    {
        Console.WriteLine("=== AUTO START ===");
        StartScriptJoinBattle();
    }

    private static void StartScriptJoinBattle()
    {
        var gameStateManager = new GameApplication(BotState.InMainMenu);
        var currentState = gameStateManager.CurrentState;
        var threadHolder = new ThreadHolder(currentState);
        while (true)
        {
            if (WindowHelper.IsRoyalRevoltRunning())
            {
                if (WindowHelper.TryGetRoyalRevoltWindow(out var rect))
                {
                    Util.w = rect.Right - rect.Left;
                    Util.h = rect.Bottom - rect.Top;

                    // Console.WriteLine($"[INFO] Game resolution: {Util.w}x{Util.h}");
                    //if (GameStateManager.GamePaused)
                    //{
                    //    Console.WriteLine("⏸ Game mất focus — tạm dừng bot...");
                    //    Thread.Sleep(1000);
                    //    continue;
                    //}

                    // chỗ nãy hãy tạo 1 hàm check trạng thái game hiện tại -> sẽ có action hợp lí
                    // currentState = checkCurrentFrame();
                    switch (currentState)
                    {
                        case BotState.InMainMenu:
                            if (ClickBattleIcon())
                            {
                                currentState = BotState.JoiningBattle;
                            }
                            break;

                        case BotState.JoiningBattle:
                            if (ClickAttackIcon())
                            {
                                threadHolder.StartBattle();
                                currentState = BotState.InBattle;
                            }
                            break;

                        case BotState.InBattle:
                            //if (ThreadHolder.IsBattleEnded)
                            //{
                            //    currentState = BotState.BattleEnded;
                            //}
                            break;

                        case BotState.BattleEnded:
                            if (ClickContinueIcon())
                            {
                                currentState = BotState.OpeningChest;
                            }
                            break;

                        case BotState.OpeningChest:
                            if (ClickChests())
                            {
                                currentState = BotState.InMainMenu;
                            }
                            break;

                        default:
                            currentState = BotState.InMainMenu;
                            threadHolder.KillAllThreads();
                            break;
                    }
                    threadHolder.CurrentState = currentState;
                    Console.WriteLine($"Current State: {currentState}");
                    Thread.Sleep(500);
                }
            }
            else
            {
                Console.WriteLine("[WARN] Can not get game resolution, fallback to full screen.");
                Thread.Sleep(1000);
            }
        }
    }

    private static bool ClickBattleIcon() => WindowHelper.Click((int)(Util.w * 0.92), (int)(Util.h * 0.88), "battle", 2);
    private static bool ClickAttackIcon() => WindowHelper.Click((int)(Util.w * 0.74), (int)(Util.h * 0.83), "attack");
    private static bool ClickContinueIcon() => WindowHelper.Click((int)(Util.w * 0.75), (int)(Util.h * 0.85), "continue");
    private static bool ClickCollectIcon() => WindowHelper.Click((int)(Util.w * 0.6), (int)(Util.h * 0.7), "collect");
    private static bool ClickChests()
    {
        var isAllOpened = false;
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

            isAllOpened = WindowHelper.Click(clickX, clickY, $"chest");
            Thread.Sleep(rnd.Next(250, 500));
            if (AfterSelectChest())
            {
                Thread.Sleep(rnd.Next(250, 500));
            }
        }

        Console.WriteLine("[CHEST] Clicked 3 random chests");
        return isAllOpened;
    }
    private static bool AfterSelectChest()
    {
        return WindowHelper.Click((int)(Util.w * 0.3), (int)(Util.h * 0.7), "get_it") ||
         WindowHelper.Click((int)(Util.w * 0.6), (int)(Util.h * 0.7), "sell");
    }
}
