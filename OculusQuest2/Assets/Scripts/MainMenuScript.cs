using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public GameObject settingsCanvas;
    
    public void StartGame()
    {
        // Use transition manager to go to opening sequence
        TransitionManager.instance.ChangeScene(2);
    }

    // Go to the settings menu
    public void GoToSettingsMenu()
    {
        settingsCanvas.SetActive(true);
        gameObject.SetActive(false);
    }

    // Quit the game
    public void QuitGame()
    {
        Application.Quit();
    }
}
