using System;
using System.Threading;
using System.Windows.Forms;

namespace KlavyeRGB
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            L.Set(Settings.Load().Lang);   // kayıtlı dil (yoksa İngilizce)

            bool created;
            using (var mutex = new Mutex(true, "KlavyeRGB_TekKopya_3b8d", out created))
            {
                if (!created)
                {
                    MessageBox.Show(L.T("Klavye RGB zaten çalışıyor. Sağ alttaki sistem tepsisindeki simgeye bak."),
                        L.T("Klavye RGB"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                bool tray = false;
                foreach (string a in args)
                    if (a.Equals("/tray", StringComparison.OrdinalIgnoreCase)) tray = true;

                Application.Run(new MainForm(tray));
                GC.KeepAlive(mutex);
            }
        }
    }
}
