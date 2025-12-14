using UnityEngine;
using System.Text.RegularExpressions;

public class PronunciationManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public string geminiApiKey;
    public string targetWord = "apple";

    [Header("Referanslar")]
    public SpeechRecorder recorder;
    public GeminiSTT geminiService;

    public void StartPronunciationTest()
    {
        if (!CheckRefs()) return;

        Debug.Log($"Lütfen şu kelimeyi söyleyin: {targetWord}");
        recorder.StartRecording();
    }

    public void StopPronunciationTest()
    {
        if (!CheckRefs()) return;

        byte[] wav = recorder.StopRecordingToWavBytes();
        if (wav == null || wav.Length == 0)
        {
            Debug.LogError("Kayıt alınamadı / çok kısa.");
            return;
        }

        // ⬇⬇⬇  ***BURASI DÜZELTİLDİ — ARTIK Transcribe() KULLANIYOR*** 
        geminiService.Transcribe(geminiApiKey, wav, (transcript) =>
        {
            if (string.IsNullOrEmpty(transcript))
            {
                Debug.LogError("Transcript alınamadı.");
                return;
            }

            // temizle
            string heard = Normalize(transcript);
            string target = Normalize(targetWord);

            Debug.Log($"Gemini duydu: {heard}");

            int score = SimilarityScore(target, heard);
            Debug.Log($"Hedef: {target} | Söylenen: {heard} | Puan: {score}/100");

            if (score >= 85) Debug.Log("Harika! Çok doğru söyledin!");
            else if (score >= 60) Debug.Log("Fena değil, biraz daha net söyle!");
            else Debug.Log("Tekrar deneyelim!");
        });
    }

    private bool CheckRefs()
    {
        if (recorder == null || geminiService == null)
        {
            Debug.LogError("Script referansları eksik! Inspector'da Recorder ve Gemini Service ata.");
            return false;
        }
        if (string.IsNullOrEmpty(geminiApiKey))
        {
            Debug.LogError("Gemini API Key boş!");
            return false;
        }
        return true;
    }

    private string Normalize(string s)
    {
        s = s.ToLowerInvariant().Trim();
        s = Regex.Replace(s, @"[^a-z\s]", "");
        s = Regex.Replace(s, @"\s+", " ").Trim();
        return s;
    }

    private int SimilarityScore(string a, string b)
    {
        if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) return 100;
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;

        int dist = Levenshtein(a, b);
        float maxLen = Mathf.Max(a.Length, b.Length);
        float sim = 1f - (dist / maxLen);
        return Mathf.Clamp(Mathf.RoundToInt(sim * 100f), 0, 100);
    }

    private int Levenshtein(string s, string t)
    {
        int n = s.Length, m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; i++) d[i, 0] = i;
        for (int j = 0; j <= m; j++) d[0, j] = j;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = Mathf.Min(
                    Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost
                );
            }
        }
        return d[n, m];
    }
}
