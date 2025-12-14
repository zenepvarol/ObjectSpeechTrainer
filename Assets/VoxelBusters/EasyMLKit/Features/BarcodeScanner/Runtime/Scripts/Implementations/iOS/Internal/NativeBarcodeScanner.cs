#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.EasyMLKit.Implementations.iOS.Internal
{
    internal class NativeBarcodeScanner
    {
        #region Native Bindings

        [DllImport("__Internal")]
        public static extern IntPtr MLKit_BarcodeScanner_Init();

        [DllImport("__Internal")]
        public static extern void MLKit_BarcodeScanner_SetListener(IntPtr barcodeScannerPtr,
                                      IntPtr listenerTag,
                                      NativeBarcodeScannerListener.BarcodeScannerPrepareSuccessNativeCallback prepareSuccessCallback,
                                      NativeBarcodeScannerListener.BarcodeScannerPrepareFailedNativeCallback prepareFailedCallback,
                                      NativeBarcodeScannerListener.BarcodeScannerScanSuccessNativeCallback scanSuccessCallback,
                                      NativeBarcodeScannerListener.BarcodeScannerScanFailedNativeCallback scanFailedCallback);
        [DllImport("__Internal")]
        public static extern void MLKit_BarcodeScanner_Prepare(IntPtr barcodeScannerPtr, IntPtr inputSourcePtr, IntPtr barcodeScanOptionsPtr);

        [DllImport("__Internal")]
        public static extern void MLKit_BarcodeScanner_Process(IntPtr barcodeScannerPtr);

        [DllImport("__Internal")]
        public static extern void MLKit_BarcodeScanner_Close(IntPtr barcodeScannerPtr);


        #endregion

        #region Fields

        IntPtr m_nativeHandle;

        #endregion

        #region Constructor

        public NativeBarcodeScanner()
        {
            m_nativeHandle = MLKit_BarcodeScanner_Init();
        }

        #endregion

        #region Public methods

        public void Close()
        {
            MLKit_BarcodeScanner_Close(m_nativeHandle);
        }

        public void Prepare(IntPtr inputSource, NativeBarcodeScanOptions options)
        {
            MLKit_BarcodeScanner_Prepare(m_nativeHandle, inputSource, options.NativeHandle);
        }
        public void Process()
        {
            MLKit_BarcodeScanner_Process(m_nativeHandle);
        }
        public void SetListener(NativeBarcodeScannerListener listener)
        {
            MLKit_BarcodeScanner_SetListener(m_nativeHandle, listener.NativeHandle, NativeBarcodeScannerListener.PrepareSuccessNativeCallback,
                                                    NativeBarcodeScannerListener.PrepareFailedNativeCallback,
                                                    NativeBarcodeScannerListener.ScanSuccessNativeCallback,
                                                    NativeBarcodeScannerListener.ScanFailedNativeCallback);
        }

        #endregion
    }
}
#endif