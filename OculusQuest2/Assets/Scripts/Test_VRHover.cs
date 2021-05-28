using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_VRHover : MonoBehaviour
{
    public Renderer render;

    public Material onMat;
    private Material offMat;



    void Start()
    {
        offMat = render.material;
    }


    public void OnHoverEnter()
    {
        render.material = onMat;
    }
    public void OnHoverExit()
    {
        render.material = offMat;
    }

}
