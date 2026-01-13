using UnityEngine;
using UnityEngine.UI; // <-- Bak TMPro yok, sadece UI var.

namespace OpenCVForUnityExample
{
    public class DebugEkrani : MonoBehaviour
    {
        public static DebugEkrani Instance;
        public Text yaziKutusu; // <-- TMP_Text DEÐÝL, düz Text

        void Awake()
        {
            Instance = this;
            if (yaziKutusu != null) yaziKutusu.text = "";
        }

        public static void TekCumleYaz(string mesaj, bool hataMi = false)
        {
            if (Instance == null || Instance.yaziKutusu == null) return;

            string renk = hataMi ? "red" : "#008900";
            Instance.yaziKutusu.text = $"<color={renk}>{mesaj}</color>";
        }
    }
}