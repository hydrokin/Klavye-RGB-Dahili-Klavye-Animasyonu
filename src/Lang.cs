using System;
using System.Collections.Generic;

namespace KlavyeRGB
{
    /// <summary>
    /// Arayüz dilleri. Anahtar Türkçe metnin kendisidir; diğer diller tablodan gelir.
    /// Varsayılan dil İngilizcedir.
    /// </summary>
    public static class L
    {
        public static readonly string[] Codes = { "en", "tr", "de", "es", "fr", "pt", "ru", "zh" };
        public static readonly string[] NativeNames = { "English", "Türkçe", "Deutsch", "Español", "Français", "Português", "Русский", "中文 (简体)" };

        static volatile int lang;   // Codes içindeki sıra; 0 = English
        static readonly Dictionary<string, string[]> table = new Dictionary<string, string[]>();
        /// <summary>Çevirisi bulunamayan anahtarlar (test için).</summary>
        public static readonly HashSet<string> Missing = new HashSet<string>();

        public static int Index { get { return lang; } }
        public static string Code { get { return Codes[lang]; } }
        public static IEnumerable<string> Keys { get { return table.Keys; } }

        public static void Set(string code)
        {
            int i = Array.IndexOf(Codes, code);
            lang = i >= 0 ? i : 0;
        }

        public static string T(string tr)
        {
            if (tr == null) return null;
            int l = lang;
            if (l == 1) return tr;
            string[] v;
            if (table.TryGetValue(tr, out v)) return v[l == 0 ? 0 : l - 1];
            lock (Missing) Missing.Add(tr);
            return tr;
        }

        //        Türkçe (anahtar)   English   Deutsch   Español   Français   Português   Русский   中文
        static void A(string tr, string en, string de, string es, string fr, string pt, string ru, string zh)
        {
            table[tr] = new[] { en, de, es, fr, pt, ru, zh };
        }

        static void Same(string s) { A(s, s, s, s, s, s, s, s); }

        static L()
        {
            // ---------------------------------------------------------------- uygulama
            A("Klavye RGB", "Keyboard RGB", "Tastatur RGB", "Teclado RGB", "Clavier RGB", "Teclado RGB", "Клавиатура RGB", "键盘 RGB");
            A("Dahili Klavye Animasyonu", "Built-in Keyboard Animations", "Animationen für die integrierte Tastatur", "Animaciones del teclado integrado", "Animations du clavier intégré", "Animações do teclado integrado", "Анимации встроенной клавиатуры", "内置键盘灯效");
            A("dahili klavye (Clevo, tek bölge RGB)", "built-in keyboard (Clevo, single-zone RGB)", "integrierte Tastatur (Clevo, Einzonen-RGB)", "teclado integrado (Clevo, RGB de una zona)", "clavier intégré (Clevo, RVB une zone)", "teclado integrado (Clevo, RGB de zona única)", "встроенная клавиатура (Clevo, однозонная RGB)", "内置键盘（Clevo，单区 RGB）");
            A("Windows açılınca başlat", "Start with Windows", "Mit Windows starten", "Iniciar con Windows", "Lancer avec Windows", "Iniciar com o Windows", "Запуск с Windows", "开机自启动");
            A("Efektler", "Effects", "Effekte", "Efectos", "Effets", "Efeitos", "Эффекты", "效果");
            A("Efekt ara…", "Search effects…", "Effekte suchen…", "Buscar efectos…", "Rechercher…", "Buscar efeitos…", "Поиск эффектов…", "搜索效果…");
            A("Hız:", "Speed:", "Tempo:", "Velocidad:", "Vitesse :", "Velocidade:", "Скорость:", "速度：");
            A("Hassasiyet:", "Sensitivity:", "Empfindlichkeit:", "Sensibilidad:", "Sensibilité :", "Sensibilidade:", "Чувствительность:", "灵敏度：");
            A("Tepki hızı:", "Response:", "Reaktion:", "Respuesta:", "Réactivité :", "Resposta:", "Отклик:", "响应速度：");
            A("Parlaklık:", "Brightness:", "Helligkeit:", "Brillo:", "Luminosité :", "Brilho:", "Яркость:", "亮度：");
            A("Renk 1", "Color 1", "Farbe 1", "Color 1", "Couleur 1", "Cor 1", "Цвет 1", "颜色 1");
            A("Renk 2", "Color 2", "Farbe 2", "Color 2", "Couleur 2", "Cor 2", "Цвет 2", "颜色 2");
            A("Renk dengesini düzelt", "Correct color balance", "Farbbalance korrigieren", "Corregir balance de color", "Corriger l'équilibre des couleurs", "Corrigir balanço de cor", "Коррекция баланса цвета", "校正色彩平衡");
            A("• Program sadece canlı renk gönderir, klavyenin kayıtlı ayarlarını değiştirmez.\n• Durdurunca ya da kapatınca klavye kendi rengine/efektine geri döner.\n• Fn tuşlarıyla parlaklığı ayarlamaya devam edebilirsin.\n• Tepsiye küçültürsen animasyon arka planda sürer.",
              "• The app only sends live colors; it never changes the keyboard's saved settings.\n• When you stop or close it, the keyboard returns to its own color/effect.\n• You can still adjust brightness with the Fn keys.\n• Minimize to tray and the animation keeps running in the background.",
              "• Das Programm sendet nur Live-Farben und ändert keine gespeicherten Tastatureinstellungen.\n• Beim Stoppen oder Schließen kehrt die Tastatur zu ihrer eigenen Farbe bzw. ihrem Effekt zurück.\n• Die Helligkeit lässt sich weiterhin mit den Fn-Tasten einstellen.\n• Im Infobereich läuft die Animation im Hintergrund weiter.",
              "• El programa solo envía colores en vivo; no cambia la configuración guardada del teclado.\n• Al detenerlo o cerrarlo, el teclado vuelve a su propio color o efecto.\n• Puedes seguir ajustando el brillo con las teclas Fn.\n• Minimizado en la bandeja, la animación sigue en segundo plano.",
              "• Le programme envoie uniquement des couleurs en direct ; il ne modifie pas les réglages enregistrés du clavier.\n• À l'arrêt ou à la fermeture, le clavier retrouve sa couleur ou son effet d'origine.\n• Vous pouvez toujours régler la luminosité avec les touches Fn.\n• Réduit dans la zone de notification, l'animation continue en arrière-plan.",
              "• O programa só envia cores ao vivo; não altera as configurações salvas do teclado.\n• Ao parar ou fechar, o teclado volta à sua própria cor ou efeito.\n• Você ainda pode ajustar o brilho com as teclas Fn.\n• Minimizado na bandeja, a animação continua em segundo plano.",
              "• Программа только передаёт цвета в реальном времени и не меняет сохранённые настройки клавиатуры.\n• После остановки или закрытия клавиатура возвращается к своему цвету или эффекту.\n• Яркость по-прежнему можно менять клавишами Fn.\n• В трее анимация продолжает работать в фоне.",
              "• 程序只发送实时颜色，不会更改键盘已保存的设置。\n• 停止或关闭后，键盘会恢复原来的颜色或效果。\n• 仍可用 Fn 键调节亮度。\n• 最小化到托盘后，动画会在后台继续运行。");
            A("Mors koduyla yazılacak metin:", "Text to send in Morse code:", "Text für den Morsecode:", "Texto para código Morse:", "Texte à transmettre en morse :", "Texto para código Morse:", "Текст для азбуки Морзе:", "要用摩尔斯电码显示的文字：");
            A("Harfler, rakamlar ve Türkçe karakterler desteklenir. Boşluk kelime arası bekleme yapar. Renk 1 ile yanar.",
              "Letters, digits and Turkish characters are supported. A space adds a pause between words. Lights up in Color 1.",
              "Buchstaben, Ziffern und türkische Zeichen werden unterstützt. Ein Leerzeichen erzeugt eine Wortpause. Leuchtet in Farbe 1.",
              "Admite letras, números y caracteres turcos. Un espacio añade una pausa entre palabras. Se ilumina con el Color 1.",
              "Lettres, chiffres et caractères turcs acceptés. Une espace ajoute une pause entre les mots. S'allume en Couleur 1.",
              "Aceita letras, números e caracteres turcos. Um espaço adiciona uma pausa entre palavras. Acende na Cor 1.",
              "Поддерживаются буквы, цифры и турецкие символы. Пробел — пауза между словами. Светится цветом 1.",
              "支持字母、数字和土耳其语字符。空格表示单词间停顿。以颜色 1 显示。");
            A("+ Adım ekle", "+ Add step", "+ Schritt", "+ Añadir paso", "+ Ajouter", "+ Adicionar", "+ Добавить", "+ 添加步骤");
            A("Çoğalt", "Duplicate", "Duplizieren", "Duplicar", "Dupliquer", "Duplicar", "Дублировать", "复制");
            A("Sil", "Delete", "Löschen", "Eliminar", "Supprimer", "Excluir", "Удалить", "删除");
            A("▲ Yukarı", "▲ Up", "▲ Hoch", "▲ Subir", "▲ Monter", "▲ Subir", "▲ Вверх", "▲ 上移");
            A("▼ Aşağı", "▼ Down", "▼ Runter", "▼ Bajar", "▼ Descendre", "▼ Descer", "▼ Вниз", "▼ 下移");
            A("Aç…", "Open…", "Öffnen…", "Abrir…", "Ouvrir…", "Abrir…", "Открыть…", "打开…");
            A("Kaydet…", "Save…", "Speichern…", "Guardar…", "Enregistrer…", "Salvar…", "Сохранить…", "保存…");
            A("Başlat", "Start", "Starten", "Iniciar", "Démarrer", "Iniciar", "Старт", "开始");
            A("Durdur", "Stop", "Stoppen", "Detener", "Arrêter", "Parar", "Стоп", "停止");
            A("Hazır. Bir efekt seç ve Başlat'a bas.", "Ready. Pick an effect and press Start.", "Bereit. Wähle einen Effekt und drücke Starten.", "Listo. Elige un efecto y pulsa Iniciar.", "Prêt. Choisissez un effet et cliquez sur Démarrer.", "Pronto. Escolha um efeito e clique em Iniciar.", "Готово. Выберите эффект и нажмите «Старт».", "就绪。选择一个效果，然后点击“开始”。");
            A("Tepsiye küçült", "Minimize to tray", "In den Infobereich", "Minimizar a bandeja", "Réduire en icône", "Minimizar p/ bandeja", "Свернуть в трей", "最小化到托盘");
            A("Göster", "Show", "Anzeigen", "Mostrar", "Afficher", "Mostrar", "Показать", "显示");
            A("Çıkış", "Exit", "Beenden", "Salir", "Quitter", "Sair", "Выход", "退出");
            A("durdu", "stopped", "gestoppt", "detenido", "arrêté", "parado", "остановлено", "已停止");
            A("Adım", "Step", "Schritt", "Paso", "Étape", "Passo", "Шаг", "步骤");
            A("Renk (tıkla)", "Color (click)", "Farbe (klicken)", "Color (clic)", "Couleur (clic)", "Cor (clique)", "Цвет (нажмите)", "颜色（点击）");
            A("Süre (ms)", "Duration (ms)", "Dauer (ms)", "Duración (ms)", "Durée (ms)", "Duração (ms)", "Длит. (мс)", "时长 (ms)");
            A("Yumuşak geçiş", "Smooth fade", "Weicher Übergang", "Transición suave", "Fondu", "Transição suave", "Плавный переход", "平滑过渡");
            A("Süre 30 ile 20000 ms arasında olmalı", "Duration must be between 30 and 20000 ms", "Dauer muss zwischen 30 und 20000 ms liegen", "La duración debe estar entre 30 y 20000 ms", "La durée doit être comprise entre 30 et 20000 ms", "A duração deve estar entre 30 e 20000 ms", "Длительность должна быть от 30 до 20000 мс", "时长必须在 30 到 20000 毫秒之间");
            A("Başlangıç ayarı yazılamadı: ", "Could not save the startup setting: ", "Autostart-Einstellung konnte nicht gespeichert werden: ", "No se pudo guardar el inicio automático: ", "Impossible d'enregistrer le démarrage automatique : ", "Não foi possível salvar a inicialização automática: ", "Не удалось сохранить автозапуск: ", "无法保存开机启动设置：");
            A("Çalışıyor: ", "Running: ", "Läuft: ", "En marcha: ", "En cours : ", "Em execução: ", "Работает: ", "运行中：");
            A("Durduruldu. Klavye kendi rengine döndü.", "Stopped. The keyboard is back to its own color.", "Gestoppt. Die Tastatur hat wieder ihre eigene Farbe.", "Detenido. El teclado volvió a su propio color.", "Arrêté. Le clavier a retrouvé sa couleur.", "Parado. O teclado voltou à própria cor.", "Остановлено. Клавиатура вернулась к своему цвету.", "已停止。键盘已恢复原来的颜色。");
            A("Karışık", "Mix", "Mix", "Mezcla", "Mélange", "Mistura", "Микс", "混合");
            A("Şu anki renk: ", "Current color: ", "Aktuelle Farbe: ", "Color actual: ", "Couleur actuelle : ", "Cor atual: ", "Текущий цвет: ", "当前颜色：");
            A("Klavyenin kayıtlı rengi: ", "Keyboard's saved color: ", "Gespeicherte Tastaturfarbe: ", "Color guardado del teclado: ", "Couleur enregistrée du clavier : ", "Cor salva do teclado: ", "Сохранённый цвет клавиатуры: ", "键盘已保存的颜色：");
            A("Klavye animasyonu", "Keyboard animation", "Tastaturanimation", "Animación de teclado", "Animation clavier", "Animação de teclado", "Анимация клавиатуры", "键盘动画");
            A("Metin dosyası", "Text file", "Textdatei", "Archivo de texto", "Fichier texte", "Arquivo de texto", "Текстовый файл", "文本文件");
            A("Tüm dosyalar", "All files", "Alle Dateien", "Todos los archivos", "Tous les fichiers", "Todos os arquivos", "Все файлы", "所有文件");
            A("Animasyon aç", "Open animation", "Animation öffnen", "Abrir animación", "Ouvrir une animation", "Abrir animação", "Открыть анимацию", "打开动画");
            A("Animasyonu kaydet", "Save animation", "Animation speichern", "Guardar animación", "Enregistrer l'animation", "Salvar animação", "Сохранить анимацию", "保存动画");
            A("animasyonum", "my-animation", "meine-animation", "mi-animacion", "mon-animation", "minha-animacao", "моя-анимация", "我的动画");
            A("Dosyada geçerli adım bulunamadı.", "No valid steps found in the file.", "Keine gültigen Schritte in der Datei gefunden.", "No se encontraron pasos válidos en el archivo.", "Aucune étape valide dans le fichier.", "Nenhum passo válido encontrado no arquivo.", "В файле нет допустимых шагов.", "文件中没有有效的步骤。");
            A("Dosya açılamadı: ", "Could not open the file: ", "Datei konnte nicht geöffnet werden: ", "No se pudo abrir el archivo: ", "Impossible d'ouvrir le fichier : ", "Não foi possível abrir o arquivo: ", "Не удалось открыть файл: ", "无法打开文件：");
            A("Kaydedilemedi: ", "Could not save: ", "Speichern fehlgeschlagen: ", "No se pudo guardar: ", "Échec de l'enregistrement : ", "Não foi possível salvar: ", "Не удалось сохранить: ", "无法保存：");
            A("Animasyon arka planda devam ediyor. Açmak için simgeye çift tıkla.", "The animation keeps running in the background. Double-click the icon to open.", "Die Animation läuft im Hintergrund weiter. Zum Öffnen auf das Symbol doppelklicken.", "La animación sigue en segundo plano. Haz doble clic en el icono para abrir.", "L'animation continue en arrière-plan. Double-cliquez sur l'icône pour ouvrir.", "A animação continua em segundo plano. Clique duas vezes no ícone para abrir.", "Анимация продолжается в фоне. Дважды щёлкните значок, чтобы открыть.", "动画在后台继续运行。双击图标即可打开。");
            A("Klavye RGB zaten çalışıyor. Sağ alttaki sistem tepsisindeki simgeye bak.", "Keyboard RGB is already running. Look for its icon in the system tray (bottom right).", "Tastatur RGB läuft bereits. Das Symbol befindet sich im Infobereich unten rechts.", "Teclado RGB ya se está ejecutando. Busca su icono en la bandeja del sistema (abajo a la derecha).", "Clavier RGB est déjà lancé. Son icône se trouve dans la zone de notification (en bas à droite).", "O Teclado RGB já está em execução. Procure o ícone na bandeja do sistema (canto inferior direito).", "Клавиатура RGB уже запущена. Её значок находится в трее (справа внизу).", "键盘 RGB 已在运行。请在右下角的系统托盘中查找其图标。");
            A("Clevo klavye sürücüsü (InsydeDCHU.dll) bulunamadı. \"Fn hotkeys and OSD\" sürücüsü kurulu olmalı.", "Clevo keyboard driver (InsydeDCHU.dll) not found. The \"Fn hotkeys and OSD\" driver must be installed.", "Clevo-Tastaturtreiber (InsydeDCHU.dll) nicht gefunden. Der Treiber \"Fn hotkeys and OSD\" muss installiert sein.", "No se encontró el controlador de teclado Clevo (InsydeDCHU.dll). Debe estar instalado el controlador \"Fn hotkeys and OSD\".", "Pilote clavier Clevo (InsydeDCHU.dll) introuvable. Le pilote « Fn hotkeys and OSD » doit être installé.", "Driver de teclado Clevo (InsydeDCHU.dll) não encontrado. O driver \"Fn hotkeys and OSD\" precisa estar instalado.", "Драйвер клавиатуры Clevo (InsydeDCHU.dll) не найден. Должен быть установлен драйвер «Fn hotkeys and OSD».", "未找到 Clevo 键盘驱动（InsydeDCHU.dll）。需要安装 “Fn hotkeys and OSD” 驱动。");
            A("InsydeDCHU.dll yüklenemedi (hata {0}).", "Could not load InsydeDCHU.dll (error {0}).", "InsydeDCHU.dll konnte nicht geladen werden (Fehler {0}).", "No se pudo cargar InsydeDCHU.dll (error {0}).", "Impossible de charger InsydeDCHU.dll (erreur {0}).", "Não foi possível carregar InsydeDCHU.dll (erro {0}).", "Не удалось загрузить InsydeDCHU.dll (ошибка {0}).", "无法加载 InsydeDCHU.dll（错误 {0}）。");
            A("Klavyeye komut gönderilemedi: {0}", "Could not send command to the keyboard: {0}", "Befehl konnte nicht an die Tastatur gesendet werden: {0}", "No se pudo enviar el comando al teclado: {0}", "Impossible d'envoyer la commande au clavier : {0}", "Não foi possível enviar o comando ao teclado: {0}", "Не удалось отправить команду клавиатуре: {0}", "无法向键盘发送命令：{0}");

            // ---------------------------------------------------------------- kategoriler
            A("TEMEL", "BASICS", "GRUNDLAGEN", "BÁSICOS", "BASES", "BÁSICOS", "ОСНОВНЫЕ", "基础");
            A("GÖKKUŞAĞI", "RAINBOW", "REGENBOGEN", "ARCOÍRIS", "ARC-EN-CIEL", "ARCO-ÍRIS", "РАДУГА", "彩虹");
            A("DOĞA", "NATURE", "NATUR", "NATURALEZA", "NATURE", "NATUREZA", "ПРИРОДА", "自然");
            A("ÇAKAR VE RİTİM", "FLASHERS & RHYTHM", "BLINKLICHT & RHYTHMUS", "DESTELLOS Y RITMO", "GYROPHARES ET RYTHME", "SIRENES E RITMO", "МИГАЛКИ И РИТМ", "闪烁与节奏");
            A("MÜZİK", "MUSIC", "MUSIK", "MÚSICA", "MUSIQUE", "MÚSICA", "МУЗЫКА", "音乐");
            A("ETKİLEŞİMLİ", "INTERACTIVE", "INTERAKTIV", "INTERACTIVOS", "INTERACTIFS", "INTERATIVOS", "ИНТЕРАКТИВНЫЕ", "互动");
            A("SİSTEM VE ZAMAN", "SYSTEM & TIME", "SYSTEM & ZEIT", "SISTEMA Y HORA", "SYSTÈME ET HEURE", "SISTEMA E HORA", "СИСТЕМА И ВРЕМЯ", "系统与时间");
            A("TAKIM VE BAYRAK RENKLERİ", "TEAM & FLAG COLORS", "TEAM- & FLAGGENFARBEN", "COLORES DE EQUIPOS Y BANDERAS", "COULEURS D'ÉQUIPES ET DRAPEAUX", "CORES DE TIMES E BANDEIRAS", "ЦВЕТА КОМАНД И ФЛАГОВ", "球队与旗帜配色");
            A("KARIŞIK VE ÖZEL", "MIX & CUSTOM", "MIX & EIGENE", "MEZCLA Y PERSONALIZADO", "MÉLANGE ET PERSO", "MISTURA E PERSONALIZADO", "МИКС И СВОИ", "混合与自定义");

            // ---------------------------------------------------------------- efekt adları
            A("Sabit Renk", "Static Color", "Statische Farbe", "Color fijo", "Couleur fixe", "Cor fixa", "Постоянный цвет", "固定颜色");
            A("Gökkuşağı", "Rainbow", "Regenbogen", "Arcoíris", "Arc-en-ciel", "Arco-íris", "Радуга", "彩虹");
            A("Gökkuşağı Nefes", "Rainbow Breathing", "Regenbogen-Atmung", "Respiración arcoíris", "Respiration arc-en-ciel", "Respiração arco-íris", "Радужное дыхание", "彩虹呼吸");
            A("Nefes", "Breathing", "Atmung", "Respiración", "Respiration", "Respiração", "Дыхание", "呼吸");
            A("İki Renk Geçişi", "Two-Color Fade", "Zweifarbiger Übergang", "Transición de dos colores", "Fondu deux couleurs", "Transição de duas cores", "Переход двух цветов", "双色渐变");
            A("Flaş", "Strobe", "Stroboskop", "Estroboscopio", "Stroboscope", "Estroboscópio", "Стробоскоп", "频闪");
            A("Polis Çakarı", "Police Lights", "Polizeilicht", "Luces de policía", "Gyrophare police", "Sirene policial", "Полицейская мигалка", "警灯");
            A("Kalp Atışı", "Heartbeat", "Herzschlag", "Latido", "Battement de cœur", "Batimento cardíaco", "Сердцебиение", "心跳");
            A("Ateş", "Fire", "Feuer", "Fuego", "Feu", "Fogo", "Огонь", "火焰");
            A("Okyanus", "Ocean", "Ozean", "Océano", "Océan", "Oceano", "Океан", "海洋");
            A("Rastgele Renkler", "Random Colors", "Zufallsfarben", "Colores aleatorios", "Couleurs aléatoires", "Cores aleatórias", "Случайные цвета", "随机颜色");
            A("Müzik: Ses Seviyesi", "Music: Volume Level", "Musik: Lautstärke", "Música: Volumen", "Musique : Volume", "Música: Volume", "Музыка: громкость", "音乐：音量");
            A("Müzik: Vuruş", "Music: Beat", "Musik: Beat", "Música: Ritmo", "Musique : Beat", "Música: Batida", "Музыка: бит", "音乐：节拍");
            A("Ekran Rengi", "Screen Color", "Bildschirmfarbe", "Color de pantalla", "Couleur de l'écran", "Cor da tela", "Цвет экрана", "屏幕颜色");
            A("İşlemci Yükü", "CPU Load", "CPU-Last", "Carga de CPU", "Charge CPU", "Uso da CPU", "Загрузка ЦП", "CPU 负载");
            A("Pil Durumu", "Battery Status", "Akkustand", "Estado de batería", "État de la batterie", "Estado da bateria", "Заряд батареи", "电池状态");
            A("Özel Animasyon (kendin yap)", "Custom Animation (make your own)", "Eigene Animation", "Animación personalizada", "Animation personnalisée", "Animação personalizada", "Своя анимация", "自定义动画");
            A("İki Renk Nefes", "Two-Color Breathing", "Zweifarbige Atmung", "Respiración de dos colores", "Respiration deux couleurs", "Respiração de duas cores", "Дыхание двух цветов", "双色呼吸");
            A("İki Renk Yanıp Sönme", "Two-Color Blink", "Zweifarbiges Blinken", "Parpadeo de dos colores", "Clignotement deux couleurs", "Pisca duas cores", "Мигание двух цветов", "双色闪烁");
            A("Yıldız Işıltısı", "Twinkle", "Funkeln", "Destellos", "Scintillement", "Cintilar", "Мерцание звёзд", "星光闪烁");
            A("Nabız", "Pulse", "Puls", "Pulso", "Pulsation", "Pulso", "Пульс", "脉冲");
            A("Mors Kodu", "Morse Code", "Morsecode", "Código Morse", "Code Morse", "Código Morse", "Азбука Морзе", "摩尔斯电码");
            A("Gökkuşağı Flaş", "Rainbow Strobe", "Regenbogen-Blitz", "Destello arcoíris", "Flash arc-en-ciel", "Flash arco-íris", "Радужные вспышки", "彩虹频闪");
            A("Plazma", "Plasma", "Plasma", "Plasma", "Plasma", "Plasma", "Плазма", "等离子");
            A("Rastgele Flaşlar", "Random Flashes", "Zufallsblitze", "Destellos aleatorios", "Flashs aléatoires", "Flashes aleatórios", "Случайные вспышки", "随机闪光");
            A("Mum Işığı", "Candlelight", "Kerzenlicht", "Luz de vela", "Bougie", "Luz de vela", "Свеча", "烛光");
            A("Fırtına", "Storm", "Gewitter", "Tormenta", "Orage", "Tempestade", "Гроза", "雷暴");
            A("Alarm", "Alarm", "Alarm", "Alarma", "Alarme", "Alarme", "Тревога", "警报");
            A("Disko", "Disco", "Disco", "Disco", "Disco", "Discoteca", "Диско", "迪斯科");
            A("Matrix", "Matrix", "Matrix", "Matrix", "Matrix", "Matrix", "Матрица", "黑客帝国");
            A("Müzik: Renk Akışı", "Music: Color Flow", "Musik: Farbfluss", "Música: Flujo de color", "Musique : Flux de couleurs", "Música: Fluxo de cor", "Музыка: поток цвета", "音乐：色彩流动");
            A("Müzik: Tek Renk", "Music: Single Color", "Musik: Eine Farbe", "Música: Un color", "Musique : Une couleur", "Música: Uma cor", "Музыка: один цвет", "音乐：单色");
            A("Yazarken Parla", "Typing Glow", "Tipp-Leuchten", "Brillo al escribir", "Lueur à la frappe", "Brilho ao digitar", "Подсветка при наборе", "打字发光");
            A("Yazma Hızı", "Typing Speed", "Tippgeschwindigkeit", "Velocidad de escritura", "Vitesse de frappe", "Velocidade de digitação", "Скорость набора", "打字速度");
            A("Fare ile Renk", "Mouse Color", "Mausfarbe", "Color con el ratón", "Couleur à la souris", "Cor com o mouse", "Цвет мышью", "鼠标调色");
            A("RAM Kullanımı", "RAM Usage", "RAM-Auslastung", "Uso de RAM", "Utilisation de la RAM", "Uso de RAM", "Использование ОЗУ", "内存占用");
            A("İnternet Trafiği", "Network Traffic", "Netzwerkverkehr", "Tráfico de red", "Trafic réseau", "Tráfego de rede", "Сетевой трафик", "网络流量");
            A("Günün Saati", "Time of Day", "Tageszeit", "Hora del día", "Moment de la journée", "Hora do dia", "Время суток", "一天时段");
            A("Pomodoro Zamanlayıcı", "Pomodoro Timer", "Pomodoro-Timer", "Temporizador Pomodoro", "Minuteur Pomodoro", "Timer Pomodoro", "Таймер Pomodoro", "番茄钟");
            A("Karışık (otomatik geçiş)", "Mix (auto switch)", "Mix (automatischer Wechsel)", "Mezcla (cambio automático)", "Mélange (changement auto)", "Mistura (troca automática)", "Микс (автосмена)", "混合（自动切换）");
            A("Pastel Gökkuşağı", "Pastel Rainbow", "Pastell-Regenbogen", "Arcoíris pastel", "Arc-en-ciel pastel", "Arco-íris pastel", "Пастельная радуга", "柔和彩虹");
            A("Renk Döngüsü", "Color Cycle", "Farbzyklus", "Ciclo de colores", "Cycle de couleurs", "Ciclo de cores", "Цветовой цикл", "颜色循环");
            A("Kuzey Işıkları", "Northern Lights", "Nordlicht", "Aurora boreal", "Aurore boréale", "Aurora boreal", "Северное сияние", "北极光");
            A("Gün Batımı", "Sunset", "Sonnenuntergang", "Atardecer", "Coucher de soleil", "Pôr do sol", "Закат", "日落");
            A("Orman", "Forest", "Wald", "Bosque", "Forêt", "Floresta", "Лес", "森林");
            A("Lav Lambası", "Lava Lamp", "Lavalampe", "Lámpara de lava", "Lampe à lave", "Lâmpada de lava", "Лава-лампа", "熔岩灯");
            A("Buz", "Ice", "Eis", "Hielo", "Glace", "Gelo", "Лёд", "冰");
            A("Galaksi", "Galaxy", "Galaxie", "Galaxia", "Galaxie", "Galáxia", "Галактика", "星系");
            A("Ambulans", "Ambulance", "Rettungswagen", "Ambulancia", "Ambulance", "Ambulância", "Скорая помощь", "救护车");
            A("İtfaiye", "Fire Truck", "Feuerwehr", "Bomberos", "Pompiers", "Bombeiros", "Пожарная", "消防车");
            A("Neon Şehir", "Neon City", "Neonstadt", "Ciudad neón", "Ville néon", "Cidade neon", "Неоновый город", "霓虹城市");
            A("Toksik", "Toxic", "Toxisch", "Tóxico", "Toxique", "Tóxico", "Токсичный", "毒绿");
            A("Trafik Lambası", "Traffic Light", "Ampel", "Semáforo", "Feu tricolore", "Semáforo", "Светофор", "红绿灯");
            A("Türk Bayrağı", "Turkish Flag", "Türkische Flagge", "Bandera turca", "Drapeau turc", "Bandeira turca", "Флаг Турции", "土耳其国旗");
            Same("Galatasaray");
            Same("Fenerbahçe");
            Same("Beşiktaş");
            Same("Trabzonspor");

            // ---------------------------------------------------------------- efekt açıklamaları
            A("Klavye seçtiğin renkte sabit yanar. Program açık kaldıkça bu renk kalır.",
              "The keyboard stays lit in your chosen color while the app is running.",
              "Die Tastatur leuchtet in der gewählten Farbe, solange das Programm läuft.",
              "El teclado se queda en el color elegido mientras el programa esté abierto.",
              "Le clavier reste allumé dans la couleur choisie tant que le programme tourne.",
              "O teclado fica na cor escolhida enquanto o programa estiver aberto.",
              "Клавиатура светится выбранным цветом, пока работает программа.",
              "程序运行期间，键盘保持所选颜色。");
            A("Klavye gökkuşağının bütün renklerinden yumuşakça geçer.",
              "Smoothly cycles through every color of the rainbow.",
              "Wechselt sanft durch alle Farben des Regenbogens.",
              "Recorre suavemente todos los colores del arcoíris.",
              "Passe en douceur par toutes les couleurs de l'arc-en-ciel.",
              "Passa suavemente por todas as cores do arco-íris.",
              "Плавно переливается всеми цветами радуги.",
              "平滑循环彩虹的所有颜色。");
            A("Her nefeste farklı bir renkte yavaşça yanıp söner.",
              "Slowly fades in and out, with a new color on every breath.",
              "Pulsiert langsam, bei jedem Atemzug in einer neuen Farbe.",
              "Se enciende y apaga lentamente con un color nuevo en cada respiración.",
              "S'allume et s'éteint lentement, avec une nouvelle couleur à chaque respiration.",
              "Acende e apaga devagar, com uma cor nova a cada respiração.",
              "Медленно загорается и гаснет, каждый раз новым цветом.",
              "缓慢明暗变化，每次呼吸换一种颜色。");
            A("Seçtiğin renk nefes alır gibi yavaşça yanıp söner.",
              "Your chosen color slowly fades in and out, like breathing.",
              "Die gewählte Farbe pulsiert langsam wie ein Atemzug.",
              "El color elegido se enciende y apaga lentamente, como una respiración.",
              "La couleur choisie s'allume et s'éteint lentement, comme une respiration.",
              "A cor escolhida acende e apaga devagar, como uma respiração.",
              "Выбранный цвет медленно загорается и гаснет, как дыхание.",
              "所选颜色像呼吸一样缓慢明暗变化。");
            A("Seçtiğin iki renk arasında yumuşakça gidip gelir.",
              "Smoothly shifts back and forth between your two colors.",
              "Wechselt sanft zwischen deinen zwei Farben hin und her.",
              "Pasa suavemente de un color a otro y vuelve.",
              "Passe en douceur d'une couleur à l'autre, en va-et-vient.",
              "Alterna suavemente entre as duas cores escolhidas.",
              "Плавно переходит между двумя выбранными цветами.",
              "在两种颜色之间平滑来回过渡。");
            A("Seçtiğin renkte çift flaş çakar.",
              "Double flash in your chosen color.",
              "Doppelblitz in der gewählten Farbe.",
              "Doble destello en el color elegido.",
              "Double flash dans la couleur choisie.",
              "Flash duplo na cor escolhida.",
              "Двойная вспышка выбранным цветом.",
              "以所选颜色双闪。");
            A("Kırmızı ve mavi sırayla üçer kez çakar.",
              "Red and blue flash three times each, in turn.",
              "Rot und Blau blitzen abwechselnd je dreimal.",
              "Rojo y azul destellan tres veces cada uno, por turnos.",
              "Rouge et bleu clignotent trois fois chacun, à tour de rôle.",
              "Vermelho e azul piscam três vezes cada, alternando.",
              "Красный и синий поочерёдно мигают по три раза.",
              "红色和蓝色轮流各闪三次。");
            A("Seçtiğin renkte çift vuruşlu kalp ritmi.",
              "Double-beat heart rhythm in your chosen color.",
              "Doppelschlag-Herzrhythmus in der gewählten Farbe.",
              "Ritmo de latido doble en el color elegido.",
              "Rythme cardiaque à double battement dans la couleur choisie.",
              "Ritmo de batimento duplo na cor escolhida.",
              "Двойной ритм сердца выбранным цветом.",
              "以所选颜色呈现双拍心跳节奏。");
            A("Kızıl ve turuncu tonlarda titreyen alev ışığı.",
              "Flickering flame light in deep red and orange.",
              "Flackerndes Flammenlicht in Rot- und Orangetönen.",
              "Luz de llama parpadeante en rojos y naranjas.",
              "Lueur de flamme vacillante, rouge et orange.",
              "Luz de chama tremeluzente em vermelho e laranja.",
              "Мерцающее пламя в красных и оранжевых тонах.",
              "红橙色调的摇曳火光。");
            A("Mavi ve turkuaz tonlarda dalgalanan sakin ışık.",
              "Calm, rippling light in blue and turquoise.",
              "Ruhiges, wogendes Licht in Blau und Türkis.",
              "Luz tranquila y ondulante en azules y turquesas.",
              "Lumière calme et ondulante, bleue et turquoise.",
              "Luz calma e ondulante em azul e turquesa.",
              "Спокойный волнующийся свет в синих и бирюзовых тонах.",
              "蓝色与青绿色的平静波光。");
            A("Rastgele renkler arasında yumuşak geçişler yapar.",
              "Smooth transitions between random colors.",
              "Sanfte Übergänge zwischen zufälligen Farben.",
              "Transiciones suaves entre colores aleatorios.",
              "Transitions douces entre des couleurs aléatoires.",
              "Transições suaves entre cores aleatórias.",
              "Плавные переходы между случайными цветами.",
              "在随机颜色之间平滑过渡。");
            A("Bilgisayardan çıkan sese göre renk yeşilden kırmızıya döner ve parlaklaşır. Kaydırıcı hassasiyeti ayarlar.",
              "Color goes from green to red and brightens with your PC's sound. The slider sets sensitivity.",
              "Die Farbe wechselt mit dem PC-Ton von Grün zu Rot und wird heller. Der Regler stellt die Empfindlichkeit ein.",
              "El color pasa de verde a rojo y se aviva con el sonido del PC. El control ajusta la sensibilidad.",
              "La couleur passe du vert au rouge et s'intensifie avec le son du PC. Le curseur règle la sensibilité.",
              "A cor vai do verde ao vermelho e fica mais forte com o som do PC. O controle ajusta a sensibilidade.",
              "Цвет меняется от зелёного к красному и становится ярче вместе со звуком ПК. Ползунок задаёт чувствительность.",
              "颜色随电脑声音从绿变红并变亮。滑块调节灵敏度。");
            A("Müzikteki her vuruşta klavye yeni bir renkte parlar ve söner. Kaydırıcı hassasiyeti ayarlar.",
              "The keyboard flashes a new color on every beat. The slider sets sensitivity.",
              "Bei jedem Beat blitzt die Tastatur in einer neuen Farbe. Der Regler stellt die Empfindlichkeit ein.",
              "El teclado destella con un color nuevo en cada golpe. El control ajusta la sensibilidad.",
              "Le clavier flashe d'une nouvelle couleur à chaque temps. Le curseur règle la sensibilité.",
              "O teclado pisca numa cor nova a cada batida. O controle ajusta a sensibilidade.",
              "На каждый удар клавиатура вспыхивает новым цветом. Ползунок задаёт чувствительность.",
              "每个节拍键盘都会闪出新颜色。滑块调节灵敏度。");
            A("Klavye ekrandaki baskın renge uyar (film, oyun, video). Kaydırıcı tepki hızını ayarlar.",
              "Matches the dominant color on your screen (movies, games, videos). The slider sets response speed.",
              "Übernimmt die vorherrschende Bildschirmfarbe (Filme, Spiele, Videos). Der Regler stellt die Reaktion ein.",
              "Imita el color dominante de la pantalla (películas, juegos, vídeos). El control ajusta la respuesta.",
              "Reprend la couleur dominante de l'écran (films, jeux, vidéos). Le curseur règle la réactivité.",
              "Acompanha a cor predominante da tela (filmes, jogos, vídeos). O controle ajusta a resposta.",
              "Повторяет преобладающий цвет на экране (фильмы, игры, видео). Ползунок задаёт скорость отклика.",
              "跟随屏幕上的主色（电影、游戏、视频）。滑块调节响应速度。");
            A("İşlemci boştayken yeşil, yük arttıkça sarı ve kırmızı olur. %90'ı geçerse kırmızı nabız atar.",
              "Green when the CPU is idle, turning yellow and red as load rises. Pulses red above 90%.",
              "Grün im Leerlauf, mit steigender Last gelb und rot. Pulsiert rot über 90 %.",
              "Verde con la CPU en reposo; amarillo y rojo al subir la carga. Late en rojo por encima del 90 %.",
              "Vert au repos, puis jaune et rouge quand la charge augmente. Pulse en rouge au-delà de 90 %.",
              "Verde com a CPU ociosa; amarelo e vermelho conforme a carga sobe. Pulsa em vermelho acima de 90%.",
              "Зелёный в простое, жёлтый и красный при росте нагрузки. Выше 90% пульсирует красным.",
              "CPU 空闲时为绿色，负载升高时变黄变红。超过 90% 时红色脉动。");
            A("Pil doluyken yeşil, azaldıkça sarı ve kırmızı olur. Şarj olurken nefes alır, %15'in altında kırmızı yanıp söner.",
              "Green when full, turning yellow and red as it drains. Breathes while charging, blinks red below 15%.",
              "Grün bei vollem Akku, gelb und rot beim Entladen. Atmet beim Laden, blinkt rot unter 15 %.",
              "Verde con la batería llena; amarillo y rojo al descargarse. Respira al cargar y parpadea en rojo por debajo del 15 %.",
              "Vert quand elle est pleine, puis jaune et rouge en se vidant. Respire en charge, clignote en rouge sous 15 %.",
              "Verde com bateria cheia; amarelo e vermelho ao descarregar. Respira ao carregar e pisca em vermelho abaixo de 15%.",
              "Зелёный при полном заряде, жёлтый и красный по мере разряда. «Дышит» при зарядке, ниже 15% мигает красным.",
              "电量满时为绿色，耗电时变黄变红。充电时呼吸，低于 15% 时红色闪烁。");
            A("Kendi animasyonunu adım adım tasarla: her adımın rengini, süresini ve bir sonraki renge yumuşak mı yoksa ani mi geçeceğini seç. Hız kaydırıcısı tüm süreleri ölçekler.",
              "Design your own animation step by step: choose each step's color, duration and whether it fades smoothly into the next. The speed slider scales all durations.",
              "Gestalte deine Animation Schritt für Schritt: Farbe, Dauer und ob sie weich in den nächsten Schritt übergeht. Der Tempo-Regler skaliert alle Zeiten.",
              "Diseña tu animación paso a paso: color, duración y si pasa suavemente al siguiente. El control de velocidad escala todas las duraciones.",
              "Créez votre animation étape par étape : couleur, durée et fondu vers l'étape suivante. Le curseur de vitesse ajuste toutes les durées.",
              "Crie sua animação passo a passo: cor, duração e se passa suavemente para o próximo. O controle de velocidade ajusta todas as durações.",
              "Создайте анимацию по шагам: цвет, длительность и плавность перехода к следующему шагу. Ползунок скорости меняет все длительности.",
              "逐步设计你的动画：设置每一步的颜色、时长以及是否平滑过渡到下一步。速度滑块会缩放所有时长。");
            A("Bir nefes Renk 1, bir nefes Renk 2 ile yanıp söner.",
              "Breathes once in Color 1, then once in Color 2.",
              "Atmet einmal in Farbe 1, dann einmal in Farbe 2.",
              "Respira una vez con el Color 1 y otra con el Color 2.",
              "Respire une fois en Couleur 1, puis une fois en Couleur 2.",
              "Respira uma vez na Cor 1 e outra na Cor 2.",
              "Один вдох цветом 1, следующий — цветом 2.",
              "先以颜色 1 呼吸一次，再以颜色 2 呼吸一次。");
            A("Renk 1 ile Renk 2 arasında keskin geçişlerle değişir.",
              "Switches sharply between Color 1 and Color 2.",
              "Wechselt hart zwischen Farbe 1 und Farbe 2.",
              "Alterna bruscamente entre el Color 1 y el Color 2.",
              "Alterne franchement entre Couleur 1 et Couleur 2.",
              "Alterna bruscamente entre a Cor 1 e a Cor 2.",
              "Резко переключается между цветом 1 и цветом 2.",
              "在颜色 1 和颜色 2 之间硬切换。");
            A("Seçtiğin renk loş yanar, arada parlak ışıltılar çakar.",
              "Your color glows dimly with bright sparkles now and then.",
              "Die Farbe leuchtet gedimmt, ab und zu funkelt es hell.",
              "Tu color brilla tenue con destellos ocasionales.",
              "Votre couleur luit faiblement, avec des scintillements de temps en temps.",
              "Sua cor brilha fraca, com cintilações de vez em quando.",
              "Цвет светится приглушённо, иногда вспыхивают яркие искры.",
              "所选颜色微亮，不时闪出亮光。");
            A("Renk 1'den Renk 2'ye hızlı nabız atışları.",
              "Quick pulses from Color 1 to Color 2.",
              "Schnelle Pulse von Farbe 1 zu Farbe 2.",
              "Pulsos rápidos del Color 1 al Color 2.",
              "Pulsations rapides de Couleur 1 vers Couleur 2.",
              "Pulsos rápidos da Cor 1 para a Cor 2.",
              "Быстрые импульсы от цвета 1 к цвету 2.",
              "从颜色 1 到颜色 2 的快速脉冲。");
            A("Yazdığın metni seçtiğin renkle Mors koduyla klavyeye yazar.",
              "Spells out your text in Morse code using your chosen color.",
              "Morst deinen Text in der gewählten Farbe.",
              "Escribe tu texto en código Morse con el color elegido.",
              "Transmet votre texte en morse avec la couleur choisie.",
              "Escreve seu texto em código Morse com a cor escolhida.",
              "Передаёт ваш текст азбукой Морзе выбранным цветом.",
              "用所选颜色以摩尔斯电码显示你的文字。");
            A("Her flaşta gökkuşağının bir sonraki rengi çakar.",
              "Each flash shows the next color of the rainbow.",
              "Jeder Blitz zeigt die nächste Regenbogenfarbe.",
              "Cada destello muestra el siguiente color del arcoíris.",
              "Chaque flash affiche la couleur suivante de l'arc-en-ciel.",
              "Cada flash mostra a próxima cor do arco-íris.",
              "Каждая вспышка — следующий цвет радуги.",
              "每次闪烁显示彩虹的下一种颜色。");
            A("Üst üste binen dalgalarla akan, öngörülemeyen renk plazması.",
              "An unpredictable plasma of colors flowing in overlapping waves.",
              "Unvorhersehbares Farbplasma aus sich überlagernden Wellen.",
              "Un plasma de colores impredecible que fluye en ondas superpuestas.",
              "Un plasma de couleurs imprévisible qui ondule en vagues superposées.",
              "Um plasma de cores imprevisível fluindo em ondas sobrepostas.",
              "Непредсказуемая цветовая плазма из наложенных волн.",
              "由叠加波形流动、难以预测的彩色等离子。");
            A("Rastgele zamanlarda rastgele renkli flaşlar patlar.",
              "Random colored flashes burst at random moments.",
              "Zufällige farbige Blitze zu zufälligen Zeitpunkten.",
              "Destellos de colores aleatorios en momentos aleatorios.",
              "Des flashs de couleurs aléatoires à des moments aléatoires.",
              "Flashes de cores aleatórias em momentos aleatórios.",
              "Вспышки случайных цветов в случайные моменты.",
              "随机时刻闪出随机颜色。");
            A("Sıcak, titreyen ve ara sıra kısılan mum alevi.",
              "A warm, flickering candle flame that dips now and then.",
              "Warme, flackernde Kerzenflamme, die ab und zu schwächer wird.",
              "Llama de vela cálida y parpadeante que a veces se atenúa.",
              "Flamme de bougie chaude et vacillante qui faiblit parfois.",
              "Chama de vela quente e tremeluzente que às vezes diminui.",
              "Тёплое мерцающее пламя свечи, иногда притухающее.",
              "温暖摇曳、偶尔变暗的烛火。");
            A("Karanlık mavi gökyüzünde ara sıra çakan şimşekler.",
              "Occasional lightning strikes across a dark blue sky.",
              "Gelegentliche Blitze an einem dunkelblauen Himmel.",
              "Relámpagos ocasionales en un cielo azul oscuro.",
              "Des éclairs de temps en temps dans un ciel bleu nuit.",
              "Relâmpagos ocasionais num céu azul-escuro.",
              "Редкие молнии на тёмно-синем небе.",
              "深蓝夜空中不时划过闪电。");
            A("Siren gibi hızla kabarıp sönen kırmızı ışık.",
              "Red light that swells and fades fast, like a siren.",
              "Rotes Licht, das wie eine Sirene schnell an- und abschwillt.",
              "Luz roja que sube y baja rápido, como una sirena.",
              "Lumière rouge qui monte et retombe vite, comme une sirène.",
              "Luz vermelha que sobe e desce rápido, como uma sirene.",
              "Красный свет быстро нарастает и гаснет, как сирена.",
              "像警笛一样快速明暗起伏的红光。");
            A("120 BPM tempoda her vuruşta yeni bir renk patlar.",
              "A new color bursts on every beat at 120 BPM.",
              "Bei 120 BPM eine neue Farbe auf jedem Beat.",
              "Un color nuevo en cada golpe a 120 BPM.",
              "Une nouvelle couleur à chaque temps, à 120 BPM.",
              "Uma cor nova a cada batida, a 120 BPM.",
              "Новый цвет на каждый удар в темпе 120 BPM.",
              "以 120 BPM 节奏，每拍迸发新颜色。");
            A("Dijital yağmur gibi titreşen yeşil.",
              "Green that flickers like digital rain.",
              "Grün, das wie digitaler Regen flackert.",
              "Verde que parpadea como lluvia digital.",
              "Du vert qui scintille comme une pluie numérique.",
              "Verde que tremeluz como chuva digital.",
              "Зелёный, мерцающий как цифровой дождь.",
              "像数字雨一样闪烁的绿色。");
            A("Müzik yükseldikçe renkler daha hızlı akar ve parlaklaşır. Kaydırıcı hassasiyeti ayarlar.",
              "Colors flow faster and brighter as the music gets louder. The slider sets sensitivity.",
              "Je lauter die Musik, desto schneller und heller fließen die Farben. Der Regler stellt die Empfindlichkeit ein.",
              "Los colores fluyen más rápido y brillantes cuando sube la música. El control ajusta la sensibilidad.",
              "Les couleurs défilent plus vite et plus fort quand la musique monte. Le curseur règle la sensibilité.",
              "As cores fluem mais rápido e mais fortes quando a música aumenta. O controle ajusta a sensibilidade.",
              "Чем громче музыка, тем быстрее и ярче текут цвета. Ползунок задаёт чувствительность.",
              "音乐越响，颜色流动越快越亮。滑块调节灵敏度。");
            A("Seçtiğin renk müziğin sesine göre parlayıp kısılır. Kaydırıcı hassasiyeti ayarlar.",
              "Your chosen color brightens and dims with the music. The slider sets sensitivity.",
              "Die gewählte Farbe wird mit der Musik heller und dunkler. Der Regler stellt die Empfindlichkeit ein.",
              "El color elegido sube y baja de brillo con la música. El control ajusta la sensibilidad.",
              "La couleur choisie s'intensifie et faiblit avec la musique. Le curseur règle la sensibilité.",
              "A cor escolhida aumenta e diminui de brilho com a música. O controle ajusta a sensibilidade.",
              "Выбранный цвет становится ярче и тусклее в такт музыке. Ползунок задаёт чувствительность.",
              "所选颜色随音乐明暗变化。滑块调节灵敏度。");
            A("Her tuşa basışta klavye parlar ve rengi bir adım değişir. Hangi tuşa basıldığı kaydedilmez.",
              "The keyboard lights up and shifts color with every key press. Which keys you press is never recorded.",
              "Bei jedem Tastendruck leuchtet die Tastatur auf und ändert die Farbe. Welche Tasten gedrückt werden, wird nie gespeichert.",
              "El teclado se ilumina y cambia de color con cada pulsación. Nunca se registra qué teclas pulsas.",
              "Le clavier s'illumine et change de couleur à chaque touche. Les touches pressées ne sont jamais enregistrées.",
              "O teclado acende e muda de cor a cada tecla. Nunca é registrado quais teclas você pressiona.",
              "При каждом нажатии клавиатура вспыхивает и меняет цвет. Какие клавиши нажаты, не записывается.",
              "每次按键，键盘都会亮起并变换颜色。不会记录你按了哪些键。");
            A("Hızlı yazdıkça renk maviden yeşile, sarıya ve kırmızıya döner. Hangi tuşa basıldığı kaydedilmez.",
              "The faster you type, the more the color shifts from blue to green, yellow and red. Which keys you press is never recorded.",
              "Je schneller du tippst, desto mehr wechselt die Farbe von Blau über Grün und Gelb zu Rot. Welche Tasten gedrückt werden, wird nie gespeichert.",
              "Cuanto más rápido escribes, más pasa el color de azul a verde, amarillo y rojo. Nunca se registra qué teclas pulsas.",
              "Plus vous tapez vite, plus la couleur passe du bleu au vert, au jaune puis au rouge. Les touches pressées ne sont jamais enregistrées.",
              "Quanto mais rápido você digita, mais a cor vai do azul ao verde, amarelo e vermelho. Nunca é registrado quais teclas você pressiona.",
              "Чем быстрее печатаете, тем сильнее цвет смещается от синего к зелёному, жёлтому и красному. Какие клавиши нажаты, не записывается.",
              "打字越快，颜色越从蓝变绿、变黄再变红。不会记录你按了哪些键。");
            A("Fareyi sağa sola götürdükçe renk, yukarı aşağı götürdükçe parlaklık değişir.",
              "Move the mouse left/right to change color, up/down to change brightness.",
              "Maus nach links/rechts ändert die Farbe, nach oben/unten die Helligkeit.",
              "Mueve el ratón a izquierda/derecha para cambiar el color y arriba/abajo para el brillo.",
              "Déplacez la souris à gauche/droite pour la couleur, en haut/bas pour la luminosité.",
              "Mova o mouse para os lados para mudar a cor e para cima/baixo para o brilho.",
              "Влево-вправо — цвет, вверх-вниз — яркость.",
              "左右移动鼠标改变颜色，上下移动改变亮度。");
            A("Bellek kullanımını gösterir: az kullanımda yeşil, doldukça kırmızı. %90 üstünde nabız atar.",
              "Shows memory usage: green when low, red as it fills up. Pulses above 90%.",
              "Zeigt die Speicherauslastung: grün bei wenig, rot wenn voll. Pulsiert über 90 %.",
              "Muestra el uso de memoria: verde si es bajo, rojo al llenarse. Late por encima del 90 %.",
              "Affiche l'usage mémoire : vert si faible, rouge quand elle se remplit. Pulse au-delà de 90 %.",
              "Mostra o uso de memória: verde quando baixo, vermelho quando cheia. Pulsa acima de 90%.",
              "Показывает загрузку памяти: зелёный — мало, красный — почти заполнена. Выше 90% пульсирует.",
              "显示内存使用率：低时绿色，接近占满时红色。超过 90% 时脉动。");
            A("İndirme mavi, yükleme turuncu yanar. Trafik arttıkça parlaklık artar.",
              "Downloads glow blue, uploads orange. Brighter as traffic increases.",
              "Downloads leuchten blau, Uploads orange. Mehr Verkehr = heller.",
              "Las descargas brillan en azul y las subidas en naranja. Más tráfico, más brillo.",
              "Téléchargements en bleu, envois en orange. Plus de trafic = plus lumineux.",
              "Downloads brilham em azul, uploads em laranja. Mais tráfego, mais brilho.",
              "Загрузка — синий, отдача — оранжевый. Чем больше трафик, тем ярче.",
              "下载为蓝色，上传为橙色。流量越大越亮。");
            A("Renk saate göre değişir: şafakta turuncu, öğlen beyaz, akşam altın sarısı, gece mor-lacivert.",
              "Color follows the clock: orange at dawn, white at noon, golden in the evening, purple-navy at night.",
              "Die Farbe folgt der Uhrzeit: orange im Morgengrauen, weiß am Mittag, golden am Abend, lila-blau in der Nacht.",
              "El color sigue la hora: naranja al amanecer, blanco a mediodía, dorado al atardecer, morado y azul marino de noche.",
              "La couleur suit l'heure : orange à l'aube, blanc à midi, doré le soir, violet et bleu nuit la nuit.",
              "A cor acompanha o relógio: laranja ao amanhecer, branco ao meio-dia, dourado à tarde, roxo-azul à noite.",
              "Цвет зависит от времени: оранжевый на рассвете, белый в полдень, золотой вечером, фиолетово-синий ночью.",
              "颜色随时间变化：黎明橙色，正午白色，傍晚金色，夜晚紫蓝色。");
            A("25 dakika çalışma: klavye yeşilden kırmızıya döner, son 10 saniye yanıp söner. Ardından 5 dakika mola: mavi nefes alır. Başlatınca sayaç sıfırlanır.",
              "25 min of work: fades from green to red, blinks in the last 10 s. Then a 5 min break: breathes blue. Resets when started.",
              "25 Min. Arbeit: von Grün zu Rot, blinkt in den letzten 10 s. Dann 5 Min. Pause: atmet blau. Beginnt bei jedem Start neu.",
              "25 min de trabajo: pasa de verde a rojo y parpadea los últimos 10 s. Luego 5 min de descanso: respira en azul. Se reinicia al iniciar.",
              "25 min de travail : du vert au rouge, clignote les 10 dernières secondes. Puis 5 min de pause : respire en bleu. Se réinitialise au démarrage.",
              "25 min de trabalho: vai do verde ao vermelho e pisca nos últimos 10 s. Depois 5 min de pausa: respira em azul. Reinicia ao iniciar.",
              "25 мин работы: от зелёного к красному, последние 10 с мигает. Затем 5 мин перерыва: «дышит» синим. Сбрасывается при запуске.",
              "25 分钟工作：由绿变红，最后 10 秒闪烁。随后 5 分钟休息：蓝色呼吸。每次开始时重置。");
            A("Her 15 saniyede bir başka efekte yumuşakça geçer. Hız kaydırıcısı hem efektleri hem geçişleri hızlandırır.",
              "Smoothly switches to another effect every 15 seconds. The speed slider speeds up both effects and switches.",
              "Wechselt alle 15 Sekunden sanft zu einem anderen Effekt. Der Tempo-Regler beschleunigt Effekte und Wechsel.",
              "Cambia suavemente a otro efecto cada 15 segundos. El control de velocidad acelera efectos y cambios.",
              "Passe en douceur à un autre effet toutes les 15 secondes. Le curseur de vitesse accélère effets et transitions.",
              "Muda suavemente para outro efeito a cada 15 segundos. O controle de velocidade acelera efeitos e trocas.",
              "Каждые 15 секунд плавно переключается на другой эффект. Ползунок скорости ускоряет и эффекты, и смену.",
              "每 15 秒平滑切换到另一个效果。速度滑块同时加快效果和切换。");
            A("Yumuşak, pastel tonlarda yavaş gökkuşağı.",
              "A slow rainbow in soft pastel shades.",
              "Langsamer Regenbogen in sanften Pastelltönen.",
              "Arcoíris lento en suaves tonos pastel.",
              "Arc-en-ciel lent aux tons pastel doux.",
              "Arco-íris lento em tons pastel suaves.",
              "Медленная радуга в мягких пастельных тонах.",
              "柔和粉彩色调的慢速彩虹。");
            A("Yedi ana renk arasında kısa geçişlerle adım adım dolaşır.",
              "Steps through the seven main colors with short transitions.",
              "Geht mit kurzen Übergängen die sieben Grundfarben durch.",
              "Recorre los siete colores principales con transiciones cortas.",
              "Parcourt les sept couleurs principales avec de courtes transitions.",
              "Percorre as sete cores principais com transições curtas.",
              "Шаг за шагом проходит семь основных цветов с короткими переходами.",
              "以短暂过渡依次切换七种主色。");
            A("Yeşil, turkuaz ve mor tonlarda dalgalanan aurora.",
              "An aurora rippling in green, turquoise and purple.",
              "Polarlicht, das in Grün, Türkis und Lila wogt.",
              "Una aurora ondulante en verde, turquesa y morado.",
              "Une aurore ondulant en vert, turquoise et violet.",
              "Uma aurora ondulando em verde, turquesa e roxo.",
              "Сияние, переливающееся зелёным, бирюзовым и фиолетовым.",
              "绿色、青绿色和紫色交织的极光。");
            A("Turuncudan pembeye, mora ve geri yavaşça kayar.",
              "Slowly drifts from orange to pink, purple and back.",
              "Gleitet langsam von Orange über Pink zu Lila und zurück.",
              "Pasa lentamente de naranja a rosa, morado y vuelta.",
              "Glisse lentement de l'orange au rose, au violet, puis revient.",
              "Passa devagar do laranja ao rosa, roxo e volta.",
              "Медленно переходит от оранжевого к розовому, фиолетовому и обратно.",
              "由橙色缓缓过渡到粉色、紫色，再回来。");
            A("Yaprak arasından süzülen ışık gibi yeşil tonlar.",
              "Green tones, like light filtering through leaves.",
              "Grüntöne wie Licht, das durch Blätter fällt.",
              "Tonos verdes, como luz entre las hojas.",
              "Des tons verts, comme la lumière à travers les feuilles.",
              "Tons de verde, como luz passando entre as folhas.",
              "Зелёные тона, как свет сквозь листву.",
              "像穿过树叶的光一样的绿色调。");
            A("Kırmızı, turuncu ve magentada ağır ağır akan lav.",
              "Lava flowing slowly in red, orange and magenta.",
              "Langsam fließende Lava in Rot, Orange und Magenta.",
              "Lava que fluye lentamente en rojo, naranja y magenta.",
              "De la lave qui coule lentement, rouge, orange et magenta.",
              "Lava fluindo devagar em vermelho, laranja e magenta.",
              "Медленно текущая лава в красных, оранжевых и пурпурных тонах.",
              "红、橙、品红色缓缓流动的熔岩。");
            A("Beyaz ve buz mavisi tonlarda soğuk parıltı.",
              "A cold shimmer in white and ice blue.",
              "Kaltes Schimmern in Weiß und Eisblau.",
              "Un brillo frío en blanco y azul hielo.",
              "Un miroitement froid, blanc et bleu glacier.",
              "Um brilho frio em branco e azul-gelo.",
              "Холодное мерцание белого и ледяного голубого.",
              "白色与冰蓝色的冷光闪烁。");
            A("Mor, lacivert ve pembe tonlarda uzay bulutları.",
              "Space clouds in purple, navy and pink.",
              "Weltraumnebel in Lila, Dunkelblau und Pink.",
              "Nubes espaciales en morado, azul marino y rosa.",
              "Nuages spatiaux violets, bleu nuit et roses.",
              "Nuvens espaciais em roxo, azul-marinho e rosa.",
              "Космические облака в фиолетовых, тёмно-синих и розовых тонах.",
              "紫色、深蓝和粉色的星云。");
            A("Kırmızı ve beyaz sırayla çakar.",
              "Red and white flash in turn.",
              "Rot und Weiß blitzen abwechselnd.",
              "Rojo y blanco destellan por turnos.",
              "Rouge et blanc clignotent à tour de rôle.",
              "Vermelho e branco piscam alternadamente.",
              "Красный и белый мигают поочерёдно.",
              "红色与白色轮流闪烁。");
            A("Kırmızı ve turuncu hızlı çakar.",
              "Fast red and orange flashes.",
              "Schnelle rote und orange Blitze.",
              "Destellos rápidos en rojo y naranja.",
              "Flashs rapides rouges et orange.",
              "Flashes rápidos em vermelho e laranja.",
              "Быстрые красные и оранжевые вспышки.",
              "红色与橙色快速闪烁。");
            A("Neon pembe ve camgöbeği arasında gidip gelir.",
              "Swings between neon pink and cyan.",
              "Pendelt zwischen Neonpink und Cyan.",
              "Oscila entre rosa neón y cian.",
              "Oscille entre rose néon et cyan.",
              "Alterna entre rosa neon e ciano.",
              "Переливается между неоново-розовым и голубым.",
              "在霓虹粉与青色之间来回切换。");
            A("Zehir yeşili tonlarda titreyen nabız.",
              "A flickering pulse in poison-green shades.",
              "Flackernder Puls in Giftgrün.",
              "Un pulso parpadeante en tonos verde veneno.",
              "Une pulsation vacillante dans des verts poison.",
              "Um pulso tremeluzente em tons de verde-veneno.",
              "Мерцающий пульс в ядовито-зелёных тонах.",
              "毒绿色调的闪烁脉冲。");
            A("Yeşil, sarı, kırmızı sırasıyla değişir.",
              "Changes green, yellow, red in order.",
              "Wechselt der Reihe nach Grün, Gelb, Rot.",
              "Cambia a verde, amarillo y rojo en orden.",
              "Passe au vert, au jaune puis au rouge.",
              "Muda para verde, amarelo e vermelho, em ordem.",
              "По очереди: зелёный, жёлтый, красный.",
              "依次切换绿、黄、红。");
            A("Kırmızı ve beyaz dalgalanır.",
              "Red and white ripple.",
              "Rot und Weiß wogen.",
              "Rojo y blanco ondulan.",
              "Rouge et blanc ondulent.",
              "Vermelho e branco ondulam.",
              "Переливы красного и белого.",
              "红白交替起伏。");
            A("Sarı ve kırmızı.", "Yellow and red.", "Gelb und Rot.", "Amarillo y rojo.", "Jaune et rouge.", "Amarelo e vermelho.", "Жёлтый и красный.", "黄色与红色。");
            A("Sarı ve lacivert.", "Yellow and navy.", "Gelb und Marineblau.", "Amarillo y azul marino.", "Jaune et bleu marine.", "Amarelo e azul-marinho.", "Жёлтый и тёмно-синий.", "黄色与藏青色。");
            A("Siyah ve beyaz.", "Black and white.", "Schwarz und Weiß.", "Negro y blanco.", "Noir et blanc.", "Preto e branco.", "Чёрный и белый.", "黑色与白色。");
            A("Bordo ve mavi.", "Claret and blue.", "Bordeaux und Blau.", "Granate y azul.", "Bordeaux et bleu.", "Grená e azul.", "Бордовый и синий.", "酒红色与蓝色。");
        }
    }
}
