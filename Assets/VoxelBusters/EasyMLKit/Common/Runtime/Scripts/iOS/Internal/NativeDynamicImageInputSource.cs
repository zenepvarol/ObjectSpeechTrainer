#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VoxelBusters.EasyMLKit.Implementations.iOS.Internal
{
    public class NativeDynamicImageInputSource
    {
#region Native Bindings

        [DllImport("__Internal")]
        public static extern IntPtr MLKit_CreateDynamicImageInputSourceFeed_InitWithTexture(IntPtr texturePtr, int x, int y, int width, int height);

        [DllImport("__Internal")]
        public static extern void MLKit_DynamicImageInputSourceFeed_OnCameraFrameReceived(IntPtr inputSourcePtr);

#endregion

        public IntPtr NativeHandle
        {
            get;
            private set;
        }

        public NativeDynamicImageInputSource(WebCamTexture webCamTexture, Rect selectedRegion)
        {
            NativeHandle = MLKit_CreateDynamicImageInputSourceFeed_InitWithTexture(webCamTexture.GetNativeTexturePtr(), (int)selectedRegion.x, (int)selectedRegion.y, (int)selectedRegion.width, (int)selectedRegion.height);
        }

        public void OnCameraFrameReceived()
        {
            MLKit_DynamicImageInputSourceFeed_OnCameraFrameReceived(NativeHandle);
        }
    }
}
#endif