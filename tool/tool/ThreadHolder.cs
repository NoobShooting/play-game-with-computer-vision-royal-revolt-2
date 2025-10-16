namespace tool
{
    public class ThreadHolder
    {
        public GameState CurrentState { get; set; } = GameState.Idle;
        private Thread? moveThread = null, skillThread = null;
        public ThreadHolder(GameState currentState)
        {
            CurrentState = currentState;
        } 
        private void MoveCharacter()
        {
            double angle = 40, step = 20;
            var gameInstance = GameApplication.GetInstance();
            var width = gameInstance.width;
            var height = gameInstance.height;
            while (CurrentState == GameState.InBattle)
            {
                int cx = width / 2, cy = (int)(height * 0.55);
                int radius = (int)(Math.Min(width, height) * 0.12);
                angle += step;
                if (angle > 80) angle = 100;

                double rad = Math.PI * angle / 180.0;
                int distance = 100;
                int tx = cx + (int)(radius * Math.Cos(rad)) + distance;
                int ty = cy - (int)(radius * Math.Sin(rad)) - distance;

                Console.WriteLine($"Moving to ({tx},{ty})");
                SystemUtil.SetCursorPos(tx, ty);
                SystemUtil.mouse_event(Util.MOUSEEVENTF_LEFTDOWN, cx, cy, 0, UIntPtr.Zero);
                Thread.Sleep(10000);
                SystemUtil.mouse_event(Util.MOUSEEVENTF_LEFTUP, cx, cy, 0, UIntPtr.Zero);
            }
        } 
        private void ClickSkill()
        {
            string keys = "123567eqw";
            while (CurrentState == GameState.InBattle)
            {
                SendKeys.SendWait(keys);
                Thread.Sleep(300);
            }
        }
        public void StartBattle()
        {
            Console.WriteLine("Battle start");
            startThread(moveThread, MoveCharacter);
            startThread(skillThread, ClickSkill);
        }
        private void startThread(Thread? thread, ThreadStart action)
        {
            if (thread != null) return;
            thread = new Thread(action) { IsBackground = true };
            thread.Start();
        } 
        private void endThread(Thread? thread)
        {
            if (thread == null) return;
            thread?.Join(500);
            thread = null;
        }
        public void KillAllThreads()
        {
            endThread(moveThread);
            endThread(skillThread);
            Console.WriteLine("[BOT] Battle End");
        }
    }
}
