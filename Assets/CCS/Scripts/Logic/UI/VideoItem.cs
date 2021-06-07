using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VideoItem : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{

	[SerializeField] private GameObject txtAll;
	[SerializeField] private GameObject txt;

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		txtAll.SetActive(true);
		txt.SetActive(false);
	}
	
	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		txtAll.SetActive(false);
		txt.SetActive(true);
	}
	
}
