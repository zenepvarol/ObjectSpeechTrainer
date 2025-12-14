using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TranslationUI : MonoBehaviour
{
    public TextMeshProUGUI englishText;
    public TextMeshProUGUI translatedText;
    public TextMeshProUGUI sentenceText;

    private Dictionary<string, string> turkishTranslations = new Dictionary<string, string>()
    {
        {"scissors", "makas"},
        {"apple", "elma"},
        {"person", "kiþi"},
        {"cup", "fincan"},
        {"bottle", "þiþe"}
    };

    private Dictionary<string, string> exampleSentences = new Dictionary<string, string>()
    {
        {"scissors", "I use scissors to cut paper."},
        {"apple", "The apple is red and sweet."},
        {"person", "The person is smiling."},
        {"cup", "She drinks tea from a cup."},
        {"bottle", "There is water in the bottle."}
    };

    void Start()
    {
        // Örnek olarak varsayýlan metin
        englishText.text = "Object: ";
        translatedText.text = "";
        sentenceText.text = "";
    }

    public void UpdateDetectedObject(string detected)
    {
        englishText.text = "Object: " + detected;

        string selectedLanguage = PlayerPrefs.GetString("SelectedLanguage", "English");
        if (selectedLanguage == "Turkish" && turkishTranslations.ContainsKey(detected))
        {
            translatedText.text = "Türkçesi: " + turkishTranslations[detected];
            sentenceText.text = "Örnek: " + exampleSentences[detected];
        }
        else
        {
            translatedText.text = "";
            sentenceText.text = exampleSentences.ContainsKey(detected)
                ? "Example: " + exampleSentences[detected]
                : "";
        }
    }
}
