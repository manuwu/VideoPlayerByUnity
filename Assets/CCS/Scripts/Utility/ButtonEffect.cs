using UnityEngine;
using UnityEngine.EventSystems;
using CCS;
using UnityEngine.UI;

public class ButtonEffect : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    //是否在点击时修改大小
    public bool isChangeScale=true;

    public GameObject Image;


    public void OnPointerDown(PointerEventData eventData)
    {
        if (gameObject.activeSelf==false)
        {
            return;
        }

        if (Image != null)
        {
            Image.gameObject.SetActive(true);
        }
        if (isChangeScale == false)
        {
            return;
        }
        RectTransform rtf = this.transform.GetComponent("RectTransform") as RectTransform;
        if (rtf!=null)
        {
            rtf.localScale = new Vector3(1.05f, 1.05f, rtf.localScale.z);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }
        if (Image != null)
        {
            Image.gameObject.SetActive(false);
        }
        if (isChangeScale == false)
        {
            return;
        }
        RectTransform rtf = this.transform.GetComponent("RectTransform") as RectTransform;
        if (rtf != null)
        {
            rtf.localScale = new Vector3(1f, 1f, rtf.localScale.z);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
    }
}
