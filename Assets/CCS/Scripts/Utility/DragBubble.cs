using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class DragBubble : MonoBehaviour,IDragHandler,IEndDragHandler,IBeginDragHandler
 {
    public RectTransform target;
    private Vector2[] anchor = new Vector2[4];
    private Camera mainCamera;
    public RectTransform rectRoot;
    public float duration = 0.3f;
    private Button btn;
    private Vector2 middle = Vector2.one* 0.5f;
    private Vector2 boundsX, boundsY;
    private float width = 110f;
    void Start()
    {
        if(null == target)
            target = transform.GetComponent<RectTransform>();
        btn = GetComponent<Button>();
        mainCamera = GameObject.Find("Global_UI/UICamera").transform.GetComponent<Camera>();
        width = target.sizeDelta.x;
        float w = (Screen.width - width) / 2;
        float h = (Screen.height - width) / 2;
        boundsX = new Vector2(-w, w);
        boundsY = new Vector2(-h, h);
        Excute();
    }
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 pos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(target,
        eventData.position, mainCamera, out pos))
        {
            target.position = pos;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        Excute();
    }

    void Excute()
    {
        float x = target.anchoredPosition.x / Screen.width;
        float y = target.anchoredPosition.y / Screen.height;
        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            target.SetInsetAndSizeFromParentEdge(x >= 0 ? RectTransform.Edge.Right : RectTransform.Edge.Left, 0, width);
        }
        else
        {
            target.SetInsetAndSizeFromParentEdge(y >= 0 ? RectTransform.Edge.Top : RectTransform.Edge.Bottom, 0, width);
        }

        if(target.anchoredPosition.x < boundsX.x)
        {
            target.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, width);
        }
        else if (target.anchoredPosition.x > boundsX.y)
        {
            target.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, width);
        }

        if (target.anchoredPosition.y < boundsY.x)
        {
            target.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, width);
        }
        if (target.anchoredPosition.y > boundsY.y)
        {
            target.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, width);
        }
        btn.enabled = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        btn.enabled = false;
        target.anchorMin = middle;
        target.anchorMax = middle;
    }
}
