#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Grabbit
{
    public class ColliderMeshDeletionPostProcessor : UnityEditor.AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (!asset)
                return AssetDeleteResult.DidNotDelete;

            GrabbitEditor.Instance.ColliderMeshContainer.RemoveMesh(asset);
            return AssetDeleteResult.DidNotDelete;
        }
    }
}
#endif