using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class GlobePuzzleController : MonoBehaviour
{
    /*
      - Reset the puzzle if the player pushes the wrong button
        - Check which button has been pushed
        - Check the order number of the button that has been pushed
        - if it is not the first one then reset the puzzle
        - if the first button has been pushed then leave it down
        - if the player gets the 2nd button incorrect then reset the puzzle
    */
    public XRGrabInteractable[] interactables;

    public Transform globeTop;
    public float openTime;
    public float angle;

    public UnityEvent onPuzzleComplete;
    public UnityEvent onPuzzleFail;


    private List<HandButton> pushedButtons = new List<HandButton>();
    private int correctAnswers = 0;
    

    // Called when a button is pushed
    public void ButtonPressed(HandButton button)
	{
        // Check if the button was correct
        StartCoroutine(CheckButton(button));
    }

    IEnumerator CheckButton(HandButton button)
    {
        yield return new WaitForSeconds(2);

        // Add the button to pushed buttons
        pushedButtons.Add(button);

        // Determine the expected result
        HandButton.ORDER expectedOrder = HandButton.ORDER.First;
        switch (correctAnswers)
        {
            case 1:
                expectedOrder = HandButton.ORDER.Second;
                break;
            case 2:
                expectedOrder = HandButton.ORDER.Third;
                break;
            case 3:
                expectedOrder = HandButton.ORDER.Fourth;
                break;
            case 4:
                expectedOrder = HandButton.ORDER.Fifth;
                break;
            default:
                break;
        }

        // Check if the answer is correct
        if (button.orderNumber == expectedOrder)
        {
            correctAnswers++;

            // Check if this was the last answer
            if (correctAnswers == 5)
			{
                // Enable interactables
                foreach (var inter in interactables)
                {
                    inter.interactionLayerMask |= LayerMask.GetMask("Interactable");
                }

                onPuzzleComplete.Invoke();
                // Disable the buttons
                foreach (var item in pushedButtons)
				{
                    item.enabled = false;
				}
                // Open the globe
                StartCoroutine(OpenGlobe());
            }
        }
        else
        {
            // Reset all pushed buttons
            foreach (var item in pushedButtons)
            {
                item.ResetButton();
            }
            pushedButtons.Clear();

            correctAnswers = 0;
        }
    }

    // Open the globe if the player pushes the buttons in the correct order
    private IEnumerator OpenGlobe()
	{
        Quaternion start = globeTop.rotation;
        Quaternion end = start * Quaternion.Euler(angle, 0, 0);

        float t = 0;
        while (t < openTime)
		{
            globeTop.rotation = Quaternion.Slerp(start, end, t / openTime);

            t += Time.deltaTime;
            yield return null;
		}
	}
}
