using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock_Obj : MonoBehaviour
{
    private GameObject obj;


    private void OnTriggerEnter(Collider other)
    {
        //if we entered a lock area, save a ref to it
        if (other.CompareTag("lockArea"))
        {
            obj = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if we exited the saved lock area, remove the ref to it
        if (other.gameObject == obj)
        {
            obj = null;
        }
    }


    public void OnDropped()
    {
        if (obj == null)
        {
            return;
        }

        //move this to the lock area
        transform.position = obj.transform.position;

    }
}
