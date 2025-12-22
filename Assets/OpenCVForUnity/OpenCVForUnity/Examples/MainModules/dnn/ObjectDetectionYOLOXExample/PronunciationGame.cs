using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;
using System.IO;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

namespace OpenCVForUnityExample
{
    public class PronunciationGame : MonoBehaviour
    {
        [Header("Gemini Ayarları")]
        public string geminiApiKey = "BURAYA_API_KEY_YAZIN";

        [Header("Dil Ayarları")]
        public static string CurrentLanguage = "English";

        [Header("UI Bağlantıları")]
        public Text scoreText;

        // Hedef kelime
        private string currentTargetWord = "";

        // Ses kaydı değişkenleri
        private AudioClip recordingClip;
        private string deviceName;
        private bool isRecording = false;

        // Gemini Adresi
        private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent";

        // Firebase Referansı
        private DatabaseReference dbReference;

        public static PronunciationGame Instance;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            // --- FIREBASE BAŞLATMA ---
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Firebase hazır, referansı al
                    dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                    Debug.Log("Firebase Bağlantısı Başarılı!");
                }
                else
                {
                    Debug.LogError(System.String.Format(
                      "Firebase hatası: {0}", dependencyStatus));
                }
            });

            // Mikrofon kontrolü
            if (Microphone.devices.Length > 0)
            {
                deviceName = Microphone.devices[0];
                Debug.Log("Mikrofon Hazır: " + deviceName);
            }
            else
            {
                Debug.LogError("MİKROFON BULUNAMADI!");
                if (scoreText) scoreText.text = "Mikrofon Yok!";
            }
        }

        public void SetTargetWord(string detectedObjectName)
        {
            currentTargetWord = detectedObjectName.Trim();
            Debug.Log("Yeni Hedef: " + currentTargetWord);
            if (scoreText) scoreText.text = "Şunu oku: " + currentTargetWord.ToUpper();
        }

        public void StartRecording()
        {
            if (string.IsNullOrEmpty(currentTargetWord))
            {
                if (scoreText) scoreText.text = "Önce bir nesne göster!";
                return;
            }

            if (string.IsNullOrEmpty(deviceName)) return;

            isRecording = true;
            // 10 saniyelik kayıt limiti
            recordingClip = Microphone.Start(deviceName, false, 10, 44100);

            if (scoreText) scoreText.text = "Dinliyorum...";
            Debug.Log($"Kayıt Başladı... (Hedef: {currentTargetWord})");
        }

        public void StopAndCheck()
        {
            if (!isRecording) return;

            isRecording = false;
            int position = Microphone.GetPosition(deviceName);
            Microphone.End(deviceName);

            if (scoreText) scoreText.text = "⏳ Puanlanıyor...";

            byte[] wavData = ConvertToWav(recordingClip, position);
            StartCoroutine(SendToGemini(wavData));
        }

        IEnumerator SendToGemini(byte[] audioData)
        {
            string cleanKey = geminiApiKey.Trim();
            string url = $"{API_URL}?key={cleanKey}";

            string base64Audio = Convert.ToBase64String(audioData);

            // --- GÜNCELLEME 2: Gemini Prompt (Torpil Engelleyici) ---
            // Eski kodda hedef kelimeyi söylüyorduk, Gemini düzeltiyordu.
            // Şimdi "Ne duyuyorsan onu yaz, düzeltme yapma" diyoruz.
            string promptText = "Listen to this audio. Transcribe exactly the single word that is spoken. Do not auto-correct. If it sounds like 'bottle', write 'bottle'.";

            string jsonBody = $@"
            {{
                ""contents"": [{{
                    ""parts"": [
                        {{ ""text"": ""{promptText}"" }},
                        {{
                            ""inline_data"": {{
                                ""mime_type"": ""audio/wav"",
                                ""data"": ""{base64Audio}""
                            }}
                        }}
                    ]
                }}]
            }}";

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    if (scoreText) scoreText.text = "Bağlantı Hatası!";
                    Debug.LogError($"❌ HATA: {request.downloadHandler.text}");
                }
                else
                {
                    string responseText = request.downloadHandler.text;
                    string spokenWord = ExtractTextFromJson(responseText);

                    // Puanı Hesapla
                    int score = CalculateScore(currentTargetWord, spokenWord);

                    // --- EKRANA YAZDIRMA ---
                    if (scoreText)
                    {
                        if (score >= 75)
                        {
                            scoreText.text = $"⭐⭐⭐ HARİKA! ({score})\nÇok güzel okudun!";
                            scoreText.color = Color.green;
                        }
                        else if (score >= 50)
                        {
                            scoreText.text = $"⭐ İYİ ({score})\nAnlaşıldı ama daha net söylemelisin.";
                            scoreText.color = new Color(1f, 0.64f, 0f); // Turuncu
                        }
                        else
                        {
                            scoreText.text = $"❌ OLMADI ({score})\nAlgılanan: {spokenWord}";
                            scoreText.color = Color.red;
                        }
                    }

                    Debug.Log($"Hedef: {currentTargetWord} | Duyulan: {spokenWord} | Puan: {score} | Dil: {CurrentLanguage}");

                    // --- FIREBASE'E KAYDET ---
                    SaveToFirebase(currentTargetWord, score);
                }
            }
        }

        // --- FIREBASE KAYIT FONKSİYONU ---
        void SaveToFirebase(string word, int score)
        {
            // --- GÜNCELLEME 1: Düşük Puan Filtresi ---
            // Eğer puan 50'den düşükse, veritabanını kirletme ve fonksiyondan çık.
            if (score < 50)
            {
                Debug.Log($"Puan düşük ({score}), veritabanına kaydedilmedi.");
                return;
            }

            if (dbReference == null)
            {
                Debug.LogWarning("Firebase hazır değil, kayıt yapılamadı.");
                return;
            }

            // Benzersiz bir ID oluştur (Push)
            string key = dbReference.Child("scores").Push().Key;

            // Veri paketini hazırla (DİL BİLGİSİ İLE)
            UserScore data = new UserScore(word, score, CurrentLanguage);
            string json = JsonUtility.ToJson(data);

            // Veritabanına yaz
            dbReference.Child("scores").Child(key).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
                if (task.IsCompleted)
                {
                    Debug.Log($"SONUÇ VERİTABANINA KAYDEDİLDİ! ({CurrentLanguage})");
                }
                else
                {
                    Debug.LogError("Kayıt başarısız: " + task.Exception);
                }
            });
        }

        // --- YARDIMCI FONKSİYONLAR ---
        string ExtractTextFromJson(string json)
        {
            try
            {
                string marker = "\"text\": \"";
                int start = json.IndexOf(marker);
                if (start == -1) return "???";

                start += marker.Length;
                int end = json.IndexOf("\"", start);

                return json.Substring(start, end - start)
                           .Replace("\\n", "")
                           .Trim();
            }
            catch { return "Hata"; }
        }

        byte[] ConvertToWav(AudioClip clip, int position)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(Encoding.UTF8.GetBytes("RIFF"), 0, 4);
                stream.Write(BitConverter.GetBytes(36 + position * 2), 0, 4);
                stream.Write(Encoding.UTF8.GetBytes("WAVE"), 0, 4);
                stream.Write(Encoding.UTF8.GetBytes("fmt "), 0, 4);
                stream.Write(BitConverter.GetBytes(16), 0, 4);
                stream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
                stream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
                stream.Write(BitConverter.GetBytes(44100), 0, 4);
                stream.Write(BitConverter.GetBytes(44100 * 2), 0, 4);
                stream.Write(BitConverter.GetBytes((ushort)2), 0, 2);
                stream.Write(BitConverter.GetBytes((ushort)16), 0, 2);
                stream.Write(Encoding.UTF8.GetBytes("data"), 0, 4);
                stream.Write(BitConverter.GetBytes(position * 2), 0, 4);

                float[] data = new float[position];
                clip.GetData(data, 0);

                foreach (var sample in data)
                {
                    short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * 32767f);
                    stream.Write(BitConverter.GetBytes(intSample), 0, 2);
                }

                return stream.ToArray();
            }
        }

        int CalculateScore(string target, string received)
        {
            string s = target.ToLower().Trim();
            string t = received.ToLower().Trim()
                               .Replace(".", "")
                               .Replace("!", "");

            if (s == t) return 100;

            int n = s.Length;
            int m = t.Length;

            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m == 0 ? 100 : 0;
            if (m == 0) return 0;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    d[i, j] = Mathf.Min(
                        Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }

            float maxLen = Mathf.Max(n, m);
            float similarity = 1.0f - ((float)d[n, m] / maxLen);
            return Mathf.Clamp((int)(similarity * 100), 0, 100);
        }
    }

    // --- GÜNCELLENMİŞ VERİ YAPISI ---
    [Serializable]
    public class UserScore
    {
        public string word;
        public int score;
        public string language; // YENİ: Dil bilgisi
        public string date;

        public UserScore(string word, int score, string language)
        {
            this.word = word;
            this.score = score;
            this.language = language; // Dili kaydet
            this.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}