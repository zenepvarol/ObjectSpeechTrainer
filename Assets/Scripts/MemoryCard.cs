using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoryCard : MonoBehaviour
{
    public GameObject frontSide;
    public TMP_Text wordText;
    public Button button;

    [HideInInspector] public string myWord;
    [HideInInspector] public string myLanguage;

    private MemoryGameManager gameManager;

    public void Setup(string word, string language, MemoryGameManager manager)
    {
        myWord = word;
        myLanguage = language;
        wordText.text = word;
        gameManager = manager;
        if (frontSide != null) frontSide.SetActive(false);
    }

    public void OnCardClicked()
    {
        if (frontSide.activeSelf || gameManager == null) return;
        gameManager.CardSelected(this);

        // KARTIN KENDÝ DÝLÝNDE KONUÞMASI BURASI:
        if (TextToSpeechManager.Instance != null)
            TextToSpeechManager.Instance.Speak(myWord, myLanguage);
    }

    public void FlipOpen() { if (frontSide != null) frontSide.SetActive(true); }
    public void FlipClose() { if (frontSide != null) frontSide.SetActive(false); }
    public void MatchFound() { if (button != null) button.interactable = false; }
}