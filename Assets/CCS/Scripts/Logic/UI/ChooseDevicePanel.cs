using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCS;
using UnityEngine.UI;
using SimpleJSON;
using System;
using System.Linq;

public class ChooseDevicePanel : PanelBase
{
    //Trans
    private Button closeBtn;
    private Button playBtn;
    private Toggle chooseAllToggle;
    //Datas
    private DataGrid devicesDG;
    private Dictionary<int, Transform> diviceItemList = new Dictionary<int, Transform>();
    private JSONNode devicesData;
    static string currentChooseSeinum;
    
    private int      currentChooseVideoId;
    private string   currentChooseVideoUrl;
    private int      currentChooseFileType;
    private string   currentChooseMd5;
    private List<Devices> deviceList=new List<Devices>();
    public override void Init(params object[] args)
    {
        base.Init(args);
        currentChooseVideoUrl = args[0].ToString();
        currentChooseFileType = (int)args[1];
        currentChooseMd5 = args[2].ToString();
        currentChooseVideoId = (int)args[3];
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
        playBtn = skinTrans.Find("mask/bg/playBtn").GetComponent<Button>();
        chooseAllToggle = skinTrans.Find("mask/bg/tittleRoot/Toggle").GetComponent<Toggle>();
        devicesDG = skinTrans.Find("mask/bg/devicesList/contenView").GetComponent<DataGrid>();
        AddUIEvent();
    }

    void AddUIEvent()
    {
        closeBtn.onClick.AddListener(OnCloseBtnClick);
        playBtn.onClick.AddListener(OnPlayBtnClick);
        chooseAllToggle.onValueChanged.AddListener(OnClickChooseAllToggle);
    }

    void OnClickChooseAllToggle(bool isOn)
    {
        if (isOn)
        {
            deviceList.Clear();
            foreach (var vari in diviceItemList)
            {
                vari.Value.Find("Toggle").GetComponent<Toggle>().isOn = true;
            }
        }
        else
        {
            foreach (var vari in diviceItemList)
            {
                vari.Value.Find("Toggle").GetComponent<Toggle>().isOn = false;
            }
            deviceList.Clear();
        }
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
        PanManager.ClosePanel(PanelName.ChooseDevicePanel);
    }

    private void OnPlayBtnClick()
    {
        AdminMessage msg = new AdminMessage();
        msg.Type = DataType.AdminEvent;
        msg.Data.Control  = ControlState.Play;
        msg.Data.Progress = 0;
        msg.Data.Resource.Id  = currentChooseVideoId;
        msg.Data.Resource.Uri = currentChooseVideoUrl;
        msg.Data.Resource.FileType.Id = currentChooseFileType;
        msg.Data.Resource.Md5         = currentChooseMd5;
        msg.Data.Devices =deviceList.ToArray() ;

        NetManager.SendMessage(Util.ObjectToJson(msg));
        PanManager.OpenPanel<VideoPlayPanel>(PanelName.VideoPlayPanel, currentChooseVideoUrl,null);
        PanManager.ClosePanel(PanelName.ChooseDevicePanel);
        PanManager.AllHidenWithout(PanelName.VideoPlayPanel);
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
        obj.transform.Find("Toggle").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        obj.transform.Find("Toggle").GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
        {
            if (isOn)
            {
                deviceList.Add(new Devices(json["id"].AsInt,json["serialNumber"]));
            }
            else
            {
                Devices dev=null;
                foreach (Devices devices in deviceList)
                {
                    if (devices.SerialNumber.Equals(json["serialNumber"]))
                    {
                        dev = devices;
                        break;
                    }
                }

                if (dev != null)
                {
                    deviceList.Remove(dev);
                }
            }
        });

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
