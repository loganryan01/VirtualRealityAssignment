using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class SlideBookshelfScript : MonoBehaviour
{
    public Transform[] books;

    public XRSocketInteractor bookSocket;

    public float moveTime;
    public float moveDistance;

    public UnityEvent onBookPlaced;



    void Start()
    {
        bookSocket.onSelectEntered.AddListener(OnBookPlaced);
    }

    private void OnBookPlaced(XRBaseInteractable interactable)
	{
        onBookPlaced.Invoke();

        // Lock the position and rotation of the object and make it a child of this
        interactable.transform.position = bookSocket.transform.position;
        interactable.transform.rotation = bookSocket.transform.rotation;
        interactable.transform.parent = transform;

        //interactable.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        Rigidbody rb = interactable.GetComponent<Rigidbody>();
        Destroy(interactable);
        Destroy(rb);


        // Disable the socket
        bookSocket.socketActive = false;

        // Make books children of this again
        foreach (var book in books)
		{
            book.parent = transform;
            book.GetComponent<RailSystem>().enabled = false;
		}
        // Move the bookself
        StartCoroutine(MoveBookshelf());
    }

    private IEnumerator MoveBookshelf()
	{
        Vector3 start = transform.position;
        Vector3 end = start + new Vector3(moveDistance, 0, 0);

        float t = 0;
        while (t < moveTime)
		{
            transform.position = Vector3.Lerp(start, end, t / moveTime);

            t += Time.deltaTime;
            yield return null;
		}
	}
}
