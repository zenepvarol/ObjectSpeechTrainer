using System;
using System.Collections;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.EasyMLKit
{
    public class WebCamTextureInputSource : IImageInputSource
    {
        private WebCamTexture m_webCamTexture;

        public event Action CameraFrameReceived;

        private float m_width;
        private float m_height;

        public WebCamTexture Texture
        {
            get
            {
                return m_webCamTexture;
            }
        }

        public Texture2D TextureSentForProcessing
        {
            get; set;
            
        }

        public Rect SelectedRegion
        {
            get;
        }


        public WebCamTextureInputSource(WebCamTexture webCamTexture, Rect? selectedRegion = null)
        {
            m_webCamTexture     = webCamTexture;
            m_width             = webCamTexture.width;
            m_height            = webCamTexture.height;
            SelectedRegion      = selectedRegion != null ?  selectedRegion.Value : new Rect(0, 0, m_width, m_height);
            SurrogateCoroutine.StartCoroutine(TriggerFrameUpdate());
        }

        private IEnumerator TriggerFrameUpdate()
        {
            while(true)
            {
                yield return new WaitForEndOfFrame();
                if (m_webCamTexture.didUpdateThisFrame && CameraFrameReceived != null)
                    CameraFrameReceived();
            }
        }

        public void Close()
        {
            SurrogateCoroutine.StopCoroutine(TriggerFrameUpdate());
            m_webCamTexture.Stop();
        }

        public float GetWidth()
        {
            return m_width;
        }

        public float GetHeight()
        {
            return m_height;
        }
    }
}
