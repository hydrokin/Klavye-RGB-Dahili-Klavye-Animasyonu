using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace KlavyeRGB
{
    public sealed class Settings
    {
        public string EffectName = "Gökkuşağı";
        public int Speed = 8;
        public int Brightness = 100;
        public int Color1 = 0x00C8FF;
        public int Color2 = 0xFF00A0;
        public bool Correction = true;
        public string MorseText = "SOS";
        public string Lang = "en";   // varsayılan arayüz dili İngilizce
        public List<Keyframe> Frames;

        static string FolderPath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KlavyeRGB"); }
        }
        static string FilePath { get { return Path.Combine(FolderPath, "ayarlar.txt"); } }

        public static Settings Load()
        {
            var s = new Settings();
            try
            {
                if (!File.Exists(FilePath)) return s;
                var frameLines = new List<string>();
                foreach (string raw in File.ReadAllLines(FilePath, Encoding.UTF8))
                {
                    int eq = raw.IndexOf('=');
                    if (eq <= 0) continue;
                    string k = raw.Substring(0, eq).Trim(), v = raw.Substring(eq + 1).Trim();
                    int n;
                    switch (k)
                    {
                        case "effect": s.EffectName = v; break;
                        case "speed": if (int.TryParse(v, out n)) s.Speed = Math.Max(0, Math.Min(16, n)); break;
                        case "brightness": if (int.TryParse(v, out n)) s.Brightness = Math.Max(5, Math.Min(100, n)); break;
                        case "color1": if (FrameIO.TryParseHex(v, out n)) s.Color1 = n; break;
                        case "color2": if (FrameIO.TryParseHex(v, out n)) s.Color2 = n; break;
                        case "correction": s.Correction = v != "0"; break;
                        case "morse": s.MorseText = v; break;
                        case "lang": if (Array.IndexOf(L.Codes, v) >= 0) s.Lang = v; break;
                        case "frame": frameLines.Add(v); break;
                    }
                }
                var frames = FrameIO.Parse(frameLines);
                if (frames.Count > 0) s.Frames = frames;
            }
            catch { }
            return s;
        }

        public void Save()
        {
            Directory.CreateDirectory(FolderPath);
            var sb = new StringBuilder();
            sb.AppendLine("lang=" + Lang);
            sb.AppendLine("effect=" + EffectName);
            sb.AppendLine("speed=" + Speed.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("brightness=" + Brightness.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("color1=" + Color1.ToString("X6"));
            sb.AppendLine("color2=" + Color2.ToString("X6"));
            sb.AppendLine("correction=" + (Correction ? "1" : "0"));
            sb.AppendLine("morse=" + (MorseText ?? "").Replace("\r", " ").Replace("\n", " "));
            if (Frames != null)
                foreach (var f in Frames) sb.AppendLine("frame=" + FrameIO.FormatLine(f));
            File.WriteAllText(FilePath, sb.ToString(), new UTF8Encoding(false));
        }
    }

    /// <summary>.klavyeanim dosyası: her satır "RRGGBB süre_ms yumuşak(1/0)", # ile başlayanlar yorum.</summary>
    public static class FrameIO
    {
        public static bool TryParseHex(string s, out int value)
        {
            s = (s ?? "").Trim().TrimStart('#');
            if (s.Length == 6 && int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)) return true;
            value = 0;
            return false;
        }

        public static List<Keyframe> Parse(IEnumerable<string> lines)
        {
            var list = new List<Keyframe>();
            foreach (string raw in lines)
            {
                // "#FF0000 500 1" gibi # ile yazılmış renkler veri; okunamayan satırlar (ör. "# not") yorum sayılır.
                string line = raw.Trim();
                if (line.Length == 0) continue;
                string[] p = line.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (p.Length < 2) continue;
                int color, ms;
                if (!TryParseHex(p[0], out color) || !int.TryParse(p[1], out ms)) continue;
                bool fade = p.Length < 3 || p[2] != "0";
                list.Add(new Keyframe(color, Math.Max(30, Math.Min(20000, ms)), fade));
            }
            return list;
        }

        public static string FormatLine(Keyframe f)
        {
            return f.Color.ToString("X6") + " " + f.Ms.ToString(CultureInfo.InvariantCulture) + " " + (f.Fade ? "1" : "0");
        }

        public static void Save(string path, List<Keyframe> frames)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Klavye RGB animasyonu");
            sb.AppendLine("# renk(RRGGBB) süre_ms yumuşak_geçiş(1/0)");
            foreach (var f in frames) sb.AppendLine(FormatLine(f));
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        }
    }

    public static class AutoStart
    {
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string ValueName = "KlavyeRGB";

        public static bool IsEnabled()
        {
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(RunKey))
                return k != null && k.GetValue(ValueName) != null;
        }

        public static void Set(bool on)
        {
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(RunKey))
            {
                if (on) k.SetValue(ValueName, "\"" + Application.ExecutablePath + "\" /tray");
                else k.DeleteValue(ValueName, false);
            }
        }
    }
}
