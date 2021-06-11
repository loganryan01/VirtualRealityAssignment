using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class EditorSimulatorToggle : MonoBehaviour
{
    public GameObject deviceSimulator;
    public UnityTemplateProjects.SimpleCameraController cameraController;


    void Awake()
    {
        if (Application.isEditor)
        {
            // Set player prefs to be teleportation, as smooth movement is not nessesary with the simulator
            PlayerPrefs.SetString("Controls", "Teleportation");

            deviceSimulator.SetActive(true);
            cameraController.enabled = false;
        }
        else
        {
            deviceSimulator.SetActive(false);
            cameraController.enabled = true;
        }
    }
}
