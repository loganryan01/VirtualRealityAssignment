#if UNITY_EDITOR


using System;
using UnityEditor;

namespace Grabbit
{
    public class PrefabModeProcessor : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            if (!GrabbitEditor.IsInstanceCreated)
            {
                return paths;
            }
            
            var tool = GrabbitEditor.Instance.CurrentTool;
            if (tool && tool.Active && tool.IsPrefabMode)
            {
             
                EditorApplication.update += ResetToolAfterSave;
                return paths;
            }
            
            return paths;
        }

        private static void ResetToolAfterSave()
        {
            EditorApplication.update -= ResetToolAfterSave;
            GrabbitEditor.Instance.CurrentTool.ResetOnPrefabModeSave();
        }
    }
}
#endif