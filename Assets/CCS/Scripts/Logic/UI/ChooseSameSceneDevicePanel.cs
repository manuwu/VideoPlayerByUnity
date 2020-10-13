using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCS;
using UnityEngine.UI;
using SimpleJSON;
using System;
using System.Linq;

public class ChooseSameSceneDevicePanel : PanelBase
{
    //Trans
    private Button closeBtn;
    //Datas
    private DataGrid devicesDG;
    private Dictionary<int, Transform> diviceItemList = new Dictionary<int, Transform>();
    private JSONNode devicesData;

    private List<Devices> deviceList=new List<Devices>();
    public override void Init(params object[] args)
    {
        base.Init(args);
    }

    public override void OnShowing()
    {
        base.OnShowing();
        if (isInit)
        {
            return;
        }
        isInit = true;
        Transform skinTrans = skin.transform;
        closeBtn = skinTrans.Find("mask/bg/closeBtn").GetComponent<Button>();
        devicesDG = skinTrans.Find("mask/bg/devicesList/contenView").GetComponent<DataGrid>();
        AddUIEvent();
    }

    void AddUIEvent()
    {
        closeBtn.onClick.AddListener(OnCloseBtnClick);
    }
    public override void AddEvent()
    {
        base.AddEvent();
        FrameMsgHandler.AddListener(NetMessageConst.UpdateOnlineDeviceInfo, UpdateOnlineDeviceInfo);
        GetDevicesListReq();
    }

    private void OnEnable()
    {
        if (isInit)
        {
            GetDevicesListReq();
        }
    }

    public override void RemoveEvent()
    {
        base.RemoveEvent();
        FrameMsgHandler.RemoveListener(NetMessageConst.UpdateOnlineDeviceInfo, UpdateOnlineDeviceInfo);
    }

    private void OnCloseBtnClick()
    {
        PanManager.ClosePanel(PanelName.ChooseSameSceneDevicePanel);
    }

    void GetDevicesListReq()
    {
        string url = string.Format("{0}{1}", AppConst.IP, NetMessageConst.GetDevicesInfoList);
        NetManager.HttpGetReq(url, GetDevicesListResp);
    }

    void GetDevicesListResp(string msg)
    {
        Debug.Log("manu GetDevicesListResp"+ msg);
        devicesDG.Destroy();
        JSONNode node  = JSON.Parse(msg);
        devicesData = node["data"].AsArray;
        devicesDG.MaxLength = devicesData.Count;
        deviceList.Clear();
        diviceItemList.Clear();
        ItemRender[] dgirs = devicesDG.getItemRenders();
        for (int i = 0; i < dgirs.Length; i++)
        {
            dgirs[i].AddItemSetDataFunc((int index) =>
            {
                SetToggleItemDate(dgirs[i].gameObj, devicesData[index]);
            });
            int idx = dgirs[i].m_renderData;
            SetToggleItemDate(dgirs[i].gameObj, devicesData[idx]);
        }
        NetMsgHandler.SendMsg(NetMessageConst.UpdateAllDeviceInfo, devicesData.Count.ToString());
    }

    void SetToggleItemDate(GameObject obj, JSONNode json)
    {
        string seriaNum = json["serialNumber"];
        obj.transform.Find("id").GetComponent<Text>().text = seriaNum;

        diviceItemList[json["id"].AsInt] = obj.transform;
        obj.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        obj.GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
        {
            if (isOn)
            {
                GetSameScreenReq(seriaNum);
            }
            else
            {
                CancelSameScreenReq();
            }
        });
    }

    void GetSameScreenReq(string devNum)
    {
        NetManager.HttpGetReq(string.Format("{0}{1}",AppConst.IP,
            string.Format(NetMessageConst.GetSameScreenMsg,devNum)), GetSameScreenResp);
    }
    
    void GetSameScreenResp(string json)
    {
        Debug.Log("manu GetSameScreenResp "+json);
        JSONNode jsonNode = JSON.Parse(json);
        if (jsonNode["code"].AsInt == 0)
        {
            PanManager.OpenPanel<VideoPlayPanel>(PanelName.VideoPlayPanel, 
                jsonNode["data"]["Resource"]["uri"].ToString().Trim('"'),jsonNode["data"]["Progress"].AsLong);
            PanManager.AllHidenWithout(PanelName.VideoPlayPanel); 
        }
        else
        {
            Debug.LogError("manu GetSameScreenResp is error "+json );
        }
    }
    
    void CancelSameScreenReq()
    {
        NetManager.HttpGetReq(string.Format("{0}{1}",AppConst.IP, NetMessageConst.CancelSameScreenMsg),null);
    }
    
    void UpdateOnlineDeviceInfo(JSONNode jsonNode)
    {
        for (int i = 0; i < jsonNode.Count; i++)
        {
            Transform item;
            if (diviceItemList.TryGetValue(jsonNode[i]["UserDevice"]["id"].AsInt, out item))
            {
                Text connectTxt=item.Find("connectStay").GetComponent<Text>();
                connectTxt.text= "已连接";
                connectTxt.color=Color.green;
                if (PlayState.Pause.Equals(jsonNode[i]["PlayerState"]))
                {
                    item.Find("playStay").GetComponent<Text>().text = "已暂停";
                }
                else if (PlayState.Play.Equals(jsonNode[i]["PlayerState"]))
                {
                    item.Find("playStay").GetComponent<Text>().text = "已播放";
                }
                else if (PlayState.Idle.Equals(jsonNode[i]["PlayerState"]))
                {
                    item.Find("playStay").GetComponent<Text>().text = "未播放";
                }
            }
        }
    }

    public override void OnCloseing()
    {
        base.OnCloseing();
        skin.SetActive(false);
    }

    public override void OnClosed()
    {
        base.OnClosed();
    }

    public override void OnDestroy()
    {
        DestroyObject(skin);
        Component.Destroy(this);
    }

}
