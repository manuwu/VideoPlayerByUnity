using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMsgHandler {

    public delegate void DelMsgHandler(string msg);

    private static Dictionary<string, DelMsgHandler> mDicMsgs = new Dictionary<string, DelMsgHandler>();

    //添加监听者
    public static void AddListener(string msgType, DelMsgHandler handler)
    {
        //判空
        if (mDicMsgs == null)
            mDicMsgs = new Dictionary<string, DelMsgHandler>();
        if (!mDicMsgs.ContainsKey(msgType))
            mDicMsgs.Add(msgType, null);
        //增加监听
        mDicMsgs[msgType] += handler;
    }
    /// <summary>
    /// 去除对参数handler的监听
    /// </summary>
    /// <param name="msgType">消息类型</param>
    /// <param name="handler">被监听方法</param>
    public static void RemoveListener(string msgType, DelMsgHandler handler)
    {
        if (mDicMsgs != null && mDicMsgs.ContainsKey(msgType))
            mDicMsgs[msgType] -= handler;
    }

    /// <summary>
    /// 清除所有的监听者
    /// </summary>
    public static void ClearAllListeners()
    {
        if (mDicMsgs != null) mDicMsgs.Clear();
    }

    /// <summary>
    /// 分发消息
    /// </summary>
    /// <param name="msgType">消息类型</param>
    /// <param name="msg">分发的内容</param>
    public static void SendMsg(string msgType, string msg)
    {
        if (string.IsNullOrEmpty(msg))
            return;
        DelMsgHandler handler;
        if (mDicMsgs != null && mDicMsgs.TryGetValue(msgType, out handler))
        {
            if (handler != null)
                handler(msg);
        }
    }

    /// <summary>
    /// 分发消息
    /// </summary>
    /// <param name="msgType">消息类型</param>
    /// <param name="msg">分发的内容</param>
    public static void SendMsg(string msgType, JSONNode msg)
    {
        if (msg.Count==0)
            return;
        DelMsgHandler handler;
        if (mDicMsgs != null && mDicMsgs.TryGetValue(msgType, out handler))
        {
            if (handler != null)
                handler(msg);
        }
    }
}
