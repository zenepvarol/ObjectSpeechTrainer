using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.NativePlugins;
using VoxelBusters.EasyMLKit.Internal;

namespace VoxelBusters.EasyMLKit
{
    public static class ImplementationSchema
    {
        #region Constants

        private     const   string      kMainAssembly                   = "VoxelBusters.EasyMLKit";
        
        private     const   string      kIOSAssembly                    = kMainAssembly;

        private     const   string      kAndroidAssembly                = kMainAssembly;

        private     const   string      kSimulatorAssembly              = kMainAssembly;

        private     const   string      kRootNamespace                  = "VoxelBusters.EasyMLKit";

        #endregion

        #region Static properties

        public static NativeFeatureRuntimeConfiguration BarcodeScanner 
        { 
            get { return GetBarcodeScannerConfig(); } 
        }

        public static NativeFeatureRuntimeConfiguration ObjectDetectorAndTracker
        {
            get { return GetObjectDetectorAndTrackerConfig(); }
        }

        public static NativeFeatureRuntimeConfiguration TextRecognizer
        {
            get { return GetTextRecognizerConfig(); }
        }

        public static NativeFeatureRuntimeConfiguration FaceDetector
        {
            get { return GetFaceDetectorConfig(); }
        }

        public static NativeFeatureRuntimeConfiguration DigitalInkRecognizer
        {
            get { return GetDigitalInkRecognizerConfig(); }
        }

        #endregion

        #region Constructors

        private static NativeFeatureRuntimeConfiguration GetBarcodeScannerConfig()
        {
            return GetConfig(NativeFeatureType.kBarcodeScanner);
        }

        private static NativeFeatureRuntimeConfiguration GetObjectDetectorAndTrackerConfig()
        {
            return GetConfig(NativeFeatureType.kObjectDetectorAndTracker);
        }

        private static NativeFeatureRuntimeConfiguration GetTextRecognizerConfig()
        {
            return GetConfig(NativeFeatureType.kTextRecognizer);
        }


        private static NativeFeatureRuntimeConfiguration GetFaceDetectorConfig()
        {
            return GetConfig(NativeFeatureType.kFaceDetector);
        }


        private static NativeFeatureRuntimeConfiguration GetDigitalInkRecognizerConfig()
        {
            return GetConfig(NativeFeatureType.kDigitalInkRecognizer);
        }


        private static NativeFeatureRuntimeConfiguration GetConfig(string featureName)
        {
            string featureNamespace = $"{kRootNamespace}";
            string nativeInterfaceType = featureName + "Implementation";

            string featureAssembly = $"{kMainAssembly}.{featureName}";

            return new NativeFeatureRuntimeConfiguration(
                packages: new NativeFeatureRuntimePackage[]
                {
                    NativeFeatureRuntimePackage.IPhonePlayer(assembly: featureAssembly, ns: featureNamespace + ".Implementations.iOS", nativeInterfaceType: nativeInterfaceType),
                    NativeFeatureRuntimePackage.Android(assembly: featureAssembly, ns: featureNamespace + ".Implementations.Android", nativeInterfaceType: nativeInterfaceType),
                },
                simulatorPackage:   NativeFeatureRuntimePackage.Generic(assembly: featureAssembly, ns: featureNamespace + ".Implementations.Simulator", nativeInterfaceType: nativeInterfaceType),
                fallbackPackage:    NativeFeatureRuntimePackage.Generic(assembly: featureAssembly, ns: featureNamespace + ".Implementations.Null", nativeInterfaceType: nativeInterfaceType));
        }

        #endregion

        #region Public static methods

        public static NativeFeatureRuntimeConfiguration GetRuntimeConfiguration(string featureName)
        {
            switch (featureName)
            {
                case NativeFeatureType.kBarcodeScanner:
                    return BarcodeScanner;

                case NativeFeatureType.kObjectDetectorAndTracker:
                    return ObjectDetectorAndTracker;

                case NativeFeatureType.kTextRecognizer:
                    return TextRecognizer;

                case NativeFeatureType.kDigitalInkRecognizer:
                    return DigitalInkRecognizer;

                case NativeFeatureType.kFaceDetector:
                    return FaceDetector;

                default:
                    DebugLogger.LogError("No runtime configuration found for feature : " + featureName);
                    return null;
            }
        }

        public static KeyValuePair<string, NativeFeatureRuntimeConfiguration>[] GetAllRuntimeConfigurations(bool includeInactive = true, EasyMLKitSettings settings = null)
        {
            Assert.IsTrue(includeInactive || (settings != null), "Arg settings is null.");

            var configurations = new List<KeyValuePair<string, NativeFeatureRuntimeConfiguration>>();

            var map = new Dictionary<string, NativeFeatureRuntimeConfiguration>()
            {
                {NativeFeatureType.kBarcodeScanner,  GetBarcodeScannerConfig()},
                {NativeFeatureType.kObjectDetectorAndTracker,  GetObjectDetectorAndTrackerConfig()},
                {NativeFeatureType.kTextRecognizer,  GetTextRecognizerConfig()},
                {NativeFeatureType.kFaceDetector,  GetFaceDetectorConfig()},
                {NativeFeatureType.kDigitalInkRecognizer, GetDigitalInkRecognizerConfig() }
            };

            foreach (var feature in map)
            {
                if (includeInactive || ((settings != null) && settings.IsFeatureUsed(feature.Key)))
                {
                    configurations.Add(feature);
                }
            }
            return configurations.ToArray();
        }

        #endregion
    }
}