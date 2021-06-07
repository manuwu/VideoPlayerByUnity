using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCS;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class ControlPanel : PanelBase
{
    //Trans
    private Button poweOffBtn;
    private Button restartBtn;
    private Button stopPlayAllBtn;
    //Datas
    private DataGrid devicesDG;
    private Dictionary<string, Transform> diviceItemList = new Dictionary<string, Transform>();
    private JSONNode devicesData;
    private string currentSameSceneDeviceSeriNum;
    
    private Color green=new Color(0,0.569f,0.42f);
    private Color yellow=new Color(0.780f,0.549f,0);
    private Color res=new Color(0.667f,0,0);

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
        currentSameSceneDeviceSeriNum = string.Empty;
        Transform skinTrans = skin.transform;
        poweOffBtn = skinTrans.Find("powerOffBtn").GetComponent<Button>();
        restartBtn = skinTrans.Find("restartBtn").GetComponent<Button>();
        stopPlayAllBtn = skinTrans.Find("stopPlayAllBtn").GetComponent<Button>();
        devicesDG = skinTrans.Find("devicesList/contenView").GetComponent<DataGrid>();
        AddUIEvent();
    }

    void AddUIEvent()
    {
        poweOffBtn.onClick.AddListener(OnPoweOffClick);
        restartBtn.onClick.AddListener(OnRestartClick);
        stopPlayAllBtn.onClick.AddListener(OnStopPlayAllBtnClick);
    }

    public override void AddEvent()
    {
        base.AddEvent();
        FrameMsgHandler.AddListener(NetMessageConst.UpdateOnlineDeviceInfo, UpdateOnlineDeviceInfo);
        GetSameScreenDeviceReq();
    }

    private void OnEnable()
    {
        if (isInit)
            GetSameScreenDeviceReq();
    }

    public override void RemoveEvent()
    {
        base.RemoveEvent();
        FrameMsgHandler.RemoveListener(NetMessageConst.UpdateOnlineDeviceInfo, UpdateOnlineDeviceInfo);
    }

    private void OnPoweOffClick()
    {
        AdminMessage msg = new AdminMessage();
        msg.Type = DataType.AdminEvent;
        msg.Data.Control = ControlState.TurnOff;
        NetManager.SendMessage(Util.ObjectToJson(msg));
    }

    private void OnRestartClick()
    {
        AdminMessage msg = new AdminMessage();
        msg.Type = DataType.AdminEvent;
        msg.Data.Control = ControlState.Reboot;
        NetManager.SendMessage(Util.ObjectToJson(msg));
    }
    
    private void OnStopPlayAllBtnClick()
    {
        AdminMessage msg = new AdminMessage();
        msg.Type         = DataType.AdminEvent;
        msg.Data.Control = ControlState.Stop;
        NetManager.SendMessage(Util.ObjectToJson(msg));
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
        string id = json["id"];
        obj.transform.Find("num").GetComponent<Text>().text = id;
        string seriaNum = json["serialNumber"];
        obj.transform.Find("id").GetComponent<Text>().text = seriaNum;
        diviceItemList[seriaNum] = obj.transform;
        Toggle sameSceneToggle=obj.transform.Find("sameBtn").GetComponent<Toggle>();
        sameSceneToggle.onValueChanged.RemoveAllListeners();
        if (currentSameSceneDeviceSeriNum.Equals(seriaNum))
        {
            sameSceneToggle.isOn = true;
            obj.transform.Find("sameBtn/Text").GetComponent<Text>().text = "取消同屏";
        }
        else
        {
            sameSceneToggle.isOn = false;
            obj.transform.Find("sameBtn/Text").GetComponent<Text>().text = "同屏";
        }
        sameSceneToggle.onValueChanged.AddListener((bool isOn) =>
        {
            if (isOn)
            {
                obj.transform.Find("sameBtn/Text").GetComponent<Text>().text = "取消同屏";
                SetSameScreenReq(seriaNum);
            }
            else
            {
                CancelSameScreenReq();
                obj.transform.Find("sameBtn/Text").GetComponent<Text>().text = "同屏";
            }
        });
        
        obj.transform.Find("pausePlayBtn").GetComponent<Button>().onClick.RemoveAllListeners();
        obj.transform.Find("pausePlayBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            if (!BtnClickToken.TakeToken(1.5f))
            {
                return;
            }
            AdminMessage msg = new AdminMessage();
            msg.Type = DataType.AdminEvent;
            msg.Data.Control = ControlState.Stop;
            Devices[] devices={new Devices(int.Parse(id),seriaNum)};
            msg.Data.Devices = devices;
            NetManager.SendMessage(Util.ObjectToJson(msg));
        });
    }

    void CancelSameScreenReq()
    {
        NetManager.HttpGetReq(string.Format("{0}{1}",AppConst.IP, NetMessageConst.CancelSameScreenMsg),null);
    }
    
    void SetSameScreenReq(string devNum)
    {
        string url = string.Format("{0}{1}", AppConst.IP, NetMessageConst.SetSameScreenMsg);
        Dictionary<string, string> post = new Dictionary<string, string>();
        post.Add("sn", devNum);
        NetManager.HttpPostReq(url, post, SetSameScreenResp);
    }

    void SetSameScreenResp(string json)
    {
        Debug.Log("manu SetSameScreenResp json"+json);
    }
    
    void GetSameScreenDeviceReq()
    {
        string url = string.Format("{0}{1}", AppConst.IP, NetMessageConst.GetSameScreenDeviceMsg);
        NetManager.HttpGetReq(url, GetSameScreenDeviceResp);
    }

    void GetSameScreenDeviceResp(string msg)
    {
        Debug.Log("manu GetSameScreenDeviceResp "+msg);
        JSONNode node  = JSON.Parse(msg);
        if (node["code"].AsInt == 0)
        {
            currentSameSceneDeviceSeriNum = node["data"];
        }

        GetDevicesListReq();
    }

    void UpdateOnlineDeviceInfo(JSONNode jsonNode)
    {
        for (int i = 0; i < jsonNode.Count; i++)
        {
            Transform item;
            if (diviceItemList.TryGetValue(jsonNode[i]["userDevice"]["serialNumber"], out item))
            {
                Text connectTxt=item.Find("connectStay").GetComponent<Text>();
                connectTxt.text= "已连接";
                connectTxt.color=Color.green;
                if (PlayState.Pause.Equals(jsonNode[i]["playerState"]))
                {
                    item.Find("playStay").GetComponent<Text>().text = "已暂停";
                    item.Find("pausePlayBtn").gameObject.SetActive(true);
                }
                else if (PlayState.Play.Equals(jsonNode[i]["playerState"]))
                {
                    item.Find("playStay").GetComponent<Text>().text = "正在播放";
                    item.Find("pausePlayBtn").gameObject.SetActive(true);
                }
                else if (PlayState.Idle.Equals(jsonNode[i]["playerState"]))
                {
                    item.Find("playStay").GetComponent<Text>().text = "未播放";
                    item.Find("pausePlayBtn").gameObject.SetActive(false);
                }

                int power = jsonNode[i]["powerState"].AsInt;

                if (power < 33)
                {
                    item.Find("power").GetComponent<Image>().color = res;
                }
                else if (power > 33 && power < 66)
                {
                    item.Find("power").GetComponent<Image>().color = yellow;
                }
                else if (power > 66)
                {
                    item.Find("power").GetComponent<Image>().color = green;
                }

                double wifi = jsonNode[i]["signalStrength"].AsDouble;
                item.transform.Find("wifi").GetComponent<Image>().sprite = TPManager.GetSprite("SignAtlas", string.Format("ic_signal_wifi{0}", Math.Floor(wifi / 20)));
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
