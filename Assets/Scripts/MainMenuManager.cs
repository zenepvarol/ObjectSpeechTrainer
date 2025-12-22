using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void SelectLanguage(string languageCode)
    {
        OpenCVForUnityExample.PronunciationGame.CurrentLanguage = languageCode;
        SceneManager.LoadScene("ObjectDetectionYOLOXExample");
    }

    public void OpenMiniGames()
    {
        Debug.Log("Mini Oyunlar Açýlýyor...");
        SceneManager.LoadScene("MiniGameScene");
    }
}