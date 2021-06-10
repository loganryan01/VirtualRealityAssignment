#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Grabbit;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

[Serializable]
public class GrabbitRange
{
    public BoxBoundsHandle BoxHandle;
    [NonSerialized] public List<MeshRenderer> SelectedRenderers = new List<MeshRenderer>();

    public Vector3 position = Vector3.zero;
    public Vector3 size = Vector3.one;
    public bool IsDefault => size == Vector3.one && position == Vector3.zero;
    [NonSerialized] public bool IsInitialized;
    public bool GrowToSelection = true;

    public void Initialize()
    {
        SelectedRenderers.Clear();
        BoxHandle = new BoxBoundsHandle {center = position, size = size};
        if (position == Vector3.zero)
            MoveToCameraCenter();
        else
        {
            Bounds bounds = new Bounds(BoxHandle.center, BoxHandle.size);
            if (SceneView.lastActiveSceneView)
                SceneView.lastActiveSceneView.Frame(bounds);
        }

        IsInitialized = true;
    }

    public void MoveToCameraCenter()
    {
        var prevSelection = Selection.objects;
        GameObject go = new GameObject();
        Selection.objects = new Object[] {go};
        SceneView.lastActiveSceneView.MoveToView();
        Selection.objects = prevSelection;
        BoxHandle.center = go.transform.position;
        Object.DestroyImmediate(go);
    }

    public void ShowLimitationGUI()
    {
        if (!IsInitialized)
            Initialize();

        Handles.zTest = CompareFunction.GreaterEqual;

        Handles.color = Color.white;
        Handles.DrawWireCube(BoxHandle.center, BoxHandle.size);

        Handles.zTest = CompareFunction.LessEqual;
        Handles.color = Color.red;
        Handles.DrawWireCube(BoxHandle.center, BoxHandle.size);

        Handles.zTest = CompareFunction.Always;

        Handles.Label(BoxHandle.center + BoxHandle.size.x * Vector3.right,
            "<color='red'>Grabbit Limitation Zone</color>", GrabbitEditor.Instance.CurrentSettings.Rich);

        Handles.color = Color.white;
    }

    private void ResetBoxHandleIfNeeded()
    {
        if (BoxHandle == null)
        {
            BoxHandle = new BoxBoundsHandle();
            BoxHandle.size = size;
            BoxHandle.center = position;
        }
    }

    public void RestrictToSelection()
    {
        if (!Selection.activeGameObject)
        {
            MoveToCameraCenter();
            BoxHandle.size = Vector3.one;
            return;
        }

        Bounds bounds;

        var renderers = Selection.activeGameObject.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0)
            bounds = new Bounds(Selection.activeGameObject.transform.position, Vector3.one);
        else
        {
            bounds = renderers.First().bounds;
            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        int i = 0;
        foreach (var o in Selection.gameObjects)
        {
            renderers = o.GetComponentsInChildren<MeshRenderer>();
            if (renderers.Length == 0)
                bounds.Encapsulate(o.transform.position);
            else
            {
                foreach (var renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            i++;
            if (i > 200)
                return;
        }

        position = BoxHandle.center = bounds.center;
        size = BoxHandle.size = bounds.size;
    }

    public void ConfigurationGUI()
    {
        if (!IsInitialized)
            Initialize();

        position = BoxHandle.center = Handles.PositionHandle(BoxHandle.center, Quaternion.identity);
        BoxHandle.DrawHandle();

        if (GrowToSelection && Selection.activeGameObject)
        {
            Bounds bounds = new Bounds(BoxHandle.center, BoxHandle.size);

            int i = 0;
            foreach (var o in Selection.gameObjects)
            {
                var renderers = o.GetComponentsInChildren<MeshRenderer>();
                if (renderers.Length == 0)
                    bounds.Encapsulate(o.transform.position);
                else
                {
                    foreach (var renderer in renderers)
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }

                i++;
                if (i > 200)
                    return;
            }

            position = BoxHandle.center = bounds.center;
            size = BoxHandle.size = bounds.size;
        }
    }

    public void ClearSelection()
    {
        SelectedRenderers.Clear();
    }

    public void SelectObjectsFromScenes()
    {
        SelectedRenderers.Clear();
        var renderers = Object.FindObjectsOfType<MeshRenderer>();

        Bounds bounds = new Bounds(BoxHandle.center, BoxHandle.size);
        foreach (var renderer in renderers)
        {
            if (bounds.Intersects(renderer.bounds))
            {
                SelectedRenderers.Add(renderer);
            }
        }
    }
}
#endif