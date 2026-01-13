using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
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
    private string currentLanguage; // Kelimenin orijinal dilini tutmak için
    private DatabaseReference dbReference;
    private bool isInteractable = true;

    void Start()
    {
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
            StartGameWithBackup();
        }
    }

    void LoadWordFromFirebase()
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;

        if (currentUser == null)
        {
            StartGameWithBackup();
            return;
        }

        string userId = currentUser.UserId;

        // Kullanýcýnýn klasörüne odaklan
        dbReference.Child("scores").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                StartGameWithBackup();
                return;
            }

            DataSnapshot snapshot = task.Result;
            List<CardData> possibleWords = new List<CardData>();

            if (snapshot != null && snapshot.Exists)
            {
                foreach (DataSnapshot child in snapshot.Children)
                {
                    string json = child.GetRawJsonValue();
                    if (string.IsNullOrEmpty(json)) continue;

                    SimpleScoreData data = JsonUtility.FromJson<SimpleScoreData>(json);

                    // Dil fark etmeksizin tüm kelimeleri CardData olarak topla
                    if (data != null && data.word.Length >= 3)
                    {
                        possibleWords.Add(new CardData { word = data.word.ToUpper(), language = data.language });
                    }
                }
            }

            if (possibleWords.Count > 0)
            {
                // Rastgele bir kelime seç ve dilini kaydet
                CardData selected = possibleWords[Random.Range(0, possibleWords.Count)];
                currentLanguage = selected.language;
                StartGame(selected.word);
            }
            else
            {
                StartGameWithBackup();
            }
        });
    }

    void StartGameWithBackup()
    {
        currentLanguage = "English"; // Yedek kelimeler için varsayýlan dil
        StartGame(backupWords[Random.Range(0, backupWords.Count)]);
    }

    void StartGame(string word)
    {
        currentWord = word.ToUpper();
        isInteractable = true;

        if (feedbackText != null) feedbackText.gameObject.SetActive(false);

        foreach (Transform child in answerArea) Destroy(child.gameObject);
        foreach (Transform child in letterArea) Destroy(child.gameObject);

        char[] chars = currentWord.ToCharArray();
        ShuffleArray(chars);

        foreach (char c in chars)
        {
            GameObject obj = Instantiate(letterPrefab, letterArea);
            ScrambleLetter script = obj.GetComponent<ScrambleLetter>();
            if (script != null) script.Setup(c, this);
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
        foreach (Transform child in answerArea)
            child.GetComponent<Image>().color = Color.red;

        yield return new WaitForSeconds(1.5f);

        List<Transform> letters = new List<Transform>();
        foreach (Transform child in answerArea) letters.Add(child);

        foreach (Transform t in letters)
        {
            ScrambleLetter script = t.GetComponent<ScrambleLetter>();
            if (script != null)
            {
                script.MoveToPool(letterArea);
                t.GetComponent<Image>().color = Color.white;
            }
        }

        isInteractable = true;
    }

    IEnumerator OnLevelComplete()
    {
        isInteractable = false;
        foreach (Transform child in answerArea)
            child.GetComponent<Image>().color = Color.green;

        if (feedbackText != null)
        {
            feedbackText.text = "TEBRÝKLER!";
            feedbackText.gameObject.SetActive(true);
        }

        // KELÝMENÝN KENDÝ AKSANIYLA OKUNMASI
        if (TextToSpeechManager.Instance != null)
            TextToSpeechManager.Instance.Speak(currentWord, currentLanguage);

        yield return new WaitForSeconds(2.0f);
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
            TextToSpeechManager.Instance.Speak(currentWord, currentLanguage);
    }

    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MiniGameScene");
    }
}