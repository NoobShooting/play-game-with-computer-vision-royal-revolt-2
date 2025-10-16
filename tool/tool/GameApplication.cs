namespace tool
{
    public class GameApplication
    {
        public static string AppName = "Royal Revolt 2";
        public BotState CurrentState { get; set; } = BotState.Idle;
        public GameApplication(BotState state)
        {
            CurrentState = state;
        }
    }
}
