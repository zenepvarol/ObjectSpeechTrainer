using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiAPI : MonoBehaviour
{
    public string apiKey = "";

    private string apiUrl =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=";

    public IEnumerator SendAudioToGemini(byte[] wavData, Action<string> callback)
    {
        Debug.Log("Gemini: Ses gönderiliyor...");

        string base64Audio = Convert.ToBase64String(wavData);

        string jsonData = @"
        {
          ""contents"": [
            {
              ""parts"": [
                {
                  ""input_audio"": {
                    ""mime_type"": ""audio/wav"",
                    ""data"": """ + base64Audio + @"""
                  }
                },
                {
                  ""text"": ""Transcribe this audio.""
                }
              ]
            }
          ]
        }";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(apiUrl + apiKey, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Gemini API Error: " + request.error);
            callback?.Invoke("");
            yield break;
        }

        string res = request.downloadHandler.text;
        Debug.Log("Gemini Response: " + res);

        string transcript = ExtractText(res);

        callback?.Invoke(transcript);
    }

    private string ExtractText(string json)
    {
        string key = "\"text\":";
        int i = json.IndexOf(key);
        if (i == -1) return "";

        int start = json.IndexOf("\"", i + key.Length) + 1;
        int end = json.IndexOf("\"", start);

        return json.Substring(start, end - start);
    }
}
