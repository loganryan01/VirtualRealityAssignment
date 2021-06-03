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
