using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth; // <-- EKLENDİ: Kullanıcıyı tanımak için şart!
using System.Collections.Generic;

public class ProgressManager : MonoBehaviour
{
    [Header("UI Bağlantıları (Sürükle-Bırak)")]
    public TMP_Text learnedListText;
    public TMP_Text studyListText;
    public TMP_Text loadingText;

    private string targetLanguage = "";

    private DatabaseReference dbReference;
    private DataSnapshot currentSnapshot;

    void OnEnable()
    {
        ResetPanel();
    }

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    void ResetPanel()
    {
        targetLanguage = "";
        currentSnapshot = null;

        if (learnedListText) learnedListText.text = "";
        if (studyListText) studyListText.text = "";
        if (loadingText) loadingText.text = "Yukarıdaki menüden bir dil seçin.";
    }

    public void ChangeReportLanguage(string languageCode)
    {
        targetLanguage = languageCode;

        if (currentSnapshot != null)
        {
            ProcessData(currentSnapshot);
        }
        else
        {
            LoadProgressData();
        }
    }

    public void LoadProgressData()
    {
        // 1. ADIM: Giriş yapmış kullanıcıyı bul
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;

        if (currentUser == null)
        {
            if (loadingText) loadingText.text = "Lütfen önce giriş yapın!";
            return;
        }

        string userId = currentUser.UserId; // Kullanıcının ID'si

        if (dbReference == null) dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        if (loadingText && !string.IsNullOrEmpty(targetLanguage))
            loadingText.text = targetLanguage + " verileri yükleniyor...";

        // 2. ADIM: Sadece O KULLANICININ verisini çek (scores -> UserID)
        dbReference.Child("scores").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                if (loadingText) loadingText.text = "Bağlantı Hatası.";
                Debug.LogError("Veri çekme hatası: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                currentSnapshot = task.Result;
                ProcessData(currentSnapshot);
            }
        });
    }

    void ProcessData(DataSnapshot snapshot)
    {
        if (string.IsNullOrEmpty(targetLanguage)) return;

        Dictionary<string, int> bestScores = new Dictionary<string, int>();

        if (loadingText) loadingText.text = targetLanguage + " Raporu Hazırlanıyor...";

        // Veri var mı kontrolü
        if (snapshot != null && snapshot.ChildrenCount > 0)
        {
            foreach (DataSnapshot child in snapshot.Children)
            {
                string json = child.GetRawJsonValue();

                // Hata önleyici try-catch (Bazen bozuk veri gelebilir)
                try
                {
                    UserScore data = JsonUtility.FromJson<UserScore>(json);

                    if (data != null)
                    {
                        // Dil kontrolü (Boşsa veya eşleşmiyorsa geç)
                        if (string.IsNullOrEmpty(data.language) || data.language != targetLanguage)
                            continue;

                        string wordKey = data.word.ToLower().Trim();

                        // En yüksek puanı tutma mantığı
                        if (bestScores.ContainsKey(wordKey))
                        {
                            if (data.score > bestScores[wordKey])
                                bestScores[wordKey] = data.score;
                        }
                        else
                        {
                            bestScores.Add(wordKey, data.score);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Veri okuma hatası (önemsiz): " + ex.Message);
                }
            }
        }

        string learnedTextContent = "";
        string studyTextContent = "";

        // Listeleri oluştur
        foreach (var item in bestScores)
        {
            string word = item.Key;
            int score = item.Value;
            string displayWord = char.ToUpper(word[0]) + word.Substring(1);

            if (score >= 70)
                learnedTextContent += $"✅ {displayWord} (%{score})\n"; // Biraz süsledim
            else
                studyTextContent += $"📖 {displayWord} (%{score})\n";
        }

        if (learnedListText) learnedListText.text = learnedTextContent;
        if (studyListText) studyListText.text = studyTextContent;

        if (bestScores.Count == 0)
        {
            if (loadingText) loadingText.text = targetLanguage + " için henüz veri yok.";
        }
        else
        {
            if (loadingText) loadingText.text = ""; // Yükleniyor yazısını sil
        }
    }
}

// NOT: Eğer bu class 'PronunciationGame.cs' dosyasında zaten varsa
// ve "Duplicate definition" hatası alırsan, aşağıdaki kısmı sil.
// Eğer yoksa kalsın.

[System.Serializable]
public class UserScore
{
    public string word;
    public int score;
    public string language;
    public string date;
}
