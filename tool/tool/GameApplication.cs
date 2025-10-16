namespace tool
{
    public class GameApplication
    {
        public static string AppName = "Royal Revolt 2";
        public static List<ActionCoordinate> ActionCoordinate = new()
            {
                new ActionCoordinate(GameAction.Battle, 0.92, 0.88),
                new ActionCoordinate(GameAction.Attack, 0.74, 0.83),
                new ActionCoordinate(GameAction.ContinueEndGame, 0.75, 0.85),
                new ActionCoordinate(GameAction.ContinuePauseGame, 0.6, 0.55),
                new ActionCoordinate(GameAction.Collect, 0.6, 0.7),
                new ActionCoordinate(GameAction.RetreatLostGame, 0.6, 0.8),
                new ActionCoordinate(GameAction.RetreatPauseGame, 0.4, 0.7),
                new ActionCoordinate(GameAction.GetIt, 0.3, 0.7),
                new ActionCoordinate(GameAction.Sell, 0.6, 0.7),
                new ActionCoordinate(GameAction.Pause, 0.05, 0.3)
            };
        public static GameApplication? instance = null;
        public int width, height;
        public GameState CurrentState { get; set; } = GameState.Idle;
        public GameApplication(GameState state) { CurrentState = state; }
        public static GameApplication GetInstance()
        {
            if (instance == null) instance = new GameApplication(GameState.InMainMenu);
            return instance;
        }
    }
    public class ActionCoordinate
    {
        public GameAction ActionName { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public ActionCoordinate(GameAction actionName, double x, double y)
        {
            ActionName = actionName;
            X = x;
            Y = y;
        }
    }
    public enum GameState
    {
        Idle,
        InMainMenu,
        JoiningBattle,
        InBattle,
        OpeningChest
    }
    public enum GameAction
    {
        Battle,
        Attack,
        ContinueEndGame,
        ContinuePauseGame,
        Collect,
        RetreatPauseGame,
        RetreatLostGame,
        Chest,
        GetIt,
        Sell,
        Pause
    }
    public static class GameActionExtensions
    {
        public static string GetName(this GameAction action)
        {
            return action switch
            {
                GameAction.Battle => "battle",
                GameAction.Attack => "attack",
                GameAction.ContinueEndGame => "continue_end_game",
                GameAction.ContinuePauseGame => "continue_pause_game",
                GameAction.Collect => "collect",
                GameAction.RetreatPauseGame => "retreat_pause_game",
                GameAction.RetreatLostGame => "retreat_lost_game",
                GameAction.Chest => "chest",
                GameAction.GetIt => "get_it",
                GameAction.Sell => "sell",
                GameAction.Pause => "pause",
                _ => string.Empty
            };
        }

        public static (int, int) GetCoordinate(this List<ActionCoordinate> actionCoordinates, GameAction action)
        {
            var dx = GameApplication.ActionCoordinate.FirstOrDefault(a => a.ActionName == action)?.X ?? 0;
            var dy = GameApplication.ActionCoordinate.FirstOrDefault(a => a.ActionName == action)?.Y ?? 0;
            var gameInstance = GameApplication.GetInstance();
            var ix = (int)(gameInstance.width * dx);
            var iy = (int)(gameInstance.height * dy);
            return (ix, iy);
        }
    }

}
