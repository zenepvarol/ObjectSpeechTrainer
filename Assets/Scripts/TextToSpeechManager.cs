using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class TextToSpeechManager : MonoBehaviour
{
    public static TextToSpeechManager Instance; 
    private AudioSource audioSource;
    private string lastSpokenText = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Speak(string text, string languageFullname)
    {
        if (text == lastSpokenText && audioSource.isPlaying)
            return;

        lastSpokenText = text;

        // "Italian" -> "it" dönüþümünü yap
        string langCode = GetLanguageCode(languageFullname);

        StartCoroutine(DownloadAndPlay(text, langCode));
    }

    IEnumerator DownloadAndPlay(string text, string languageCode)
    {
        string url = $"https://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen=32&client=tw-ob&q={UnityWebRequest.EscapeURL(text)}&tl={languageCode}";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                }
            }
            else
            {
                Debug.LogError($"TTS Hatasý ({languageCode}): " + www.error);
            }
        }
    }

    string GetLanguageCode(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return "en";

        switch (fullName.ToLower())
        {
            case "turkish": return "tr";
            case "english": return "en";
            case "german": return "de";
            case "spanish": return "es";
            case "italian": return "it";
            case "french": return "fr";
            default: return "en"; 
        }
    }
}