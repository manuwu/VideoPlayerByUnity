
using CCS;
using UnityEngine;
using UnityEngine.EventSystems;

public class UGUIClickHandler : MonoBehaviour, IPointerClickHandler
{
    public delegate void PointerEvetCallBackFunc(GameObject target, PointerEventData eventData);
    //public string m_sound = AB.AUDIO_UI;
    public event PointerEvetCallBackFunc onPointerClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Input.touchCount > 1)//Edit by limanru
            return;

        //if (!string.IsNullOrEmpty(m_sound))
        //    AudioManager.PlayUISound(m_sound);

        if (onPointerClick != null)
            onPointerClick(gameObject, eventData);
        else
            Util.LogError("系统暂未开放");
    }

    public void RemoveAllHandler(bool isDestroy = true)
    {
        onPointerClick = null;
        if (isDestroy) DestroyImmediate(this);
    }

    public static UGUIClickHandler Get(GameObject go)
    {
        UGUIClickHandler listener = go.GetComponent<UGUIClickHandler>();
        if (listener == null)
            listener = go.AddComponent<UGUIClickHandler>();
        return listener;
    }

    public static UGUIClickHandler Get(GameObject go, string sound)
    {
        UGUIClickHandler listener = Get(go);
        //listener.m_sound = sound;
        return listener;
    }

    public static UGUIClickHandler Get(Transform tran, string sound)
    {
        UGUIClickHandler listener = Get(tran);
        //listener.m_sound = sound;
        return listener;
    }

    public static UGUIClickHandler Get(Transform tran)
    {
        return Get(tran.gameObject);
    }
}

