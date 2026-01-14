# Graduation Project: AI & AR-Powered Language Learning platform

Bu proje, üniversite mezuniyet çalışmam olarak geliştirilmiş; **Nesne Algılama (YOLOv4)**, **Artırılmış Gerçeklik (AR)** ve **Üretken Yapay Zeka (Gemini API)** teknolojilerini bir araya getiren yenilikçi bir dil öğrenme mobil uygulamasıdır.

---

## Temel Özellikler

* **Gerçek Zamanlı Nesne Tanıma:** YOLOv4-Tiny mimarisi ile çevredeki 80 farklı nesne sınıfını anlık olarak tespit eder.
* **AR Etiketleme:** Tanınan nesnelerin üzerine, seçilen hedef dildeki karşılıklarını AR teknolojisiyle yerleştirir.
* **Akıllı Telaffuz Analizi:** Whisper ve Gemini API kullanarak kullanıcının konuşma pratiğini değerlendirir ve geri bildirim verir.
* **Oyunlaştırılmış Öğrenme:** Hafıza kartları ve kelime karıştırma oyunları ile kelime bilgisini pekiştirir.
* **Bulut Senkronizasyonu:** Firebase entegrasyonu ile kullanıcı skorlarını ve ilerlemesini anlık olarak saklar.

## Teknik Altyapı

| Kategori | Kullanılan Teknolojiler |
| --- | --- |
| **Geliştirme Motoru** | Unity 2022.x+ |
| **Dil** | C# (Scripts) |
| **Yapay Zeka** | YOLOv4-Tiny, OpenCV for Unity, Google Gemini, OpenAI Whisper |
| **Backend** | Firebase Auth & Realtime Database |
| **Platform** | Android (ARM64, IL2CPP Architecture) |

## Saha Testleri ve Analiz

Uygulama, gerçek dünya koşullarında test edilmiştir. Diş fırçası, makas ve kedi gibi COCO veri setinde tanımlı nesneler üzerinde yüksek başarı oranı göstermiştir. AirPods gibi tanımlı olmayan nesneler üzerinde yapılan testler, sistemin sınırlılıklarını ve gelecek geliştirme alanlarını belirlemek için dökümante edilmiştir.

## Kurulum Notları

1. Unity projesini klonlayın.
2. **EDM4U** üzerinden Android kütüphane bağımlılıklarını güncelleyin.
3. Kendi `google-services.json` dosyanızı projeye dahil edin.
4. Build Settings'ten Android platformu için APK çıktısı alın.

---

> **Not:** Bu çalışma, benim ilk kapsamlı mobil geliştirme projemdir. Proje süresince veritabanı mimarisinin yeniden yapılandırılması, kütüphane çakışmalarının çözülmesi ve çok dilli asenkron sistemlerin entegrasyonu gibi teknik süreçler bizzat yönetilmiştir.

---
