using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSTT : MonoBehaviour
{
    [TextArea]
    public string apiKey = "AIzaSyCh4QHPxqDdw_pirWQVdgAo3fQoIVGyRZE";  // BURAYA API KEY GELECEK

    private string apiURL =
        "https://speech.googleapis.com/v1p1beta1/speech:recognize?key=";

    public IEnumerator Transcribe(byte[] audioBytes, Action<string> callback)
    {
        Debug.Log("Google STT: Ses gönderiliyor...");

        string base64 = Convert.ToBase64String(audioBytes);

        string json =
        @"{
          'config':{
            'encoding':'LINEAR16',
            'sampleRateHertz':44100,
            'languageCode':'en-US'
          },
          'audio':{
            'content':'" + base64 + @"'
          }
        }";

        byte[] body = Encoding.UTF8.GetBytes(json);

        UnityWebRequest req = new UnityWebRequest(apiURL + apiKey, "POST");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Google STT Error: " + req.error);
            callback?.Invoke("");
            yield break;
        }

        Debug.Log("Google Response: " + req.downloadHandler.text);

        callback?.Invoke(Extract(req.downloadHandler.text));
    }

    private string Extract(string json)
    {
        string key = "\"transcript\": \"";

        int i = json.IndexOf(key);
        if (i < 0) return "";

        int start = i + key.Length;
        int end = json.IndexOf("\"", start);

        return json.Substring(start, end - start);
    }
}
