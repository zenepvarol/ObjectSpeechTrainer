using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoryCard : MonoBehaviour
{
    [Header("UI Baðlantýlarý")]
    public GameObject frontSide; 
    public TMP_Text wordText;   
    public Button button;       

    [HideInInspector]
    public string myWord;       

    private MemoryGameManager gameManager; 

    public void Setup(string word, MemoryGameManager manager)
    {
        myWord = word;
        wordText.text = word;
        gameManager = manager;

        if (frontSide != null)
            frontSide.SetActive(false);
    }

    public void OnCardClicked()
    {
        if (frontSide.activeSelf || gameManager == null) return;

        gameManager.CardSelected(this);

        TextToSpeechManager.Instance.Speak(myWord, OpenCVForUnityExample.PronunciationGame.CurrentLanguage);
    }

    public void FlipOpen()
    {
        if (frontSide != null) frontSide.SetActive(true);
    }

    public void FlipClose()
    {
        if (frontSide != null) frontSide.SetActive(false);
    }

    public void MatchFound()
    {
        if (button != null) button.interactable = false; 
    }

}