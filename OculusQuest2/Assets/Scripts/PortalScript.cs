using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalScript : MonoBehaviour
{
    public MeshRenderer portalMidRenderer;

    public AudioSource activateAudioSource;
    public AudioSource ongoingAudioSource;

    private bool dissolveCompleted = false;
    private MaterialPropertyBlock propBlock;

    // Start is called before the first frame update
    void Start()
    {
        // Get the dissolve shader properties
        propBlock = new MaterialPropertyBlock();
        portalMidRenderer.GetPropertyBlock(propBlock);

        // Set the Dissolve value to 1
        propBlock.SetFloat("_DisolveAmount", 1);
        portalMidRenderer.SetPropertyBlock(propBlock);
    }

    // Update is called once per frame
    void Update()
    {
        // Once the portal dissolve is completed, play the ongoing portal sound effect
        if (dissolveCompleted && !ongoingAudioSource.isPlaying)
        {
            ongoingAudioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Once the player inserts the gem, activate the portal        
        if (other.gameObject.name == "PortalGem")
        {
            other.gameObject.SetActive(false);
            StartCoroutine(DissolvePortal());
            activateAudioSource.Play();
        }

        // Once the player touches the portal
        if (other.gameObject.name == "Grab Trigger" && dissolveCompleted)
        {
            TransitionManager.instance.ChangeScene(4);
        }
    }

    // Increase the dissolve effect of the portal
    IEnumerator DissolvePortal()
    {
        float t = 0;
        float disolveTime = 2;

        while (t < disolveTime)
        {
            // Update the disolve amount value
            propBlock.SetFloat("_DisolveAmount", 1 - (t / disolveTime));

            portalMidRenderer.SetPropertyBlock(propBlock);

            t += Time.deltaTime;
            yield return null;
        }

        dissolveCompleted = true;
    }
}
