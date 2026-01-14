🎓 Graduation Project: AR-Powered Language Learning App with YOLO & AI
Bu çalışma, üniversite eğitimimin Mezuniyet Projesi (Senior Project) kapsamında geliştirilmiştir. Proje, çocukların yabancı dil öğrenme sürecini fiziksel dünya ile ilişkilendiren; Artırılmış Gerçeklik (AR), Nesne Algılama (YOLOv4) ve Üretken Yapay Zeka (Gemini) teknolojilerini birleştiren kapsamlı bir mobil eğitim platformudur.

✨ Öne Çıkan Özellikler
Gerçek Zamanlı Nesne Tanıma: YOLOv4-Tiny mimarisi ve OpenCV kullanarak çevredeki nesneleri anlık olarak tespit eder.

AR Etiketleme: Tespit edilen nesnelerin isimlerini Unity WorldSpace Canvas aracılığıyla doğrudan nesne üzerine AR etiketi olarak yansıtır.

Çok Dilli Telaffuz (TTS): İngilizce, Almanca, İspanyolca ve İtalyanca dillerinde asenkron seslendirme desteği sağlar.

Oyunlaştırılmış Eğitim: Hafıza oyunları (Memory Game) ve kelime karıştırma (Word Scramble) modülleri ile öğrenmeyi eğlenceli hale getirir.

AI Destekli Analiz: Kullanıcı telaffuzlarını Whisper ve Gemini API'leri üzerinden analiz ederek geri bildirim sunar.

Bulut Veri Yönetimi: Kullanıcı ilerlemesi ve puanlama sistemini Firebase Realtime Database üzerinden senkronize eder.

🛠️ Kullanılan Teknolojiler
Motor: Unity 2022.x+

Dil: C#

AI/ML: YOLOv4-Tiny, OpenCV for Unity, Google Gemini API, OpenAI Whisper.

Backend: Firebase Auth & Realtime Database.

Platform: Android (ARM64, IL2CPP).

📊 Saha Testleri
Uygulama, COCO veri seti temelinde test edilmiş; günlük nesneler (diş fırçası, kedi, makas vb.) üzerinde yüksek doğrulukla çalışmıştır. Tanımlı olmayan nesneler için (Örn: AirPods) hata analizi yapılmış ve gelecek çalışmalar için yol haritası belirlenmiştir.

🔧 Kurulum ve Derleme
Projeyi klonlayın.

Unity Hub üzerinden projeyi açın.

External Dependency Manager (EDM4U) aracılığıyla Android bağımlılıklarını çözümleyin.

Firebase yapılandırma dosyasını (google-services.json) ekleyin.

Build Settings'ten platformu Android olarak seçin, IL2CPP ve ARM64 ayarlarını aktif ederek APK çıktısı alın.

📝 Akademik Süreç ve Geliştirme
Bu proje, benim ilk kapsamlı mobil geliştirme ve APK yayına hazırlık deneyimimdir. Projenin akademik kurgusu ve geliştirilmesi sürecinde;

Danışman hocamın rehberliğinde teknik dökümantasyon hazırlanmış,

Bozuk veritabanı şemaları baştan inşa edilmiş,

Kütüphane çakışmaları ve platform kısıtlamaları (Android ARM64/IL2CPP) çözülerek çalışan bir ürün haline getirilmiştir.
