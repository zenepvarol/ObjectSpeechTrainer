using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using System.Collections.Generic;
using Rect = UnityEngine.Rect;


public class TurkishTextDrawer : MonoBehaviour
{
    public Font turkishFont;
    private Texture2D textTexture;
    private Material textMaterial;
    private GUIStyle style;

    void Awake()
    {
        style = new GUIStyle();
        style.font = turkishFont;
        style.fontSize = 32;
        style.normal.textColor = Color.magenta; // Pembe yazý (OpenCV'nin orijinal rengi gibi)
    }

    /// <summary>
    /// Belirtilen yazýyý OpenCV mat üstüne tam Türkçe karakter desteðiyle çizer.
    /// </summary>
    public void DrawTurkish(Mat img, string text, int x, int y)
    {
        // Texture2D oluþtur
        if (textTexture == null || textTexture.width != img.cols() || textTexture.height != img.rows())
        {
            textTexture = new Texture2D(img.cols(), img.rows(), TextureFormat.RGBA32, false);
        }

        // Boþ bir render texture oluþtur
        RenderTexture rt = new RenderTexture(img.cols(), img.rows(), 24);
        RenderTexture.active = rt;

        // Kamerasýz GUI çizimi
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, img.cols(), img.rows(), 0);
        GUI.Label(new Rect(x, y - 30, 1000, 100), text, style);
        GL.PopMatrix();

        textTexture.ReadPixels(new Rect(0, 0, img.cols(), img.rows()), 0, 0);
        textTexture.Apply();

        // Texture2D'yi OpenCV mat'e çevir
        Utils.texture2DToMat(textTexture, img);
        RenderTexture.active = null;
        rt.Release();
    }
}
