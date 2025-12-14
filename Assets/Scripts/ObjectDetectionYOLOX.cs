using UnityEngine;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.DnnModule;
using System.Collections.Generic;

public class ObjectDetectionYOLOX : MonoBehaviour
{
    WebCamTexture webcamTexture;
    Mat frameMat;
    Texture2D texture;
    Net net;

    string modelPath, classNamesPath;
    List<string> classNames;

    Renderer targetRenderer;

    void Start()
    {
        // Hangi kamerayı kullanacağımızı isme göre seçelim (konsolda gördüğün ismi yaz)
        // Gerekirse "USB webcam" yerine WebCamTexture.devices[i].name kullanabilirsin.
        string camName = null;
        foreach (var cam in WebCamTexture.devices)
        {
            Debug.Log("Kamera bulundu: " + cam.name);
            if (cam.name.Contains("USB")) camName = cam.name;
        }
        if (string.IsNullOrEmpty(camName))
            camName = WebCamTexture.devices.Length > 0 ? WebCamTexture.devices[0].name : null;

        webcamTexture = new WebCamTexture(camName, 640, 480, 30);
        webcamTexture.Play();
        Debug.Log("Kamera başlatılıyor...");


        // Model ve label yolları (senin klasör yapına göre)
        modelPath = Utils.getFilePath("OpenCVForUnityExamples/dnn/yolox_tiny.onnx");
        classNamesPath = Utils.getFilePath("OpenCVForUnityExamples/dnn/coco.names");

        net = Dnn.readNetFromONNX(modelPath);
        Debug.Log("✅ YOLOX modeli yüklendi!");

        classNames = new List<string>(System.IO.File.ReadAllLines(classNamesPath));

        // Renderer referansı (bu script CameraDisplay’in üstünde)
        targetRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (!webcamTexture.isPlaying)
        {
            Debug.Log("Kamera tekrar başlatılıyor...");
            webcamTexture.Play();
        }


        if (webcamTexture == null || !webcamTexture.didUpdateThisFrame) return;

        // 🔸 İlk kare geldiğinde boyutları artık gerçek — o anda texture ve mat oluştur.
        if (texture == null || frameMat == null ||
            texture.width != webcamTexture.width || texture.height != webcamTexture.height)
        {
            int w = Mathf.Max(1, webcamTexture.width);
            int h = Mathf.Max(1, webcamTexture.height);

            frameMat = new Mat(h, w, CvType.CV_8UC3);
            texture = new Texture2D(w, h, TextureFormat.RGBA32, false);

            // Materyalin ana dokusuna bizim texture’ı bağla
            if (targetRenderer != null)
                targetRenderer.material.mainTexture = texture;

            Debug.Log($"Init with camera size: {w}x{h}");
        }

        // Kameradan frame'i Mat'e kopyala
        Utils.webCamTextureToMat(webcamTexture, frameMat);
        Imgproc.cvtColor(frameMat, frameMat, Imgproc.COLOR_RGBA2RGB);

        // Model girişi
        Size inputSize = new Size(416, 416);
        using (Mat blob = Dnn.blobFromImage(frameMat, 1 / 255.0, inputSize, new Scalar(0, 0, 0), true, false))
        {
            net.setInput(blob);
            using (Mat output = net.forward())
            {
                // Basit post-process (confidence & class score eşikleri)
                float confThresh = 0.4f, clsThresh = 0.4f;

                if (output.total() > 0)
                {
                    int rows = output.rows();
                    for (int i = 0; i < rows; i++)
                    {
                        float confidence = (float)output.get(i, 4)[0];
                        if (confidence < confThresh) continue;

                        int classId = -1; float maxClassScore = 0f;
                        for (int c = 5; c < output.cols(); c++)
                        {
                            float s = (float)output.get(i, c)[0];
                            if (s > maxClassScore) { maxClassScore = s; classId = c - 5; }
                        }
                        if (maxClassScore < clsThresh || classId < 0 || classId >= classNames.Count) continue;

                        float cx = (float)(output.get(i, 0)[0] * frameMat.cols());
                        float cy = (float)(output.get(i, 1)[0] * frameMat.rows());
                        float ww = (float)(output.get(i, 2)[0] * frameMat.cols());
                        float hh = (float)(output.get(i, 3)[0] * frameMat.rows());

                        int left = Mathf.Clamp((int)(cx - ww / 2), 0, frameMat.cols() - 1);
                        int top = Mathf.Clamp((int)(cy - hh / 2), 0, frameMat.rows() - 1);
                        int right = Mathf.Clamp(left + (int)ww, 0, frameMat.cols() - 1);
                        int bot = Mathf.Clamp(top + (int)hh, 0, frameMat.rows() - 1);

                        Imgproc.rectangle(frameMat, new Point(left, top), new Point(right, bot), new Scalar(0, 255, 0), 2);
                        Imgproc.putText(frameMat, classNames[classId], new Point(left, Mathf.Max(0, top - 5)),
                                        Imgproc.FONT_HERSHEY_SIMPLEX, 0.8, new Scalar(0, 255, 0), 2);
                    }
                }
            }
        }

        // Ekrana bas
        Utils.matToTexture2D(frameMat, texture);
    }
}
