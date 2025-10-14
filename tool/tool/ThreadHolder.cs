using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace tool
{
    public class ThreadHolder
    {
        [DllImport("user32.dll")] static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")] static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);
        [DllImport("user32.dll")] static extern int GetSystemMetrics(int nIndex);

     

        static bool running = false;
        static Thread? moveThread, skillThread, watchdogThread;
        // =====================
        // MOVE CHARACTER
        // =====================
        static void MoveCharacter()
        {
            int cx = Util.w / 2, cy = (int)(Util.h * 0.55);
            int radius = (int)(Math.Min(Util.w, Util.h) * 0.12);
            double angle = 45, step = 15;

            while (running)
            {
                angle += step;
                if (angle > 75) angle = 10;

                double rad = Math.PI * angle / 180.0;
                int distance = 100;
                int tx = cx + (int)(radius * Math.Cos(rad)) + distance;
                int ty = cy - (int)(radius * Math.Sin(rad)) - distance;

                SetCursorPos(tx, ty);
                mouse_event(Util.MOUSEEVENTF_LEFTDOWN, cx, cy, 0, UIntPtr.Zero);
                Thread.Sleep(2500);
                mouse_event(Util.MOUSEEVENTF_LEFTUP, cx, cy, 0, UIntPtr.Zero);
            }

        }
        // =========================
        // WATCHDOG THREAD
        // =========================
        static void Watchdog()
        {
            while (running)
            {
                Thread.Sleep(5000);
                if (!WindowHelper.IsRoyalRevoltRunning() || WindowHelper.Click((int)(Util.w * 0.75), (int)(Util.h * 0.85), "continue"))
                {
                    Console.WriteLine("[WATCHDOG] Game window closed or hidden. Stopping bot...");
                    EndBattle();
                    break;
                }
            }
        }
        // =====================
        // CLICK SKILL
        // =====================
        static void ClickSkill()
        {
            string keys = "123567eqw";
            while (running)
            {
                SendKeys.SendWait(keys);
                Thread.Sleep(300);
            }
        }

        // =====================
        // START BATTLE (TỰ DỪNG SAU 3 PHÚT)
        // =====================
        public static void StartBattle()
        {
            Console.WriteLine("Vào game sau 2 giây");
            Thread.Sleep(2000);
            if (running) return;
            running = true;

            moveThread = new Thread(MoveCharacter) { IsBackground = true };
            skillThread = new Thread(ClickSkill) { IsBackground = true };
            watchdogThread = new Thread(Watchdog) { IsBackground = true };

            moveThread.Start();
            skillThread.Start();
            watchdogThread.Start();

            var minute = 2;
            Console.WriteLine($"auto trong {minute} phút...");
            Thread.Sleep(TimeSpan.FromMinutes(minute));

            EndBattle();
        }

        // =====================
        // END BATTLE
        // =====================
        static void EndBattle()
        {
            if (!running) return;
            running = false;

            moveThread?.Join(500);
            skillThread?.Join(500);
            watchdogThread?.Join(500);

            moveThread = null;
            skillThread = null;
            watchdogThread = null;

            Console.WriteLine("[BOT] Đã dừng sau 3 phút.");
        }
    }
}
