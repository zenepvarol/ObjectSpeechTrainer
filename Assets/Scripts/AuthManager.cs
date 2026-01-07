using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System;

public class AuthManager : MonoBehaviour
{
    [Header("UI Baðlantýlarý")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text feedbackText;

    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        // Oturum açýksa ana menüye git (Ýstersen burayý kapatabilirsin)
        if (auth.CurrentUser != null)
        {
            Debug.Log("Oturum zaten açýk.");
            LoadMainMenu();
        }
    }

    // --- KAYIT OL ---
    public void RegisterUser()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UpdateFeedback("Lütfen e-posta ve þifre giriniz.");
            return;
        }

        UpdateFeedback("Kayýt olunuyor...");

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled) return;

            if (task.IsFaulted)
            {
                string errorMsg = GetErrorMessage(task.Exception);
                UpdateFeedback(errorMsg); // Ekrana yaz
                Debug.LogWarning("Kayýt Bilgisi: " + errorMsg); // Kýrmýzý hata YAKMAZ, sadece sarý uyarý verir.
                return;
            }

            UpdateFeedback("Kayýt Baþarýlý! Giriþ yapýlýyor...");
            LoadMainMenu();
        });
    }

    // --- GÝRÝÞ YAP ---
    public void LoginUser()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UpdateFeedback("Lütfen bilgileri giriniz.");
            return;
        }

        UpdateFeedback("Kontrol ediliyor...");

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled) return;

            if (task.IsFaulted)
            {
                // BURASI DEÐÝÞTÝ: Artýk LogError deðil, sadece ekrana yazýyoruz.
                string errorMsg = GetErrorMessage(task.Exception);

                // 1. Ekrana o mesajý bas ("Böyle bir kullanýcý bulunamadý" vs.)
                UpdateFeedback(errorMsg);

                // 2. Konsola kýrmýzý hata basma, sadece sarý uyarý bas (Developer görsün diye)
                Debug.LogWarning("Giriþ Denemesi Baþarýsýz: " + errorMsg);

                // 3. Kullanýcýdan tekrar giriþ yapmasýný bekle (Hiçbir þey yapmana gerek yok, buton orada duruyor)
                return;
            }

            Debug.Log("Giriþ Baþarýlý.");
            UpdateFeedback("Giriþ Baþarýlý! Yönlendiriliyorsunuz...");
            LoadMainMenu();
        });
    }

    private string GetErrorMessage(Exception exception)
    {
        FirebaseException firebaseEx = exception.GetBaseException() as FirebaseException;
        if (firebaseEx != null)
        {
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            switch (errorCode)
            {
                case AuthError.UserNotFound:
                    return "Böyle bir kullanýcý yok! Lütfen önce kayýt olun."; // Senin istediðin mesaj
                case AuthError.WrongPassword:
                    return "Þifre yanlýþ! Lütfen tekrar deneyin.";
                case AuthError.EmailAlreadyInUse:
                    return "Bu e-posta zaten kayýtlý.";
                case AuthError.InvalidEmail:
                    return "Geçersiz e-posta adresi.";
                case AuthError.WeakPassword:
                    return "Þifre çok kýsa (en az 6 karakter).";
                case AuthError.MissingEmail:
                    return "E-posta yazmayý unuttunuz.";
                default:
                    return "Bir hata oluþtu. Lütfen tekrar deneyin.";
            }
        }
        return "Baðlantý hatasý.";
    }

    void UpdateFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            // Ýstersen metni kýrmýzý yapabilirsin dikkat çekmesi için:
            feedbackText.color = Color.red;
        }
    }

    void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}