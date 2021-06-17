﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CabnetScript : MonoBehaviour
{
    public Transform drawer;
    public float openTime;
    public float openDistance;

    public AudioSource audioSource;


    bool hasBeenOpened = false;



    public void OpenDrawer()
    {
        if (!hasBeenOpened)
        {
            audioSource.Play();
            StartCoroutine(OpenDrawerReal());
            hasBeenOpened = true;
        }
    }

    private IEnumerator OpenDrawerReal()
    {
        Vector3 start = drawer.position;
        Vector3 end = start + new Vector3(0, 0, openDistance);

        float t = 0;
        while (t < openTime)
        {
            drawer.position = Vector3.Lerp(start, end, t / openTime);

            t += Time.deltaTime;
            yield return null;
        }
    }
}
