using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace KlavyeRGB
{
    public sealed class MainForm : Form
    {
        static readonly Color PanelDark = Color.FromArgb(22, 24, 28);

        readonly bool startHidden;
        bool shownOnce;
        readonly Settings settings;
        readonly Engine engine;
        readonly List<object> allItems;                       // başlıklar + efektler
        readonly List<Effect> allEffects = new List<Effect>();
        readonly CustomEffect custom = new CustomEffect();
        readonly MorseEffect morse = new MorseEffect();

        Label lblDevice, lblGlobe, lblHex, lblHint, lblInfo, lblSpeed, lblSpeedVal, lblBright, lblBrightVal, lblStatus, lblMorseCaption, lblMorseHelp;
        CheckBox chkAutostart, chkCorrect;
        ComboBox cmbLang;
        GroupBox grpEffects;
        TextBox txtSearch, txtMorse;
        Panel pnlMorse, pnlCustom;
        ListBox lstEffects;
        KeyboardPreview preview;
        TrackBar trkSpeed, trkBright;
        Button btnColor1, btnColor2, btnStart, btnTray;
        readonly Button[] customButtons = new Button[7];
        DataGridView grid;
        NotifyIcon tray;
        ToolStripMenuItem miShow, miStartStop, miExit;
        Timer uiTimer;
        Icon appIcon;
        Font headerFont;
        float dpi = 1f;
        string searchCue = "";

        Effect current;
        int color1, color2;
        bool loadingGrid, loadingUi, trayHintShown, shownRunning, filling, stoppedOnce;
        int highlightedRow = -1, shownPreview = -2, lastListIndex = -1;

        static readonly string[] CustomButtonKeys = { "+ Adım ekle", "Çoğalt", "Sil", "▲ Yukarı", "▼ Aşağı", "Aç…", "Kaydet…" };
        const string InfoKey = "• Program sadece canlı renk gönderir, klavyenin kayıtlı ayarlarını değiştirmez.\n• Durdurunca ya da kapatınca klavye kendi rengine/efektine geri döner.\n• Fn tuşlarıyla parlaklığı ayarlamaya devam edebilirsin.\n• Tepsiye küçültürsen animasyon arka planda sürer.";
        const string MorseHelpKey = "Harfler, rakamlar ve Türkçe karakterler desteklenir. Boşluk kelime arası bekleme yapar. Renk 1 ile yanar.";

        public MainForm(bool startHidden)
        {
            this.startHidden = startHidden;
            settings = Settings.Load();
            L.Set(settings.Lang);
            engine = new Engine();
            allItems = EffectCatalog.Build(custom, morse);
            foreach (object o in allItems) { var e = o as Effect; if (e != null) allEffects.Add(e); }
            BuildUi();
            LoadSettingsIntoUi();
            ApplyLanguage();

            uiTimer = new Timer { Interval = 33 };
            uiTimer.Tick += OnUiTick;
            uiTimer.Start();

            if (!engine.Available) btnStart.Enabled = false;
            else if (startHidden) SetRunning(true);
            RefreshStatus();
        }

        // ------------------------------------------------------------------ arayüz

        /// <summary>96 DPI'daki ölçüyü ekran ölçeğine çevirir (ör. %125'te 100 → 125).</summary>
        int S(int v) { return (int)Math.Round(v * dpi); }

        /// <summary>
        /// Öğeyi 96 DPI koordinatlarıyla ekler ve DPI'ya göre bir kez ölçekler.
        /// WinForms'un otomatik ölçeklemesi kapalı: Segoe UI yazı tipiyle Label'ları iki kez büyütüyordu.
        /// </summary>
        T Add<T>(Control parent, T c, int x, int y, int w, int h) where T : Control
        {
            if (w > 0) c.Bounds = new Rectangle(S(x), S(y), S(w), S(h)); else c.Location = new Point(S(x), S(y));
            parent.Controls.Add(c);
            return c;
        }

        static string LaptopName()
        {
            try
            {
                using (RegistryKey k = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS"))
                {
                    if (k != null)
                    {
                        string m = Convert.ToString(k.GetValue("SystemManufacturer")).Trim();
                        string p = Convert.ToString(k.GetValue("SystemProductName")).Trim();
                        string s = (m + " " + p).Trim();
                        if (s.Length > 0) return s;
                    }
                }
            }
            catch { }
            return "Laptop";
        }

        void BuildUi()
        {
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.None;   // ölçekleme Add() içinde elle yapılıyor
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) dpi = g.DpiX / 96f;
            Font = new Font("Segoe UI", 9F);      // punto cinsinden: DPI'ya göre kendiliğinden büyür
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(S(800), S(598));
            appIcon = LoadAppIcon();
            Icon = appIcon;

            // Üst satır: cihaz adı | başlangıç | dil
            lblDevice = Add(this, new Label { AutoEllipsis = true }, 12, 14, 380, 20);
            chkAutostart = Add(this, new CheckBox { AutoSize = true }, 420, 12, 0, 0);
            chkAutostart.CheckedChanged += OnAutostartChanged;
            lblGlobe = Add(this, new Label { AutoSize = true, Text = "🌐", Font = new Font("Segoe UI Emoji", 10F) }, 630, 11, 0, 0);
            cmbLang = Add(this, new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, 656, 10, 132, 24);
            foreach (string n in L.NativeNames) cmbLang.Items.Add(n);
            cmbLang.SelectedIndexChanged += OnLanguageChanged;

            grpEffects = Add(this, new GroupBox(), 12, 40, 230, 498);
            txtSearch = Add(grpEffects, new TextBox(), 8, 22, 214, 24);
            txtSearch.HandleCreated += delegate { SendCue(); };
            txtSearch.TextChanged += delegate { FillList(); };
            lstEffects = Add(grpEffects, new ListBox { IntegralHeight = false, BorderStyle = BorderStyle.None, DrawMode = DrawMode.OwnerDrawFixed }, 8, 52, 214, 438);
            lstEffects.ItemHeight = (int)(25 * dpi);
            lstEffects.DrawItem += OnDrawEffectItem;
            lstEffects.SelectedIndexChanged += OnEffectChanged;
            lstEffects.DoubleClick += delegate { if (lstEffects.SelectedItem is Effect) SetRunning(true); };
            FillList();

            preview = Add(this, new KeyboardPreview { BackColor = PanelDark }, 254, 46, 534, 142);
            lblHex = Add(this, new Label { AutoSize = true, ForeColor = Color.DimGray }, 254, 190, 0, 0);
            lblHint = Add(this, new Label { ForeColor = Color.DimGray }, 254, 212, 534, 36);

            lblSpeed = Add(this, new Label { AutoSize = true }, 254, 258, 0, 0);
            trkSpeed = Add(this, new TrackBar { AutoSize = false, Minimum = 0, Maximum = 16, TickFrequency = 2, Value = 8, LargeChange = 2 }, 372, 250, 310, 40);
            lblSpeedVal = Add(this, new Label { AutoSize = true }, 690, 258, 0, 0);
            trkSpeed.ValueChanged += delegate { engine.SpeedStep = trkSpeed.Value; UpdateSliderLabels(); };

            lblBright = Add(this, new Label { AutoSize = true }, 254, 298, 0, 0);
            trkBright = Add(this, new TrackBar { AutoSize = false, Minimum = 5, Maximum = 100, TickFrequency = 10, Value = 100, LargeChange = 10, SmallChange = 5 }, 372, 290, 310, 40);
            lblBrightVal = Add(this, new Label { AutoSize = true }, 690, 298, 0, 0);
            trkBright.ValueChanged += delegate { engine.BrightnessPct = trkBright.Value; UpdateSliderLabels(); };

            btnColor1 = Add(this, new Button { FlatStyle = FlatStyle.Flat }, 254, 334, 120, 28);
            btnColor2 = Add(this, new Button { FlatStyle = FlatStyle.Flat }, 382, 334, 120, 28);
            btnColor1.Click += delegate { PickColor(1); };
            btnColor2.Click += delegate { PickColor(2); };
            chkCorrect = Add(this, new CheckBox { AutoSize = true, Checked = true }, 516, 339, 0, 0);
            chkCorrect.CheckedChanged += delegate { engine.ColorCorrection = chkCorrect.Checked; };

            lblInfo = Add(this, new Label { ForeColor = Color.DimGray }, 254, 376, 534, 160);

            pnlMorse = Add(this, new Panel { Visible = false }, 254, 372, 534, 166);
            lblMorseCaption = Add(pnlMorse, new Label { AutoSize = true }, 0, 4, 0, 0);
            txtMorse = Add(pnlMorse, new TextBox(), 0, 26, 420, 24);
            lblMorseHelp = Add(pnlMorse, new Label { ForeColor = Color.DimGray }, 0, 58, 520, 60);
            txtMorse.TextChanged += delegate { morse.Text = txtMorse.Text; };

            pnlCustom = Add(this, new Panel { Visible = false }, 254, 370, 534, 170);
            grid = Add(pnlCustom, new DataGridView(), 0, 0, 420, 170);
            SetupGrid();
            EventHandler[] btnActions = { OnAddFrame, OnDuplicateFrame, OnDeleteFrame, OnMoveUp, OnMoveDown, OnOpenFile, OnSaveFile };
            for (int i = 0; i < customButtons.Length; i++)
            {
                customButtons[i] = Add(pnlCustom, new Button(), 428, i * 24, 106, 23);
                customButtons[i].Click += btnActions[i];
            }

            btnStart = Add(this, new Button { Font = new Font("Segoe UI", 10.5F, FontStyle.Bold) }, 12, 548, 170, 40);
            lblStatus = Add(this, new Label { TextAlign = ContentAlignment.MiddleLeft }, 194, 548, 440, 40);
            btnTray = Add(this, new Button { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(S(120), S(30)) }, 668, 553, 0, 0);
            btnStart.Click += delegate { SetRunning(!engine.WantRunning); };
            btnTray.Click += delegate { HideToTray(); };

            var menu = new ContextMenuStrip();
            miShow = new ToolStripMenuItem("", null, delegate { ShowFromTray(); });
            miStartStop = new ToolStripMenuItem("", null, delegate { SetRunning(!engine.WantRunning); });
            miExit = new ToolStripMenuItem("", null, delegate { Close(); });
            menu.Items.AddRange(new ToolStripItem[] { miShow, miStartStop, new ToolStripSeparator(), miExit });
            Icon trayIcon;
            try { trayIcon = new Icon(appIcon, SystemInformation.SmallIconSize); } catch { trayIcon = appIcon; }
            tray = new NotifyIcon { Icon = trayIcon, ContextMenuStrip = menu, Visible = true };
            tray.DoubleClick += delegate { ShowFromTray(); };

            ResumeLayout(false);
            PerformLayout();
        }

        void SetupGrid()
        {
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AllowUserToOrderColumns = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = SystemColors.Window;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            grid.Columns.Add(new DataGridViewTextBoxColumn { ReadOnly = true, FillWeight = 35, SortMode = DataGridViewColumnSortMode.NotSortable });
            grid.Columns.Add(new DataGridViewTextBoxColumn { ReadOnly = true, FillWeight = 80, SortMode = DataGridViewColumnSortMode.NotSortable });
            grid.Columns.Add(new DataGridViewTextBoxColumn { FillWeight = 60, SortMode = DataGridViewColumnSortMode.NotSortable });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { FillWeight = 70 });

            grid.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
                var cell = grid.Rows[e.RowIndex].Cells[1];
                int col = cell.Tag is int ? (int)cell.Tag : 0xFFFFFF;
                int picked;
                if (AskColor(col, out picked)) { SetColorCell(cell, picked); SyncFramesFromGrid(); }
            };
            grid.CurrentCellDirtyStateChanged += delegate
            {
                if (grid.IsCurrentCellDirty && grid.CurrentCell is DataGridViewCheckBoxCell)
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            grid.CellValueChanged += delegate { if (!loadingGrid) SyncFramesFromGrid(); };
            grid.CellValidating += (s, e) =>
            {
                if (e.ColumnIndex != 2 || loadingGrid) return;
                int ms;
                if (!int.TryParse(Convert.ToString(e.FormattedValue), out ms) || ms < 30 || ms > 20000)
                {
                    grid.Rows[e.RowIndex].ErrorText = L.T("Süre 30 ile 20000 ms arasında olmalı");
                    e.Cancel = true;
                }
            };
            grid.CellEndEdit += (s, e) => { grid.Rows[e.RowIndex].ErrorText = ""; };
        }

        // ------------------------------------------------------------------ dil

        void OnLanguageChanged(object sender, EventArgs e)
        {
            if (loadingUi || cmbLang.SelectedIndex < 0) return;
            L.Set(L.Codes[cmbLang.SelectedIndex]);
            settings.Lang = L.Code;
            SaveSettings();          // seçim hemen kalıcı olsun
            ApplyLanguage();
        }

        /// <summary>Tüm görünen metinleri seçili dile göre yeniden yazar ve hizalar.</summary>
        void ApplyLanguage()
        {
            loadingUi = true;
            cmbLang.SelectedIndex = L.Index;
            loadingUi = false;

            Text = L.T("Klavye RGB") + " – " + L.T("Dahili Klavye Animasyonu");
            lblDevice.Text = LaptopName() + "  ·  " + L.T("dahili klavye (Clevo, tek bölge RGB)");
            chkAutostart.Text = L.T("Windows açılınca başlat");
            grpEffects.Text = L.T("Efektler") + " (" + allEffects.Count + ")";
            searchCue = L.T("Efekt ara…");
            SendCue();
            lblBright.Text = L.T("Parlaklık:");
            btnColor1.Text = L.T("Renk 1");
            btnColor2.Text = L.T("Renk 2");
            chkCorrect.Text = L.T("Renk dengesini düzelt");
            lblInfo.Text = L.T(InfoKey);
            lblMorseCaption.Text = L.T("Mors koduyla yazılacak metin:");
            lblMorseHelp.Text = L.T(MorseHelpKey);
            for (int i = 0; i < customButtons.Length; i++) customButtons[i].Text = L.T(CustomButtonKeys[i]);
            grid.Columns[0].HeaderText = L.T("Adım");
            grid.Columns[1].HeaderText = L.T("Renk (tıkla)");
            grid.Columns[2].HeaderText = L.T("Süre (ms)");
            grid.Columns[3].HeaderText = L.T("Yumuşak geçiş");
            btnTray.Text = L.T("Tepsiye küçült");
            miShow.Text = L.T("Göster");
            miExit.Text = L.T("Çıkış");
            UpdateRunButtons();
            UpdateTrayText();
            if (current != null)
            {
                lblHint.Text = current.DisplayHint;
                lblSpeed.Text = current.SliderLabel != null ? L.T(current.SliderLabel) : L.T("Hız:");
            }
            UpdateSliderLabels();
            FillList();
            shownPreview = -2;
            UpdatePreview();
            RefreshStatus();
            LayoutDynamic();
        }

        /// <summary>Metin uzunluğu dile göre değişen öğeleri hizalar.</summary>
        void LayoutDynamic()
        {
            int m = (int)(12 * dpi), gap = (int)(10 * dpi);
            int right = ClientSize.Width - m;
            cmbLang.Left = right - cmbLang.Width;
            lblGlobe.Left = cmbLang.Left - (int)(4 * dpi) - lblGlobe.Width;
            chkAutostart.Left = lblGlobe.Left - gap - chkAutostart.Width;
            lblDevice.Width = Math.Max((int)(80 * dpi), chkAutostart.Left - gap - lblDevice.Left);

            btnTray.Left = right - btnTray.Width;
            lblStatus.Width = Math.Max((int)(100 * dpi), btnTray.Left - gap - lblStatus.Left);

            int sliderLeft = Math.Max(lblSpeed.Right, lblBright.Right) + (int)(8 * dpi);
            int minLeft = (int)(372 * dpi);
            if (sliderLeft < minLeft) sliderLeft = minLeft;
            int sliderRight = lblSpeedVal.Left - (int)(8 * dpi);
            trkSpeed.Left = trkBright.Left = sliderLeft;
            trkSpeed.Width = trkBright.Width = Math.Max((int)(150 * dpi), sliderRight - sliderLeft);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        void SendCue()
        {
            if (txtSearch != null && txtSearch.IsHandleCreated)
                SendMessage(txtSearch.Handle, 0x1501, new IntPtr(1), searchCue);   // EM_SETCUEBANNER
        }

        // ------------------------------------------------------------------ renkler

        static Color ToColor(int rgb) { return Color.FromArgb((rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255); }

        static Color Contrast(int rgb)
        {
            double l = 0.299 * ((rgb >> 16) & 255) + 0.587 * ((rgb >> 8) & 255) + 0.114 * (rgb & 255);
            return l > 140 ? Color.Black : Color.White;
        }

        static void SetColorCell(DataGridViewCell cell, int rgb)
        {
            cell.Tag = rgb;
            cell.Value = Rgb.Hex(rgb);
            cell.Style.BackColor = cell.Style.SelectionBackColor = ToColor(rgb);
            cell.Style.ForeColor = cell.Style.SelectionForeColor = Contrast(rgb);
        }

        bool AskColor(int currentColor, out int picked)
        {
            using (var dlg = new ColorDialog { FullOpen = true, AnyColor = true, Color = ToColor(currentColor) })
            {
                picked = currentColor;
                if (dlg.ShowDialog(this) != DialogResult.OK) return false;
                picked = (dlg.Color.R << 16) | (dlg.Color.G << 8) | dlg.Color.B;
                return true;
            }
        }

        void PickColor(int which)
        {
            int picked;
            if (!AskColor(which == 1 ? color1 : color2, out picked)) return;
            if (which == 1) color1 = picked; else color2 = picked;
            ApplyColors();
        }

        void ApplyColors()
        {
            engine.Color1 = color1;
            engine.Color2 = color2;
            btnColor1.BackColor = ToColor(color1); btnColor1.ForeColor = Contrast(color1);
            btnColor2.BackColor = ToColor(color2); btnColor2.ForeColor = Contrast(color2);
        }

        // ------------------------------------------------------------------ ayarlar

        void LoadSettingsIntoUi()
        {
            loadingUi = true;
            trkSpeed.Value = settings.Speed;
            trkBright.Value = settings.Brightness;
            chkCorrect.Checked = settings.Correction;
            engine.SpeedStep = settings.Speed;
            engine.BrightnessPct = settings.Brightness;
            engine.ColorCorrection = settings.Correction;
            color1 = settings.Color1;
            color2 = settings.Color2;
            ApplyColors();
            LoadGrid(settings.Frames ?? CustomEffect.Defaults(), 0);
            txtMorse.Text = settings.MorseText;
            morse.Text = settings.MorseText;

            bool auto = false;
            try
            {
                auto = AutoStart.IsEnabled();
                if (auto) AutoStart.Set(true);   // exe taşındıysa yolu güncelle
            }
            catch { }
            chkAutostart.Checked = auto;

            Effect want = null;
            foreach (var e in allEffects) if (e.Name == settings.EffectName) want = e;
            if (want == null) foreach (var e in allEffects) if (e is RainbowEffect) want = e;
            loadingUi = false;
            lstEffects.SelectedItem = want;
        }

        void SaveSettings()
        {
            if (current != null) settings.EffectName = current.Name;   // Türkçe ad = kalıcı kimlik
            settings.Speed = trkSpeed.Value;
            settings.Brightness = trkBright.Value;
            settings.Color1 = color1;
            settings.Color2 = color2;
            settings.Correction = chkCorrect.Checked;
            settings.MorseText = txtMorse.Text;
            settings.Lang = L.Code;
            settings.Frames = custom.GetFrames();
            try { settings.Save(); } catch { }
        }

        void OnAutostartChanged(object sender, EventArgs e)
        {
            if (loadingUi) return;
            try { AutoStart.Set(chkAutostart.Checked); }
            catch (Exception ex) { MessageBox.Show(this, L.T("Başlangıç ayarı yazılamadı: ") + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        // ------------------------------------------------------------------ efekt listesi

        void OnDrawEffectItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            object item = lstEffects.Items[e.Index];
            var header = item as EffectHeader;
            if (header != null)
            {
                if (headerFont == null) headerFont = new Font(lstEffects.Font.FontFamily, lstEffects.Font.Size * 0.85f, FontStyle.Bold);
                using (var b = new SolidBrush(lstEffects.BackColor)) e.Graphics.FillRectangle(b, e.Bounds);
                var hr = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 4, e.Bounds.Width - 8, e.Bounds.Height - 4);
                TextRenderer.DrawText(e.Graphics, header.DisplayText, headerFont, hr, Color.FromArgb(138, 60, 255),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
                using (var p = new Pen(Color.FromArgb(225, 220, 240)))
                    e.Graphics.DrawLine(p, e.Bounds.X + 4, e.Bounds.Bottom - 1, e.Bounds.Right - 4, e.Bounds.Bottom - 1);
                return;
            }
            bool sel = (e.State & DrawItemState.Selected) != 0;
            Color bg = sel ? Color.FromArgb(0, 120, 215) : lstEffects.BackColor;
            using (var b = new SolidBrush(bg)) e.Graphics.FillRectangle(b, e.Bounds);
            var eff = (Effect)item;
            string text = (engine.IsRunning && eff == current ? "▶ " : "") + eff.DisplayName;
            var r = new Rectangle(e.Bounds.X + 14, e.Bounds.Y, e.Bounds.Width - 14, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, text, lstEffects.Font, r, sel ? Color.White : lstEffects.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        /// <summary>Aramada büyük/küçük harf ve Türkçe karakterler fark etmesin: "ates" → "Ateş".</summary>
        static string Fold(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char ch in s.ToLowerInvariant())
            {
                switch (ch)
                {
                    case 'ç': sb.Append('c'); break;
                    case 'ğ': sb.Append('g'); break;
                    case 'ı': case 'İ': case 'i': sb.Append('i'); break;
                    case 'ö': sb.Append('o'); break;
                    case 'ş': sb.Append('s'); break;
                    case 'ü': sb.Append('u'); break;
                    case 'á': case 'à': case 'â': case 'ã': case 'ä': sb.Append('a'); break;
                    case 'é': case 'è': case 'ê': case 'ë': sb.Append('e'); break;
                    case 'í': case 'î': case 'ï': sb.Append('i'); break;
                    case 'ó': case 'ô': case 'õ': sb.Append('o'); break;
                    case 'ú': case 'û': sb.Append('u'); break;
                    case 'ñ': sb.Append('n'); break;
                    case 'ß': sb.Append("ss"); break;
                    case '̇': break;   // "İ".ToLowerInvariant() sonrası kalan nokta
                    default: sb.Append(ch); break;
                }
            }
            return sb.ToString();
        }

        /// <summary>Listeyi arama metnine göre doldurur; başlıklar sadece eşleşen efekt varsa görünür.</summary>
        void FillList()
        {
            string q = Fold(txtSearch.Text.Trim());
            var items = new List<object>();
            EffectHeader pending = null;
            foreach (object o in allItems)
            {
                var h = o as EffectHeader;
                if (h != null) { pending = h; continue; }
                var eff = (Effect)o;
                bool match = q.Length == 0 || Fold(eff.DisplayName).Contains(q) || Fold(eff.Name).Contains(q)
                             || (pending != null && (Fold(pending.DisplayText).Contains(q) || Fold(pending.Text).Contains(q)));
                if (!match) continue;
                if (pending != null) { items.Add(pending); pending = null; }
                items.Add(eff);
            }
            filling = true;
            lstEffects.BeginUpdate();
            lstEffects.Items.Clear();
            foreach (object o in items) lstEffects.Items.Add(o);
            int sel = current != null ? lstEffects.Items.IndexOf(current) : -1;
            lstEffects.SelectedIndex = sel;
            lstEffects.EndUpdate();
            lastListIndex = sel;
            filling = false;
            if (sel < 0 && q.Length > 0)
            {
                for (int i = 0; i < lstEffects.Items.Count; i++)
                    if (lstEffects.Items[i] is Effect) { lstEffects.TopIndex = Math.Max(0, i - 1); break; }
            }
        }

        void OnEffectChanged(object sender, EventArgs e)
        {
            if (filling) return;
            int idx = lstEffects.SelectedIndex;
            if (idx >= 0 && lstEffects.Items[idx] is EffectHeader)
            {
                // Başlık seçilemez: hareket yönünde bir sonraki efekte atla
                int dir = idx < lastListIndex ? -1 : 1;
                int j = idx + dir;
                if (j < 0 || j >= lstEffects.Items.Count) j = idx - dir;
                if (j >= 0 && j < lstEffects.Items.Count && lstEffects.Items[j] is Effect) lstEffects.SelectedIndex = j;
                else lstEffects.SelectedIndex = lastListIndex;
                return;
            }
            lastListIndex = idx;
            var eff = lstEffects.SelectedItem as Effect;
            if (eff == null || eff == current) return;
            current = eff;
            engine.Effect = eff;
            lblHint.Text = current.DisplayHint;
            btnColor1.Visible = current.ColorCount >= 1;
            btnColor2.Visible = current.ColorCount >= 2;
            bool slider = current.SliderLabel != null;
            lblSpeed.Text = slider ? L.T(current.SliderLabel) : L.T("Hız:");
            trkSpeed.Enabled = slider;
            pnlCustom.Visible = current == custom;
            pnlMorse.Visible = current == morse;
            lblInfo.Visible = current != custom && current != morse;
            ClearCustomHighlight();
            UpdateSliderLabels();
            RefreshStatus();
            UpdateTrayText();
            lstEffects.Invalidate();
            if (IsHandleCreated) LayoutDynamic();
        }

        void UpdateSliderLabels()
        {
            System.Globalization.CultureInfo ci;
            try { ci = System.Globalization.CultureInfo.GetCultureInfo(L.Code); } catch { ci = System.Globalization.CultureInfo.InvariantCulture; }
            lblSpeedVal.Text = trkSpeed.Enabled ? Engine.SpeedMultiplier(trkSpeed.Value).ToString("0.00", ci) + "x" : "–";
            lblBrightVal.Text = (trkBright.Value / 100.0).ToString("P0", ci);   // en "100%", tr "%100", fr "100 %"
        }

        // ------------------------------------------------------------------ çalıştırma

        void SetRunning(bool on)
        {
            if (!engine.Available || current == null) return;
            engine.WantRunning = on;
            if (!on) { stoppedOnce = true; ClearCustomHighlight(); }
            UpdateRunButtons();
            RefreshStatus();
            UpdateTrayText();
            lstEffects.Invalidate();
        }

        void UpdateRunButtons()
        {
            bool on = engine.WantRunning;
            btnStart.Text = on ? "■  " + L.T("Durdur") : "▶  " + L.T("Başlat");
            miStartStop.Text = on ? L.T("Durdur") : L.T("Başlat");
        }

        void UpdateTrayText()
        {
            string t = L.T("Klavye RGB") + " – " + (engine.WantRunning && current != null ? current.DisplayName : L.T("durdu"));
            tray.Text = t.Length > 63 ? t.Substring(0, 63) : t;
        }

        /// <summary>Durum satırını geçerli duruma ve dile göre yazar.</summary>
        void RefreshStatus()
        {
            if (!engine.Available) { SetStatus(Dchu.LastError ?? engine.Status, true); return; }
            if (engine.StatusError) { SetStatus(Dchu.LastError ?? engine.Status, true); return; }
            if (engine.WantRunning && current != null)
            {
                var mix = current as MixEffect;
                string s = L.T("Çalışıyor: ") + current.DisplayName;
                if (mix != null && engine.IsRunning && mix.CurrentName != null)
                    s = L.T("Çalışıyor: ") + L.T("Karışık") + " → " + L.T(mix.CurrentName);
                SetStatus(s, false);
            }
            else if (stoppedOnce) SetStatus(L.T("Durduruldu. Klavye kendi rengine döndü."), false);
            else SetStatus(L.T("Hazır. Bir efekt seç ve Başlat'a bas."), false);
        }

        bool statusIsError;
        void SetStatus(string text, bool error)
        {
            if (lblStatus.Text == text && statusIsError == error) return;
            lblStatus.Text = text;
            lblStatus.ForeColor = error ? Color.Firebrick : SystemColors.ControlText;
            statusIsError = error;
        }

        void UpdatePreview()
        {
            int pc = engine.PreviewColor;
            if (pc == shownPreview) return;
            shownPreview = pc;
            if (pc < 0) return;
            preview.LitColor = ToColor(pc);
            lblHex.Text = (engine.IsRunning ? L.T("Şu anki renk: ") : L.T("Klavyenin kayıtlı rengi: ")) + Rgb.Hex(pc);
        }

        void OnUiTick(object sender, EventArgs e)
        {
            UpdatePreview();
            if (engine.IsRunning != shownRunning)
            {
                shownRunning = engine.IsRunning;
                lstEffects.Invalidate();
                shownPreview = -2;
            }
            RefreshStatus();
            UpdateCustomHighlight();
        }

        // ------------------------------------------------------------------ özel animasyon editörü

        void LoadGrid(List<Keyframe> frames, int selectIndex)
        {
            loadingGrid = true;
            grid.Rows.Clear();
            for (int i = 0; i < frames.Count; i++)
            {
                var f = frames[i];
                int r = grid.Rows.Add(i + 1, "", f.Ms, f.Fade);
                SetColorCell(grid.Rows[r].Cells[1], f.Color);
            }
            loadingGrid = false;
            highlightedRow = -1;
            custom.SetFrames(frames);
            if (grid.Rows.Count > 0)
            {
                selectIndex = Math.Max(0, Math.Min(grid.Rows.Count - 1, selectIndex));
                grid.ClearSelection();
                grid.Rows[selectIndex].Selected = true;
                grid.CurrentCell = grid.Rows[selectIndex].Cells[0];
            }
        }

        List<Keyframe> FramesFromGrid()
        {
            var list = new List<Keyframe>();
            foreach (DataGridViewRow row in grid.Rows)
            {
                int col = row.Cells[1].Tag is int ? (int)row.Cells[1].Tag : 0xFFFFFF;
                int ms;
                if (!int.TryParse(Convert.ToString(row.Cells[2].Value), out ms)) ms = 500;
                bool fade = row.Cells[3].Value is bool && (bool)row.Cells[3].Value;
                list.Add(new Keyframe(col, Math.Max(30, Math.Min(20000, ms)), fade));
            }
            return list;
        }

        void SyncFramesFromGrid() { custom.SetFrames(FramesFromGrid()); }

        int SelectedRow()
        {
            if (grid.CurrentRow != null) return grid.CurrentRow.Index;
            return grid.Rows.Count - 1;
        }

        void OnAddFrame(object sender, EventArgs e)
        {
            var frames = FramesFromGrid();
            int at = SelectedRow() + 1;
            int ms = frames.Count > 0 ? frames[Math.Max(0, at - 1)].Ms : 500;
            frames.Insert(at, new Keyframe(0xFFFFFF, ms, true));
            LoadGrid(frames, at);
        }

        void OnDuplicateFrame(object sender, EventArgs e)
        {
            var frames = FramesFromGrid();
            int at = SelectedRow();
            if (at < 0) return;
            frames.Insert(at + 1, frames[at]);
            LoadGrid(frames, at + 1);
        }

        void OnDeleteFrame(object sender, EventArgs e)
        {
            var frames = FramesFromGrid();
            int at = SelectedRow();
            if (at < 0 || frames.Count <= 1) return;
            frames.RemoveAt(at);
            LoadGrid(frames, at);
        }

        void OnMoveUp(object sender, EventArgs e) { MoveFrame(-1); }
        void OnMoveDown(object sender, EventArgs e) { MoveFrame(1); }

        void MoveFrame(int dir)
        {
            var frames = FramesFromGrid();
            int at = SelectedRow(), to = at + dir;
            if (at < 0 || to < 0 || to >= frames.Count) return;
            var tmp = frames[at]; frames[at] = frames[to]; frames[to] = tmp;
            LoadGrid(frames, to);
        }

        static string AnimFilter(bool withExtras)
        {
            string f = L.T("Klavye animasyonu") + " (*.klavyeanim)|*.klavyeanim";
            if (withExtras) f += "|" + L.T("Metin dosyası") + " (*.txt)|*.txt|" + L.T("Tüm dosyalar") + "|*.*";
            return f;
        }

        void OnOpenFile(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Filter = AnimFilter(true), Title = L.T("Animasyon aç") })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var frames = FrameIO.Parse(File.ReadAllLines(dlg.FileName, Encoding.UTF8));
                    if (frames.Count == 0) { MessageBox.Show(this, L.T("Dosyada geçerli adım bulunamadı."), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                    LoadGrid(frames, 0);
                }
                catch (Exception ex) { MessageBox.Show(this, L.T("Dosya açılamadı: ") + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        void OnSaveFile(object sender, EventArgs e)
        {
            grid.EndEdit();
            using (var dlg = new SaveFileDialog { Filter = AnimFilter(false), Title = L.T("Animasyonu kaydet"), FileName = L.T("animasyonum") + ".klavyeanim" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try { FrameIO.Save(dlg.FileName, FramesFromGrid()); }
                catch (Exception ex) { MessageBox.Show(this, L.T("Kaydedilemedi: ") + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        void UpdateCustomHighlight()
        {
            int idx = (engine.IsRunning && current == custom) ? custom.CurrentIndex : -1;
            if (idx == highlightedRow) return;
            ClearCustomHighlight();
            if (idx >= 0 && idx < grid.Rows.Count)
            {
                grid.Rows[idx].Cells[0].Style.BackColor = Color.FromArgb(255, 220, 90);
                grid.Rows[idx].Cells[0].Value = "▶ " + (idx + 1);
            }
            highlightedRow = idx;
        }

        void ClearCustomHighlight()
        {
            if (highlightedRow >= 0 && highlightedRow < grid.Rows.Count)
            {
                loadingGrid = true;
                grid.Rows[highlightedRow].Cells[0].Style.BackColor = Color.Empty;
                grid.Rows[highlightedRow].Cells[0].Value = highlightedRow + 1;
                loadingGrid = false;
            }
            highlightedRow = -1;
        }

        // ------------------------------------------------------------------ tepsi / pencere

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LayoutDynamic();   // DPI ölçeklemesinden sonra yeniden hizala
        }

        protected override void SetVisibleCore(bool value)
        {
            if (startHidden && !shownOnce)
            {
                shownOnce = true;
                if (!IsHandleCreated) CreateHandle();
                value = false;
            }
            base.SetVisibleCore(value);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized) HideToTray();
        }

        void HideToTray()
        {
            Hide();
            if (!trayHintShown)
            {
                trayHintShown = true;
                tray.ShowBalloonTip(3000, L.T("Klavye RGB"), L.T("Animasyon arka planda devam ediyor. Açmak için simgeye çift tıkla."), ToolTipIcon.Info);
            }
        }

        void ShowFromTray()
        {
            Show();
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            Activate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.Cancel) return;
            SaveSettings();
            uiTimer.Stop();
            engine.Shutdown();   // klavyeyi kendi ayarına döndürür
            tray.Visible = false;
            tray.Dispose();
        }

        static Icon LoadAppIcon()
        {
            try
            {
                using (var s = typeof(MainForm).Assembly.GetManifestResourceStream("KlavyeRGB.app.ico"))
                    if (s != null) return new Icon(s);
            }
            catch { }
            return SystemIcons.Application;
        }
    }

    /// <summary>Işığı yanan küçük bir klavye çizimi.</summary>
    public sealed class KeyboardPreview : Control
    {
        Color lit = Color.Black;

        public KeyboardPreview()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        public Color LitColor
        {
            get { return lit; }
            set { if (lit != value) { lit = value; Invalidate(); } }
        }

        public static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            var p = new GraphicsPath();
            float d = Math.Max(1f, radius * 2);
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        static Color Mix(Color a, Color b, float t)
        {
            return Color.FromArgb((int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);

            float intensity = Math.Max(lit.R, Math.Max(lit.G, lit.B)) / 255f;
            var body = new RectangleF(Width * 0.1f, Height * 0.12f, Width * 0.8f, Height * 0.76f);

            var glowRect = RectangleF.Inflate(body, body.Width * 0.12f, body.Height * 0.35f);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(glowRect);
                using (var glow = new PathGradientBrush(path))
                {
                    glow.CenterColor = Color.FromArgb((int)(150 * intensity), lit);
                    glow.SurroundColors = new[] { Color.FromArgb(0, lit) };
                    g.FillEllipse(glow, glowRect);
                }
            }

            using (var path = RoundedRect(body, Height * 0.06f))
            using (var b = new SolidBrush(Color.FromArgb(30, 32, 37)))
                g.FillPath(b, path);

            Color keyFace = Mix(Color.FromArgb(40, 42, 48), lit, 0.18f + 0.25f * intensity);
            Color keyEdge = Mix(Color.FromArgb(55, 58, 64), lit, 0.75f);
            float pad = body.Height * 0.08f;
            var area = new RectangleF(body.X + pad, body.Y + pad, body.Width - 2 * pad, body.Height - 2 * pad);
            const int cols = 15, rows = 5;
            float kw = area.Width / cols, kh = area.Height / rows, gap = Math.Max(1.5f, kw * 0.12f);
            using (var face = new SolidBrush(keyFace))
            using (var edge = new Pen(keyEdge, Math.Max(1f, kw * 0.06f)))
            {
                for (int r = 0; r < rows; r++)
                {
                    int c = 0;
                    while (c < cols)
                    {
                        int span = (r == rows - 1 && c == 4) ? 7 : 1;
                        var key = new RectangleF(area.X + c * kw + gap / 2, area.Y + r * kh + gap / 2, span * kw - gap, kh - gap);
                        using (var kp = RoundedRect(key, kh * 0.15f))
                        {
                            g.FillPath(face, kp);
                            g.DrawPath(edge, kp);
                        }
                        c += span;
                    }
                }
            }
        }
    }
}
