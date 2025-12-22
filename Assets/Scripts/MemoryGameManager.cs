using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using OpenCVForUnityExample;

public class MemoryGameManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject cardPrefab;
    public Transform gridContainer;
    public GameObject gameOverPanel; 

    [Header("Yedek Kelimeler")]
    public List<string> backupWords = new List<string> { "Test", "Game", "Unity", "Code", "Play", "Fun" };

    private List<MemoryCard> openCards = new List<MemoryCard>();
    private bool canClick = true;

    // Oyun Takibi Ýçin Yeni Deðiþkenler
    private int totalPairs;      // Toplam bulunmasý gereken çift sayýsý
    private int matchesFound;    // Þu ana kadar bulunan çift sayýsý
    private List<string> currentWordList; // Yeniden baþlatmak için listeyi hafýzada tut

    private DatabaseReference dbReference;

    void Start()
    {
        // Paneli gizle
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        StartCoroutine(InitializeFirebaseAndLoad());
    }

    IEnumerator InitializeFirebaseAndLoad()
    {
        var task = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Result == DependencyStatus.Available)
        {
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            LoadWordsFromFirebase();
        }
        else
        {
            StartGame(backupWords);
        }
    }

    void LoadWordsFromFirebase()
    {
        string currentLang = PronunciationGame.CurrentLanguage;

        dbReference.Child("scores").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                StartGame(backupWords);
                return;
            }

            DataSnapshot snapshot = task.Result;
            HashSet<string> learnedWordsSet = new HashSet<string>();

            foreach (DataSnapshot child in snapshot.Children)
            {
                string json = child.GetRawJsonValue();
                SimpleScoreData data = JsonUtility.FromJson<SimpleScoreData>(json);

                if (data != null && data.language == currentLang)
                {
                    learnedWordsSet.Add(data.word.ToUpper());
                }
            }

            List<string> finalWordList = new List<string>(learnedWordsSet);

            if (finalWordList.Count >= 6)
            {
                ShuffleList(finalWordList);
                StartGame(finalWordList.GetRange(0, 6));
            }
            else if (finalWordList.Count > 0)
            {
                StartGame(finalWordList);
            }
            else
            {
                StartGame(backupWords);
            }
        });
    }

    // --- OYUN KURULUMU ---
    void StartGame(List<string> sourceList)
    {
        currentWordList = new List<string>(sourceList); // Listeyi kaydet (Restart için)
        totalPairs = sourceList.Count;                  // Hedef çift sayýsýný belirle
        matchesFound = 0;                               // Sayacý sýfýrla

        if (gameOverPanel != null) gameOverPanel.SetActive(false); // Paneli kapat

        // 1. Kelimeleri Çiftle
        List<string> deck = new List<string>();
        foreach (string word in sourceList)
        {
            deck.Add(word);
            deck.Add(word);
        }

        ShuffleList(deck);

        // Masayý Temizle
        foreach (Transform child in gridContainer) Destroy(child.gameObject);

        // Kartlarý Daðýt
        foreach (string word in deck)
        {
            GameObject cardObj = Instantiate(cardPrefab, gridContainer);
            MemoryCard cardScript = cardObj.GetComponent<MemoryCard>();
            if (cardScript != null) cardScript.Setup(word, this);
        }
    }

    // --- KART MANTIÐI ---
    public void CardSelected(MemoryCard card)
    {
        if (!canClick || openCards.Contains(card)) return;

        card.FlipOpen();
        openCards.Add(card);

        if (openCards.Count == 2) StartCoroutine(CheckMatch());
    }

    IEnumerator CheckMatch()
    {
        canClick = false;
        yield return new WaitForSeconds(1.0f);

        if (openCards[0].myWord == openCards[1].myWord)
        {
            // EÞLEÞTÝ!
            openCards[0].MatchFound();
            openCards[1].MatchFound();

            matchesFound++; // Sayacý artýr
            CheckGameOver(); // Oyun bitti mi kontrol et
        }
        else
        {
            openCards[0].FlipClose();
            openCards[1].FlipClose();
        }
        openCards.Clear();
        canClick = true;
    }

    // --- OYUN SONU KONTROLÜ ---
    void CheckGameOver()
    {
        if (matchesFound >= totalPairs)
        {
            Debug.Log("OYUN BÝTTÝ! TEBRÝKLER!");
            // Biraz bekle sonra paneli aç
            StartCoroutine(ShowWinPanel());
        }
    }

    IEnumerator ShowWinPanel()
    {
        yield return new WaitForSeconds(0.5f);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // Opsiyonel: Bitiþ sesi veya alkýþ efekti çaldýrabilirsin
    }

    // --- BUTON FONKSÝYONLARI ---

    // 1. Tekrar Oyna
    public void RestartGame()
    {
        StartGame(currentWordList); // Ayný kelimelerle yeniden baþlat
    }

    // 2. Menüye Dön
    public void BackToMenu()
    {
        SceneManager.LoadScene("MiniGameScene");
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}

[System.Serializable]
public class SimpleScoreData
{
    public string word;
    public int score;
    public string language;
}