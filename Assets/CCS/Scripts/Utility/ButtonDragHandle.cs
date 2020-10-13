using CCS;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ButtonDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public ScrollRect mScrollRect = null;
    public void OnBeginDrag(PointerEventData eventData)
    {
        //Util.CallMethod("GlobalListener", "UIDragHandle", 1);
        if (null != mScrollRect)
        {
            mScrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Util.CallMethod("GlobalListener", "UIDragHandle", 2);
        if (null != mScrollRect)
        {
            mScrollRect.OnDrag(eventData);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        //Util.CallMethod("GlobalListener", "UIDragHandle", 3);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Util.CallMethod("GlobalListener", "UIDragHandle", 4);
        if (null != mScrollRect)
        {
            mScrollRect.OnEndDrag(eventData);
        }
    }
}
