using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCameraForCanvas : MonoBehaviour
{
    public Canvas[] canvases;



    void Awake()
    {
        foreach (var canvas in canvases)
		{
            canvas.worldCamera = Camera.main;
		}
    }
}
