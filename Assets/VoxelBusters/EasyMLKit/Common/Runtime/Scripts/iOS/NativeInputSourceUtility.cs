#if UNITY_IOS
using System;
using UnityEngine;

namespace VoxelBusters.EasyMLKit.Implementations.iOS.Internal
{
    public static class NativeInputSourceUtility
    {
        public static IntPtr CreateInputSource(IInputSource inputSource)
        {
            if (inputSource is LiveCameraInputSource)
            {
                LiveCameraInputSource input = inputSource as LiveCameraInputSource;
                NativeLiveCameraInputSource nativeInputSource = new NativeLiveCameraInputSource(input.IsFrontFacing, input.EnableFlash, input.MaxResolution, input.Viewport);
                return nativeInputSource.NativeHandle;
            }
            else if (inputSource is ImageInputSource)
            {
                ImageInputSource imageInput = inputSource as ImageInputSource;
                NativeImageInputSource nativeInputSource = new NativeImageInputSource(imageInput.GetBytes());
                return nativeInputSource.NativeHandle;
            }
            else if (inputSource is WebCamTextureInputSource)
            {
                WebCamTextureInputSource input = inputSource as WebCamTextureInputSource;
                Rect selectedRegion = input.SelectedRegion;
                NativeDynamicImageInputSource nativeInputSource = new NativeDynamicImageInputSource(input.Texture, selectedRegion);

                input.CameraFrameReceived += () =>
                {
                    nativeInputSource.OnCameraFrameReceived();
                };

                return nativeInputSource.NativeHandle;
            }
#if EASY_ML_KIT_SUPPORT_AR_FOUNDATION
            else if (inputSource is ARFoundationCameraInputSource)
            {
                ARFoundationCameraInputSource arInputSource = (ARFoundationCameraInputSource)inputSource;
                NativeArKitInputSource nativeInputSource = new NativeArKitInputSource(arInputSource.Session);

                arInputSource.Register();
                arInputSource.CameraFrameReceived += () =>
                {
                    nativeInputSource.OnCameraFrameReceived();
                };

                return nativeInputSource.NativeHandle;
            }
#endif
            else
            {
                throw new Exception("Input source not implemented!");
            }
        }
    }
}
#endif