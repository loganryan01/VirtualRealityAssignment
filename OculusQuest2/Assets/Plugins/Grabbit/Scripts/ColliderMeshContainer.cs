#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Grabbit.VHACD;
using UnityEditor;
using UnityEngine;

namespace Grabbit
{
    [Serializable]
    public class MeshList
    {
        public int maxMeshCount;
        public List<Mesh> Meshes;
        public int resolution;
    }

    [CreateAssetMenu(fileName = "Grabbit Settings", menuName = "Tools/Grabbit/Create New Collider Container",
        order = 1)]
    public class ColliderMeshContainer : ScriptableObject
    {
        [SerializeField] [HideInInspector] private MeshMeshListDictionary colliderMeshes = new MeshMeshListDictionary();

        public List<Mesh> GetMeshListAndRegenerateIfNeeded(Mesh mesh, GrabbitSettings settings)
        {
            var list = colliderMeshes[mesh];
            if (list.resolution != settings.ColliderResolution || list.maxMeshCount != settings.MaxMeshCollidersCreated)
                RegenerateFromMesh(mesh, settings);

            return list.Meshes;
        }

        public bool IsMeshDefined(Mesh mesh)
        {
            if (colliderMeshes.ContainsKey(mesh))
            {
                if (colliderMeshes[mesh].Meshes.Contains(null))
                {
                    colliderMeshes[mesh].Meshes.RemoveAll(_ => !_);
                    EditorUtility.SetDirty(this);
                }
                
                return colliderMeshes[mesh].Meshes.Count > 0;
            }

            return false;
        }

        public void GenerateAllColliders(GrabbitSettings settings)
        {
            var ids = AssetDatabase.FindAssets("t:Mesh", new[] {"Assets"});
            Debug.LogFormat("Grabbit Analysis: {0} meshes found. Generating Colliders...", ids.Length);

            var vhacdGenerator = CreateAndConfigureGenerator(settings);
            var i = 0;

            foreach (var id in ids)
            {
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(id));

                if (colliderMeshes.ContainsKey(mesh))
                    continue;

                if (EditorUtility.DisplayCancelableProgressBar(
                    "Grabbit Is Generating Colliders To Be Used In The Scene",
                    $"Analyzing {mesh.name} ({i + 1} out of {ids.Length})",
                    (float) i / ids.Length))
                    break;

                try
                {
                    if (!mesh)
                        continue;
                    var meshes = vhacdGenerator.GenerateConvexMeshes(mesh);

                    foreach (var collidingMesh in meshes) AssetDatabase.AddObjectToAsset(collidingMesh, this);

                    colliderMeshes.Add(mesh,
                        new MeshList
                        {
                            maxMeshCount = settings.MaxMeshCollidersCreated, resolution = settings.ColliderResolution,
                            Meshes = meshes
                        });
                }
                catch (Exception)
                {
                    // ignored
                }


                i++;
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.SetDirty(this);
            Debug.LogFormat("Grabbit Colliders Generated!", ids.Length);
            AssetDatabase.SaveAssets();
        }

        private static VhacdGenerator CreateAndConfigureGenerator(GrabbitSettings settings)
        {
            var vhacdGenerator = new VhacdGenerator();
            vhacdGenerator.parameters.m_maxConvexHulls =
                (uint) settings.MaxMeshCollidersCreated;
            vhacdGenerator.parameters.m_resolution = (uint) settings.ColliderResolution;
            return vhacdGenerator;
        }

        public void RegisterCollidersFromMeshFiltersInScene(GrabbitSettings settings, params MeshFilter[] filters)
        {
            var vhacdGenerator = CreateAndConfigureGenerator(settings);
            var i = 0;
            foreach (var filter in filters)
            {
                var mesh = filter.sharedMesh;

                if (EditorUtility.DisplayCancelableProgressBar(
                    "Grabbit Is Generating Colliders In The Scene",
                    $"Analyzing {mesh.name} ({i + 1} out of {filters.Length})",
                    (float) i / filters.Length))
                    break;

                //TODO: check for hidden assets and so on
                if (colliderMeshes.ContainsKey(mesh))
                    continue;

                var meshes = vhacdGenerator.GenerateConvexMeshes(mesh);

                foreach (var collidingMesh in meshes) AssetDatabase.AddObjectToAsset(collidingMesh, this);

                colliderMeshes.Add(mesh,
                    new MeshList
                    {
                        maxMeshCount = settings.MaxMeshCollidersCreated, resolution = settings.ColliderResolution,
                        Meshes = meshes
                    });
                i++;
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public bool ShouldRegenerate(MeshCollider collider, GrabbitSettings settings)
        {
            var mesh = collider.sharedMesh;
            if (!colliderMeshes.ContainsKey(mesh))
                return true;

            return colliderMeshes[mesh].resolution != settings.ColliderResolution;
        }

        public void RegenerateFromMesh(Mesh mesh, GrabbitSettings settings)
        {
            if (colliderMeshes.ContainsKey(mesh))
                RemoveMesh(mesh);
            GenerateFromMesh(mesh, settings);
        }

        public void RegenerateFromCollider(MeshCollider collider, GrabbitSettings settings)
        {
            var mesh = collider.sharedMesh;
            if (colliderMeshes.ContainsKey(mesh))
                RemoveMesh(mesh);

            RegisterCollidersFromSelection(collider, settings);
        }

        public void RegisterCollidersFromSelection(MeshCollider collider, GrabbitSettings settings)
        {
            var mesh = collider.sharedMesh;

            GenerateFromMesh(mesh, settings);
        }

        private static int MaxEstimatedTickForGeneration = 60 * 60;

        private void GenerateFromMesh(Mesh mesh, GrabbitSettings settings)
        {
            if (IsMeshDefined(mesh))
                return;

            var vhacdGenerator = CreateAndConfigureGenerator(settings);

            vhacdGenerator.ThreadedGenerateConvexMeshes(mesh);

            bool shouldGenerate = true;
            int i = 0;
            while (!vhacdGenerator.IsThreadedMeshGenerationDone())
            {
                if (EditorUtility.DisplayCancelableProgressBar(
                    $"Grabbit is generating colliders for {mesh.name}",
                    "Generating...",
                    (float) i / MaxEstimatedTickForGeneration))
                {
                    vhacdGenerator.AbortThread();
                    shouldGenerate = false;
                    break;
                }

                i++;
            }

            if (shouldGenerate)
                vhacdGenerator.GenerateMeshesAfterThreadCompletion();
            List<Mesh> meshes = shouldGenerate ? vhacdGenerator.RetrieveThreadMesh() : new List<Mesh>();
            //NonThreadedMeshGeneration(mesh, settings, vhacdGenerator);

            if (meshes.Count == 0 && mesh.vertexCount > 0)
            {
                if (shouldGenerate)
                    Debug.LogWarning(
                        $"Grabbit Warning: The collider generation failed for {mesh.name}, only convex collider available.");
                else
                {
                    Debug.Log(
                        $"Grabbit Warning: The collider generation was cancelled for {mesh.name}, only convex collider available.");
                }

                EditorUtility.ClearProgressBar();
            }

            foreach (var collidingMesh in meshes) AssetDatabase.AddObjectToAsset(collidingMesh, this);

            if (colliderMeshes.ContainsKey(mesh))
            {
                colliderMeshes[mesh] = new MeshList
                {
                    maxMeshCount = settings.MaxMeshCollidersCreated, resolution = settings.ColliderResolution,
                    Meshes = meshes
                };
            }
            else
            {
                colliderMeshes.Add(mesh,
                    new MeshList
                    {
                        maxMeshCount = settings.MaxMeshCollidersCreated, resolution = settings.ColliderResolution,
                        Meshes = meshes
                    });
            }


            EditorUtility.DisplayProgressBar(
                $"Grabbit is generating colliders for {mesh.name}",
                "Generating...",
                1);
            EditorUtility.ClearProgressBar();
            EditorUtility.SetDirty(this);
        }

        private List<Mesh> NonThreadedMeshGeneration(Mesh mesh, GrabbitSettings settings, VhacdGenerator vhacdGenerator)
        {
            //TODO: check for hidden assets and so on
            return vhacdGenerator.GenerateConvexMeshes(mesh);
        }


        public void ClearColliders()
        {
            foreach (var pair in colliderMeshes)
            foreach (var mesh in pair.Value.Meshes)
            {
                if (!mesh)
                    continue;
                AssetDatabase.RemoveObjectFromAsset(mesh);
                DestroyImmediate(mesh);
            }

            colliderMeshes.Clear();

            string path = AssetDatabase.GetAssetPath(this);
            var objs = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var obj in objs)
            {
                if (obj == this)
                    continue;

                AssetDatabase.RemoveObjectFromAsset(obj);
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void RemoveMesh(Mesh mesh)
        {
            if (!colliderMeshes.ContainsKey(mesh))
                return;

            var list = colliderMeshes[mesh];
            foreach (var createdMesh in list.Meshes)
            {
                if (!createdMesh)
                    return;

                AssetDatabase.RemoveObjectFromAsset(createdMesh);
                DestroyImmediate(createdMesh);
            }

            colliderMeshes.Remove(mesh);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif