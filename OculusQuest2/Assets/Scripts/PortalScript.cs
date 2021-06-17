using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalScript : MonoBehaviour
{
    public MeshRenderer portalMidRenderer;

    public Material deactiveMaterial;
    public Material activateMaterial;

    public AudioSource activateAudioSource;
    public AudioSource ongoingAudioSource;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PortalGem")
        {
            portalMidRenderer.material = activateMaterial;
            activateAudioSource.Play();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "PortalGem")
        {
            ongoingAudioSource.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "PortalGem")
        {
            portalMidRenderer.material = deactiveMaterial;
            ongoingAudioSource.Stop();
        }
    }
}
