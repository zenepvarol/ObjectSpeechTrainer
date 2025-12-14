using UnityEngine;
using UnityEngine.SceneManagement;

public class LanguageSelector : MonoBehaviour
{
    // Seçilen dili tutmak için
    public static string selectedLanguage = "Turkish";

    // Türkçe butonuna týklanýnca çaðrýlýr
    public void SelectTurkish()
    {
        selectedLanguage = "Turkish";
        PlayerPrefs.SetString("language", selectedLanguage); // kalýcý kaydet
        PlayerPrefs.Save();
        SceneManager.LoadScene("ARScene"); // AR sahnesine geç
    }

    // Ýngilizce butonuna týklanýnca çaðrýlýr
    public void SelectEnglish()
    {
        selectedLanguage = "English";
        PlayerPrefs.SetString("language", selectedLanguage);
        PlayerPrefs.Save();
        SceneManager.LoadScene("ARScene");
    }

    // Ayarlar butonuna týklanýnca çaðrýlýr
    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }
}
