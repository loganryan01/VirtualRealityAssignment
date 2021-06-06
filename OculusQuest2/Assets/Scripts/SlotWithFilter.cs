using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SlotWithFilter : XRSocketInteractor
{
	// What objects can be filtered by
    public enum FilterType
	{
        Name,
        Tag,
        Layer
	}

    [Space]
    public FilterType filterBy;
	// The filter to check against the sellected property
	public string filter;



	public override bool CanHover(XRBaseInteractable interactable)
	{
		return base.CanHover(interactable) && FilterInteractable(interactable);
	}
	public override bool CanSelect(XRBaseInteractable interactable)
	{
		return base.CanSelect(interactable) && FilterInteractable(interactable);
	}

	private bool FilterInteractable(XRBaseInteractable interactable)
	{
		switch (filterBy)
		{
			case FilterType.Name:
				return interactable.name == filter;
			case FilterType.Tag:
				return interactable.CompareTag(filter);
			case FilterType.Layer:
				return interactable.gameObject.layer == LayerMask.NameToLayer(filter);

			default:
				return true;
		}
	}
}
