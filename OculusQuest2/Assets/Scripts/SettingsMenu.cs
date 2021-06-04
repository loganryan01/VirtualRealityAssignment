﻿using System.Collections;
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

    // Continuous Controls
    public ActionBasedContinuousTurnProvider continuousTurnProvider;
    public Toggle continuousTurnToggle;

    // Main Menu Object
    public GameObject mainMenuObject;
    public AudioMixer mixer;

    public void Start()
    {
        string controls = PlayerPrefs.GetString("Controls");
        string rotationControls = PlayerPrefs.GetString("Rotation");

        if (controls == "Teleportation")
        {
            teleportationProvider.enabled = true;
            continuousMoveProvider.enabled = false;

            teleportationToggle.isOn = true;
        }
        else
        {
            teleportationProvider.enabled = false;
            continuousMoveProvider.enabled = true;

            continuousToggle.isOn = true;
        }

        if (rotationControls == "Snap Turn")
        {
            snapTurnProvider.enabled = true;
            continuousTurnProvider.enabled = false;

            snapTurnToggle.isOn = true;
        }
        else
        {
            snapTurnProvider.enabled = false;
            continuousTurnProvider.enabled = true;

            continuousTurnToggle.isOn = true;
        }
    }

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

            teleportationProvider.enabled = false;
            continuousMoveProvider.enabled = true;
        }
        else if (teleportationToggle.isOn && continuousToggle.isOn)
        {
            teleportationToggle.isOn = false;

            teleportationProvider.enabled = false;
            continuousMoveProvider.enabled = true;
        }

        PlayerPrefs.SetString("Controls", "Continuous");
    }

    public void EnableSnapTurn()
    {
        if (snapTurnToggle.isOn && !continuousTurnToggle.isOn)
        {
            snapTurnProvider.enabled = true;
            continuousTurnProvider.enabled = false;
        }
        else if (!snapTurnToggle.isOn && !continuousTurnToggle.isOn)
        {
            snapTurnToggle.isOn = true;

            snapTurnProvider.enabled = true;
            continuousTurnProvider.enabled = false;
        }
        else if (snapTurnToggle.isOn && continuousTurnToggle.isOn)
        {
            continuousTurnToggle.isOn = false;

            snapTurnProvider.enabled = true;
            continuousTurnProvider.enabled = false;
        }

        PlayerPrefs.SetString("Rotation", "Snap Turn");
    }

    public void EnableContinuousTurn()
    {
        if (!snapTurnToggle.isOn && continuousTurnToggle.isOn)
        {
            snapTurnProvider.enabled = false;
            continuousTurnProvider.enabled = true;
        }
        else if (!snapTurnToggle.isOn && !continuousTurnToggle.isOn)
        {
            continuousTurnToggle.isOn = true;

            snapTurnProvider.enabled = false;
            continuousTurnProvider.enabled = true;
        }
        else if (snapTurnToggle.isOn && continuousTurnToggle.isOn)
        {
            snapTurnToggle.isOn = false;

            snapTurnProvider.enabled = false;
            continuousTurnProvider.enabled = true;
        }

        PlayerPrefs.SetString("Rotation", "Continuous");
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