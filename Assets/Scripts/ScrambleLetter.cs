using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScrambleLetter : MonoBehaviour
{
    public TMP_Text letterText;  // Harfin yazdýðý TextMeshPro
    private char myChar;         // Harfin kendisi
    private WordScrambleManager manager;

    public void Setup(char c, WordScrambleManager gameManager)
    {
        myChar = c;
        letterText.text = c.ToString();
        manager = gameManager;
    }

    public void OnClick()
    {
        if (manager != null)
        {
            manager.LetterClicked(this);
        }
    }

    public void MoveToAnswer(Transform parent)
    {
        transform.SetParent(parent);
    }

    public void MoveToPool(Transform parent)
    {
        transform.SetParent(parent);
    }

    public char GetChar()
    {
        return myChar;
    }
}