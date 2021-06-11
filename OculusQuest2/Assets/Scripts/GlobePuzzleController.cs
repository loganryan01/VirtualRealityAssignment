using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    //private HandButton[] globeButtons;
    private List<HandButton> globeButtons = new List<HandButton>();

    [HideInInspector]
    public HandButton buttonPushed;
    
    private int correctAnswers = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (buttonPushed != null && buttonPushed.arrived)
        {
            HandButton.ORDER buttonNumber = HandButton.ORDER.First;
            
            switch (correctAnswers)
            {
                case 1:
                    buttonNumber = HandButton.ORDER.Second;
                    break;
                case 2:
                    buttonNumber = HandButton.ORDER.Third;
                    break;
                case 3:
                    buttonNumber = HandButton.ORDER.Fourth;
                    break;
                case 4:
                    buttonNumber = HandButton.ORDER.Fifth;
                    break;
                default:
                    break;
            }

            buttonPushed.arrived = false;
            StartCoroutine(CheckButton(buttonNumber));
        }
    }

    IEnumerator CheckButton(HandButton.ORDER buttonNumber)
    {
        yield return new WaitForSeconds(2);

        if (buttonPushed != null && buttonPushed.orderNumber == buttonNumber)
        {
            correctAnswers++;
            globeButtons.Add(buttonPushed);
            Debug.Log("Correct Answer");
        }
        else if (buttonPushed != null)
        {
            Debug.Log("Incorrect Answer. Resetting Puzzle!");
            buttonPushed.ResetButton();

            foreach (var item in globeButtons)
            {
                item.ResetButton();
            }

            globeButtons.Clear();

            correctAnswers = 0;
        }

        buttonPushed = null;
    }
}
