namespace tool
{
    public class ThreadHolder
    {
        public BotState CurrentState { get; set; } = BotState.Idle;
        private Thread? moveThread, skillThread;
        public ThreadHolder(BotState currentState)
        {
            CurrentState = currentState;
        }

        // =====================
        // MOVE CHARACTER
        // =====================
        private void MoveCharacter()
        {
            while (CurrentState == BotState.InBattle)
            {
                int cx = Util.w / 2, cy = (int)(Util.h * 0.55);
                int radius = (int)(Math.Min(Util.w, Util.h) * 0.12);
                double angle = 45, step = 15;
                angle += step;
                if (angle > 75) angle = 10;

                double rad = Math.PI * angle / 180.0;
                int distance = 100;
                int tx = cx + (int)(radius * Math.Cos(rad)) + distance;
                int ty = cy - (int)(radius * Math.Sin(rad)) - distance;

                Console.WriteLine($"Moving to ({cx},{cy})");
                SystemUtil.SetCursorPos(tx, ty);
                SystemUtil.mouse_event(Util.MOUSEEVENTF_LEFTDOWN, cx, cy, 0, UIntPtr.Zero);
                Thread.Sleep(2500);
                SystemUtil.mouse_event(Util.MOUSEEVENTF_LEFTUP, cx, cy, 0, UIntPtr.Zero);
            }
        }

        // =====================
        // CLICK SKILL
        // =====================
        private void ClickSkill()
        {
            string keys = "123567eqw";
            while (CurrentState == BotState.InBattle)
            {
                SendKeys.SendWait(keys);
                Thread.Sleep(300);
            }
        }

        // =====================
        // START BATTLE (TỰ DỪNG SAU 3 PHÚT)
        // =====================
        public void StartBattle()
        {
            Console.WriteLine("Battle start");
            Thread.Sleep(2000);

            startThread(moveThread, MoveCharacter);
            startThread(skillThread, ClickSkill);
        }

        private void startThread(Thread? thread, ThreadStart action)
        {
            if (thread != null) return;
            thread = new Thread(action) { IsBackground = true };
            thread.Start();
        }
        // =====================
        // END BATTLE
        // =====================
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
