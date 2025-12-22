using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameMenuManager : MonoBehaviour
{
    public void OpenMemoryGame()
    {
        SceneManager.LoadScene("MemoryGameScene");
    }

    public void OpenOtherGame()
    {
        Debug.Log("Bu oyun daha yapýlmadý!");
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void OpenWordScramble()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("WordScrambleScene");
    }
}