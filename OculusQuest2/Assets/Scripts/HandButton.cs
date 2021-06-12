using System.Collections;
using System.Collections.Generic;
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

    public ORDER orderNumber;
    public GlobePuzzleController globePuzzleController;

    public float pressTime;
    public float popUpTime;
    public float pressedScale;


    private Vector3 initialScale;
    private Vector3 finalScale;

    private bool pressed = false;



	void Start()
	{
        initialScale = transform.localScale;
        finalScale = initialScale * pressedScale;
    }


	private void OnTriggerEnter(Collider other)
    {
        if (!pressed && other.gameObject.layer == LayerMask.NameToLayer("Hand"))
        {
            pressed = true;
            // Move the button down
            StartCoroutine(MoveButton(true));
        }
    }
    // Return the button to the original position
    public void ResetButton()
    {
        // Move the button back up
        StartCoroutine(MoveButton(false));
    }

    private IEnumerator MoveButton(bool isMovingDown)
	{
        Vector3 startScale = transform.localScale;
        Vector3 endScale = isMovingDown ? finalScale : initialScale;
        float moveTime = isMovingDown ? pressTime : popUpTime;

        // Scale the button. Because the button position is at the center of the globe, this will move it down
        float t = 0;
        while (t < moveTime)
		{
            transform.localScale = Vector3.Lerp(startScale, endScale, t / moveTime);

            t += Time.deltaTime;
            yield return null;
        }


        if (isMovingDown)
		{
            // Tell the controller that we have been pressed
            globePuzzleController.ButtonPressed(this);
        }
		else
		{
            // Button has been moved back up, reset flag
            pressed = false;
        }
    }
}
