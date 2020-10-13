using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
 

public class EventHandler : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    delegate void OnPointerDownEvent();
    OnPointerDownEvent _OnPointerDownEvent;
    delegate void OnPointerEnterEvent();
    OnPointerDownEvent _OnPointerEnterEvent;
    delegate void OnOnPointerExitEvent();
    OnPointerDownEvent _OnPointerExitEvent;
    delegate void OnPointerUpEvent();
    OnPointerDownEvent _OnPointerUpEvent;
    delegate void OnLongPressDownEvent();
    OnPointerDownEvent _OnLongPressDownEvent;
    delegate void OnSwipEvent(float x, float y, float timeDiff);
    OnSwipEvent _OnSwipEvent;

    public float holdTime = 0.3f;
    private Vector2 downPosition, upPosition;
    public Vector2 LastDownPosition
    {
        get { return downPosition; }
    }
    public Vector2 LastUpPosition
    {
        get { return upPosition; }
    }
    private float downTime;
    public void OnPointerDown(PointerEventData eventData)
    {
        downPosition = eventData.position;
        if (null != _OnPointerDownEvent)
        {
            _OnPointerDownEvent();
        }
        Invoke("OnLongPress", holdTime);
        downTime = Time.realtimeSinceStartup;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (null != _OnPointerEnterEvent)
        {
            _OnPointerEnterEvent();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (null != _OnPointerExitEvent)
        {
            _OnPointerExitEvent();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        upPosition = eventData.position;
        CancelInvoke("OnLongPress");
        if (null != _OnPointerUpEvent)
        {
            _OnPointerUpEvent();
        }
        if (null != _OnSwipEvent)
        {
            float xOffset = upPosition.x - downPosition.x;
            float yOffset = upPosition.y - downPosition.y;
            _OnSwipEvent(xOffset, yOffset, Time.realtimeSinceStartup - downTime);
        }

    }

    public void AddOnPointerDown(GameObject go)
    {
        if (go == null ) return;
        _OnPointerDownEvent = delegate() {
            //luafunc.Call(go);
        } ;
    }

    public void AddOnPointerEnter(GameObject go)
    {
        if (go == null ) return;
        _OnPointerEnterEvent = delegate ()
        {
            //luafunc.Call(go);
        };
    }

    public void AddOnPointerExit(GameObject go)
    {
        if (go == null ) return;
        _OnPointerExitEvent = delegate ()
        {
            //luafunc.Call(go);
        };
    }

    public void AddOnPointerUp(GameObject go)
    {
        if (go == null ) return;
        _OnPointerUpEvent = delegate ()
        {
            //luafunc.Call(go);
        };
    }

    public void AddOnLongPressDown(GameObject go)
    {
        if (go == null ) return;
        _OnLongPressDownEvent = delegate ()
        {
            //luafunc.Call(go);
        };
    }

    public void AddOnSwip(GameObject go)
    {
        if (go == null ) return;
        _OnSwipEvent = delegate (float x,float y,float time)
        {
            //luafunc.Call(go,x,y, time);
        };
    }
    void OnLongPress()
    {
        if (null != _OnLongPressDownEvent)
        {
            _OnLongPressDownEvent();
        }
    }

    
}
