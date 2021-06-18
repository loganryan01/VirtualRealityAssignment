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

    private List<GameObject> keyPieces = new List<GameObject>();



    void Start()
    {
        // Add event listeners
        leftBlock.GetComponentInChildren<XRSocketInteractor>().onSelectEntered.AddListener((XRBaseInteractable inter) => OnKeyPieceLocked(inter, leftBlock.GetComponentInChildren<XRSocketInteractor>()));
        rightBlock.GetComponentInChildren<XRSocketInteractor>().onSelectEntered.AddListener((XRBaseInteractable inter) => OnKeyPieceLocked(inter, rightBlock.GetComponentInChildren<XRSocketInteractor>()));

        leftRotate = leftBlock.GetComponentInChildren<SnapRotate>();
        rightRotate = rightBlock.GetComponentInChildren<SnapRotate>();
    }


    // Called when a key piece is placed in a slot
    private void OnKeyPieceLocked(XRBaseInteractable interactable, XRSocketInteractor socket)
	{
        // Add the object to the list of key pieces
        keyPieces.Add(interactable.gameObject);

        // Prevent it from being picked up again
        interactable.interactionLayerMask = 0;
        // Snap the object to the socket, making it a child
        interactable.transform.position = socket.transform.position;
        interactable.transform.rotation = socket.transform.rotation * Quaternion.Euler(90,0,0);
        interactable.transform.parent = socket.transform;
        // Set kinematic late
        StartCoroutine(KeyPieceLockedLate(interactable));


        keyPieceCount++;
        // Check when both key pieces have been placed
        if (keyPieceCount == 2)
        {
            OnKeysPlaced();
        }
    }
    private IEnumerator KeyPieceLockedLate(XRBaseInteractable interactable)
    {
        yield return null;
        // Set interactable to kinematic after a frame so it gets dropped properly
        interactable.GetComponent<Rigidbody>().isKinematic = true;
    }

    // Called when both key pieces have been placed
    private void OnKeysPlaced()
	{
        // Get the rigidbodies
        Rigidbody leftRB = leftBlock.GetComponent<Rigidbody>();
        Rigidbody rightRB = rightBlock.GetComponent<Rigidbody>();

        // Fix the rigidbodies data
        leftRB.ResetCenterOfMass();
        leftRB.ResetInertiaTensor();
        rightRB.ResetCenterOfMass();
        rightRB.ResetInertiaTensor();

        // Enable spinning for the blocks
        leftRB.isKinematic = false;
        leftRotate.enabled = true;
        leftBlock.GetComponentInChildren<XRGrabInteractable>().interactionLayerMask = LayerMask.GetMask("Interactable");
        rightRB.isKinematic = false;
        rightRotate.enabled = true;
        rightBlock.GetComponentInChildren<XRGrabInteractable>().interactionLayerMask = LayerMask.GetMask("Interactable");
    }

    // Called when the blocks have been rotated to the correct positions
    private IEnumerator OnBlocksRotated()
    {
        // Get the rigidbodies
        Rigidbody leftRB = leftBlock.GetComponent<Rigidbody>();
        Rigidbody rightRB = rightBlock.GetComponent<Rigidbody>();

        // Disable spinning the blocks and lock rotation
        leftBlock.transform.rotation = Quaternion.Euler(leftBlockRotation, 0, 0);
        leftRB.isKinematic = true;
        leftRB.constraints = RigidbodyConstraints.FreezeAll;
        leftRotate.enabled = false;
        leftBlock.GetComponentInChildren<XRGrabInteractable>().interactionLayerMask = 0;

        rightBlock.transform.rotation = Quaternion.Euler(rightBlockRotation, 0, 0);
        rightRB.isKinematic = true;
        rightRB.constraints = RigidbodyConstraints.FreezeAll;
        rightRotate.enabled = false;
        rightBlock.GetComponentInChildren<XRGrabInteractable>().interactionLayerMask = 0;


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


        // Disable the key piece in the block
        foreach (var obj in keyPieces)
        {
            obj.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        }

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
