using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class TeleportActivate : MonoBehaviour
{
    // Access the GameObject that contains the teleport controller script.
    public GameObject teleportController;

    // Reference to the Input Action Reference that contains the button mapping data for activation.
    public InputActionReference teleportCancelReference;

    [Space]
    [Header("Teleport Events")]
    // These will group Unity event calls that you can add in the inspector
    public UnityEvent onTeleportActivate;
    public UnityEvent onTeleportCancel;
    
    // Start is called before the first frame update
    void Start()
    {
        //teleportCancelReference.action.started += TeleportModeActivate;
        //teleportCancelReference.action.canceled += TeleportModeCancel;
    }


    void Update()
    {
        // If using teleportation and the action has been started
        if (teleportCancelReference.action.phase == InputActionPhase.Started)
        {
            onTeleportActivate.Invoke();
        }
        else
        {
            Invoke("DelayTeleportationDeactivate", .1f);
        }
    }





    // This will let us call a series of events created in the onTeleportActivate events in the inspector
    private void TeleportModeActivate(InputAction.CallbackContext obj) => onTeleportActivate.Invoke();

    // This will delay the call of the DelayTeleportation function for 0.1 of a second
    private void TeleportModeCancel(InputAction.CallbackContext obj) => Invoke("DelayTeleportationDeactivate", .1f);

    // This will let us call a series of events created in the onTeleportCancel events in the inspector
    private void DelayTeleportationDeactivate() => onTeleportCancel.Invoke();

}
