using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SocketWithFixedHoverMesh : XRSocketInteractor
{
    // Taken from XRSocketInteractor, as it is private. As a result, there is a redundant object in XRSocketInteractor that is not being used
    readonly Dictionary<XRBaseInteractable, MeshFilter[]> meshFilterCache = new Dictionary<XRBaseInteractable, MeshFilter[]>();



    // This function creates the transform for the hover mesh, and has one line changed (calculation of finalPosition), but is private in XRSocketInteractor, so 
    // DrawHoveredInteractables() has to be overriden to use this function, without being changed itself
    Matrix4x4 GetInteractableAttachMatrix(XRGrabInteractable interactable, MeshFilter meshFilter, Vector3 scale)
    {
        var interactableLocalPosition = Vector3.zero;
        var interactableLocalRotation = Quaternion.identity;

        if (interactable.attachTransform != null)
        {
            // localPosition doesn't take into account scaling of parent objects, so scale attachpoint by lossyScale which is the global scale.
            interactableLocalPosition = Vector3.Scale(interactable.attachTransform.localPosition, interactable.attachTransform.lossyScale);
            interactableLocalRotation = interactable.attachTransform.localRotation;
        }

        var finalRotation = attachTransform.rotation * interactableLocalRotation;
        var finalPosition = attachTransform.position - attachTransform.rotation * interactableLocalPosition;   // Rotate the interactable attach point to pivot

        if (interactable.transform != meshFilter.transform)
        {
            finalPosition += Vector3.Scale(interactable.transform.InverseTransformPoint(meshFilter.transform.position), interactable.transform.lossyScale);
            finalRotation *= Quaternion.Inverse(Quaternion.Inverse(meshFilter.transform.rotation) * interactable.transform.rotation);
        }

        return Matrix4x4.TRS(finalPosition, finalRotation, scale);
    }

    protected override void DrawHoveredInteractables()
    {
        var materialToDrawWith = selectTarget == null ? interactableHoverMeshMaterial : interactableCantHoverMeshMaterial;
        if (materialToDrawWith == null)
            return;

        var cam = Camera.main;
        if (cam == null)
            return;

        var cullingMask = cam.cullingMask;

        var hoveredScale = Mathf.Max(0f, interactableHoverScale);

        foreach (var hoverTarget in hoverTargets)
        {
            var grabTarget = hoverTarget as XRGrabInteractable;
            if (grabTarget == null || grabTarget == selectTarget)
                continue;

            if (!meshFilterCache.TryGetValue(grabTarget, out var interactableMeshFilters))
                continue;

            if (interactableMeshFilters == null || interactableMeshFilters.Length == 0)
                continue;

            foreach (var meshFilter in interactableMeshFilters)
            {
                // TODO By only checking the main camera culling flags, but drawing the mesh in all cameras,
                // aren't we ignoring the culling mask of non-main cameras? Or does DrawMesh handle culling
                // automatically, making this early continue unnecessary?
                if (meshFilter == null || (cullingMask & (1 << meshFilter.gameObject.layer)) == 0)
                    continue;

                for (var submeshIndex = 0; submeshIndex < meshFilter.sharedMesh.subMeshCount; ++submeshIndex)
                {
                    Graphics.DrawMesh(
                        meshFilter.sharedMesh,
                        GetInteractableAttachMatrix(grabTarget, meshFilter, meshFilter.transform.lossyScale * hoveredScale),
                        materialToDrawWith,
                        gameObject.layer, // TODO Why use this Interactor layer instead of the Interactable layer?
                        null, // Draw mesh in all cameras (default value)
                        submeshIndex);
                }
            }
        }
    }
    protected override void OnHoverEntering(XRBaseInteractable interactable)
    {
        base.OnHoverEntering(interactable);
        MeshFilter[] interactableMeshFilters = interactable.GetComponentsInChildren<MeshFilter>();
        if (interactableMeshFilters.Length > 0)
        {
            meshFilterCache.Add(interactable, interactableMeshFilters);
        }
    }
    protected override void OnHoverExiting(XRBaseInteractable interactable)
    {
        base.OnHoverExiting(interactable);
        meshFilterCache.Remove(interactable);
    }
}
