using UnityEngine;
using UnityEngine.UI; // <-- Bak SADECE UI var, TMPro YOK!

namespace OpenCVForUnityExample
{
    public class DebugEkrani : MonoBehaviour
    {
        public static DebugEkrani Instance;
        public Text yaziKutusu; // <-- Burada da Text var, TMP yok.

        void Awake()
        {
            Instance = this;
            // Eðer kutu boþsa hata verme, sadece geç
            if (yaziKutusu != null) yaziKutusu.text = "";
        }

        public static void TekCumleYaz(string mesaj, bool hataMi = false)
        {
            if (Instance == null || Instance.yaziKutusu == null) return;

            string renk = hataMi ? "red" : "#00FF00";
            Instance.yaziKutusu.text = $"<color={renk}>{mesaj}</color>";
        }
    }
}