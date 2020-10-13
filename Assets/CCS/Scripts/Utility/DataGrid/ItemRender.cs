using UnityEngine;
using System.Collections;
using System;

public class ItemRender : MonoBehaviour
{
    public int m_renderData;
    public GameObject gameObj;
    [HideInInspector]
    public DataGrid m_owner;
    private Action<int> itemSetDataFunc = null;
    public void Awake() { }
    public void AddItemSetDataFunc(Action<int> callBack)
    {
        itemSetDataFunc = callBack;
    }

    public void SetData(int data)
    {
        if (data == m_renderData)
            return;
        m_renderData = data;
        if (null != itemSetDataFunc)
        {
            itemSetDataFunc(m_renderData);
        }
    }

    void Destroy()
    {
        if (itemSetDataFunc != null)
        {
            itemSetDataFunc = null;
        }
    }
}
