#if UNITY_EDITOR

using System.Linq;
using UnityEditor;
using VoxelBusters.CoreLibrary.Editor;

namespace VoxelBusters.EasyMLKit.Editor
{
    public static class EasyMLKitFeatureUtility 
    {
        public static void UpdateFeatureStatuses(string[] usedFeaturesNames)
        {
            var features = AssetDatabaseUtility.FindAssetObjects<EasyMLKitFeature>();

            foreach (var each in features)
            {
                var featureName = each.name;

                if (!usedFeaturesNames.Contains(featureName))
                {
                    each.DisableFeature();
                }
                else
                {
                    each.EnableFeature();
                }
            }

            AssetDatabase.Refresh();
        }
    }
}
#endif