using UnityEngine;
using UnityEngine.SceneManagement;

public class LanguageMenu : MonoBehaviour
{
    public void SelectEnglish()
    {
        SetLanguage("English");
    }

    public void SelectTurkish()
    {
        SetLanguage("Turkish");
    }

    public void SelectGerman()
    {
        SetLanguage("German");
    }

    public void SelectItalian()
    {
        SetLanguage("Italian");
    }

    public void SelectSpanish()
    {
        SetLanguage("Spanish");
    }

    private void SetLanguage(string language)
    {
        PlayerPrefs.SetString("SelectedLanguage", language);
        PlayerPrefs.Save();
        SceneManager.LoadScene("ObjectDetectionYOLOXExample");
    }
}
