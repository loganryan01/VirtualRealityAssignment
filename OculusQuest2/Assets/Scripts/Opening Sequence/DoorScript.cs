using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
	public AudioSource audioClip;



	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Hand"))
		{
			audioClip.Play();
			// Use transition manager to go to the main level
			TransitionManager.instance.ChangeScene(3);
		}
	}
}
