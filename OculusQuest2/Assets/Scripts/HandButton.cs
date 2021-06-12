using UnityEngine;

public class HandButton : MonoBehaviour
{
    public enum ORDER
    {
        First = 0,
        Second,
        Third,
        Fourth,
        Fifth
    }
    
    public float speed = .5f;
    public Vector3 depth;
    public ORDER orderNumber;
    public GlobePuzzleController globePuzzleController;

    [HideInInspector]
    public bool arrived = false;

    private float fraction = 0;
    private bool pressed = false;
    private bool reset = false;
    private Vector3 startPosition;
    private Vector3 endPosition;

    private void Start()
    {
        startPosition = transform.position;
        endPosition = transform.position - depth;
    }

    private void Update()
    {
        //Debug.Log(startPosition);
        
        if (pressed && !reset)
        {
            if (fraction < 1)
            {
                fraction += Time.deltaTime * speed;
                transform.position = Vector3.Lerp(startPosition, endPosition, fraction);
            }
            else
            {
                arrived = true;
            }
        }

        if (reset)
        {
            if (fraction < 1)
            {
                fraction += Time.deltaTime * speed;
                transform.position = Vector3.Lerp(endPosition, startPosition, fraction);
            }
            else 
            {
                fraction = 0;
                reset = false;
                pressed = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!pressed && other.gameObject.name == "Index" && globePuzzleController.buttonPushed == null)
        {
            pressed = true;
            globePuzzleController.buttonPushed = this;
        }
    }

    // Return the button to the original position
    public void ResetButton()
    {
        reset = true;
        fraction = 0;
    }
}
