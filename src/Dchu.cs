using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace KlavyeRGB
{
    /// <summary>Clevo'nun kayıtlı (Fn tuşları / Control Center) klavye ışığı ayarı.</summary>
    public struct SavedLight
    {
        public byte R, G, B;
        public int Mode;   // 8 = sabit renk, diğerleri donanım efektleri
        public bool On;
    }

    /// <summary>
    /// Clevo/Insyde "DCHU" arayüzü (InsydeDCHU.dll, Clevo ACPI köprü sürücüsüyle gelir).
    /// Sadece klavye ışığı komutu (103 = 0x67) kullanılır, kalıcı ayarlara yazılmaz.
    /// Protokol: tuxedo-drivers (clevo_interfaces.h) ve opencontrol ile doğrulandı.
    /// </summary>
    public static class Dchu
    {
        const int CmdKeyboardLight = 103;
        const int ReadOk = 0x1000;
        static readonly object sync = new object();
        static bool loaded;

        public static string DllPath { get; private set; }

        // Hata, dil değişince yeniden çevrilebilsin diye anahtar + parametre olarak tutulur.
        static volatile string errKey, errArg;
        public static string LastError
        {
            get { string k = errKey; return k == null ? null : string.Format(L.T(k), errArg); }
        }
        static void SetError(string key, string arg) { errArg = arg; errKey = key; }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string path);
        [DllImport("InsydeDCHU.dll")]
        static extern int SetDCHU_Data(int command, byte[] buffer, int length);
        [DllImport("InsydeDCHU.dll")]
        static extern int ReadAppSettings(int page, int offset, int length, byte[] buffer);

        public static bool Load()
        {
            if (loaded) return true;
            string path = FindDll();
            if (path == null)
            {
                SetError("Clevo klavye sürücüsü (InsydeDCHU.dll) bulunamadı. \"Fn hotkeys and OSD\" sürücüsü kurulu olmalı.", null);
                return false;
            }
            if (LoadLibrary(path) == IntPtr.Zero)
            {
                SetError("InsydeDCHU.dll yüklenemedi (hata {0}).", Marshal.GetLastWin32Error().ToString());
                return false;
            }
            DllPath = path;
            loaded = true;
            return true;
        }

        static string FindDll()
        {
            try
            {
                using (RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\CCDCHUService"))
                {
                    string img = k == null ? null : k.GetValue("ImagePath") as string;
                    if (!string.IsNullOrEmpty(img))
                    {
                        img = Environment.ExpandEnvironmentVariables(img.Trim().Trim('"'));
                        if (img.StartsWith(@"\??\")) img = img.Substring(4);
                        string p = Path.Combine(Path.GetDirectoryName(img), "InsydeDCHU.dll");
                        if (File.Exists(p)) return p;
                    }
                }
            }
            catch { }
            try
            {
                string repo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "DriverStore", "FileRepository");
                string best = null;
                DateTime bestTime = DateTime.MinValue;
                foreach (string d in Directory.GetDirectories(repo, "acpibridge*"))
                {
                    string p = Path.Combine(d, "InsydeDCHU.dll");
                    if (!File.Exists(p)) continue;
                    DateTime t = File.GetLastWriteTimeUtc(p);
                    if (t > bestTime) { best = p; bestTime = t; }
                }
                return best;
            }
            catch { return null; }
        }

        static bool Send(byte b0, byte b1, byte b2, byte b3)
        {
            if (!loaded) return false;
            try
            {
                lock (sync) SetDCHU_Data(CmdKeyboardLight, new byte[] { b0, b1, b2, b3 }, 4);
                return true;
            }
            catch (Exception ex)
            {
                SetError("Klavyeye komut gönderilemedi: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>Tüm klavyenin rengi (tek bölge = 0xF0). Tampon sırası G, R, B, bölge.</summary>
        public static bool SetColor(byte r, byte g, byte b) { return Send(g, r, b, 0xF0); }
        public static bool TurnOn() { return Send(0, 0, 1, 0xE0); }
        public static bool TurnOff() { return Send(0, 0, 0, 0xE0); }

        public static SavedLight ReadSaved()
        {
            var s = new SavedLight { R = 255, G = 255, B = 255, Mode = 8, On = true };
            if (!loaded) return s;
            try
            {
                byte[] b = new byte[4];
                lock (sync)
                {
                    if (ReadAppSettings(2, 81, 3, b) == ReadOk) { s.R = b[0]; s.G = b[1]; s.B = b[2]; }
                    b = new byte[4];
                    if (ReadAppSettings(2, 32, 1, b) == ReadOk) s.Mode = b[0];
                    b = new byte[4];
                    if (ReadAppSettings(2, 84, 1, b) == ReadOk) s.On = b[0] != 0;
                }
            }
            catch { }
            return s;
        }

        /// <summary>Klavyeyi kayıtlı ayarına (renk, donanım efekti ya da kapalı) döndürür.</summary>
        public static void Restore()
        {
            if (!loaded) return;
            SavedLight s = ReadSaved();
            if (!s.On) { TurnOff(); return; }
            SetColor(s.R, s.G, s.B);
            byte tag = ModeTag(s.Mode);
            if (tag != 0) Send(0, 0, 0, tag);
        }

        // Kayıtlı mod numarası -> donanım efekt komutu (opencontrol eşlemesi)
        static byte ModeTag(int mode)
        {
            switch (mode)
            {
                case 0: return 112; // rastgele
                case 2: return 16;  // nefes
                case 3: return 51;  // döngü
                case 4: return 176; // dalga
                case 5: return 128; // dans
                case 6: return 144; // tempo
                case 7: return 160; // flaş
                default: return 0;  // 8 = sabit renk
            }
        }
    }
}
