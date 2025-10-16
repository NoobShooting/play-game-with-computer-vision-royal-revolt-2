using tool;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== AUTO START ===");
        StartScriptJoinBattle();
    }
    private static int width, height;
    private static void StartScriptJoinBattle()
    {
        var currentState = GameState.InMainMenu;
        var threadHolder = new ThreadHolder(currentState);
        while (true)
        {
            if (WindowHelper.IsRoyalRevoltRunning())
            {
                if (WindowHelper.TryGetRoyalRevoltWindow(out var rect))
                {
                    Console.WriteLine("=========New round=========");
                    width = GameApplication.GetInstance().width = rect.Right - rect.Left;
                    height = GameApplication.GetInstance().height = rect.Bottom - rect.Top;
                    switch (currentState)
                    {
                        case GameState.InMainMenu:
                            if (WindowHelper.Click(GameAction.Battle, 2)) currentState = GameState.JoiningBattle;
                            break;
                        case GameState.JoiningBattle:
                            if (WindowHelper.Click(GameAction.Attack))
                            {
                                WindowHelper.Click(GameAction.Collect);
                                Thread.Sleep(1000);
                                currentState = GameState.InBattle;
                                threadHolder.StartBattle();
                            }
                            break;
                        case GameState.InBattle:
                            if (WindowHelper.Click(GameAction.ContinueEndGame, 2)) currentState = GameState.OpeningChest;
                            else if (WindowHelper.Click(GameAction.RetreatLostGame)) currentState = GameState.InMainMenu;
                            break;
                        case GameState.OpeningChest:
                            threadHolder.KillAllThreads();
                            if (ClickChests()) currentState = GameState.InMainMenu;
                            break;
                        case GameState.Idle:
                            if (WindowHelper.Click(GameAction.ContinuePauseGame)) threadHolder.CurrentState = GameState.InBattle;
                            break;
                        default:
                            currentState = TemplateHelper.CheckCurrentFrame();
                            break;
                    }
                    Console.WriteLine($"Old Thread State: {threadHolder.CurrentState}");
                    threadHolder.CurrentState = currentState;
                    Console.WriteLine($"New Thread State: {threadHolder.CurrentState}");
                    Console.WriteLine($"Resolution: {width}x{height}");
                    Thread.Sleep(500);
                }
            }
            else
            {
                currentState = threadHolder.CurrentState = GameState.Idle;
                Console.WriteLine("[WARN] Can not get game resolution, fallback to full screen.");
                Thread.Sleep(1000);
            }
        }
    }
    private static bool ClickChests()
    {
        var isAllOpened = false;
        double[] xRatios = { 0.25, 0.5, 0.75 };
        double[] yRatios = { 0.55, 0.75 };

        var rnd = new Random();

        var positions = new List<(double x, double y)>();
        foreach (var y in yRatios)
            foreach (var x in xRatios)
                positions.Add((x, y));

        var selected = positions.OrderBy(_ => rnd.Next()).Take(3);

        foreach (var (x, y) in selected)
        {
            double jitterX = x + (rnd.NextDouble() - 0.5) * 0.02;
            double jitterY = y + (rnd.NextDouble() - 0.5) * 0.02;

            var chestCoordinate = new ActionCoordinate(GameAction.Chest, Math.Clamp(jitterX, 0.05, 0.95), Math.Clamp(jitterY, 0.05, 0.95));
            GameApplication.ActionCoordinate.Add(chestCoordinate);
            isAllOpened = WindowHelper.Click(GameAction.Chest);
            Thread.Sleep(rnd.Next(250, 500));
            if (AfterSelectChest())
            {
                Thread.Sleep(rnd.Next(250, 500));
            }
            GameApplication.ActionCoordinate.Remove(chestCoordinate);
        }

        Console.WriteLine("[CHEST] Clicked 3 random chests");
        return isAllOpened;
    }
    private static bool AfterSelectChest()
    {
        return WindowHelper.Click(GameAction.GetIt) ||
         WindowHelper.Click(GameAction.Sell);
    }
}
