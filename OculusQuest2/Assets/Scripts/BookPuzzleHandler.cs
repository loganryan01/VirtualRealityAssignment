using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BookPuzzleHandler : MonoBehaviour
{
    // Order to pull books in using their index
    public int[] puzzleBookOrder;
    
    public UnityEvent onPuzzleComplete;

    int[] currentOrder = { -1, -1, -1 };



    public void OnBookPulled(int index)
    {
        // Add book to current order
        for (int i = 0; i < currentOrder.Length; i++)
        {
            // If the book has already been added, ignore it
            if (currentOrder[i] == index)
			{
                return;
			}

            // If the index in unassigned, use it
            if (currentOrder[i] == -1)
            {
                currentOrder[i] = index;
                break;
            }
        }

        // Check if the books have been pulled in the correct order
        bool isCorrectOrder = true;
        for (int i = 0; i < puzzleBookOrder.Length; i++)
        {
            if (currentOrder[i] != puzzleBookOrder[i])
            {
                isCorrectOrder = false;
                break;
            }
        }

        if (isCorrectOrder)
        {
            onPuzzleComplete.Invoke();
            this.enabled = false;
        }
    }

    public void OnBookReturned(int index)
    {
        // Reset current order
        for (int i = 0; i < currentOrder.Length; i++)
        {
            currentOrder[i] = -1;
        }
    }
}
