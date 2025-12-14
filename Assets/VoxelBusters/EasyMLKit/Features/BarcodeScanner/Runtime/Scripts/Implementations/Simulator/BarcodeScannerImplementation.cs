#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.EasyMLKit.Internal;

namespace VoxelBusters.EasyMLKit.Implementations.Simulator
{
    public class BarcodeScannerImplementation : BarcodeScannerImplementationBase
    {
        public BarcodeScannerImplementation() : base(isAvailable: true)
        {
        }

        public override void Prepare(IImageInputSource inputSource, BarcodeScannerOptions options, OnPrepareCompleteInternalCallback callback)
        {
            if (callback != null)
            {
                callback(null);
            }
        }

        public override void Process(OnProcessUpdateInternalCallback<BarcodeScannerResult> callback)
        {
            if (callback != null)
            {
                UnityEngine.Rect rect = new UnityEngine.Rect(37,37, 950, 950);
                Barcode barcode = new Barcode(BarcodeFormat.AZTEC, BarcodeValueType.UNKNOWN, "testing", "test", null, "Display test", rect, GetCornerPoints(rect));
                
                callback(new BarcodeScannerResult(new List<Barcode>(){barcode, barcode }, null));
            }
        }

        private Vector2[] GetCornerPoints(Rect rect)
        {
            throw new NotImplementedException();
        }

        public override void Close(OnCloseInternalCallback callback)
        {
            if (callback != null)
            {
                callback(null);
            }
        }
    }
}
#endif