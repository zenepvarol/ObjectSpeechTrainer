#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.EasyMLKit.Implementations.iOS.Internal
{
    public class NativeLiveCameraInputSource
    {
#region Native Bindings

        [DllImport("__Internal")]
        public static extern IntPtr MLKit_LiveCameraInputSourceFeed_Init(bool isFrontFacingMode, bool enableFlashLight, NativeSize maxResolution, UnityRect viewport);

#endregion

        public IntPtr NativeHandle
        {
            get;
            private set;
        }

        public NativeLiveCameraInputSource(bool isFrontFacingMode = false, bool enableFlashLight = false, Vector2 maxResolution = default(Vector2) , Rect viewport = default(Rect))
        {
            NativeHandle = MLKit_LiveCameraInputSourceFeed_Init(isFrontFacingMode, enableFlashLight,new NativeSize(maxResolution.x, maxResolution.y),(UnityRect)viewport);
        }
    }
}
#endif