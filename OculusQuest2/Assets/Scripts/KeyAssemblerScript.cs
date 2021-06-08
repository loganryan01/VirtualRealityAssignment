using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KeyAssemblerScript : MonoBehaviour
{
    //prefab to instantiate when keys are combined
    public GameObject completeKeyPrefab;
    //the socket the complete key should be put in
    public XRSocketInteractor completeKeySocket;

    public GameObject leftBlock;
    public GameObject rightBlock;

    //the expected rotation of the left block
    public float leftBlockRotation;
    //the expected rotation of the right block
    public float rightBlockRotation;
    //threshold for accepting block rotation
    public float threshold = 5;

    //temp. how much to move each block
    public float moveDistance;
    //how long the animation is
    public float moveTime = 1;
    //effect used when complete key is created
    public ParticleSystem effect;


    private SnapRotate leftRotate;
    private SnapRotate rightRotate;
    // How many keys have been collected
    private int keyPieceCount = 0;



    void Start()
    {
        // Add event listeners
        leftBlock.GetComponentInChildren<XRSocketInteractor>().onSelectEntered.AddListener(OnKeyPieceLocked);
        rightBlock.GetComponentInChildren<XRSocketInteractor>().onSelectEntered.AddListener(OnKeyPieceLocked);

        leftRotate = leftBlock.GetComponent<SnapRotate>();
        rightRotate = rightBlock.GetComponent<SnapRotate>();
    }


    // Called when a key piece is placed in a slot
    private void OnKeyPieceLocked( XRBaseInteractable interactable)
	{
        // Change layer mask from Interactable and Key Piece to only Key Piece to prevent the player from picking it back up, while letting the socket keep it
        interactable.interactionLayerMask = LayerMask.GetMask("Key Piece");

        keyPieceCount++;
        // Check when both key pieces have been placed
        if (keyPieceCount == 2)
        {
            OnKeysPlaced();
        }
    }

    // Called when both key pieces have been placed
    private void OnKeysPlaced()
	{
        // Enable spinning for the blocks
        leftBlock.GetComponent<Rigidbody>().isKinematic = false;
        leftRotate.enabled = true;
        leftBlock.GetComponent<XRGrabInteractable>().interactionLayerMask = LayerMask.GetMask("Interactable");
        rightBlock.GetComponent<Rigidbody>().isKinematic = false;
        rightRotate.enabled = true;
        rightBlock.GetComponent<XRGrabInteractable>().interactionLayerMask = LayerMask.GetMask("Interactable");
    }

    // Called when the blocks have been rotated to the correct positions
    private IEnumerator OnBlocksRotated()
    {
        // Disable spinning the blocks and lock rotation
        leftBlock.transform.rotation = Quaternion.Euler(leftBlockRotation, 0, 0);
        leftBlock.GetComponent<Rigidbody>().isKinematic = true;
        leftBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        leftRotate.enabled = false;
        leftBlock.GetComponent<XRGrabInteractable>().interactionLayerMask = 0;

        rightBlock.transform.rotation = Quaternion.Euler(rightBlockRotation, 0, 0);
        rightBlock.GetComponent<Rigidbody>().isKinematic = true;
        rightBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        rightRotate.enabled = false;
        rightBlock.GetComponent<XRGrabInteractable>().interactionLayerMask = 0;


        // Get the initial positions of the blocks
        Vector3 leftStart = leftBlock.transform.position;
        Vector3 rightStart = rightBlock.transform.position;

        float t = 0;
        while (t < moveTime)
		{
            // Move the blocks together over time
            leftBlock.transform.position = leftStart + new Vector3(0, 0, moveDistance * (t / moveTime));
            rightBlock.transform.position = rightStart - new Vector3(0, 0, moveDistance * (t / moveTime));
            
            t += Time.deltaTime;
            yield return null;
		}


        // Disable the key pieces. They cant be destroied because other scripts still have refrences to them
        leftBlock.GetComponentInChildren<XRSocketInteractor>().selectTarget.gameObject.SetActive(false);
        rightBlock.GetComponentInChildren<XRSocketInteractor>().selectTarget.gameObject.SetActive(false);
        // Disable the socket interactors
        leftBlock.GetComponentInChildren<XRSocketInteractor>().socketActive = false;
        rightBlock.GetComponentInChildren<XRSocketInteractor>().socketActive = false;

        // Play particle effect
        effect.Play();

        // Instantiate the complete key in a socket between the blocks
        completeKeySocket.socketActive = true;
        Instantiate(completeKeyPrefab, completeKeySocket.transform.position, completeKeySocket.transform.rotation);
    }

    // Called when the complete key is removed from its socket
    public void OnCompleteKeyTaken()
    {
        // Disable the socket so the key cant be put back into it
        completeKeySocket.socketActive = false;
    }


    void Update()
	{
        // Both keys have been placed, so the blocks are rotatable
		if (keyPieceCount == 2)
		{
            // Get the rotation of the blocks
            float leftRot = leftRotate.GetRotation();
            float rightRot = rightRotate.GetRotation();

            // Check if the blocks are at the correct rotation
            if (leftRot > leftBlockRotation - threshold && leftRot < leftBlockRotation + threshold &&
                rightRot > rightBlockRotation - threshold && rightRot < rightBlockRotation + threshold)
			{
                keyPieceCount++;
                StartCoroutine(OnBlocksRotated());
            }
		}
	}
}
