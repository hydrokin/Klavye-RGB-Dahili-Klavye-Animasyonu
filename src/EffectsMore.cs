using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KlavyeRGB
{
    /// <summary>Efekt listesindeki kategori başlığı (seçilemez).</summary>
    public sealed class EffectHeader
    {
        public readonly string Text;
        public EffectHeader(string text) { Text = text; }
        public string DisplayText { get { return L.T(Text); } }
        public override string ToString() { return DisplayText; }
    }

    // ------------------------------------------------------------------ genel amaçlı efekt türleri

    /// <summary>Renk paleti içinde sırayla dolaşır. fade: 1 = tamamen yumuşak, 0 = ani geçiş.</summary>
    public sealed class PaletteEffect : Effect
    {
        readonly string name, hint;
        readonly double seg, fade, flicker;
        readonly Rgb[] cols;

        public PaletteEffect(string name, string hint, double seg, double fade, double flicker, params int[] colors)
        {
            this.name = name; this.hint = hint; this.seg = seg; this.fade = fade; this.flicker = flicker;
            cols = new Rgb[colors.Length];
            for (int i = 0; i < colors.Length; i++) cols[i] = Rgb.FromInt(colors[i]);
        }

        public override string Name { get { return name; } }
        public override string Hint { get { return hint; } }

        public override Rgb Frame(FrameContext c)
        {
            double x = c.AnimTime / seg;
            long i = (long)Math.Floor(x);
            double f = x - i;
            int n = cols.Length;
            Rgb a = cols[(int)(i % n)], b = cols[(int)((i + 1) % n)];
            Rgb col = a;
            if (fade > 0)
            {
                double t = Math.Max(0, (f - (1 - fade)) / fade);
                col = Rgb.Lerp(a, b, (1 - Math.Cos(Math.PI * t)) / 2);
            }
            if (flicker > 0) col = col.Scale(1 - flicker + flicker * Noise(c.AnimTime * 4.3));
            return col;
        }
    }

    /// <summary>Palet içinde gürültüyle rastgele dolaşan, dalgalanan efekt.</summary>
    public sealed class DriftEffect : Effect
    {
        readonly string name, hint;
        readonly double rate, depth;
        readonly Rgb[] cols;

        public DriftEffect(string name, string hint, double rate, double depth, params int[] colors)
        {
            this.name = name; this.hint = hint; this.rate = rate; this.depth = depth;
            cols = new Rgb[colors.Length];
            for (int i = 0; i < colors.Length; i++) cols[i] = Rgb.FromInt(colors[i]);
        }

        public override string Name { get { return name; } }
        public override string Hint { get { return hint; } }

        public override Rgb Frame(FrameContext c)
        {
            double t = c.AnimTime * rate;
            int n = cols.Length;
            double pos = (t * 0.35 + Noise(t) * n) % n;
            int i = (int)Math.Floor(pos);
            double f = pos - i;
            Rgb col = Rgb.Lerp(cols[i % n], cols[(i + 1) % n], (1 - Math.Cos(Math.PI * f)) / 2);
            return col.Scale(1 - depth + depth * Noise(t * 2.7 + 50));
        }
    }

    /// <summary>Adım adım çakar deseni; -1 = sönük.</summary>
    public sealed class FlashPatternEffect : Effect
    {
        readonly string name, hint;
        readonly double step;
        readonly int[] pattern;

        public FlashPatternEffect(string name, string hint, double step, params int[] pattern)
        {
            this.name = name; this.hint = hint; this.step = step; this.pattern = pattern;
        }

        public override string Name { get { return name; } }
        public override string Hint { get { return hint; } }

        public override Rgb Frame(FrameContext c)
        {
            int v = pattern[(int)(Math.Floor(c.AnimTime / step) % pattern.Length)];
            return v < 0 ? Rgb.Black : Rgb.FromInt(v);
        }
    }

    // ------------------------------------------------------------------ renk kullanan temel efektler

    public sealed class TwoColorBreathEffect : Effect
    {
        public override string Name { get { return "İki Renk Nefes"; } }
        public override string Hint { get { return "Bir nefes Renk 1, bir nefes Renk 2 ile yanıp söner."; } }
        public override int ColorCount { get { return 2; } }
        public override Rgb Frame(FrameContext c)
        {
            bool first = ((long)Math.Floor(c.AnimTime / 4.0)) % 2 == 0;
            return (first ? c.C1 : c.C2).Scale(Breath(c.AnimTime, 4.0));
        }
    }

    public sealed class TwoColorBlinkEffect : Effect
    {
        public override string Name { get { return "İki Renk Yanıp Sönme"; } }
        public override string Hint { get { return "Renk 1 ile Renk 2 arasında keskin geçişlerle değişir."; } }
        public override int ColorCount { get { return 2; } }
        public override Rgb Frame(FrameContext c)
        {
            return ((long)Math.Floor(c.AnimTime / 0.5)) % 2 == 0 ? c.C1 : c.C2;
        }
    }

    public sealed class TwinkleEffect : Effect
    {
        public override string Name { get { return "Yıldız Işıltısı"; } }
        public override string Hint { get { return "Seçtiğin renk loş yanar, arada parlak ışıltılar çakar."; } }
        public override int ColorCount { get { return 1; } }
        public override Rgb Frame(FrameContext c)
        {
            const double seg = 0.18;
            long i = (long)Math.Floor(c.AnimTime / seg);
            Rgb baseCol = c.C1.Scale(0.22);
            if (Hash(i) < 0.6) return baseCol;
            double local = c.AnimTime - i * seg;
            Rgb spark = Rgb.Lerp(c.C1, new Rgb(1, 1, 1), 0.6);
            return Rgb.Lerp(baseCol, spark, Math.Exp(-local * 14));
        }
    }

    public sealed class PulseEffect : Effect
    {
        public override string Name { get { return "Nabız"; } }
        public override string Hint { get { return "Renk 1'den Renk 2'ye hızlı nabız atışları."; } }
        public override int ColorCount { get { return 2; } }
        public override Rgb Frame(FrameContext c)
        {
            double p = c.AnimTime % 0.8;
            return Rgb.Lerp(c.C2, c.C1, Math.Exp(-p * 6));
        }
    }

    public sealed class MorseEffect : Effect
    {
        const double Unit = 0.12;
        static readonly Dictionary<char, string> Table = new Dictionary<char, string>
        {
            {'A',".-"},{'B',"-..."},{'C',"-.-."},{'D',"-.."},{'E',"."},{'F',"..-."},{'G',"--."},{'H',"...."},
            {'I',".."},{'J',".---"},{'K',"-.-"},{'L',".-.."},{'M',"--"},{'N',"-."},{'O',"---"},{'P',".--."},
            {'Q',"--.-"},{'R',".-."},{'S',"..."},{'T',"-"},{'U',"..-"},{'V',"...-"},{'W',".--"},{'X',"-..-"},
            {'Y',"-.--"},{'Z',"--.."},{'0',"-----"},{'1',".----"},{'2',"..---"},{'3',"...--"},{'4',"....-"},
            {'5',"....."},{'6',"-...."},{'7',"--..."},{'8',"---.."},{'9',"----."},{'.',".-.-.-"},{',',"--..--"},
            {'?',"..--.."},{'!',"-.-.--"},{'/',"-..-."},{'-',"-....-"},{'@',".--.-."},
            {'Ç',"-.-.."},{'Ğ',"--.-."},{'Ö',"---."},{'Ş',".--.."},{'Ü',"..--"}
        };

        volatile int[] segs = new int[0];   // + yanık birim, - sönük birim
        volatile int total;
        string text = "";

        public override string Name { get { return "Mors Kodu"; } }
        public override string Hint { get { return "Yazdığın metni seçtiğin renkle Mors koduyla klavyeye yazar."; } }
        public override int ColorCount { get { return 1; } }

        public string Text
        {
            get { return text; }
            set { text = value ?? ""; Build(); }
        }

        void Build()
        {
            var list = new List<int>();
            string t = text.ToUpperInvariant().Replace('İ', 'I').Replace('ı', 'I');
            foreach (string word in t.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int before = list.Count;
                foreach (char ch in word)
                {
                    string code;
                    if (!Table.TryGetValue(ch, out code)) continue;
                    foreach (char s in code) { list.Add(s == '.' ? 1 : 3); list.Add(-1); }
                    list[list.Count - 1] = -3;
                }
                if (list.Count > before) list[list.Count - 1] = -7;
            }
            int sum = 0;
            foreach (int s in list) sum += Math.Abs(s);
            segs = list.ToArray();
            total = sum;
        }

        public override Rgb Frame(FrameContext c)
        {
            int[] s = segs;
            int tot = total;
            if (tot == 0 || s.Length == 0) return Rgb.Black;
            double pos = (c.AnimTime / Unit) % tot;
            foreach (int v in s)
            {
                int len = Math.Abs(v);
                if (pos < len) return v > 0 ? c.C1 : Rgb.Black;
                pos -= len;
            }
            return Rgb.Black;
        }
    }

    // ------------------------------------------------------------------ gökkuşağı ailesi

    public sealed class RainbowStrobeEffect : Effect
    {
        public override string Name { get { return "Gökkuşağı Flaş"; } }
        public override string Hint { get { return "Her flaşta gökkuşağının bir sonraki rengi çakar."; } }
        public override Rgb Frame(FrameContext c)
        {
            long i = (long)Math.Floor(c.AnimTime / 0.25);
            double local = c.AnimTime - i * 0.25;
            return local < 0.1 ? Rgb.Hsv(i * 0.13, 1, 1) : Rgb.Black;
        }
    }

    public sealed class PlasmaEffect : Effect
    {
        public override string Name { get { return "Plazma"; } }
        public override string Hint { get { return "Üst üste binen dalgalarla akan, öngörülemeyen renk plazması."; } }
        public override Rgb Frame(FrameContext c)
        {
            double t = c.AnimTime;
            double h = 0.5 + 0.25 * Math.Sin(t * 0.7) + 0.15 * Math.Sin(t * 1.3 + 1) + 0.1 * Math.Sin(t * 2.3 + 2);
            double v = 0.75 + 0.25 * Math.Sin(t * 1.7 + 0.5);
            return Rgb.Hsv(h, 1, v);
        }
    }

    public sealed class RandomFlashEffect : Effect
    {
        public override string Name { get { return "Rastgele Flaşlar"; } }
        public override string Hint { get { return "Rastgele zamanlarda rastgele renkli flaşlar patlar."; } }
        public override Rgb Frame(FrameContext c)
        {
            const double seg = 0.45;
            long i = (long)Math.Floor(c.AnimTime / seg);
            if (Hash(i) < 0.45) return Rgb.Black;
            double local = c.AnimTime - i * seg;
            return Rgb.Hsv(Hash(i + 999), 1, 1).Scale(Math.Exp(-local * 7));
        }
    }

    // ------------------------------------------------------------------ doğa

    public sealed class CandleEffect : Effect
    {
        public override string Name { get { return "Mum Işığı"; } }
        public override string Hint { get { return "Sıcak, titreyen ve ara sıra kısılan mum alevi."; } }
        public override Rgb Frame(FrameContext c)
        {
            double t = c.AnimTime;
            double v = 0.55 + 0.25 * Noise(t * 2.1) + 0.15 * Noise(t * 9.3 + 20);
            if (Noise(t * 0.6 + 7) < 0.18) v *= 0.6;
            return Rgb.Lerp(new Rgb(1, 0.35, 0.03), new Rgb(1, 0.55, 0.12), Noise(t * 1.3 + 3)).Scale(v);
        }
    }

    public sealed class StormEffect : Effect
    {
        public override string Name { get { return "Fırtına"; } }
        public override string Hint { get { return "Karanlık mavi gökyüzünde ara sıra çakan şimşekler."; } }
        public override Rgb Frame(FrameContext c)
        {
            const double seg = 3.5;
            double t = c.AnimTime;
            long i = (long)Math.Floor(t / seg);
            double local = t - i * seg;
            Rgb sky = new Rgb(0.02, 0.05, 0.25).Scale(0.6 + 0.4 * Noise(t * 0.8));
            if (Hash(i) < 0.25) return sky;                       // bu aralıkta şimşek yok
            double at = Hash(i + 17) * (seg - 1.0);
            double d = local - at;
            if (d < 0) return sky;
            double flash = 0;
            if (d < 0.06) flash = 1;
            else if (d >= 0.14 && d < 0.4) flash = Math.Exp(-(d - 0.14) * 9);
            return Rgb.Lerp(sky, new Rgb(0.85, 0.9, 1), flash);
        }
    }

    // ------------------------------------------------------------------ çakar ve ritim

    public sealed class AlarmEffect : Effect
    {
        public override string Name { get { return "Alarm"; } }
        public override string Hint { get { return "Siren gibi hızla kabarıp sönen kırmızı ışık."; } }
        public override Rgb Frame(FrameContext c)
        {
            double s = 0.5 + 0.5 * Math.Sin(2 * Math.PI * c.AnimTime * 1.5);
            return new Rgb(1, 0, 0).Scale(0.08 + 0.92 * s * s);
        }
    }

    public sealed class DiscoEffect : Effect
    {
        public override string Name { get { return "Disko"; } }
        public override string Hint { get { return "120 BPM tempoda her vuruşta yeni bir renk patlar."; } }
        public override Rgb Frame(FrameContext c)
        {
            long i = (long)Math.Floor(c.AnimTime / 0.5);
            double local = c.AnimTime - i * 0.5;
            return Rgb.Hsv(Hash(i) + i * 0.381966, 1, 0.25 + 0.75 * Math.Exp(-local * 6));
        }
    }

    public sealed class MatrixEffect : Effect
    {
        public override string Name { get { return "Matrix"; } }
        public override string Hint { get { return "Dijital yağmur gibi titreşen yeşil."; } }
        public override Rgb Frame(FrameContext c)
        {
            long i = (long)Math.Floor(c.AnimTime / 0.08);
            double h = Hash(i);
            if (h > 0.94) return new Rgb(0.6, 1, 0.6);
            return new Rgb(0, 1, 0.15).Scale(0.2 + 0.8 * h * h * h);
        }
    }

    // ------------------------------------------------------------------ müzik

    /// <summary>Ses seviyesini otomatik kazançla 0..1 aralığına çevirir.</summary>
    public sealed class AudioLevel
    {
        double level, runningMax = 0.05;
        public double Update(double dt, double sensitivity)
        {
            double peak = AudioMeter.Instance.Peak();
            runningMax = Math.Max(peak, Math.Max(0.05, runningMax * (1 - dt * 0.25)));
            double v = peak < 0.003 ? 0 : Math.Min(1.0, peak / runningMax * sensitivity);
            level = Math.Max(v, level - dt * 2.0);
            return level;
        }
    }

    public sealed class AudioFlowEffect : Effect
    {
        readonly AudioLevel audio = new AudioLevel();
        double hue;
        public override string Name { get { return "Müzik: Renk Akışı"; } }
        public override string Hint { get { return "Müzik yükseldikçe renkler daha hızlı akar ve parlaklaşır. Kaydırıcı hassasiyeti ayarlar."; } }
        public override string SliderLabel { get { return "Hassasiyet:"; } }
        public override bool ScalesTime { get { return false; } }
        public override void End() { AudioMeter.Instance.Release(); }
        public override Rgb Frame(FrameContext c)
        {
            double lv = audio.Update(c.Dt, c.Slider);
            hue += c.Dt * (0.03 + lv * 1.2);
            return Rgb.Hsv(hue, 1, 0.2 + 0.8 * lv);
        }
    }

    public sealed class AudioMonoEffect : Effect
    {
        readonly AudioLevel audio = new AudioLevel();
        public override string Name { get { return "Müzik: Tek Renk"; } }
        public override string Hint { get { return "Seçtiğin renk müziğin sesine göre parlayıp kısılır. Kaydırıcı hassasiyeti ayarlar."; } }
        public override int ColorCount { get { return 1; } }
        public override string SliderLabel { get { return "Hassasiyet:"; } }
        public override bool ScalesTime { get { return false; } }
        public override void End() { AudioMeter.Instance.Release(); }
        public override Rgb Frame(FrameContext c) { return c.C1.Scale(0.05 + 0.95 * audio.Update(c.Dt, c.Slider)); }
    }

    // ------------------------------------------------------------------ etkileşimli

    /// <summary>Tuş basışlarını sayar (hangi tuş olduğu kaydedilmez).</summary>
    public sealed class KeyActivity
    {
        [DllImport("user32.dll")] static extern short GetAsyncKeyState(int vk);
        readonly bool[] down = new bool[256];
        bool primed;

        public int Poll()
        {
            int presses = 0;
            for (int vk = 0x08; vk < 0xFF; vk++)
            {
                if (vk == 0x10 || vk == 0x11 || vk == 0x12) continue;   // sol/sağ ayrı sayılıyor
                bool d = (GetAsyncKeyState(vk) & 0x8000) != 0;
                if (d && !down[vk] && primed) presses++;
                down[vk] = d;
            }
            primed = true;
            return presses;
        }

        public void Reset() { Array.Clear(down, 0, down.Length); primed = false; }
    }

    public sealed class TypingFlashEffect : Effect
    {
        readonly KeyActivity keys = new KeyActivity();
        double env, hue;
        public override string Name { get { return "Yazarken Parla"; } }
        public override string Hint { get { return "Her tuşa basışta klavye parlar ve rengi bir adım değişir. Hangi tuşa basıldığı kaydedilmez."; } }
        public override string SliderLabel { get { return null; } }
        public override bool ScalesTime { get { return false; } }
        public override void Begin() { keys.Reset(); env = 0; }
        public override Rgb Frame(FrameContext c)
        {
            int p = keys.Poll();
            if (p > 0) { env = 1; hue += 0.07 * p; }
            env *= Math.Exp(-c.Dt * 3.5);
            return Rgb.Hsv(hue, 1, 0.08 + 0.92 * env);
        }
    }

    public sealed class TypingSpeedEffect : Effect
    {
        readonly KeyActivity keys = new KeyActivity();
        double rate;
        public override string Name { get { return "Yazma Hızı"; } }
        public override string Hint { get { return "Hızlı yazdıkça renk maviden yeşile, sarıya ve kırmızıya döner. Hangi tuşa basıldığı kaydedilmez."; } }
        public override string SliderLabel { get { return null; } }
        public override bool ScalesTime { get { return false; } }
        public override void Begin() { keys.Reset(); rate = 0; }
        public override Rgb Frame(FrameContext c)
        {
            int p = keys.Poll();
            rate = rate * Math.Exp(-c.Dt / 1.5) + p / 1.5;          // ~saniyedeki tuş sayısı
            double k = Math.Min(1, rate / 9.0);
            return Rgb.Hsv(0.66 * (1 - k), 1, 0.25 + 0.75 * Math.Min(1, rate / 3.0));
        }
    }

    public sealed class MouseColorEffect : Effect
    {
        [StructLayout(LayoutKind.Sequential)] struct POINT { public int X, Y; }
        [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT p);
        public override string Name { get { return "Fare ile Renk"; } }
        public override string Hint { get { return "Fareyi sağa sola götürdükçe renk, yukarı aşağı götürdükçe parlaklık değişir."; } }
        public override string SliderLabel { get { return null; } }
        public override bool ScalesTime { get { return false; } }
        public override Rgb Frame(FrameContext c)
        {
            POINT p;
            if (!GetCursorPos(out p)) return Rgb.Black;
            var vs = SystemInformation.VirtualScreen;
            double x = (p.X - vs.Left) / (double)Math.Max(1, vs.Width), y = (p.Y - vs.Top) / (double)Math.Max(1, vs.Height);
            return Rgb.Hsv(x * 0.95, 1, 1 - 0.85 * Math.Max(0, Math.Min(1, y)));
        }
    }

    // ------------------------------------------------------------------ sistem ve zaman

    public sealed class RamEffect : Effect
    {
        [StructLayout(LayoutKind.Sequential)]
        struct MEMORYSTATUSEX
        {
            public uint dwLength, dwMemoryLoad;
            public ulong ullTotalPhys, ullAvailPhys, ullTotalPageFile, ullAvailPageFile, ullTotalVirtual, ullAvailVirtual, ullAvailExtendedVirtual;
        }
        [DllImport("kernel32.dll")] static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX m);

        double load, shown, lastSample = -10;
        public override string Name { get { return "RAM Kullanımı"; } }
        public override string Hint { get { return "Bellek kullanımını gösterir: az kullanımda yeşil, doldukça kırmızı. %90 üstünde nabız atar."; } }
        public override string SliderLabel { get { return null; } }
        public override bool ScalesTime { get { return false; } }
        public override Rgb Frame(FrameContext c)
        {
            if (c.RealTime - lastSample >= 1)
            {
                var m = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)) };
                if (GlobalMemoryStatusEx(ref m)) load = m.dwMemoryLoad / 100.0;
                lastSample = c.RealTime;
            }
            shown += (load - shown) * Math.Min(1, c.Dt * 2);
            Rgb col = Rgb.Hsv(1 / 3.0 * (1 - Math.Max(0, (shown - 0.3) / 0.7)), 1, 1);
            if (shown > 0.9) col = col.Scale(0.35 + 0.65 * (0.5 + 0.5 * Math.Cos(c.RealTime * 8)));
            return col;
        }
    }

    public sealed class NetworkEffect : Effect
    {
        long lastIn = -1, lastOut;
        double lastSample = -10, rateIn, rateOut, shownV, shownMix;
        public override string Name { get { return "İnternet Trafiği"; } }
        public override string Hint { get { return "İndirme mavi, yükleme turuncu yanar. Trafik arttıkça parlaklık artar."; } }
        public override string SliderLabel { get { return null; } }
        public override bool ScalesTime { get { return false; } }
        public override void Begin() { lastIn = -1; lastSample = -10; }

        void Sample(double now)
        {
            long bin = 0, bout = 0;
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up) continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback || ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;
                    IPInterfaceStatistics s = ni.GetIPStatistics();
                    bin += s.BytesReceived; bout += s.BytesSent;
                }
            }
            catch { return; }
            double dt = now - lastSample;
            if (lastIn >= 0 && dt > 0)
            {
                rateIn = Math.Max(0, (bin - lastIn) / dt);
                rateOut = Math.Max(0, (bout - lastOut) / dt);
            }
            lastIn = bin; lastOut = bout; lastSample = now;
        }

        public override Rgb Frame(FrameContext c)
        {
            if (c.RealTime - lastSample >= 0.5) Sample(c.RealTime);
            double total = rateIn + rateOut;
            double v = total < 2000 ? 0 : Math.Min(1, Math.Log10(total / 2000) / Math.Log10(5e6 / 2000));
            double mix = total > 0 ? rateOut / total : 0;
            shownV += (v - shownV) * Math.Min(1, c.Dt * 4);
            shownMix += (mix - shownMix) * Math.Min(1, c.Dt * 4);
            return Rgb.Hsv(0.6 + (0.08 + 1 - 0.6) * shownMix, 1, 0.06 + 0.94 * shownV);
        }
    }

    public sealed class TimeOfDayEffect : Effect
    {
        static readonly double[] Hours = { 0, 5, 6.5, 8, 11, 14, 17, 19, 20.5, 22, 24 };
        static readonly int[] Colors = { 0x140850, 0x2A1060, 0xFF5E3A, 0xFFB36B, 0xFFF2DD, 0xDDEEFF, 0xFFB040, 0xFF4A1A, 0x9A2AFF, 0x3A12A0, 0x140850 };
        public override string Name { get { return "Günün Saati"; } }
        public override string Hint { get { return "Renk saate göre değişir: şafakta turuncu, öğlen beyaz, akşam altın sarısı, gece mor-lacivert."; } }
        public override string SliderLabel { get { return null; } }
        public override bool ScalesTime { get { return false; } }
        public override Rgb Frame(FrameContext c)
        {
            DateTime now = DateTime.Now;
            double h = now.Hour + now.Minute / 60.0 + now.Second / 3600.0;
            for (int i = 0; i < Hours.Length - 1; i++)
                if (h >= Hours[i] && h < Hours[i + 1])
                    return Rgb.Lerp(Rgb.FromInt(Colors[i]), Rgb.FromInt(Colors[i + 1]), (h - Hours[i]) / (Hours[i + 1] - Hours[i]));
            return Rgb.FromInt(Colors[0]);
        }
    }

    public sealed class PomodoroEffect : Effect
    {
        const double Work = 25 * 60, Rest = 5 * 60;
        double start = -1;
        public override string Name { get { return "Pomodoro Zamanlayıcı"; } }
        public override string Hint { get { return "25 dakika çalışma: klavye yeşilden kırmızıya döner, son 10 saniye yanıp söner. Ardından 5 dakika mola: mavi nefes alır. Başlatınca sayaç sıfırlanır."; } }
        public override string SliderLabel { get { return null; } }
        public override bool ScalesTime { get { return false; } }
        public override void Begin() { start = -1; }
        public override Rgb Frame(FrameContext c)
        {
            if (start < 0) start = c.RealTime;
            double e = (c.RealTime - start) % (Work + Rest);
            bool blink = (c.RealTime % 0.5) < 0.25;
            if (e < Work)
            {
                if (Work - e < 10) return blink ? new Rgb(1, 0, 0) : Rgb.Black;
                return Rgb.Hsv(1 / 3.0 * (1 - e / Work), 1, 1);
            }
            if (Work + Rest - e < 10) return blink ? new Rgb(0, 1, 0) : Rgb.Black;
            return new Rgb(0.1, 0.4, 1).Scale(Breath(e, 4));
        }
    }

    // ------------------------------------------------------------------ karışık

    public sealed class MixEffect : Effect
    {
        const double Seg = 15, Fade = 1.5;
        readonly Effect[] pool;

        public MixEffect(Effect[] pool) { this.pool = pool; }

        public override string Name { get { return "Karışık (otomatik geçiş)"; } }
        public override string Hint { get { return "Her 15 saniyede bir başka efekte yumuşakça geçer. Hız kaydırıcısı hem efektleri hem geçişleri hızlandırır."; } }
        public string CurrentName { get; private set; }

        int Pick(long i)
        {
            int n = pool.Length;
            int a = (int)(Hash(i * 31 + 7) * n) % n;
            int prev = (int)(Hash((i - 1) * 31 + 7) * n) % n;
            return a == prev ? (a + 1) % n : a;
        }

        public override Rgb Frame(FrameContext c)
        {
            double x = c.AnimTime / Seg;
            long i = (long)Math.Floor(x);
            double local = (x - i) * Seg;
            Effect a = pool[Pick(i)];
            CurrentName = a.Name;
            Rgb ca = a.Frame(c);
            if (local < Seg - Fade) return ca;
            Effect b = pool[Pick(i + 1)];
            double t = (local - (Seg - Fade)) / Fade;
            return Rgb.Lerp(ca, b.Frame(c), (1 - Math.Cos(Math.PI * t)) / 2);
        }
    }

    // ------------------------------------------------------------------ katalog

    public static class EffectCatalog
    {
        /// <summary>Kategori başlıkları ve efektlerden oluşan liste.</summary>
        public static List<object> Build(CustomEffect custom, MorseEffect morse)
        {
            var L = new List<object>();
            const int Off = -1;

            L.Add(new EffectHeader("TEMEL"));
            Effect sabit = new StaticEffect(), nefes = new BreathEffect(), gecis = new FadeEffect();
            L.Add(sabit); L.Add(nefes); L.Add(gecis);
            L.Add(new TwoColorBreathEffect());
            L.Add(new TwoColorBlinkEffect());
            L.Add(new PulseEffect());
            L.Add(new StrobeEffect());
            Effect kalp = new HeartEffect(), isilti = new TwinkleEffect();
            L.Add(kalp); L.Add(isilti);
            L.Add(morse);

            L.Add(new EffectHeader("GÖKKUŞAĞI"));
            Effect gok = new RainbowEffect(), gokNefes = new RainbowBreathEffect();
            Effect pastel = new PaletteEffect("Pastel Gökkuşağı", "Yumuşak, pastel tonlarda yavaş gökkuşağı.", 1.6, 1, 0,
                0xFF9AA2, 0xFFDAC1, 0xFFF5BA, 0xB5EAD7, 0xA0E7FF, 0xC7CEEA, 0xE2B6FF);
            Effect dongu = new PaletteEffect("Renk Döngüsü", "Yedi ana renk arasında kısa geçişlerle adım adım dolaşır.", 0.9, 0.25, 0,
                0xFF0000, 0xFF7F00, 0xFFFF00, 0x00FF00, 0x00FFFF, 0x0000FF, 0xFF00FF);
            Effect gokFlas = new RainbowStrobeEffect(), plazma = new PlasmaEffect(), rastgele = new RandomFadeEffect(), rFlas = new RandomFlashEffect();
            L.Add(gok); L.Add(gokNefes); L.Add(pastel); L.Add(dongu); L.Add(gokFlas); L.Add(plazma); L.Add(rastgele); L.Add(rFlas);

            L.Add(new EffectHeader("DOĞA"));
            Effect ates = new FireEffect(), mum = new CandleEffect(), okyanus = new OceanEffect(), firtina = new StormEffect();
            Effect aurora = new DriftEffect("Kuzey Işıkları", "Yeşil, turkuaz ve mor tonlarda dalgalanan aurora.", 0.35, 0.45,
                0x00FF7F, 0x00E5CC, 0x2F6BFF, 0x7B2FFF, 0x00FF99);
            Effect gunBatimi = new PaletteEffect("Gün Batımı", "Turuncudan pembeye, mora ve geri yavaşça kayar.", 3.0, 1, 0,
                0xFF4500, 0xFF7F50, 0xFF2D7A, 0x8A2BE2, 0xFF2D7A, 0xFF7F50);
            Effect orman = new DriftEffect("Orman", "Yaprak arasından süzülen ışık gibi yeşil tonlar.", 0.5, 0.35,
                0x00FF40, 0x66FF00, 0x00B050, 0xA8FF3E);
            Effect lav = new DriftEffect("Lav Lambası", "Kırmızı, turuncu ve magentada ağır ağır akan lav.", 0.25, 0.25,
                0xFF1000, 0xFF6000, 0xFF0060, 0xFF3000);
            Effect buz = new DriftEffect("Buz", "Beyaz ve buz mavisi tonlarda soğuk parıltı.", 0.6, 0.3,
                0xFFFFFF, 0xA0E8FF, 0x40B0FF, 0xD0F4FF);
            Effect galaksi = new DriftEffect("Galaksi", "Mor, lacivert ve pembe tonlarda uzay bulutları.", 0.3, 0.4,
                0x3A0CA3, 0x7209B7, 0xF72585, 0x4361EE, 0x4CC9F0);
            L.Add(ates); L.Add(mum); L.Add(okyanus); L.Add(aurora); L.Add(gunBatimi); L.Add(orman); L.Add(firtina); L.Add(lav); L.Add(buz); L.Add(galaksi);

            L.Add(new EffectHeader("ÇAKAR VE RİTİM"));
            Effect polis = new PoliceEffect();
            Effect ambulans = new FlashPatternEffect("Ambulans", "Kırmızı ve beyaz sırayla çakar.", 0.08,
                0xFF0000, Off, 0xFF0000, Off, 0xFFFFFF, Off, 0xFFFFFF, Off);
            Effect itfaiye = new FlashPatternEffect("İtfaiye", "Kırmızı ve turuncu hızlı çakar.", 0.07,
                0xFF0000, Off, 0xFF0000, Off, 0xFF5000, Off, 0xFF5000, Off, Off);
            Effect alarm = new AlarmEffect(), disko = new DiscoEffect(), matrix = new MatrixEffect();
            Effect neon = new PaletteEffect("Neon Şehir", "Neon pembe ve camgöbeği arasında gidip gelir.", 1.2, 0.5, 0,
                0xFF00C8, 0x00F0FF, 0x8000FF, 0x00F0FF);
            Effect toksik = new PaletteEffect("Toksik", "Zehir yeşili tonlarda titreyen nabız.", 0.7, 1, 0.35,
                0x39FF14, 0xB0FF00, 0x00FF60);
            Effect trafik = new PaletteEffect("Trafik Lambası", "Yeşil, sarı, kırmızı sırasıyla değişir.", 2.0, 0, 0,
                0x00FF00, 0xFFC000, 0xFF0000);
            L.Add(polis); L.Add(ambulans); L.Add(itfaiye); L.Add(alarm); L.Add(disko); L.Add(neon); L.Add(matrix); L.Add(toksik); L.Add(trafik);

            L.Add(new EffectHeader("MÜZİK"));
            L.Add(new AudioLevelEffect());
            L.Add(new AudioBeatEffect());
            L.Add(new AudioFlowEffect());
            L.Add(new AudioMonoEffect());

            L.Add(new EffectHeader("ETKİLEŞİMLİ"));
            L.Add(new ScreenEffect());
            L.Add(new TypingFlashEffect());
            L.Add(new TypingSpeedEffect());
            L.Add(new MouseColorEffect());

            L.Add(new EffectHeader("SİSTEM VE ZAMAN"));
            L.Add(new CpuEffect());
            L.Add(new RamEffect());
            L.Add(new NetworkEffect());
            L.Add(new BatteryEffect());
            L.Add(new TimeOfDayEffect());
            L.Add(new PomodoroEffect());

            L.Add(new EffectHeader("TAKIM VE BAYRAK RENKLERİ"));
            L.Add(new PaletteEffect("Türk Bayrağı", "Kırmızı ve beyaz dalgalanır.", 1.5, 1, 0, 0xFF0000, 0xFF0000, 0xFFFFFF));
            L.Add(new PaletteEffect("Galatasaray", "Sarı ve kırmızı.", 1.2, 0.6, 0, 0xFFB000, 0xFF0000));
            L.Add(new PaletteEffect("Fenerbahçe", "Sarı ve lacivert.", 1.2, 0.6, 0, 0xFFD000, 0x0018A0));
            L.Add(new PaletteEffect("Beşiktaş", "Siyah ve beyaz.", 1.2, 0.6, 0, 0xFFFFFF, 0x000000));
            L.Add(new PaletteEffect("Trabzonspor", "Bordo ve mavi.", 1.2, 0.6, 0, 0xA00030, 0x30A0FF));

            L.Add(new EffectHeader("KARIŞIK VE ÖZEL"));
            L.Add(new MixEffect(new[] { gok, gokNefes, pastel, dongu, plazma, rastgele, rFlas, ates, mum, okyanus, aurora,
                gunBatimi, orman, firtina, lav, buz, galaksi, polis, disko, neon, matrix, gokFlas }));
            L.Add(custom);
            return L;
        }
    }
}
