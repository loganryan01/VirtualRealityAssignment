using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class BallMachineScript : MonoBehaviour
{
    public XRSocketInteractor socket;

    public UnityEvent onBallPlaced;


    // Temp stuff untill proper assetes are added
    public GameObject puzzlePiecePrefab;
    public Transform spawnPoint;
    public float spawnDelay;
    


    void Start()
    {
        socket.onSelectEntered.AddListener(OnBallPlaced);
    }

    // Called when the ball is placed in the socket
    private void OnBallPlaced(XRBaseInteractable interactable)
	{
        // Disable the socket and delete the ball
        socket.socketActive = false;
        interactable.gameObject.SetActive(false);

        // Call event
        onBallPlaced.Invoke();


        StartCoroutine(SpawnPuzzlePiece());
    }

    // Temp code untill proper assets are added
    private IEnumerator SpawnPuzzlePiece()
	{
        yield return new WaitForSeconds(spawnDelay);

        GameObject obj = Instantiate(puzzlePiecePrefab, spawnPoint.position, spawnPoint.rotation);
        // Fix the name so it will go into the socket
        obj.name = "Puzzle Piece 5";
	}
}
