using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BookPuzzleHandler : MonoBehaviour
{
    // Order to pull books in using their index
    public int[] puzzleBookOrder;
    [Space]
    public UnityEvent onPuzzleComplete;
    public UnityEvent onPuzzleFailed;

    List<int> currentOrder = new List<int>();
    List<int> pulledBooks = new List<int>();



    public void OnBookPulled(int index)
    {
        // If this book has not been returned yet, ignore it
        if (pulledBooks.Contains(index))
		{
            return;
        }
        
        pulledBooks.Add(index);
        currentOrder.Add(index);


        // If enough books have been pulled, check if the order is correct
        if (currentOrder.Count == puzzleBookOrder.Length)
		{
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
			else
			{
                onPuzzleFailed.Invoke();
                currentOrder.Clear();
            }
        }
    }

    public void OnBookReturned(int index)
	{
        pulledBooks.Remove(index);
	}
}
