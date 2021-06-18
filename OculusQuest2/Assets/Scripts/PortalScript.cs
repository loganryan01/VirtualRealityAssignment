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
        propBlock = new MaterialPropertyBlock();
        portalMidRenderer.GetPropertyBlock(propBlock);

        propBlock.SetFloat("_DisolveAmount", 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (dissolveCompleted && !ongoingAudioSource.isPlaying)
        {
            ongoingAudioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PortalGem")
        {
            Destroy(other.gameObject);
            StartCoroutine(DissolvePortal());
            activateAudioSource.Play();
        }

        if (other.gameObject.layer == 12 && propBlock.GetFloat("_DisolveAmount") == 0)
        {
            TransitionManager.instance.ChangeScene(4);
        }
    }

    IEnumerator DissolvePortal()
    {
        float t = 0;
        float disolveTime = activateAudioSource.clip.length;

        while (t < disolveTime)
        {
            // Update the disolve amount value
            propBlock.SetFloat("_DisolveAmount", 1 - (t / disolveTime));

            if (propBlock.GetFloat("_DisolveAmount") == 0)
            {
                dissolveCompleted = true;
            }

            portalMidRenderer.SetPropertyBlock(propBlock);

            t += Time.deltaTime;
            yield return null;
        }
    }
}
