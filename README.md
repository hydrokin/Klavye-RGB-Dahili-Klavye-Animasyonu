# Klavye RGB – Dahili Klavye Animasyonu

MEDION ERAZER Deputy P60i'nin (Clevo şasi) dahili RGB klavyesi için animasyon programı. **58 efekt**, 9 kategori.

## Nasıl çalışıyor?

Laptopun klavye ışığını gömülü denetleyici (EC) yönetiyor. Program, Clevo sürücüsüyle gelen `InsydeDCHU.dll` üzerinden klavyeye canlı renk komutu gönderiyor. Bu, Fn tuşlarının kullandığı yolun aynısı.

- Klavye **tek bölgeli**: bütün tuşlar aynı anda aynı renkte yanar. Animasyonlar rengin ve parlaklığın zamanla değişmesiyle yapılıyor.
- **Yönetici izni gerekmez.**
- Program kayıtlı klavye ayarlarını **hiç değiştirmez**. Durdurduğunda ya da kapattığında klavye kendi rengine veya efektine geri döner.

## Kullanım

1. `KlavyeRGB.exe` dosyasını çalıştır.
2. Soldan bir efekt seç (üstteki kutuya yazarak arayabilirsin), **Başlat**'a bas. Efekte çift tıklamak da başlatır.

| Ayar | Ne yapar |
|---|---|
| 🌐 Dil | Arayüz dili: English (varsayılan), Türkçe, Deutsch, Español, Français, Português, Русский, 中文. Seçim anında uygulanır ve hemen kaydedilir; program yeniden açılınca aynı dille gelir. |
| Efekt ara… | Listeyi süzer. Türkçe karakter şart değil: "ates" yazınca "Ateş" bulunur. |
| Hız / Hassasiyet / Tepki hızı | Animasyon hızı. Müzik efektlerinde hassasiyet, ekran renginde tepki hızı olur. |
| Parlaklık | Renk parlaklığı (%5–100). Fn tuşlarıyla donanım parlaklığını ayrıca ayarlayabilirsin. |
| Renk 1 / Renk 2 | Renk kullanan efektlerde görünür. |
| Renk dengesini düzelt | Tek bölge RGB LED'lerin kırmızı ve maviye kaçmasını dengeler. |
| Tepsiye küçült | Animasyon arka planda sürer. |
| Windows açılınca başlat | Bilgisayar açılınca program tepside başlar ve son efekti oynatır. |

## Efektler

| Kategori | Efektler |
|---|---|
| **Temel** | Sabit Renk, Nefes, İki Renk Geçişi, İki Renk Nefes, İki Renk Yanıp Sönme, Nabız, Flaş, Kalp Atışı, Yıldız Işıltısı, Mors Kodu (yazdığın metni yazar) |
| **Gökkuşağı** | Gökkuşağı, Gökkuşağı Nefes, Pastel Gökkuşağı, Renk Döngüsü, Gökkuşağı Flaş, Plazma, Rastgele Renkler, Rastgele Flaşlar |
| **Doğa** | Ateş, Mum Işığı, Okyanus, Kuzey Işıkları, Gün Batımı, Orman, Fırtına (şimşekli), Lav Lambası, Buz, Galaksi |
| **Çakar ve ritim** | Polis Çakarı, Ambulans, İtfaiye, Alarm, Disko, Neon Şehir, Matrix, Toksik, Trafik Lambası |
| **Müzik** | Ses Seviyesi, Vuruş, Renk Akışı, Tek Renk |
| **Etkileşimli** | Ekran Rengi, Yazarken Parla, Yazma Hızı, Fare ile Renk |
| **Sistem ve zaman** | İşlemci Yükü, RAM Kullanımı, İnternet Trafiği, Pil Durumu, Günün Saati, Pomodoro Zamanlayıcı |
| **Takım ve bayrak** | Türk Bayrağı, Galatasaray, Fenerbahçe, Beşiktaş, Trabzonspor |
| **Karışık ve özel** | Karışık (15 saniyede bir otomatik başka efekte geçer), Özel Animasyon (kendin yap) |

"Yazarken Parla" ve "Yazma Hızı" efektleri sadece kaç tuşa basıldığını sayar. Hangi tuşa basıldığı hiçbir yerde tutulmaz ve kaydedilmez.

### Özel animasyon (kendin yap)

Tablodaki her satır bir adım:

- **Renk** hücresine tıklayınca renk seçici açılır.
- **Süre (ms)** o adımın ne kadar süreceğini belirler.
- **Yumuşak geçiş** işaretliyse renk bir sonraki adımın rengine yavaşça akar.

**Kaydet…** ile `.klavyeanim` dosyasına kaydedip **Aç…** ile geri yükleyebilirsin. Dosya düz metindir, her satır `RRGGBB süre_ms yumuşak(1/0)` biçimindedir:

```
# kırmızıdan maviye yumuşak geçiş, sonra beyaz flaş
#FF0000 1000 1
#0000FF 1000 0
#FFFFFF 100 0
#000000 300 0
```

## Kaynaktan derleme

`derle.bat` dosyasını çalıştırman yeterli. Windows'ta hazır gelen .NET Framework derleyicisini kullanır. Kaynak kodlar ve uygulama simgesi (`app.ico`) `src\` klasöründe.

Ayarlar `%APPDATA%\KlavyeRGB\ayarlar.txt` dosyasında tutulur.

## Teknik not

Klavye komutu `103` (0x67), tampon `{G, R, B, 0xF0}`. Bu protokol [tuxedo-drivers](https://github.com/tuxedocomputers/tuxedo-drivers) (`clevo_interfaces.h`) ve [opencontrol](https://github.com/douweschulte/opencontrol) ile doğrulandı. Program yalnızca bu ışık komutunu kullanır. Fan veya güç komutlarına dokunmaz.
