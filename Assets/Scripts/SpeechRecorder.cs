using UnityEngine;
using System;
using System.IO;

public class SpeechRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    public int sampleRate = 44100;
    public int maxSeconds = 10;

    private string deviceName;
    private AudioClip recordingClip;
    private float startTime;

    void Awake()
    {
        if (Microphone.devices.Length > 0)
        {
            deviceName = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("Mikrofon bulunamadý!");
        }
    }

    public void StartRecording()
    {
        if (string.IsNullOrEmpty(deviceName)) return;

        // loop=false, maxSeconds buffer
        recordingClip = Microphone.Start(deviceName, false, maxSeconds, sampleRate);
        startTime = Time.time;
        Debug.Log("Kayýt baþladý...");
    }

    public byte[] StopRecordingToWavBytes()
    {
        if (string.IsNullOrEmpty(deviceName)) return null;
        if (!Microphone.IsRecording(deviceName)) return null;
        if (recordingClip == null) return null;

        Microphone.End(deviceName);

        float duration = Time.time - startTime;
        duration = Mathf.Clamp(duration, 0.1f, maxSeconds);

        Debug.Log($"Kayýt bitti. Süre: {duration} sn");

        return AudioClipToWav(recordingClip, duration);
    }

    private byte[] AudioClipToWav(AudioClip clip, float duration)
    {
        int channels = clip.channels;
        int frequency = clip.frequency;

        int sampleCount = Mathf.Min(
            (int)(duration * frequency * channels),
            clip.samples * channels
        );

        float[] samples = new float[sampleCount];
        clip.GetData(samples, 0);

        using (var ms = new MemoryStream())
        {
            WriteWavHeader(ms, sampleCount, frequency, channels);
            WriteWavData(ms, samples);
            return ms.ToArray();
        }
    }

    private void WriteWavHeader(Stream stream, int sampleCount, int frequency, int channels)
    {
        int byteRate = frequency * channels * 2; // 16-bit
        int dataSize = sampleCount * 2;

        // RIFF header
        stream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(36 + dataSize), 0, 4);
        stream.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, 4);

        // fmt chunk
        stream.Write(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4);                // PCM chunk size
        stream.Write(BitConverter.GetBytes((ushort)1), 0, 2);         // PCM format
        stream.Write(BitConverter.GetBytes((ushort)channels), 0, 2);
        stream.Write(BitConverter.GetBytes(frequency), 0, 4);
        stream.Write(BitConverter.GetBytes(byteRate), 0, 4);
        stream.Write(BitConverter.GetBytes((ushort)(channels * 2)), 0, 2); // block align
        stream.Write(BitConverter.GetBytes((ushort)16), 0, 2);              // bits per sample

        // data chunk
        stream.Write(System.Text.Encoding.ASCII.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(dataSize), 0, 4);
    }

    private void WriteWavData(Stream stream, float[] samples)
    {
        // float [-1..1] -> int16
        for (int i = 0; i < samples.Length; i++)
        {
            short s = (short)(Mathf.Clamp(samples[i], -1f, 1f) * 32767);
            stream.Write(BitConverter.GetBytes(s), 0, 2);
        }
    }
}
