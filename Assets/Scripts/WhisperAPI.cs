using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class WhisperAPI : MonoBehaviour
{
    public IEnumerator SendAudioToWhisper(byte[] audioData, System.Action<string> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("model", "whisper-1");
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");

        UnityWebRequest request = UnityWebRequest.Post(
            "https://api.openai.com/v1/audio/transcriptions",
            form
        );

        request.SetRequestHeader("Authorization", "Bearer " + Keys.OPENAI_API_KEY);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Whisper Error: " + request.error);
            callback?.Invoke(null);
        }
        else
        {
            string json = request.downloadHandler.text;
            Debug.Log("Whisper Result: " + json);

            WhisperResult result = JsonUtility.FromJson<WhisperResult>(json);
            callback?.Invoke(result.text);
        }
    }
}

[System.Serializable]
public class WhisperResult
{
    public string text;
}
