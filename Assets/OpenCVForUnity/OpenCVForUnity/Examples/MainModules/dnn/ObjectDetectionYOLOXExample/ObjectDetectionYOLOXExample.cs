#if !UNITY_WSA_10_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityIntegration;
using OpenCVForUnity.UnityIntegration.Helper.Source2Mat;
using OpenCVForUnity.UnityIntegration.Worker.DnnModule;

namespace OpenCVForUnityExample
{
    [RequireComponent(typeof(MultiSource2MatHelper))]
    public class ObjectDetectionYOLOXExample : MonoBehaviour
    {
        [Header("Output")]
        public RawImage ResultPreview;
        [Header("UI")]
        public Toggle UseAsyncInferenceToggle;
        public bool UseAsyncInference = false;
        [Header("Model Settings")]
        public string Model = "OpenCVForUnityExamples/dnn/yolox_tiny.onnx";
        public float ConfThreshold = 0.25f;
        public float NmsThreshold = 0.45f;
        public int TopK = 300;
        public int InpWidth = 416;
        public int InpHeight = 416;

        private YOLOXObjectDetector detector;
        private string _classesFilepath;
        private string _modelFilepath;
        private string[] labels;
        private Texture2D _texture;
        private MultiSource2MatHelper _multiSource2MatHelper;
        private Mat _bgrMat;
        private FpsMonitor _fpsMonitor;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private AudioSource audioSource;
        private string cacheDir;
        private bool ttsRequestInProgress = false;
        private float ttsBackoffUntil = -1f;

        private string currentCandidate = "";
        private int consecutiveFrames = 0;
        private int requiredStableFrames = 8;
        private readonly Dictionary<string, float> lastPlayedTimes = new();
        private float perLabelCooldown = 5f;
        private float globalCooldown = 1.5f;
        private float lastAnySpokenTime = -999f;

        private static readonly Dictionary<string, string> ttsLangCodes = new Dictionary<string, string>()
        {
            {"English", "en"}, {"Turkish", "tr"}, {"German", "de"}, {"French", "fr"},
            {"Spanish", "es"}, {"Italian", "it"}, {"Japanese", "ja"}, {"Korean", "ko"},
        };

        private async void Start()
        {
            _fpsMonitor = GetComponent<FpsMonitor>();
            _multiSource2MatHelper = GetComponent<MultiSource2MatHelper>();
            _multiSource2MatHelper.OutputColorFormat = Source2MatHelperColorFormat.RGBA;
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

#if !UNITY_WEBGL || UNITY_EDITOR
            if (UseAsyncInferenceToggle != null) UseAsyncInferenceToggle.isOn = UseAsyncInference;
#else
            if (UseAsyncInferenceToggle != null) { UseAsyncInferenceToggle.isOn = false; UseAsyncInferenceToggle.interactable = false; }
#endif

            string selectedLang = PlayerPrefs.GetString("SelectedLanguage", "English");
            switch (selectedLang)
            {
                case "Turkish": _classesFilepath = await OpenCVEnv.GetFilePathTaskAsync("OpenCVForUnityExamples/dnn/coco_tr.names", cancellationToken: _cts.Token); break;
                case "German": _classesFilepath = await OpenCVEnv.GetFilePathTaskAsync("OpenCVForUnityExamples/dnn/coco_de.names", cancellationToken: _cts.Token); break;
                case "Italian": _classesFilepath = await OpenCVEnv.GetFilePathTaskAsync("OpenCVForUnityExamples/dnn/coco_it.names", cancellationToken: _cts.Token); break;
                case "Spanish": _classesFilepath = await OpenCVEnv.GetFilePathTaskAsync("OpenCVForUnityExamples/dnn/coco_es.names", cancellationToken: _cts.Token); break;
                default: _classesFilepath = await OpenCVEnv.GetFilePathTaskAsync("OpenCVForUnityExamples/dnn/coco.names", cancellationToken: _cts.Token); break;
            }

            _modelFilepath = await OpenCVEnv.GetFilePathTaskAsync(Model, cancellationToken: _cts.Token);
            if (!string.IsNullOrEmpty(_classesFilepath) && File.Exists(_classesFilepath)) labels = File.ReadAllLines(_classesFilepath, Encoding.UTF8);
            else labels = new string[0];

            cacheDir = Path.Combine(Application.persistentDataPath, "tts_cache");
            if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);

            Run();
        }

        private void Run()
        {
            if (string.IsNullOrEmpty(_modelFilepath)) return;
            detector = new YOLOXObjectDetector(_modelFilepath, _classesFilepath, new Size(InpWidth, InpHeight), ConfThreshold, NmsThreshold, TopK);
            _multiSource2MatHelper.Initialize();
        }

        public void OnSourceToMatHelperInitialized()
        {
            Mat rgbaMat = _multiSource2MatHelper.GetMat();
            _texture = new Texture2D(rgbaMat.cols(), rgbaMat.rows(), TextureFormat.RGBA32, false);
            OpenCVMatUtils.MatToTexture2D(rgbaMat, _texture);
            ResultPreview.texture = _texture;
            ResultPreview.GetComponent<AspectRatioFitter>().aspectRatio = (float)_texture.width / _texture.height;
            _bgrMat = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC3);
        }

        private void Update()
        {
            if (_multiSource2MatHelper.IsPlaying() && _multiSource2MatHelper.DidUpdateThisFrame())
            {
                Mat rgbaMat = _multiSource2MatHelper.GetMat();
                if (detector != null)
                {
                    Imgproc.cvtColor(rgbaMat, _bgrMat, Imgproc.COLOR_RGBA2BGR);
                    using (Mat objects = detector.Detect(_bgrMat))
                    {
                        Imgproc.cvtColor(_bgrMat, rgbaMat, Imgproc.COLOR_BGR2RGBA);
                        detector.Visualize(rgbaMat, objects, false, true);

                        string label = TryGetTopLabel(objects);
                        if (!string.IsNullOrEmpty(label))
                        {
                            if (label == currentCandidate) consecutiveFrames++;
                            else { currentCandidate = label; consecutiveFrames = 1; }

                            if (consecutiveFrames >= requiredStableFrames)
                            {
                                if (Time.time - lastAnySpokenTime >= globalCooldown && (!lastPlayedTimes.ContainsKey(label) || Time.time - lastPlayedTimes[label] >= perLabelCooldown))
                                {
                                    lastAnySpokenTime = Time.time;
                                    lastPlayedTimes[label] = Time.time;
                                }
                                consecutiveFrames = requiredStableFrames;
                            }
                        }
                        else { currentCandidate = ""; consecutiveFrames = 0; }
                    }
                }
                OpenCVMatUtils.MatToTexture2D(rgbaMat, _texture);
            }
        }

        private string TryGetTopLabel(Mat objects)
        {
            if (objects == null || objects.empty() || labels == null || labels.Length == 0) return null;
            try
            {
                int rows = objects.rows();
                int bestIdx = -1;
                double bestConf = 0.0;
                for (int i = 0; i < rows; i++)
                {
                    double[] classRaw = objects.get(i, 5);
                    double[] confRaw = objects.get(i, 4);
                    if (classRaw == null || confRaw == null) continue;
                    int labelId = (int)classRaw[0];
                    if (labelId < 0 || labelId >= labels.Length) continue;
                    if (labels[labelId] != "person" && confRaw[0] > bestConf) { bestConf = confRaw[0]; bestIdx = i; }
                }
                if (bestIdx >= 0) return labels[(int)objects.get(bestIdx, 5)[0]];
                return labels[(int)objects.get(0, 5)[0]];
            }
            catch { return null; }
        }

        private IEnumerator PlayPronunciation(string word, string langCode)
        {
            if (ttsRequestInProgress) yield break;
            ttsRequestInProgress = true;
            string cachePath = Path.Combine(cacheDir, MakeSafeFileName($"{word}_{langCode}.mp3"));

            if (!File.Exists(cachePath) && Time.time >= ttsBackoffUntil)
            {
                string url = $"https://translate.google.com/translate_tts?ie=UTF-8&q={UnityWebRequest.EscapeURL(word)}&tl={langCode}&client=tw-ob";
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
                {
                    www.SetRequestHeader("User-Agent", "Mozilla/5.0");
                    yield return www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.Success) File.WriteAllBytes(cachePath, www.downloadHandler.data);
                    else if (www.responseCode == 429) ttsBackoffUntil = Time.time + 60f;
                }
            }

            if (File.Exists(cachePath))
            {
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + cachePath.Replace("\\", "/"), AudioType.MPEG))
                {
                    yield return www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                        if (clip) { audioSource.clip = clip; audioSource.Play(); }
                    }
                }
            }
            ttsRequestInProgress = false;
        }

        private static string MakeSafeFileName(string name) { foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_'); return name; }

        public void OnSpeakButton()
        {
            if (string.IsNullOrEmpty(currentCandidate)) return;
            if (PronunciationGame.Instance != null) PronunciationGame.Instance.SetTargetWord(currentCandidate);

            string selectedLang = PlayerPrefs.GetString("SelectedLanguage", "English");
            string langCode = ttsLangCodes.ContainsKey(selectedLang) ? ttsLangCodes[selectedLang] : "en";
            StartCoroutine(PlayPronunciation(currentCandidate, langCode));
        }

        public void OnBackButtonClick() => SceneManager.LoadScene("MainMenu");
        public void OnPlayButtonClick() => _multiSource2MatHelper.Play();
        public void OnPauseButtonClick() => _multiSource2MatHelper.Pause();
        public void OnChangeCameraButtonClick() => _multiSource2MatHelper.RequestedIsFrontFacing = !_multiSource2MatHelper.RequestedIsFrontFacing;
    }
}
#endif