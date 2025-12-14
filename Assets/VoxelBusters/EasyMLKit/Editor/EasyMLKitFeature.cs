using UnityEngine;
using VoxelBusters.CoreLibrary.Editor;
using VoxelBusters.CoreLibrary.Editor.NativePlugins.Build;

namespace VoxelBusters.EasyMLKit.Editor
{
    [CreateAssetMenu(fileName = "Easy ML Kit Plugin Feature", menuName = "Voxel Busters/Easy ML Kit/Feature", order = 0)]
    public class EasyMLKitFeature : PluginFeatureObject
    {
        protected override void UpdateLinkXmlWriter(LinkXmlWriter xmlWriter)
        {
            var featureName = name;
            var configuration = ImplementationSchema.GetRuntimeConfiguration(featureName);
            xmlWriter.AddConfiguration(configuration);
        }
    }
}
