using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject teleportController;
    public PlayerPrefsScript playerPrefs;

    private bool teleporting = false;
    
    // Start is called before the first frame update
    void Start()
    {
        string controls = PlayerPrefs.GetString("Controls");

        if (controls == "Teleportation")
        {
            teleporting = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") && teleporting)
        {
            teleportController.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable") && teleporting)
        {
            teleportController.SetActive(true);
        }
    }
}
