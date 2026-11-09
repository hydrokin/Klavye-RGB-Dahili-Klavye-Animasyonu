using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KlavyeRGB
{
    public sealed class FrameContext
    {
        public double AnimTime;  // hız çarpanı uygulanmış zaman (sn)
        public double RealTime;  // gerçek zaman (sn)
        public double Dt;        // son kareden bu yana gerçek süre (sn)
        public double Slider;    // kaydırıcı çarpanı (0.25x – 4x)
        public Rgb C1, C2;       // kullanıcının seçtiği renkler
    }

    public abstract class Effect
    {
        /// <summary>Türkçe ad: değişmeyen kimlik (ayar dosyasında bu tutulur) ve çeviri anahtarı.</summary>
        public abstract string Name { get; }
        public abstract string Hint { get; }
        /// <summary>Seçili dildeki ad ve açıklama.</summary>
        public string DisplayName { get { return L.T(Name); } }
        public string DisplayHint { get { return L.T(Hint); } }
        /// <summary>Kaç kullanıcı rengi kullanır (0, 1, 2).</summary>
        public virtual int ColorCount { get { return 0; } }
        /// <summary>Kaydırıcının etiketi; null ise kaydırıcı kullanılmaz.</summary>
        public virtual string SliderLabel { get { return "Hız:"; } }
        /// <summary>Hız kaydırıcısı zamanı hızlandırsın mı (canlı veri efektlerinde hayır).</summary>
        public virtual bool ScalesTime { get { return true; } }
        public virtual void Begin() { }
        public virtual void End() { }
        public abstract Rgb Frame(FrameContext c);
        public override string ToString() { return DisplayName; }

        protected static double Breath(double t, double period)
        {
            double x = (1 - Math.Cos(2 * Math.PI * t / period)) / 2;
            return 0.03 + 0.97 * Math.Pow(x, 1.8);
        }

        protected static double Hash(long i)
        {
            unchecked
            {
                ulong x = (ulong)i * 0x9E3779B97F4A7C15UL;
                x ^= x >> 31; x *= 0xBF58476D1CE4E5B9UL; x ^= x >> 29;
                return (x % 1000003UL) / 1000003.0;
            }
        }

        /// <summary>Yumuşak 1B gürültü, 0..1.</summary>
        protected static double Noise(double x)
        {
            long i = (long)Math.Floor(x);
            double f = x - i, u = f * f * (3 - 2 * f);
            return Hash(i) + (Hash(i + 1) - Hash(i)) * u;
        }
    }

    public sealed class StaticEffect : Effect
    {
        public override string Name { get { return "Sabit Renk"; } }
        public override string Hint { get { return "Klavye seçtiğin renkte sabit yanar. Program açık kaldıkça bu renk kalır."; } }
        public override int ColorCount { get { return 1; } }
        public override string SliderLabel { get { return null; } }
        public override Rgb Frame(FrameContext c) { return c.C1; }
    }

    public sealed class RainbowEffect : Effect
    {
        public override string Name { get { return "Gökkuşağı"; } }
        public override string Hint { get { return "Klavye gökkuşağının bütün renklerinden yumuşakça geçer."; } }
        public override Rgb Frame(FrameContext c) { return Rgb.Hsv(c.AnimTime / 8.0, 1, 1); }
    }

    public sealed class RainbowBreathEffect : Effect
    {
        public override string Name { get { return "Gökkuşağı Nefes"; } }
        public override string Hint { get { return "Her nefeste farklı bir renkte yavaşça yanıp söner."; } }
        public override Rgb Frame(FrameContext c)
        {
            const double period = 4.0;
            double n = Math.Floor(c.AnimTime / period);
            return Rgb.Hsv(n * 0.381966, 1, 1).Scale(Breath(c.AnimTime, period));
        }
    }

    public sealed class BreathEffect : Effect
    {
        public override string Name { get { return "Nefes"; } }
        public override string Hint { get { return "Seçtiğin renk nefes alır gibi yavaşça yanıp söner."; } }
        public override int ColorCount { get { return 1; } }
        public override Rgb Frame(FrameContext c) { return c.C1.Scale(Breath(c.AnimTime, 4.0)); }
    }

    public sealed class FadeEffect : Effect
    {
        public override string Name { get { return "İki Renk Geçişi"; } }
        public override string Hint { get { return "Seçtiğin iki renk arasında yumuşakça gidip gelir."; } }
        public override int ColorCount { get { return 2; } }
        public override Rgb Frame(FrameContext c)
        {
            return Rgb.Lerp(c.C1, c.C2, (1 - Math.Cos(2 * Math.PI * c.AnimTime / 5.0)) / 2);
        }
    }

    public sealed class StrobeEffect : Effect
    {
        public override string Name { get { return "Flaş"; } }
        public override string Hint { get { return "Seçtiğin renkte çift flaş çakar."; } }
        public override int ColorCount { get { return 1; } }
        public override Rgb Frame(FrameContext c)
        {
            double p = c.AnimTime % 1.0;
            bool on = p < 0.08 || (p >= 0.18 && p < 0.26);
            return on ? c.C1 : Rgb.Black;
        }
    }

    public sealed class PoliceEffect : Effect
    {
        static readonly Rgb Red = new Rgb(1, 0, 0), Blue = new Rgb(0, 0.15, 1);
        public override string Name { get { return "Polis Çakarı"; } }
        public override string Hint { get { return "Kırmızı ve mavi sırayla üçer kez çakar."; } }
        public override Rgb Frame(FrameContext c)
        {
            int step = (int)(Math.Floor(c.AnimTime / 0.07) % 14);
            if (step < 6) return step % 2 == 0 ? Red : Rgb.Black;
            if (step >= 7 && step < 13) return (step - 7) % 2 == 0 ? Blue : Rgb.Black;
            return Rgb.Black;
        }
    }

    public sealed class HeartEffect : Effect
    {
        public override string Name { get { return "Kalp Atışı"; } }
        public override string Hint { get { return "Seçtiğin renkte çift vuruşlu kalp ritmi."; } }
        public override int ColorCount { get { return 1; } }

        static double Pulse(double x, double at) { return x >= at ? Math.Exp(-(x - at) * 9) : 0; }

        public override Rgb Frame(FrameContext c)
        {
            double p = c.AnimTime % 1.1;
            double v = Math.Min(1, 0.04 + Pulse(p, 0) + 0.7 * Pulse(p, 0.28));
            return c.C1.Scale(v);
        }
    }

    public sealed class FireEffect : Effect
    {
        public override string Name { get { return "Ateş"; } }
        public override string Hint { get { return "Kızıl ve turuncu tonlarda titreyen alev ışığı."; } }
        public override Rgb Frame(FrameContext c)
        {
            double n1 = Noise(c.AnimTime * 3.1), n2 = Noise(c.AnimTime * 8.7 + 100);
            double flick = 0.45 + 0.4 * n1 + 0.15 * n2;
            return Rgb.Lerp(new Rgb(1, 0.1, 0), new Rgb(1, 0.5, 0.03), n1).Scale(flick);
        }
    }

    public sealed class OceanEffect : Effect
    {
        public override string Name { get { return "Okyanus"; } }
        public override string Hint { get { return "Mavi ve turkuaz tonlarda dalgalanan sakin ışık."; } }
        public override Rgb Frame(FrameContext c)
        {
            double t = c.AnimTime;
            double hue = 0.52 + 0.07 * Math.Sin(t * 0.45) + 0.03 * (Noise(t * 0.7) - 0.5);
            double v = 0.55 + 0.45 * (0.5 + 0.5 * Math.Sin(t * 1.2 + Noise(t * 0.3) * 2));
            return Rgb.Hsv(hue, 0.9, v);
        }
    }

    public sealed class RandomFadeEffect : Effect
    {
        public override string Name { get { return "Rastgele Renkler"; } }
        public override string Hint { get { return "Rastgele renkler arasında yumuşak geçişler yapar."; } }

        static Rgb ColorAt(long i) { return Rgb.Hsv(i * 0.381966 + Hash(i) * 0.25, 0.85 + 0.15 * Hash(i + 77), 1); }

        public override Rgb Frame(FrameContext c)
        {
            const double seg = 1.6;
            long i = (long)Math.Floor(c.AnimTime / seg);
            double f = c.AnimTime / seg - i;
            return Rgb.Lerp(ColorAt(i), ColorAt(i + 1), (1 - Math.Cos(Math.PI * f)) / 2);
        }
    }

    public sealed class AudioLevelEffect : Effect
    {
        double level, runningMax = 0.05;

        public override string Name { get { return "Müzik: Ses Seviyesi"; } }
        public override string Hint { get { return "Bilgisayardan çıkan sese göre renk yeşilden kırmızıya döner ve parlaklaşır. Kaydırıcı hassasiyeti ayarlar."; } }
        public override string SliderLabel { get { return "Hassasiyet:"; } }
        public override bool ScalesTime { get { return false; } }
        public override void End() { AudioMeter.Instance.Release(); }

        public override Rgb Frame(FrameContext c)
        {
            double peak = AudioMeter.Instance.Peak();
            runningMax = Math.Max(peak, Math.Max(0.05, runningMax * (1 - c.Dt * 0.25)));
            double v = peak < 0.003 ? 0 : Math.Min(1.0, peak / runningMax * c.Slider);
            level = Math.Max(v, level - c.Dt * 2.0);
            return Rgb.Hsv(1 / 3.0 * (1 - level), 1, 0.12 + 0.88 * level);
        }
    }

    public sealed class AudioBeatEffect : Effect
    {
        double slow, lastBeat = -10, env, hue;

        public override string Name { get { return "Müzik: Vuruş"; } }
        public override string Hint { get { return "Müzikteki her vuruşta klavye yeni bir renkte parlar ve söner. Kaydırıcı hassasiyeti ayarlar."; } }
        public override string SliderLabel { get { return "Hassasiyet:"; } }
        public override bool ScalesTime { get { return false; } }
        public override void End() { AudioMeter.Instance.Release(); }

        public override Rgb Frame(FrameContext c)
        {
            double peak = AudioMeter.Instance.Peak();
            double threshold = 1.35 / Math.Sqrt(c.Slider);
            if (peak > 0.02 && peak > slow * threshold && c.RealTime - lastBeat > 0.18)
            {
                lastBeat = c.RealTime;
                hue += 0.381966;
                env = 1;
            }
            slow += (peak - slow) * Math.Min(1.0, c.Dt * 1.5);
            env *= Math.Exp(-c.Dt * 5);
            return Rgb.Hsv(hue, 1, 0.06 + 0.94 * env);
        }
    }

    public sealed class ScreenEffect : Effect
    {
        ScreenSampler sampler;
        Rgb current, target;
        double lastSample = -10;

        public override string Name { get { return "Ekran Rengi"; } }
        public override string Hint { get { return "Klavye ekrandaki baskın renge uyar (film, oyun, video). Kaydırıcı tepki hızını ayarlar."; } }
        public override string SliderLabel { get { return "Tepki hızı:"; } }
        public override bool ScalesTime { get { return false; } }

        public override void Begin() { sampler = new ScreenSampler(); lastSample = -10; }
        public override void End() { if (sampler != null) { sampler.Dispose(); sampler = null; } }

        public override Rgb Frame(FrameContext c)
        {
            if (sampler != null && c.RealTime - lastSample >= 0.06)
            {
                lastSample = c.RealTime;
                try { target = sampler.Sample(); } catch { }
            }
            double a = Math.Min(1.0, c.Dt * 6.0 * c.Slider);
            current = Rgb.Lerp(current, target, a);
            return current;
        }
    }

    public sealed class CpuEffect : Effect
    {
        [DllImport("kernel32.dll")]
        static extern bool GetSystemTimes(out long idle, out long kernel, out long user);

        long lastIdle, lastTotal;
        double usage, shown, lastSample = -10;

        public override string Name { get { return "İşlemci Yükü"; } }
        public override string Hint { get { return "İşlemci boştayken yeşil, yük arttıkça sarı ve kırmızı olur. %90'ı geçerse kırmızı nabız atar."; } }
        public override string SliderLabel { get { return null; } }
        public override bool ScalesTime { get { return false; } }
        public override void Begin() { Sample(); lastSample = -10; }

        void Sample()
        {
            long idle, kernel, user;
            if (!GetSystemTimes(out idle, out kernel, out user)) return;
            long total = kernel + user, dTotal = total - lastTotal, dIdle = idle - lastIdle;
            if (lastTotal != 0 && dTotal > 0) usage = Math.Max(0, Math.Min(1, 1.0 - (double)dIdle / dTotal));
            lastTotal = total; lastIdle = idle;
        }

        public override Rgb Frame(FrameContext c)
        {
            if (c.RealTime - lastSample >= 0.5) { Sample(); lastSample = c.RealTime; }
            shown += (usage - shown) * Math.Min(1, c.Dt * 3);
            Rgb col = Rgb.Hsv(1 / 3.0 * (1 - shown), 1, 1);
            if (shown > 0.9) col = col.Scale(0.35 + 0.65 * (0.5 + 0.5 * Math.Cos(c.RealTime * 8)));
            return col;
        }
    }

    public sealed class BatteryEffect : Effect
    {
        [StructLayout(LayoutKind.Sequential)]
        struct SYSTEM_POWER_STATUS { public byte ACLineStatus, BatteryFlag, BatteryLifePercent, SystemStatusFlag; public int BatteryLifeTime, BatteryFullLifeTime; }
        [DllImport("kernel32.dll")]
        static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS s);

        SYSTEM_POWER_STATUS st;
        double lastSample = -10;

        public override string Name { get { return "Pil Durumu"; } }
        public override string Hint { get { return "Pil doluyken yeşil, azaldıkça sarı ve kırmızı olur. Şarj olurken nefes alır, %15'in altında kırmızı yanıp söner."; } }
        public override string SliderLabel { get { return null; } }
        public override bool ScalesTime { get { return false; } }

        public override Rgb Frame(FrameContext c)
        {
            if (c.RealTime - lastSample >= 2) { GetSystemPowerStatus(out st); lastSample = c.RealTime; }
            if (st.BatteryLifePercent > 100) return new Rgb(0, 1, 0.3);          // pil bilgisi yok
            double pct = st.BatteryLifePercent / 100.0;
            Rgb col = Rgb.Hsv(1 / 3.0 * pct, 1, 1);
            bool charging = (st.BatteryFlag & 8) != 0;
            if (charging) return col.Scale(Breath(c.RealTime, 3.0));
            if (pct < 0.15 && st.ACLineStatus == 0) return (c.RealTime % 1.0) < 0.5 ? new Rgb(1, 0, 0) : Rgb.Black;
            return col;
        }
    }

    public struct Keyframe
    {
        public int Color;   // 0xRRGGBB
        public int Ms;
        public bool Fade;   // sonraki kareye yumuşak geçiş
        public Keyframe(int color, int ms, bool fade) { Color = color; Ms = ms; Fade = fade; }
    }

    public sealed class CustomEffect : Effect
    {
        volatile Keyframe[] frames = new Keyframe[0];

        public override string Name { get { return "Özel Animasyon (kendin yap)"; } }
        public override string Hint { get { return "Kendi animasyonunu adım adım tasarla: her adımın rengini, süresini ve bir sonraki renge yumuşak mı yoksa ani mi geçeceğini seç. Hız kaydırıcısı tüm süreleri ölçekler."; } }

        public int CurrentIndex { get; private set; }

        public void SetFrames(List<Keyframe> list) { frames = list.ToArray(); }
        public List<Keyframe> GetFrames() { return new List<Keyframe>(frames); }

        public override Rgb Frame(FrameContext c)
        {
            Keyframe[] f = frames;
            if (f.Length == 0) { CurrentIndex = -1; return Rgb.Black; }
            int total = 0;
            foreach (var k in f) total += k.Ms;
            if (total <= 0) { CurrentIndex = -1; return Rgb.Black; }
            double pos = (c.AnimTime * 1000.0) % total;
            for (int i = 0; i < f.Length; i++)
            {
                if (pos < f[i].Ms)
                {
                    CurrentIndex = i;
                    Rgb a = Rgb.FromInt(f[i].Color);
                    if (!f[i].Fade) return a;
                    return Rgb.Lerp(a, Rgb.FromInt(f[(i + 1) % f.Length].Color), pos / f[i].Ms);
                }
                pos -= f[i].Ms;
            }
            CurrentIndex = f.Length - 1;
            return Rgb.FromInt(f[f.Length - 1].Color);
        }

        public static List<Keyframe> Defaults()
        {
            return new List<Keyframe>
            {
                new Keyframe(0xFF2200, 900, true),
                new Keyframe(0xFF8800, 900, true),
                new Keyframe(0xFF0099, 900, true),
                new Keyframe(0x7700FF, 900, true),
                new Keyframe(0x0055FF, 900, true),
                new Keyframe(0xFFFFFF, 120, false),
                new Keyframe(0x000000, 120, false),
                new Keyframe(0xFFFFFF, 120, false),
                new Keyframe(0x000000, 500, true),
            };
        }
    }

    /// <summary>Birincil ekranı küçültüp baskın rengi bulur.</summary>
    public sealed class ScreenSampler : IDisposable
    {
        const int W = 48, H = 27;
        readonly Bitmap small = new Bitmap(W, H, PixelFormat.Format32bppRgb);

        [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("gdi32.dll")] static extern bool StretchBlt(IntPtr dst, int x, int y, int w, int h, IntPtr src, int sx, int sy, int sw, int sh, int rop);
        [DllImport("gdi32.dll")] static extern int SetStretchBltMode(IntPtr hdc, int mode);
        [DllImport("gdi32.dll")] static extern bool SetBrushOrgEx(IntPtr hdc, int x, int y, IntPtr old);

        public Rgb Sample()
        {
            Rectangle r = Screen.PrimaryScreen.Bounds;
            IntPtr screen = GetDC(IntPtr.Zero);
            try
            {
                using (Graphics g = Graphics.FromImage(small))
                {
                    IntPtr hdc = g.GetHdc();
                    try
                    {
                        SetStretchBltMode(hdc, 4); // HALFTONE = gerçek ortalama
                        SetBrushOrgEx(hdc, 0, 0, IntPtr.Zero);
                        StretchBlt(hdc, 0, 0, W, H, screen, r.X, r.Y, r.Width, r.Height, 0x00CC0020);
                    }
                    finally { g.ReleaseHdc(hdc); }
                }
            }
            finally { ReleaseDC(IntPtr.Zero, screen); }

            BitmapData d = small.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            double sr = 0, sg = 0, sb = 0, sw = 0, sv = 0;
            try
            {
                int[] px = new int[W * H];
                for (int y = 0; y < H; y++)
                    Marshal.Copy(new IntPtr(d.Scan0.ToInt64() + (long)y * d.Stride), px, y * W, W);
                foreach (int p in px)
                {
                    var c = new Rgb(((p >> 16) & 255) / 255.0, ((p >> 8) & 255) / 255.0, (p & 255) / 255.0);
                    double h, s, v;
                    c.ToHsv(out h, out s, out v);
                    double w = 0.1 + s * v * 2.5;  // canlı renkler baskın gelsin
                    sr += c.R * w; sg += c.G * w; sb += c.B * w; sw += w; sv += v;
                }
            }
            finally { small.UnlockBits(d); }

            var avg = new Rgb(sr / sw, sg / sw, sb / sw);
            double ah, asat, av;
            avg.ToHsv(out ah, out asat, out av);
            double meanV = sv / (W * H);
            return Rgb.Hsv(ah, Math.Min(1, asat * 1.35), Math.Min(1, Math.Max(av, meanV) * 1.15));
        }

        public void Dispose() { small.Dispose(); }
    }

    // ---- Windows ses çıkışı tepe değeri (WASAPI IAudioMeterInformation) ----

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    class MMDeviceEnumeratorComObject { }

    [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceEnumerator
    {
        [PreserveSig] int EnumAudioEndpoints(int dataFlow, int stateMask, out IntPtr devices);
        [PreserveSig] int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice endpoint);
    }

    [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDevice
    {
        [PreserveSig] int Activate(ref Guid iid, int clsCtx, IntPtr activationParams, [MarshalAs(UnmanagedType.IUnknown)] out object iface);
    }

    [ComImport, Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioMeterInformation
    {
        [PreserveSig] int GetPeakValue(out float peak);
    }

    public sealed class AudioMeter
    {
        public static readonly AudioMeter Instance = new AudioMeter();
        IAudioMeterInformation meter;
        DateTime nextAcquire = DateTime.MinValue;

        public float Peak()
        {
            if (DateTime.UtcNow >= nextAcquire) { Acquire(); nextAcquire = DateTime.UtcNow.AddSeconds(4); }
            if (meter == null) return 0;
            try
            {
                float p;
                if (meter.GetPeakValue(out p) == 0) return p;
            }
            catch { }
            Release();
            return 0;
        }

        void Acquire()
        {
            Release();
            object enumObj = null;
            IMMDevice dev = null;
            try
            {
                enumObj = new MMDeviceEnumeratorComObject();
                var en = (IMMDeviceEnumerator)enumObj;
                if (en.GetDefaultAudioEndpoint(0, 1, out dev) != 0 || dev == null) return;
                Guid iid = typeof(IAudioMeterInformation).GUID;
                object o;
                if (dev.Activate(ref iid, 23, IntPtr.Zero, out o) == 0) meter = (IAudioMeterInformation)o;
            }
            catch { meter = null; }
            finally
            {
                if (dev != null) Marshal.ReleaseComObject(dev);
                if (enumObj != null) Marshal.ReleaseComObject(enumObj);
            }
        }

        public void Release()
        {
            if (meter != null) { try { Marshal.ReleaseComObject(meter); } catch { } meter = null; }
            nextAcquire = DateTime.MinValue;
        }
    }
}
