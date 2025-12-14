#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.EasyMLKit.Implementations.iOS.Internal
{
    internal class NativeTextRecognizer
    {
        #region Native Bindings

        [DllImport("__Internal")]
        public static extern IntPtr MLKit_TextRecognizer_Init();

        [DllImport("__Internal")]
        public static extern void MLKit_TextRecognizer_SetListener(IntPtr TextRecognizerPtr,
                                      IntPtr listenerTag,
                                      NativeTextRecognizerListener.TextRecognizerPrepareSuccessNativeCallback prepareSuccessCallback,
                                      NativeTextRecognizerListener.TextRecognizerPrepareFailedNativeCallback prepareFailedCallback,
                                      NativeTextRecognizerListener.TextRecognizerScanSuccessNativeCallback scanSuccessCallback,
                                      NativeTextRecognizerListener.TextRecognizerScanFailedNativeCallback scanFailedCallback);
        [DllImport("__Internal")]
        public static extern void MLKit_TextRecognizer_Prepare(IntPtr TextRecognizerPtr, IntPtr inputSourcePtr, IntPtr barcodeScanOptionsPtr);

        [DllImport("__Internal")]
        public static extern void MLKit_TextRecognizer_Process(IntPtr TextRecognizerPtr);

        [DllImport("__Internal")]
        public static extern void MLKit_TextRecognizer_Close(IntPtr TextRecognizerPtr);


        #endregion

        #region Fields

        IntPtr m_nativeHandle;

        #endregion

        #region Constructor

        public NativeTextRecognizer()
        {
            m_nativeHandle = MLKit_TextRecognizer_Init();
        }

        #endregion

        #region Public methods

        public void Close()
        {
            MLKit_TextRecognizer_Close(m_nativeHandle);
        }

        public void Prepare(IntPtr inputSource, NativeTextRecognizerScanOptions options)
        {
            MLKit_TextRecognizer_Prepare(m_nativeHandle, inputSource, options.NativeHandle);
        }
        public void Process()
        {
            MLKit_TextRecognizer_Process(m_nativeHandle);
        }
        public void SetListener(NativeTextRecognizerListener listener)
        {
            MLKit_TextRecognizer_SetListener(m_nativeHandle, listener.NativeHandle, NativeTextRecognizerListener.PrepareSuccessNativeCallback,
                                                    NativeTextRecognizerListener.PrepareFailedNativeCallback,
                                                    NativeTextRecognizerListener.ScanSuccessNativeCallback,
                                                    NativeTextRecognizerListener.ScanFailedNativeCallback);
        }

        #endregion
    }
}
#endif