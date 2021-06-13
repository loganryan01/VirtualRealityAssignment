using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class NonSnapGrabInteractable : XRGrabInteractable
{
    Vector3 interactorPosition;
    Quaternion interactorRotation;


    protected override void OnSelectEntering(XRBaseInteractor interactor)
    {
        base.OnSelectEntering(interactor);

        if (interactor is XRDirectInteractor)
        {
            interactorPosition = interactor.attachTransform.localPosition;
            interactorRotation = interactor.attachTransform.localRotation;

            bool hasAttach = attachTransform != null;
            interactor.attachTransform.position = hasAttach ? attachTransform.position : transform.position;
            interactor.attachTransform.rotation = hasAttach ? attachTransform.rotation : transform.rotation;
        }
    }

    protected override void OnSelectExiting(XRBaseInteractor interactor)
    {
        base.OnSelectEntering(interactor);

        if (interactor is XRDirectInteractor)
        {
            interactor.attachTransform.localPosition = interactorPosition;
            interactor.attachTransform.localRotation = interactorRotation;
        }
    }
}
