using UnityEngine;
using UnityEngine.SceneManagement;

public class MemoryLevelUI : MonoBehaviour
{
    public void BackToLobby()
    {
        SceneManager.LoadScene("MiniGameScene");
    }
}