using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class PuzzleBoxScript : MonoBehaviour
{
    public XRSocketInteractor[] puzzleSockets;
    [Space]
    public XRSocketInteractor gemSocket;

    public Transform boxLid;
    public float openTime;
    public float openAngle;

    public UnityEvent onBoxOpen;


    private int puzzlePieceCount = 0;
    private bool isGemPlaced = false;



    void Start()
    {
        // Add listeners to puzzle piece sockets, passing the interactable and the socket
        foreach (XRSocketInteractor sock in puzzleSockets)
		{
            sock.onSelectEntered.AddListener((XRBaseInteractable interactable) => OnPuzzlePiecePlaced(interactable, sock));
		}
        // Add listener to gem socket
        gemSocket.onSelectEntered.AddListener(OnGemPlaced);
    }

    // Called when a puzzle piece has been placed
	private void OnPuzzlePiecePlaced(XRBaseInteractable interactable, XRSocketInteractor socket)
	{
        // Disable the socket and lock the object
        socket.socketActive = false;
        interactable.interactionLayerMask = 0;
        interactable.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        interactable.transform.position = socket.transform.position;
        interactable.transform.rotation = socket.transform.rotation;

        puzzlePieceCount++;
        // Check if the puzzle is complete
        if (puzzlePieceCount == puzzleSockets.Length && isGemPlaced)
		{
            OnPuzzleComplete();
		}
	}
    // Called when the gem has been placed
    private void OnGemPlaced(XRBaseInteractable interactable)
	{
        // Disable the socket and lock the object
        gemSocket.socketActive = false;
        interactable.interactionLayerMask = 0;
        interactable.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        interactable.transform.position = gemSocket.transform.position;
        interactable.transform.rotation = gemSocket.transform.rotation;
        
        isGemPlaced = true;
        // Check if the puzzle is complete
        if (puzzlePieceCount == puzzleSockets.Length && isGemPlaced)
        {
            OnPuzzleComplete();
        }
    }

    // Called when the gem and all puzzle pieces have been placed
    private void OnPuzzleComplete()
	{
        onBoxOpen.Invoke();
        // Open the box
        StartCoroutine(OpenBox());
    }

    private IEnumerator OpenBox()
	{
        Quaternion startRot = boxLid.rotation;

        float t = 0;
        while (t < openTime)
		{
            boxLid.rotation = startRot * Quaternion.Euler(openAngle * (t / openTime), 0, 0);

            t += Time.deltaTime;
            yield return null;
		}
	}
}
