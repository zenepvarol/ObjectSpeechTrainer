using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using OpenCVForUnityExample;

public class WordScrambleManager : MonoBehaviour
{
    [Header("UI Baðlantýlarý")]
    public GameObject letterPrefab;
    public Transform answerArea;
    public Transform letterArea;
    public TMP_Text feedbackText;  

    [Header("Yedek Kelimeler")]
    public List<string> backupWords = new List<string> { "UNITY", "GAME", "CODE", "TEST", "PLAY" };

    private string currentWord;
    private DatabaseReference dbReference;
    private bool isInteractable = true;

    void Start()
    {
        // Baþlangýçta yazýyý gizle
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);

        StartCoroutine(InitializeFirebase());
    }

    IEnumerator InitializeFirebase()
    {
        var task = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Result == DependencyStatus.Available)
        {
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            LoadWordFromFirebase();
        }
        else
        {
            StartGame(backupWords[Random.Range(0, backupWords.Count)]);
        }
    }

    void LoadWordFromFirebase()
    {
        string currentLang = PronunciationGame.CurrentLanguage;

        dbReference.Child("scores").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                StartGame(backupWords[Random.Range(0, backupWords.Count)]);
                return;
            }

            DataSnapshot snapshot = task.Result;
            if (snapshot == null || !snapshot.Exists)
            {
                StartGame(backupWords[Random.Range(0, backupWords.Count)]);
                return;
            }

            List<string> possibleWords = new List<string>();
            foreach (DataSnapshot child in snapshot.Children)
            {
                string json = child.GetRawJsonValue();
                if (string.IsNullOrEmpty(json)) continue;

                SimpleScoreData data = JsonUtility.FromJson<SimpleScoreData>(json);

                if (data != null && data.language == currentLang && data.word.Length >= 3)
                {
                    possibleWords.Add(data.word.ToUpper());
                }
            }

            if (possibleWords.Count > 0)
            {
                StartGame(possibleWords[Random.Range(0, possibleWords.Count)]);
            }
            else
            {
                StartGame(backupWords[Random.Range(0, backupWords.Count)]);
            }
        });
    }

    void StartGame(string word)
    {
        currentWord = word.ToUpper();
        isInteractable = true;

        // Yazýyý gizle (Eðer açýk kaldýysa)
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);

        // Temizlik
        foreach (Transform child in answerArea) Destroy(child.gameObject);
        foreach (Transform child in letterArea) Destroy(child.gameObject);

        // Karýþtýrma
        char[] chars = currentWord.ToCharArray();
        ShuffleArray(chars);

        foreach (char c in chars)
        {
            GameObject obj = Instantiate(letterPrefab, letterArea);
            ScrambleLetter script = obj.GetComponent<ScrambleLetter>();
            script.Setup(c, this);
        }
    }

    public void LetterClicked(ScrambleLetter letter)
    {
        if (!isInteractable) return;

        if (letter.transform.parent == letterArea)
        {
            letter.MoveToAnswer(answerArea);
            CheckAnswer();
        }
        else
        {
            letter.MoveToPool(letterArea);
            letter.GetComponent<Image>().color = Color.white;
        }
    }

    void CheckAnswer()
    {
        string formedWord = "";
        foreach (Transform child in answerArea)
        {
            formedWord += child.GetComponent<ScrambleLetter>().GetChar();
        }

        if (formedWord.Length != currentWord.Length) return;

        if (formedWord == currentWord)
        {
            StartCoroutine(OnLevelComplete());
        }
        else
        {
            StartCoroutine(TryAgainRoutine());
        }
    }

    IEnumerator TryAgainRoutine()
    {
        isInteractable = false;

        // Harfleri Kýrmýzý Yap
        foreach (Transform child in answerArea)
            child.GetComponent<Image>().color = Color.red;

        // "YANLIÞ" yazýsý da gösterebiliriz istersen ama þimdilik sadece kýrmýzý olsun
        yield return new WaitForSeconds(1.5f);

        // Harfleri Aþaðýya Gönder
        List<Transform> letters = new List<Transform>();
        foreach (Transform child in answerArea) letters.Add(child);

        foreach (Transform t in letters)
        {
            ScrambleLetter script = t.GetComponent<ScrambleLetter>();
            script.MoveToPool(letterArea);
            t.GetComponent<Image>().color = Color.white;
        }

        isInteractable = true;
    }

    // --- TEBRÝKLER KISMI BURADA ---
    IEnumerator OnLevelComplete()
    {
        isInteractable = false;

        // 1. Harfleri Yeþil Yap
        foreach (Transform child in answerArea)
            child.GetComponent<Image>().color = Color.green;

        // 2. TEBRÝKLER Yazýsýný Aç
        if (feedbackText != null)
        {
            feedbackText.text = "TEBRÝKLER!";
            feedbackText.gameObject.SetActive(true);
        }

        // 3. Sesi Oku
        if (TextToSpeechManager.Instance != null)
            TextToSpeechManager.Instance.Speak(currentWord, PronunciationGame.CurrentLanguage);

        // 4. Ýki Saniye Bekle (Yazýyý okusun diye)
        yield return new WaitForSeconds(2.0f);

        // 5. Yeni Soruya Geç (Yazý StartGame içinde otomatik kapanacak)
        LoadWordFromFirebase();
    }

    void ShuffleArray<T>(T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            T temp = array[i];
            int r = Random.Range(i, array.Length);
            array[i] = array[r];
            array[r] = temp;
        }
    }

    public void PlayHint()
    {
        if (TextToSpeechManager.Instance != null)
            TextToSpeechManager.Instance.Speak(currentWord, PronunciationGame.CurrentLanguage);
    }

    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MiniGameScene");
    }
}