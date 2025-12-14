using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.NativePlugins.DemoKit;
using VoxelBusters.EasyMLKit.Internal;
#if EASY_ML_KIT_SUPPORT_AR_FOUNDATION
using UnityEngine.XR.ARFoundation;
#endif

// internal namespace
namespace VoxelBusters.EasyMLKit.Demo
{
    public class TextRecognizerDemo : DemoActionPanelBase<TextRecognizerDemoAction, TextRecognizerDemoActionType>
    {

        [SerializeField]
        private RawImage m_rawImage;

        private WebCamTextureInputSource inputSourceWebCam;

        private WebCamTexture m_webCamTexture;

        #region Base class methods

        protected override void OnActionSelectInternal(TextRecognizerDemoAction selectedAction)
        {
            switch (selectedAction.ActionType)
            {
                case TextRecognizerDemoActionType.ScanTextFromImage:
                    ScanTextFromImage();
                    break;

                case TextRecognizerDemoActionType.ScanTextFromLiveCamera:
                    ScanTextFromLiveCamera();
                    //ScanTextFromWebCamTexture();
                    break;

                case TextRecognizerDemoActionType.ScanTextFromARCamera:
                    ScanTextFromARCamera();
                    break;

                case TextRecognizerDemoActionType.ResourcePage:
                    ProductResources.OpenResourcePage(NativeFeatureType.kTextRecognizer);
                    break;

                default:
                    break;
            }
        }

        #endregion

        #region Usecases methods

        private void ScanTextFromImage()
        {
            IImageInputSource inputSource = CreateImageInputSource(DemoResources.GetRandomImage());
            TextRecognizerOptions options = CreateTextRecognizerOptions();
            Scan(inputSource, options);
        }

        private void ScanTextFromLiveCamera()
        {
            IImageInputSource inputSource = CreateLiveCameraInputSource();
            TextRecognizerOptions options = CreateTextRecognizerOptions();
            Scan(inputSource, options);
        }

        private void ScanTextFromWebCamTexture()
        {
            m_webCamTexture = CreateWebCamTexture(Screen.width, Screen.height);

            StartCoroutine(WaitForWebCamTextureReadyStatus(m_webCamTexture, () =>
            {
                IImageInputSource inputSource = new WebCamTextureInputSource(m_webCamTexture);
                TextRecognizerOptions options = CreateTextRecognizerOptions();
                inputSourceWebCam = (WebCamTextureInputSource)inputSource;
                Scan(inputSource, options);
                SetWebCamTexturePreview(m_webCamTexture);
            }));
        }

        private IEnumerator WaitForWebCamTextureReadyStatus(WebCamTexture webCamTexture, Action onReady)
        {
            // Waiting until first frame is received so that we have proper texture size and data.
            while(webCamTexture.width <= 16)
            {
                yield return null;
            }

            onReady();
        }

        private WebCamTexture CreateWebCamTexture(int width, int height)
        {
            WebCamTexture webCamTexture = new WebCamTexture(requestedWidth: width, requestedHeight: height);
            webCamTexture.Play();
            return webCamTexture;
        }

        private void ScanTextFromARCamera()
        {
#if EASY_ML_KIT_SUPPORT_AR_FOUNDATION
            IImageInputSource inputSource = CreateARCameraInputSource();//Now we use live camera as input source
            TextRecognizerOptions options = CreateTextRecognizerOptions();
            Scan(inputSource, options);
#else
            Log("AR Foundation support not enabled. Add EASY_ML_KIT_SUPPORT_AR_FOUNDATION scripting define if you want to use AR Foundation camera");
#endif
        }

        #endregion

        #region Utility methods

        private IImageInputSource CreateImageInputSource(Texture2D texture)
        {
            return new ImageInputSource(texture);
        }

        private IImageInputSource CreateLiveCameraInputSource()
        {
            LiveCameraInputSource inputSource = new LiveCameraInputSource();
            inputSource.Viewport = new Rect(0, 0.15f, 1f, 0.7f);
            inputSource.MaxResolution = new Vector2(1920, 1080); //Camera resolution (provided in landscape orientation) will try to pick a resolution starting from this value and goes lower.
            return inputSource;
        }

#if EASY_ML_KIT_SUPPORT_AR_FOUNDATION
        private IImageInputSource CreateARCameraInputSource()
        {
            ARSession arSession = FindObjectOfType<ARSession>();
            ARCameraManager arCameraManager = FindObjectOfType<ARCameraManager>();
            IImageInputSource inputSource = new ARFoundationCameraInputSource(arSession, arCameraManager);
            return inputSource;
        }
#endif

        private TextRecognizerOptions CreateTextRecognizerOptions()
        {
            TextRecognizerOptions.Builder builder = new TextRecognizerOptions.Builder();
            builder.SetInputLanguage(TextRecognizerInputLanguage.Latin);
            return builder.Build();
        }

        private void Scan(IImageInputSource inputSource, TextRecognizerOptions options)
        {
            TextRecognizer scanner = new TextRecognizer();
            Debug.Log("Starting prepare...");
            scanner.Prepare(inputSource, options, OnPrepareComplete);
        }

        private void OnPrepareComplete(TextRecognizer scanner, Error error)
        {
            Debug.Log("Prepare complete..." + error);
            if (error == null)
            {
                Log("Prepare completed successfully!");
                scanner.Process(OnProcessUpdate);
            }
            else
            {
                Log("Failed preparing Text Recognizer : " + error.Description);
            }
        }

        private void OnProcessUpdate(TextRecognizer scanner, TextRecognizerResult result)
        {
            if (!result.HasError())
            {
                TextGroup textGroup = result.TextGroup;
                if (textGroup != null)
                {
                    ObjectOverlayController.Instance.ClearAll();
                    if (textGroup.Blocks != null)
                    {
                        Log(string.Format("Text : {0}", result.TextGroup.Text), false);

                        foreach (TextGroup.Block each in textGroup.Blocks)
                        {
                            ObjectOverlayController.Instance.ShowOverlay(each.BoundingBox, string.Format("{0}", each.Text));
                        }
                    }
                    //scanner.Close(null);
                }
            }
            else
            {
                Log("Text Recognizer failed processing : " + result.Error.Description, false);
            }
        }

        private void SetWebCamTexturePreview(WebCamTexture camTexture)
        {
            m_rawImage.texture = camTexture;
            m_rawImage.transform.localScale = new Vector3(1f, camTexture.videoVerticallyMirrored ? -1f : 1f, 1f);
            m_rawImage.transform.localEulerAngles = new Vector3(0, 0, -camTexture.videoRotationAngle);
        }

        #endregion
    }
}
