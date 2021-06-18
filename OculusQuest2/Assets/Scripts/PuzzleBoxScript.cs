using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class PuzzleBoxScript : MonoBehaviour
{
    public XRGrabInteractable[] interactables;

    public XRSocketInteractor[] puzzleSockets;
    [Space]
    public XRSocketInteractor gemSocket;

    public Transform boxLid;
    public float openTime;
    public float openAngle;

    public UnityEvent onBoxOpen;


    // List of correct puzzle pieces placed
    private List<XRBaseInteractable> correctPuzzlePieces = new List<XRBaseInteractable>();
    private bool isGemPlaced = false;



    void Start()
    {
        puzzleSockets[0].onSelectEntered.AddListener((XRBaseInteractable interactable) => OnPuzzlePiecePlaced(interactable, puzzleSockets[0], 1));
        puzzleSockets[0].onSelectExited.AddListener((XRBaseInteractable interactable) => OnPieceRemoved(interactable, 1));

        puzzleSockets[1].onSelectEntered.AddListener((XRBaseInteractable interactable) => OnPuzzlePiecePlaced(interactable, puzzleSockets[1], 2));
        puzzleSockets[1].onSelectExited.AddListener((XRBaseInteractable interactable) => OnPieceRemoved(interactable, 2));

        puzzleSockets[2].onSelectEntered.AddListener((XRBaseInteractable interactable) => OnPuzzlePiecePlaced(interactable, puzzleSockets[2], 3));
        puzzleSockets[2].onSelectExited.AddListener((XRBaseInteractable interactable) => OnPieceRemoved(interactable, 3));

        puzzleSockets[3].onSelectEntered.AddListener((XRBaseInteractable interactable) => OnPuzzlePiecePlaced(interactable, puzzleSockets[3], 4));
        puzzleSockets[3].onSelectExited.AddListener((XRBaseInteractable interactable) => OnPieceRemoved(interactable, 4));

        puzzleSockets[4].onSelectEntered.AddListener((XRBaseInteractable interactable) => OnPuzzlePiecePlaced(interactable, puzzleSockets[4], 5));
        puzzleSockets[4].onSelectExited.AddListener((XRBaseInteractable interactable) => OnPieceRemoved(interactable, 5));

        puzzleSockets[5].onSelectEntered.AddListener((XRBaseInteractable interactable) => OnPuzzlePiecePlaced(interactable, puzzleSockets[5], 6));
        puzzleSockets[5].onSelectExited.AddListener((XRBaseInteractable interactable) => OnPieceRemoved(interactable, 6));



        // Add listeners to puzzle piece sockets
        //for (int i = 0; i < puzzleSockets.Length; i++)
        //{
        //    puzzleSockets[i].onSelectEntered.AddListener((XRBaseInteractable interactable) => OnPuzzlePiecePlaced(interactable, puzzleSockets[i], i + 1));
        //    puzzleSockets[i].onSelectExited.AddListener((XRBaseInteractable interactable) => OnPieceRemoved(interactable, i + 1));
        //}

        // Add listener to gem socket
        gemSocket.onSelectEntered.AddListener(OnGemPlaced);
    }

    // Called when a puzzle piece has been placed
	private void OnPuzzlePiecePlaced(XRBaseInteractable interactable, XRSocketInteractor socket, int correctPuzzlePiece)
	{
        if (interactable == null)
        {
            return;
        }

        // Snap the puzzle piece to the socket imediatly
        interactable.transform.position = socket.transform.position;
        interactable.transform.rotation = socket.transform.rotation;

        // If the piece is correct, add it to the list
        if (interactable.name == "Puzzle Piece " + correctPuzzlePiece.ToString())
        {
            Debug.Log("done");
            correctPuzzlePieces.Add(interactable);
        }


        // Check if the puzzle is complete
        if (correctPuzzlePieces.Count == puzzleSockets.Length && isGemPlaced)
		{
            OnPuzzleComplete();
		}
	}
    // Called when a puzzle piece has been removed
    private void OnPieceRemoved(XRBaseInteractable interactable, int correctPuzzlePiece)
    {
        // If the piece was correct, remove it from the list
        if (interactable != null && interactable.name == "Puzzle Piece " + correctPuzzlePiece.ToString())
        {
            correctPuzzlePieces.Remove(interactable);
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
        if (correctPuzzlePieces.Count == puzzleSockets.Length && isGemPlaced)
        {
            OnPuzzleComplete();
        }
    }


    // Called when the gem and all puzzle pieces have been placed
    private void OnPuzzleComplete()
	{
        // Enable the objects
        foreach (var inter in interactables)
        {
            inter.interactionLayerMask |= LayerMask.GetMask("Interactable");
        }

        // Disable all sockets
        foreach (XRSocketInteractor socket in puzzleSockets)
        {
            socket.socketActive = false;
        }

        // Lock all puzzle pieces
        foreach (XRBaseInteractable interactable in correctPuzzlePieces)
        {
            interactable.interactionLayerMask = 0;
            interactable.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }


        onBoxOpen.Invoke();
        // Open the box
        StartCoroutine(OpenBox());
    }

    private IEnumerator OpenBox()
	{
        Quaternion startRot = boxLid.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(openAngle, 0, 0);

        float t = 0;
        while (t < openTime)
		{
            boxLid.rotation = Quaternion.Slerp(startRot, endRot, t / openTime);

            t += Time.deltaTime;
            yield return null;
		}
	}
}
