#if UNITY_IOS || UNITY_TVOS
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.Editor;
using VoxelBusters.CoreLibrary.NativePlugins;
using VoxelBusters.CoreLibrary.Editor.NativePlugins.Build;
using VoxelBusters.CoreLibrary.Editor.NativePlugins.Build.Xcode;
using VoxelBusters.EasyMLKit.Internal;
using UnityEngine;
using VoxelBusters.CoreLibrary.Editor.NativePlugins;
using System.Linq;

namespace VoxelBusters.EasyMLKit.Editor.Build.Xcode
{
    public class PBXNativePluginsProcessor : CoreLibrary.Editor.NativePlugins.Build.Xcode.PBXNativePluginsProcessor
    {
#region Properties

        private EasyMLKitSettings Settings { get; set; }

#endregion

        #region Base class methods

        public override void OnUpdateExporterObjects()
        {
            // Check whether plugin is configured
            if (!EnsureInitialised()) return;

            DebugLogger.Log(EasyMLKitDomain.Default, "Updating native plugins exporter settings.");

            foreach (var exporter in NativePluginsExporterObject.FindObjects<PBXNativePluginsExporterObject>(includeInactive: true))
            {
                switch (exporter.name)
                {
                    case NativeFeatureType.kBarcodeScanner:
                        exporter.IsEnabled = Settings.IsFeatureUsed(exporter.name);
                        break;

                    case NativeFeatureType.kTextRecognizer:
                        exporter.IsEnabled = Settings.IsFeatureUsed(exporter.name);
                        break;


                    case NativeFeatureType.kDigitalInkRecognizer:
                        exporter.IsEnabled = Settings.IsFeatureUsed(exporter.name);
                        break;

                    default:
                        DebugLogger.LogError(EasyMLKitDomain.Default, "Feature exported is not handled!");
                        break;
                }
                EditorUtility.SetDirty(exporter);
            }

            StripUnusedFeatures();
        }

        public override void OnUpdateInfoPlist(PlistDocument doc)
        {
            // Check whether plugin is configured
            if (!EnsureInitialised()) return;

            // Add usage permissions
            var     rootDict    = doc.root;
            var     permissions = GetUsagePermissions();
            foreach (string key in permissions.Keys)
            {
                rootDict.SetString(key, permissions[key]);
            }
        }

#endregion

#region Private methods

        private bool EnsureInitialised()
        {
            if (Settings != null) return true;

            if (EasyMLKitSettingsEditorUtility.TryGetDefaultSettings(out EasyMLKitSettings settings))
            {
                Settings = settings;
                return true;
            }
            else
            {
                EasyMLKitSettingsEditorUtility.ShowSettingsNotFoundErrorDialog();
                return false;
            }
        }

        private static Dictionary<string, string> GetUsagePermissions()
        {
            var requiredPermissionsDict = new Dictionary<string, string>();

            requiredPermissionsDict[InfoPlistKey.kNSCameraUsage] = "Uses camera for scanning";
            return requiredPermissionsDict;
        }


        private void StripUnusedFeatures()
        {
            var usedFeaturesNames = Settings.GetUsedFeatureNames();
            EasyMLKitFeatureUtility.UpdateFeatureStatuses(usedFeaturesNames);
        }

        #endregion
    }
}
#endif