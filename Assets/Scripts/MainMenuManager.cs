using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class MainMenuManager : MonoBehaviour
{
    [Header("Ayarlar Paneli")]
    public GameObject settingsPanel;

    [Header("Emin Misin Paneli")]
    public GameObject confirmationPanel; 

    private DatabaseReference dbReference;

    private void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
            {
                Debug.LogError("Firebase Hatasý: " + task.Result);
            }
        });
    }

    public void SelectLanguage(string languageCode)
    {
        if (OpenCVForUnityExample.PronunciationGame.Instance != null)
            OpenCVForUnityExample.PronunciationGame.CurrentLanguage = languageCode;
        else
            OpenCVForUnityExample.PronunciationGame.CurrentLanguage = languageCode;

        SceneManager.LoadScene("ObjectDetectionYOLOXExample");
    }

    public void OpenMiniGames()
    {
        Debug.Log("Mini Oyunlar Açýlýyor...");
        SceneManager.LoadScene("MiniGameScene");
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýlýyor...");
        Application.Quit();
    }

    public void OpenConfirmation()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
    }

    public void CancelReset()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    public void ConfirmReset()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Yerel veriler silindi.");

        if (dbReference != null)
        {
            dbReference.Child("scores").RemoveValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Firebase veritabaný temizlendi!");
                }
            });
        }

        CancelReset();
    }
}