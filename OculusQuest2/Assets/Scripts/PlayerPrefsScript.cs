using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerPrefsScript : MonoBehaviour
{
    string controls;

    public TeleportationProvider teleportationProvider;
    public ActionBasedContinuousMoveProvider continuousMoveProvider;

    // Start is called before the first frame update
    void Start()
    {
        controls = PlayerPrefs.GetString("Controls");

        if (controls == "Teleportation")
        {
            teleportationProvider.enabled = true;
            continuousMoveProvider.enabled = false;
        }
        else
        {
            teleportationProvider.enabled = false;
            continuousMoveProvider.enabled = true;
        }
    }
}
