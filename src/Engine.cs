using System;
using System.Diagnostics;
using System.Threading;

namespace KlavyeRGB
{
    /// <summary>
    /// Animasyonu arka plan iş parçacığında hesaplar ve klavyeye gönderir.
    /// Tüm EC komutları bu iş parçacığından gider; arayüz sadece istek bırakır.
    /// </summary>
    public sealed class Engine
    {
        const int FrameMs = 30;

        readonly Thread thread;
        readonly FrameContext ctx = new FrameContext();
        volatile bool quit;

        // Arayüzden ayarlanan değerler
        public volatile Effect Effect;
        public volatile bool WantRunning;
        public volatile int SpeedStep = 8;        // 0..16, 8 = 1x
        public volatile int BrightnessPct = 100;  // 5..100
        public volatile bool ColorCorrection = true;
        public volatile int Color1 = 0x00C8FF, Color2 = 0xFF00A0;

        // Arayüzün okuduğu durum
        public volatile bool IsRunning;
        public volatile int PreviewColor = -1;    // kullanıcıya görünen renk (0xRRGGBB)
        public volatile string Status = "";
        public volatile bool StatusError;
        public readonly bool Available;

        public Engine()
        {
            Available = Dchu.Load();
            if (!Available) { Status = Dchu.LastError; StatusError = true; }
            thread = new Thread(Loop) { IsBackground = true, Name = "KlavyeRGB" };
            thread.Start();
        }

        public static double SpeedMultiplier(int step) { return Math.Pow(2, (step - 8) / 4.0); }

        /// <summary>Durdurur, klavyeyi eski haline döndürür ve iş parçacığını bitirir.</summary>
        public void Shutdown()
        {
            WantRunning = false;
            quit = true;
            thread.Join(3000);
        }

        void Loop()
        {
            var clock = Stopwatch.StartNew();
            double last = 0, anim = 0, lastSend = -10;
            int lastSent = -1;
            bool running = false;
            Effect cur = null;

            if (Available)
            {
                SavedLight s = Dchu.ReadSaved();
                PreviewColor = (s.R << 16) | (s.G << 8) | s.B;
            }

            while (!quit)
            {
                double frameStart = clock.Elapsed.TotalSeconds;
                double dt = Math.Min(0.25, frameStart - last);
                last = frameStart;
                bool want = WantRunning && Available;

                if (want && !running)
                {
                    if (!Dchu.ReadSaved().On) Dchu.TurnOn();
                    running = true;
                    IsRunning = true;
                    anim = 0;
                    lastSent = -1;
                    cur = null;
                }
                else if (!want && running)
                {
                    StopAndRestore(ref cur);
                    running = false;
                    IsRunning = false;
                }

                if (running)
                {
                    Effect eff = Effect;
                    if (eff != cur)
                    {
                        if (cur != null) SafeEnd(cur);
                        cur = eff;
                        if (cur != null) cur.Begin();
                        anim = 0;
                    }
                    if (cur != null)
                    {
                        double mult = SpeedMultiplier(SpeedStep);
                        anim += dt * (cur.ScalesTime ? mult : 1.0);
                        ctx.AnimTime = anim; ctx.RealTime = frameStart; ctx.Dt = dt; ctx.Slider = mult;
                        ctx.C1 = Rgb.FromInt(Color1); ctx.C2 = Rgb.FromInt(Color2);

                        Rgb c;
                        try { c = cur.Frame(ctx).Clamp(); } catch { c = Rgb.Black; }
                        c = c.Scale(BrightnessPct / 100.0);
                        PreviewColor = c.ToInt();

                        Rgb hw = c;
                        if (ColorCorrection) hw = new Rgb(c.R * 180 / 255.0, c.G, c.B * 200 / 255.0); // tek bölge RGB beyaz dengesi
                        int packed = hw.ToInt();

                        // Değişince gönder; uyku/uyanma gibi durumlar için arada bir tekrarla.
                        if (packed != lastSent || frameStart - lastSend > 1.0)
                        {
                            lastSend = frameStart;
                            if (Dchu.SetColor((byte)(packed >> 16), (byte)(packed >> 8), (byte)packed))
                            {
                                lastSent = packed;
                                if (StatusError) { Status = L.T("Çalışıyor: ") + cur.DisplayName; StatusError = false; }
                            }
                            else
                            {
                                lastSent = -1;
                                Status = Dchu.LastError;
                                StatusError = true;
                            }
                        }
                    }
                }

                int spent = (int)((clock.Elapsed.TotalSeconds - frameStart) * 1000);
                Thread.Sleep(Math.Max(2, FrameMs - spent));
            }

            if (running) StopAndRestore(ref cur);
            IsRunning = false;
        }

        void StopAndRestore(ref Effect cur)
        {
            if (cur != null) SafeEnd(cur);
            cur = null;
            Dchu.Restore();
            SavedLight s = Dchu.ReadSaved();
            PreviewColor = s.On ? (s.R << 16) | (s.G << 8) | s.B : 0;
        }

        static void SafeEnd(Effect e) { try { e.End(); } catch { } }
    }
}
