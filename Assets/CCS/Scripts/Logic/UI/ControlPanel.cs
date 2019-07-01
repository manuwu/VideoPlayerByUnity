using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCS;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class ControlPanel : PanelBase {
    
    //Trans
    private Button poweOffBtn;
    private Button restartBtn;
    private Toggle currentChooseToggleSameScene;
    //Datas
    private DataGrid devicesDG;
    private Dictionary<string, Transform> diviceItemList = new Dictionary<string, Transform>();
    private JSONNode devicesData;
    static string currentChooseSeinum;
    public override void Init(params object[] args)
    {
        base.Init(args);
    }

    public override void OnShowing()
    {
        base.OnShowing();
        if (isInit)
            return;
        isInit = true;
        Transform skinTrans = skin.transform;
        poweOffBtn = skinTrans.Find("powerOffBtn").GetComponent<Button>();
        restartBtn = skinTrans.Find("restartBtn").GetComponent<Button>();
        devicesDG= skinTrans.Find("devicesList/contenView").GetComponent<DataGrid>();
        AddUIEvent();
    }

    void AddUIEvent()
    {
        poweOffBtn.onClick.AddListener(OnPoweOffClick);
        restartBtn.onClick.AddListener(OnRestartClick);
    }

    public override void AddEvent()
    {
        base.AddEvent();
        FrameMsgHandler.AddListener(NetMessageConst.UpdateOnlineDeviceInfo, UpdateOnlineDeviceInfo);
        GetDevicesListReq();
    }

    private void OnEnable()
    {
        if(isInit == true)
            GetDevicesListReq();
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

    void GetDevicesListReq()
    {
        string url = string.Format("{0}{1}", AppConst.IP, NetMessageConst.GetDevicesInfoList);
        NetManager.HttpGetReq(url, GetDevicesListResp);
    }

    void GetDevicesListResp(string msg)
    {
        Debug.Log(msg);
        devicesDG.Destroy();
        devicesData = JSON.Parse(msg);
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
        obj.transform.Find("num").GetComponent<Text>().text = (json["Id"].ToString()).Trim('"');

        string seriaNum = (json["SerialNumber"].ToString()).Trim('"');
        obj.transform.Find("id").GetComponent<Text>().text = seriaNum;

        diviceItemList[seriaNum] =obj.transform;
        if(!string.IsNullOrEmpty(currentChooseSeinum)&&currentChooseSeinum==seriaNum)
        {
            currentChooseToggleSameScene = obj.transform.Find("sameBtn").GetComponent<Toggle>();
            currentChooseToggleSameScene.isOn = true;
            currentChooseToggleSameScene.interactable = false;
        }
        obj.transform.Find("sameBtn").GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) => {
            if (isOn)
            {
                GetSameScreenReq(seriaNum);
                if (currentChooseToggleSameScene != null)
                {
                    currentChooseToggleSameScene.isOn = false;
                    currentChooseToggleSameScene.interactable = true;
                }
                currentChooseToggleSameScene = obj.transform.Find("sameBtn").GetComponent<Toggle>();
                currentChooseToggleSameScene.interactable = false;
                currentChooseSeinum = seriaNum;
            }
        });

    }

    void GetSameScreenReq(string devNum)
    {
        string url = string.Format("{0}{1}", AppConst.IP, NetMessageConst.GetSameScreenMsg);
        Dictionary<string, string> post = new Dictionary<string, string>();
        post.Add("sn", devNum);
        NetManager.HttpPostReq(url, post,null);
    }

    void UpdateOnlineDeviceInfo(JSONNode jsonNode)
    {
        Debug.Log("4444444444444");
        Debug.Log(jsonNode); 
        for (int i = 0; i < jsonNode.Count; i++)
        {
            Transform item;
            if (diviceItemList.TryGetValue((jsonNode[i]["UserDevice"]["SerialNumber"]).ToString().Trim('"'), out item))
            {
                item.Find("connectStay").GetComponent<Text>().text = "已连接";
                if (jsonNode[i]["PlayerState"]==PlayState.Pause)
                    item.Find("playStay").GetComponent<Text>().text = "已暂停";
                else if (jsonNode[i]["PlayerState"] == PlayState.Play)
                    item.Find("playStay").GetComponent<Text>().text = "已播放";
                else if (jsonNode[i]["PlayerState"] == PlayState.Idle)
                    item.Find("playStay").GetComponent<Text>().text = "未播放";

                int power = int.Parse(jsonNode[i]["PowerState"]);
                if (power<33)
                    item.Find("power").GetComponent<Image>().color = Color.red;
                else if (power >33&&power <66)
                    item.Find("power").GetComponent<Image>().color = Color.yellow;
                else if (power>66)
                    item.Find("power").GetComponent<Image>().color = Color.green;

                double wifi = double.Parse(jsonNode[i]["SignalStrength"]);
                item.transform.Find("wifi").GetComponent<Image>().sprite = TPManager.GetSprite("SignAtlas", string.Format( "ic_signal_wifi{0}",Math.Floor(wifi/20)));
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
