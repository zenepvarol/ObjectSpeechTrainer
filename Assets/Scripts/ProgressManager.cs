using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro Kütüphanesi
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;

public class ProgressManager : MonoBehaviour
{
    [Header("UI Bağlantıları (Sürükle-Bırak)")]
    public TMP_Text learnedListText; // "Öğrenilenler" listesinin Text'i
    public TMP_Text studyListText;   // "Çalışılacaklar" listesinin Text'i
    public TMP_Text loadingText;     // Ortadaki bilgilendirme yazısı

    // Başlangıçta dil seçili DEĞİL (Boş bırakıyoruz)
    private string targetLanguage = "";

    private DatabaseReference dbReference;
    private DataSnapshot currentSnapshot; // Veriyi hafızada tutmak için

    void OnEnable()
    {
        // Panel her açıldığında ekranı TEMİZLE ve seçim bekle
        ResetPanel();
    }

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // --- EKRANI SIFIRLAYAN YENİ FONKSİYON ---
    void ResetPanel()
    {
        targetLanguage = ""; // Seçili dili sıfırla

        // Listeleri temizle (Ekranda eski yazı kalmasın)
        if (learnedListText) learnedListText.text = "";
        if (studyListText) studyListText.text = "";

        // Kullanıcıya ne yapması gerektiğini söyle
        if (loadingText) loadingText.text = "Yukarıdaki menüden bir dil seçin.";
    }

    // --- BUTONLARIN KULLANACAĞI FONKSİYON ---
    public void ChangeReportLanguage(string languageCode)
    {
        targetLanguage = languageCode;
        // Debug.Log("Seçilen Dil: " + targetLanguage);

        // Eğer veriyi daha önce çektiysek tekrar internete gitme, eldekini filtrele
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
        if (dbReference == null) dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Sadece dil seçildiyse "Yükleniyor" yazsın
        if (loadingText && !string.IsNullOrEmpty(targetLanguage))
            loadingText.text = targetLanguage + " verileri yükleniyor...";

        dbReference.Child("scores").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                if (loadingText) loadingText.text = "Bağlantı Hatası.";
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
        // Eğer kullanıcı henüz bir dil seçmediyse işlem yapma
        if (string.IsNullOrEmpty(targetLanguage)) return;

        Dictionary<string, int> bestScores = new Dictionary<string, int>();

        if (loadingText) loadingText.text = targetLanguage + " Raporu Hazırlanıyor...";

        foreach (DataSnapshot child in snapshot.Children)
        {
            string json = child.GetRawJsonValue();
            UserScore data = JsonUtility.FromJson<UserScore>(json);

            if (data != null)
            {
                // *** FİLTRELEME ***
                // Verinin dili yoksa veya seçilen dille eşleşmiyorsa GEÇ
                if (string.IsNullOrEmpty(data.language) || data.language != targetLanguage)
                    continue;

                string wordKey = data.word.ToLower().Trim();

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

        // Listeleri Hazırla
        string learnedTextContent = "";
        string studyTextContent = "";

        foreach (var item in bestScores)
        {
            string word = item.Key;
            int score = item.Value;
            string displayWord = char.ToUpper(word[0]) + word.Substring(1);

            if (score >= 70)
                learnedTextContent += $"✅ {displayWord} (%{score})\n";
            else
                studyTextContent += $"⚠️ {displayWord} (%{score})\n";
        }

        // Ekrana Yazdır
        if (learnedListText) learnedListText.text = learnedTextContent;
        if (studyListText) studyListText.text = studyTextContent;

        // Veri Kontrolü
        if (bestScores.Count == 0)
        {
            if (loadingText) loadingText.text = targetLanguage + " için henüz kayıt yok.";
        }
        else
        {
            if (loadingText) loadingText.text = ""; // Veriler geldiyse yükleniyor yazısını kaldır
        }
    }
}

// UserScore sınıfı
[System.Serializable]
public class UserScore
{
    public string word;
    public int score;
    public string language;
    public string date;
}