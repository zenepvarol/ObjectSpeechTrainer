using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TextToSpeechManager : MonoBehaviour
{
    private AudioSource audioSource;
    private string lastSpokenText = "";

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Speak(string text, string languageCode = "en")
    {
        // Ayný kelimeyi üst üste tekrarlamasýn
        if (text == lastSpokenText && audioSource.isPlaying)
            return;

        lastSpokenText = text;
        StartCoroutine(DownloadAndPlay(text, languageCode));
    }

    IEnumerator DownloadAndPlay(string text, string languageCode)
    {
        // Google Translate Text-to-Speech servisi (ücretsiz)
        string url = $"https://translate.google.com/translate_tts?ie=UTF-8&q={UnityWebRequest.EscapeURL(text)}&tl={languageCode}&client=tw-ob";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("TTS hatasý: " + www.error);
            }
        }
    }
}
