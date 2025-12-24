using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
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
        if (dbReference == null) dbReference = FirebaseDatabase.DefaultInstance.RootReference;

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
        if (string.IsNullOrEmpty(targetLanguage)) return;

        Dictionary<string, int> bestScores = new Dictionary<string, int>();

        if (loadingText) loadingText.text = targetLanguage + " Raporu Hazırlanıyor...";

        if (snapshot != null && snapshot.ChildrenCount > 0)
        {
            foreach (DataSnapshot child in snapshot.Children)
            {
                string json = child.GetRawJsonValue();
                UserScore data = JsonUtility.FromJson<UserScore>(json);

                if (data != null)
                {
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
        }

        string learnedTextContent = "";
        string studyTextContent = "";

        foreach (var item in bestScores)
        {
            string word = item.Key;
            int score = item.Value;
            string displayWord = char.ToUpper(word[0]) + word.Substring(1);

            if (score >= 70)
                learnedTextContent += $"+ {displayWord} (%{score})\n";
            else
                studyTextContent += $"- {displayWord} (%{score})\n";
        }

        if (learnedListText) learnedListText.text = learnedTextContent;
        if (studyListText) studyListText.text = studyTextContent;

        if (bestScores.Count == 0)
        {
            if (loadingText) loadingText.text = targetLanguage + " için henüz kayıt yok.";
        }
        else
        {
            if (loadingText) loadingText.text = "";
        }
    }
}

[System.Serializable]
public class UserScore
{
    public string word;
    public int score;
    public string language;
    public string date;
}