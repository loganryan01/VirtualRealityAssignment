﻿using System.Collections;
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
        // Add listeners to puzzle piece sockets
        for (int i = 0; i < puzzleSockets.Length; i++)
		{
            puzzleSockets[i].onSelectEntered.AddListener((XRBaseInteractable interactable) => OnPuzzlePiecePlaced(interactable, puzzleSockets[i], i));
            puzzleSockets[i].onSelectExited.AddListener((XRBaseInteractable interactable) => OnPieceRemoved(interactable, i));
		}

        // Add listener to gem socket
        gemSocket.onSelectEntered.AddListener(OnGemPlaced);
    }

    // Called when a puzzle piece has been placed
	private void OnPuzzlePiecePlaced(XRBaseInteractable interactable, XRSocketInteractor socket, int correctPuzzlePiece)
	{
        // Snap the puzzle piece to the socket imediatly
        interactable.transform.position = socket.transform.position;
        interactable.transform.rotation = socket.transform.rotation;

        // If the piece is correct, add it to the list
        if (interactable.name == "Puzzle Piece " + correctPuzzlePiece.ToString())
        {
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
        if (interactable.name == "Puzzle Piece " + correctPuzzlePiece.ToString())
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
            inter.interactionLayerMask |= LayerMask.GetMask("interactables");
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

        float t = 0;
        while (t < openTime)
		{
            boxLid.rotation = startRot * Quaternion.Euler(openAngle * (t / openTime), 0, 0);

            t += Time.deltaTime;
            yield return null;
		}
	}
}
