using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;

public class GeminiSTT : MonoBehaviour
{
    private const string API_URL =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

    public void Transcribe(string apiKey, byte[] wavBytes, Action<string> onDone)
    {
        StartCoroutine(SendToGemini(apiKey, wavBytes, onDone));
    }

    IEnumerator SendToGemini(string apiKey, byte[] wavBytes, Action<string> onDone)
    {
        string base64 = Convert.ToBase64String(wavBytes);

        string json = $@"
        {{
          ""contents"": [
            {{
              ""parts"": [
                {{ ""text"": ""Transcribe this audio. Output only the spoken word."" }},
                {{
                  ""inline_data"": {{
                    ""mime_type"": ""audio/wav"",
                    ""data"": ""{base64}""
                  }}
                }}
              ]
            }}
          ]
        }}";

        Debug.Log("Gönderilen JSON:\n" + json);

        string url = $"{API_URL}?key={apiKey}";

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Gemini API Hatası: " + req.error);
            Debug.LogError("Status Code: " + req.responseCode);
            Debug.LogError("Response body: " + req.downloadHandler.text);
            onDone?.Invoke(null);
            yield break;
        }

        string response = req.downloadHandler.text;
        Debug.Log("Gemini Response: " + response);

        string text = Parse(response);
        onDone?.Invoke(text);
    }

    private string Parse(string json)
    {
        try
        {
            const string key = "\"text\": \"";
            int idx = json.IndexOf(key);
            if (idx == -1) return null;

            idx += key.Length;
            int end = json.IndexOf("\"", idx);

            return json.Substring(idx, end - idx)
                       .Replace("\\n", " ")
                       .Trim();
        }
        catch
        {
            return null;
        }
    }
}
