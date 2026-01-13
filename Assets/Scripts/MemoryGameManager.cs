using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
using OpenCVForUnityExample;

public class MemoryGameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform gridContainer;
    public GameObject gameOverPanel;

    public List<string> backupWords = new List<string> { "Apple", "Banana", "Cat", "Dog" };
    private List<MemoryCard> openCards = new List<MemoryCard>();
    private bool canClick = true;
    private int totalPairs;
    private int matchesFound;
    private List<CardData> currentCardDataList;
    private DatabaseReference dbReference;

    void Start()
    {
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
        else { StartGameWithBackups(); }
    }

    void LoadWordsFromFirebase()
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        if (currentUser == null) { StartGameWithBackups(); return; }

        dbReference.Child("scores").Child(currentUser.UserId).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled) { StartGameWithBackups(); return; }

            DataSnapshot snapshot = task.Result;
            Dictionary<string, string> uniqueWords = new Dictionary<string, string>();

            if (snapshot.Exists)
            {
                foreach (DataSnapshot child in snapshot.Children)
                {
                    SimpleScoreData data = JsonUtility.FromJson<SimpleScoreData>(child.GetRawJsonValue());
                    if (data != null && !string.IsNullOrEmpty(data.word))
                    {
                        string upperWord = data.word.ToUpper();
                        if (!uniqueWords.ContainsKey(upperWord)) uniqueWords.Add(upperWord, data.language);
                    }
                }
            }

            List<CardData> finalCards = new List<CardData>();
            foreach (var item in uniqueWords) finalCards.Add(new CardData { word = item.Key, language = item.Value });

            if (finalCards.Count >= 2)
            {
                ShuffleList(finalCards);
                StartGame(finalCards.GetRange(0, Mathf.Min(finalCards.Count, 6)));
            }
            else { StartGameWithBackups(); }
        });
    }

    void StartGameWithBackups()
    {
        List<CardData> backupData = new List<CardData>();
        foreach (string s in backupWords) backupData.Add(new CardData { word = s.ToUpper(), language = "English" });
        StartGame(backupData);
    }

    void StartGame(List<CardData> sourceList)
    {
        currentCardDataList = new List<CardData>(sourceList);
        totalPairs = sourceList.Count; matchesFound = 0;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        List<CardData> deck = new List<CardData>();
        foreach (CardData data in sourceList) { deck.Add(data); deck.Add(data); }
        ShuffleList(deck);
        foreach (Transform child in gridContainer) Destroy(child.gameObject);
        foreach (CardData data in deck)
        {
            GameObject cardObj = Instantiate(cardPrefab, gridContainer);
            cardObj.GetComponent<MemoryCard>().Setup(data.word, data.language, this);
        }
    }

    public void CardSelected(MemoryCard card)
    {
        if (!canClick || openCards.Contains(card)) return;
        card.FlipOpen(); openCards.Add(card);
        if (openCards.Count == 2) StartCoroutine(CheckMatch());
    }

    IEnumerator CheckMatch()
    {
        canClick = false; yield return new WaitForSeconds(1.0f);
        if (openCards[0].myWord == openCards[1].myWord)
        {
            openCards[0].MatchFound(); openCards[1].MatchFound();
            matchesFound++; if (matchesFound >= totalPairs) gameOverPanel.SetActive(true);
        }
        else { openCards[0].FlipClose(); openCards[1].FlipClose(); }
        openCards.Clear(); canClick = true;
    }

    public void RestartGame() { StartGame(currentCardDataList); }
    public void BackToMenu() { SceneManager.LoadScene("MiniGameScene"); }
    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i]; int r = Random.Range(i, list.Count);
            list[i] = list[r]; list[r] = temp;
        }
    }
}