using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class SettingsMenu : MonoBehaviour
{
    // Teleportation controls
    public TeleportationProvider teleportationProvider;
    public Toggle teleportationToggle;

    // Continuous Movement Controls
    public ActionBasedContinuousMoveProvider continuousMoveProvider;
    public Toggle continuousToggle;

    // Snap Turn Controls
    public ActionBasedSnapTurnProvider snapTurnProvider;
    public Toggle snapTurnToggle;

    // Main Menu Object
    public GameObject mainMenuObject;
    public AudioMixer mixer;
    
    

    public void EnableTeleportation()
    {
        if (teleportationToggle.isOn && !continuousToggle.isOn)
        {
            teleportationProvider.enabled = true;
            continuousMoveProvider.enabled = false;
        }
        else if (!teleportationToggle.isOn && !continuousToggle.isOn)
        {
            teleportationToggle.isOn = true;

            teleportationProvider.enabled = true;
            continuousMoveProvider.enabled = false;
        }
        else if (teleportationToggle.isOn && continuousToggle.isOn)
        {
            continuousToggle.isOn = false;

            teleportationProvider.enabled = true;
            continuousMoveProvider.enabled = false;
        }
        
        PlayerPrefs.SetString("Controls", "Teleportation");
    }

    public void EnableContinuous()
    {
        if (continuousToggle.isOn && !teleportationToggle.isOn)
        {
            teleportationProvider.enabled = false;
            continuousMoveProvider.enabled = true;
        }
        else if (!teleportationToggle.isOn && !continuousToggle.isOn)
        {
            continuousToggle.isOn = true;
        }
        else if (teleportationToggle.isOn && continuousToggle.isOn)
        {
            teleportationToggle.isOn = false;

            teleportationProvider.enabled = false;
            continuousMoveProvider.enabled = true;
        }

        PlayerPrefs.SetString("Controls", "Continuous");
    }

    public void SetLevel(float sliderValue)
    {
        mixer.SetFloat("MusicVol", Mathf.Log10(sliderValue) * 20);
    }

    public void GoToMainMenu()
    {
        mainMenuObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
