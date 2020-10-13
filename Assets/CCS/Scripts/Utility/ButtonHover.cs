using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Transform image;
    void Start()
    {
        image = gameObject.GetComponent<Button>().targetGraphic.transform;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }
        image.localScale = Vector3.one * 1.05f;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }

        image.localScale = Vector3.one;
    }
}