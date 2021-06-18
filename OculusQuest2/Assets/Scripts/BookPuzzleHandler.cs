using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class BookPuzzleHandler : MonoBehaviour
{
    public XRGrabInteractable[] interactables;

    // Order to pull books in using their index
    public int[] puzzleBookOrder;
    [Space]
    public UnityEvent onPuzzleComplete;
    public UnityEvent onPuzzleFailed;
    public UnityEvent onBookPulled;

    public Transform drawerTransform;
    public float openTime;
    public float openDistance;

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

        onBookPulled.Invoke();


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
                // Enable the objects
                foreach (var inter in interactables)
                {
                    inter.interactionLayerMask |= LayerMask.GetMask("Interactable");
                }

                onPuzzleComplete.Invoke();
                StartCoroutine(OpenDrawer());
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

    private IEnumerator OpenDrawer()
	{
        Vector3 start = drawerTransform.position;
        Vector3 end = start + new Vector3(0, 0, openDistance);

        float t = 0;
        while (t < openTime)
		{
            drawerTransform.position = Vector3.Lerp(start, end, t / openTime);

            t += Time.deltaTime;
            yield return null;
		}
    }
}
