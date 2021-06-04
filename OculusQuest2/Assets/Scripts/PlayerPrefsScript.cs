using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerPrefsScript : MonoBehaviour
{
    public TeleportationProvider teleportationProvider;
    public ActionBasedContinuousMoveProvider continuousMoveProvider;
    public ActionBasedSnapTurnProvider snapTurnProvider;
    public ActionBasedContinuousTurnProvider continuousTurnProvider;

    public static bool teleportationMovement = false;
    public static bool continuousMovement = false;

    // Start is called before the first frame update
    void Start()
    {
        string controls = "Teleportation";
        string rotationControls = PlayerPrefs.GetString("Rotation");

        if (controls == "Teleportation")
        {
            teleportationProvider.enabled = true;
            continuousMoveProvider.enabled = false;

            teleportationMovement = true;
        }
        else
        {
            teleportationProvider.enabled = false;
            continuousMoveProvider.enabled = true;

            continuousMovement = true;
        }

        if (rotationControls == "Snap Turn")
        {
            snapTurnProvider.enabled = true;
            continuousTurnProvider.enabled = false;
        }
        else
        {
            snapTurnProvider.enabled = false;
            continuousTurnProvider.enabled = true;
        }
    }
}
