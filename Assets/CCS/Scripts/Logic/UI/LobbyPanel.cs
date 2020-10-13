using System;
using UnityEngine;
using CCS;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleJSON;

public class LobbyPanel : PanelBase
{
    //Trans
    private Button     playAllNetBtn;
    private Button     playLocalBtn;
    private Toggle     homeTog;
    private Toggle     editTog;
    private DataGrid   videoDG;
    private DataGrid   tagsDG;
    private GameObject toggleTemp;
    private GameObject pageVideoTemp;
    private Transform  toggleParent;
    private Transform  videoParent;
    private GameObject videoRoot;
    private Text   greenNum;
    private Text   yellowNumt;
    private Text   redNum;
    private Text   registNumt;
    private Text   openNum;
    private Text   connectNum;
    private Text   comeinNum;
    private Toggle currentChooseItem;

    //Datas
    private Dictionary<int, GameObject> toggleData;
    //Dictionary<int, GameObject> videoPageDate;
    private int      redCount;
    private int      greenCount;
    private int      yellowCount;
    private int      connectCount;
    private string   currentChooseVideoUrl;
    private int      currentChooseVideoId;
    private int      currentChooseFileType;
    private int      currentChooseMenuId;
    private string   currentChooseMd5;
    private JSONNode currentChooseVideo;

    private VideoPlayPanel m_VideoPlayPanel;

    private void Awake()
    {
        toggleData = new Dictionary<int, GameObject>();
        //m_VideoPlayPanel
        //videoPageDate = new Dictionary<int, GameObject>();
    }

    private void Start()
    {
        Cursor.visible = true;
    }

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
        playAllNetBtn = skinTrans.Find("rightRoot/playAllNetbtn").GetComponent<Button>();
        playLocalBtn  = skinTrans.Find("rightRoot/playLocalBtn").GetComponent<Button>();
        homeTog       = skinTrans.Find("rightRoot/toggleGroup/toggleHome").GetComponent<Toggle>();
        editTog       = skinTrans.Find("rightRoot/toggleGroup/toggleEdit").GetComponent<Toggle>();

        videoRoot     = skinTrans.Find("videoListRoot").gameObject;
        tagsDG        = videoRoot.transform.Find("topRoot/toggleGroup").GetComponent<DataGrid>();
        videoDG       = videoRoot.transform.Find("videoPageRoot/videoPagetemp/contenView").GetComponent<DataGrid>();
        toggleParent  = skinTrans.Find("videoListRoot/leftRoot/toggleGroup/Viewport/Content");
        toggleTemp    = toggleParent.Find("toggleItem").gameObject;
        videoParent   = skinTrans.Find("videoListRoot/videoPageRoot");
        pageVideoTemp = videoParent.Find("videoPagetemp").gameObject;
        greenNum      = skinTrans.Find("rightRoot/detialRoot/green/Text").GetComponent<Text>();
        yellowNumt    = skinTrans.Find("rightRoot/detialRoot/yellow/Text").GetComponent<Text>();
        redNum        = skinTrans.Find("rightRoot/detialRoot/red/Text").GetComponent<Text>();
        registNumt    = skinTrans.Find("rightRoot/detialRoot/regist/Text").GetComponent<Text>();
        openNum       = skinTrans.Find("rightRoot/detialRoot/open/Text").GetComponent<Text>();
        connectNum    = skinTrans.Find("rightRoot/detialRoot/conect/Text").GetComponent<Text>();
        comeinNum     = skinTrans.Find("rightRoot/detialRoot/comein/Text").GetComponent<Text>();
        AddUIEvent();
    }

    private void AddUIEvent()
    {
        playAllNetBtn.onClick.AddListener(PlayAllNet);
        playLocalBtn.onClick.AddListener(PlayLocal);
        homeTog.onValueChanged.AddListener(OnClickHomeToggle);
        editTog.onValueChanged.AddListener(OnClickEditTogglr);
    }

    private void PlayAllNet()
    {
        if (string.IsNullOrEmpty(currentChooseVideoUrl))
        {
            PanManager.ShowToast("您还没选择视频");
            return;
        }

        PanManager.OpenPanel<ChooseDevicePanel>(PanelName.ChooseDevicePanel, 
            currentChooseVideoUrl,currentChooseFileType,currentChooseMd5,currentChooseVideoId);
    }

    private void PlayLocal()
    {
        if (currentChooseVideoUrl == null)
        {
            PanManager.ShowToast("您还没选择视频");
            return;
        }
        PanManager.OpenPanel<VideoPlayPanel>(PanelName.VideoPlayPanel, currentChooseVideoUrl,null);
//        PanManager.OpenPanel<ImagePlayPanel>(PanelName.ImagePlayPanel, currentChooseVideoUrl,null);
        currentChooseItem.isOn = false;
        currentChooseVideoUrl  = null;
//        PanManager.AllHidenWithout(PanelName.ImagePlayPanel);
        PanManager.AllHidenWithout(PanelName.VideoPlayPanel);
    }

    private void OnClickHomeToggle(bool isOn)
    {
        if (isOn)
        {
            videoRoot.SetActive(true);
            PanManager.ClosePanel(PanelName.ControlPanel);
        }
    }

    private void OnClickEditTogglr(bool isOn)
    {
        if (isOn)
        {
            videoRoot.SetActive(false);
            PanManager.OpenPanel<ControlPanel>(PanelName.ControlPanel);
        }
    }

    public override void AddEvent()
    {
        base.AddEvent();
        NetMsgHandler.AddListener(NetMessageConst.StatusEvent, StatusEvent);
        NetMsgHandler.AddListener(NetMessageConst.UpdateAllDeviceInfo, UpdateAllDeviceInfo);
        GetToggleListReq();
        GetDevicesListReq();
    }

    public override void RemoveEvent()
    {
        base.RemoveEvent();
        NetMsgHandler.RemoveListener(NetMessageConst.StatusEvent, StatusEvent);
        NetMsgHandler.RemoveListener(NetMessageConst.UpdateAllDeviceInfo, UpdateAllDeviceInfo);

    }

    private void GetToggleListReq()
    {
        string url = string.Format("{0}{1}", AppConst.IP,NetMessageConst.GetToggleInfoList);
        NetManager.HttpGetReq(url, GetToggleListResp);
    }

    private void GetToggleListResp(string msg)
    {
        Debug.Log("manu GetToggleListResp "+msg);
        JSONNode jsonNode = JSON.Parse(msg);
        JSONArray dataArr = jsonNode["data"].AsArray;
        if (toggleData.Count==0)
        {
            for (int i = 0; i < dataArr.Count; i++)
            {
                GameObject obj = GameObject.Instantiate(toggleTemp);
                obj.transform.SetParent(toggleParent,false);
                obj.transform.localScale = Vector3.one;
                obj.SetActive(true);

                toggleData.Add(dataArr[i]["id"].AsInt, obj);
                SetToggleItemDate(obj, i,dataArr[i]);
            }
        }
    }

    private void SetToggleItemDate(GameObject obj, int id, JSONNode json)
    {
        if (id == 0)
        {
            SetVideoSubPage(json["id"].AsInt);
            obj.transform.GetComponent<Toggle>().isOn = true;
        }
        obj.transform.Find("Background/Text").GetComponent<Text>().text =json["name"];
        obj.transform.Find("Background/Checkmark/Text").GetComponent<Text>().text = json["name"];
        obj.transform.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        obj.transform.GetComponent<Toggle>().onValueChanged.AddListener(
            (bool isOn)=>
            {
                if (isOn)
                    SetVideoSubPage(json["id"].AsInt);
            });
    }

    private void SetVideoSubPage(int id)
    {
        GetTagsListByToggleReq(id);
    }

    private void GetTagsListByToggleReq(int toggleID)
    {
        currentChooseMenuId = toggleID;
        string url = string.Format("{0}{1}{2}", AppConst.IP, NetMessageConst.GetTagsListByToggle, toggleID);
        NetManager.HttpGetReq(url, GetTagsListByToggleResp);
    }

    private void GetTagsListByToggleResp(string msg)
    {
        Debug.Log("GetTagsListByToggleResp "+ msg);
        
        JSONNode jsonNode = JSON.Parse(msg);
        if (jsonNode["code"].AsInt == 0)
        {
            JSONArray dataArry = jsonNode["data"].AsArray;
            tagsDG.MaxLength = dataArry.Count;
            ItemRender[] dgirs = tagsDG.getItemRenders();
            for (int i = 0; i < dgirs.Length; i++)
            {
                dgirs[i].AddItemSetDataFunc((int index) =>
                {
                    SetTagsItem(dgirs[i].gameObj, i, dataArry[index]);
                });
                int idx = dgirs[i].m_renderData;
                SetTagsItem(dgirs[i].gameObj, i, dataArry[idx]);
            }
        }
        else
        {
            Debug.LogError("GetTagsListByToggleResp has exceptiom" + msg);
        }
    }

    private void SetTagsItem(GameObject go, int index, JSONNode msg)
    {
        go.transform.Find("Background/Text").GetComponent<Text>().text = msg["name"];
        go.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        if (index == 0)
        {
            NetManager.HttpGetReq(string.Format("{0}{1}", AppConst.IP, 
                string.Format(NetMessageConst.GetVideoListByMenuTag, currentChooseMenuId, msg["id"].AsInt)), SetVideoItem);
            go.GetComponent<Toggle>().isOn = true;
        }
        go.GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
        {
            if (isOn)
            {
                int id = msg["id"].AsInt;
                NetManager.HttpGetReq(string.Format("{0}{1}", AppConst.IP, 
                    string.Format(NetMessageConst.GetVideoListByMenuTag, currentChooseMenuId, id)), SetVideoItem);
            }
        });
    }

    void SetVideoItem(string msg)
    {
        Debug.Log("manu SetVideoItem"+msg);
        JSONNode jsonNode = JSON.Parse(msg);
        JSONArray dataArr = jsonNode["data"]["items"].AsArray;
        videoDG.Destroy();
        videoDG.MaxLength = dataArr.Count;
        ItemRender[] dgirs = videoDG.getItemRenders();
        for (int i = 0; i < dgirs.Length; i++)
        {
            dgirs[i].AddItemSetDataFunc((int index) =>
            {
                SetVideoItemData(dgirs[i].gameObj, dataArr[index]);
            });
            int idx = dgirs[i].m_renderData;
            SetVideoItemData(dgirs[i].gameObj, dataArr[idx]);
        }
    }

    void SetVideoItemData(GameObject obj, JSONNode json)
    {
        try
        {
            JSONNode iconArr = json["icon"];
            if (!string.IsNullOrEmpty(iconArr["uri"]))
            {
                NetManager.HttpDownImageReq(iconArr["uri"], obj.transform.Find("icon").GetComponent<RawImage>());
            }

            obj.transform.Find("name").GetComponent<Text>().text = json["name"];
            obj.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            obj.GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
            {
                if (isOn)
                {
                    currentChooseVideoId  = json["id"].AsInt;
                    currentChooseVideoUrl = json["uri"];
                    if(currentChooseVideoUrl.StartsWith(AppConst.dirSep))
                        currentChooseVideoUrl=string.Format("{0}{1}",AppConst.IP,currentChooseVideoUrl);
                    currentChooseFileType = json["fileType"]["id"].AsInt;
                    currentChooseMd5      = json["md5"];

                    //obj.transform.Find("mark").gameObject.SetActive(true);
                    if (currentChooseItem != null && currentChooseItem != obj.GetComponent<Toggle>())
                    {
                        currentChooseItem.isOn = false;
                    }
                    currentChooseItem = obj.GetComponent<Toggle>();
                }
            });
        }
        catch (System.Exception e)  
        {
            throw e;
        }
    }

    void StatusEvent(string msg)
    {
//        Debug.Log("manu StatusEvent "+msg);
        JSONNode jsonNode   = JSON.Parse(msg);
        JSONNode userEvents = jsonNode["UserEvents"];

        if (userEvents == null)
        {
            return;
        }

        redCount        = 0;
        greenCount      = 0;
        yellowCount     = 0;
        string num      = userEvents.Count.ToString();
        openNum.text    = num;
        connectNum.text = num;
        connectCount = 0;
        for (int i = 0; i < userEvents.Count; i++)
        {
            int powerNum = userEvents[i]["PowerState"].AsInt;
            if (powerNum < 33)
            {
                redCount++;
            }
            else if (powerNum >= 33 && powerNum < 66)
            {
                yellowCount++;
            }
            else if (powerNum >= 66)
            {
                greenCount++;
            }

            if (PlayState.Play.Equals(userEvents[i]["PlayerState"]))
            {
                connectCount++;
            }
        }
        comeinNum.text = connectCount.ToString();
        greenNum.text = greenCount.ToString();
        redNum.text = redCount.ToString();
        yellowNumt.text = yellowCount.ToString();
        FrameMsgHandler.SendMsg(NetMessageConst.UpdateOnlineDeviceInfo, userEvents);
    }

    void GetDevicesListReq()
    {
        string url = string.Format("{0}{1}", AppConst.IP, NetMessageConst.GetDevicesInfoList);
        NetManager.HttpGetReq(url, GetDevicesListResp);
    }

    void GetDevicesListResp(string msg)
    {
        JSONNode devicesData = JSON.Parse(msg);
        JSONArray dataArr = devicesData["data"].AsArray;
        UpdateAllDeviceInfo(dataArr.Count.ToString());
    }

    void UpdateAllDeviceInfo(string msg)
    {
        registNumt.text = msg;
    }

    public override void OnCloseing()
    {
        base.OnCloseing();
    }

    public override void OnClosed()
    {
        base.OnClosed();
    }

    public override void OnDestroy()
    {
        toggleData.Clear();
        DestroyObject(skin);
        Component.Destroy(this);
    }
}
