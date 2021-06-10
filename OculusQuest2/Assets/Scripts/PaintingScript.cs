using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingScript : MonoBehaviour
{
    Vector3 startPosition;
    
    float rotationX = 0;
    float minRotationX = 90;
    float maxRotationX = 135;

    float rotationZ = 0;

    float minRotationZ = 60;
    float maxRotationZ = 90;

    private void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(transform.rotation.x);

        rotationX = transform.eulerAngles.x;
        //rotationX = Mathf.Clamp(rotationX, minRotationX, maxRotationX);
        transform.rotation = Quaternion.Euler(rotationX, 270, 90);
        //transform.position = new Vector3(transform.position.x, transform.position.y, startPosition.z);
        //transform.position = startPosition;

        //rotationZ = transform.eulerAngles.z;
        //rotationX = Mathf.Clamp(rotationZ, minRotationZ, maxRotationZ);
        //transform.rotation = Quaternion.Euler(90, 0, rotationZ);
        //transform.position = startPosition;

    }
}
