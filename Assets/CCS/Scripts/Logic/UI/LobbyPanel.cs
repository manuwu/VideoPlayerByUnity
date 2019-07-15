using UnityEngine;
using CCS;
using UnityEngine.UI;
using SimpleJson;
using System.Collections.Generic;
using SimpleJSON;

public class LobbyPanel : PanelBase {

    //Trans
    private Button playAllNetBtn;
    private Button playLocalBtn;
    private Toggle homeTog;
    private Toggle editTog;
    private DataGrid videoDG;
    private DataGrid tagsDG;
    private  GameObject toggleTemp;
    private GameObject pageVideoTemp;
    private Transform toggleParent;
    private Transform videoParent;
    private GameObject videoRoot;
    private Text greenNum;
    private Text yellowNumt;
    private Text redNum;
    private Text registNumt;
    private Text openNum;
    private Text connectNum;
    private Text comeinNum;
    private Toggle currentChooseItem;

    //Datas
    Dictionary<int, GameObject> toggleData;
    //Dictionary<int, GameObject> videoPageDate;
    private int redCount;
    private int greenCount;
    private int yellowCount;
    private string currentChooseVideoUrl;
    private int currentChooseVideoId;
    private int currentChooseFileType;
    private int currentChooseMenuId;
    private JSONNode currentChooseVideo;
    private void Awake()
    {
        toggleData = new Dictionary<int, GameObject>();
        //videoPageDate = new Dictionary<int, GameObject>();
    }

    public override void Init(params object[] args)
    {
        base.Init(args);
    }

    public override void OnShowing()
    {
        base.OnShowing();
        if (isInit)return;
        isInit = true;
        Transform skinTrans = skin.transform;
        playAllNetBtn = skinTrans.Find("rightRoot/playAllNetbtn").GetComponent<Button>();
        playLocalBtn = skinTrans.Find("rightRoot/playLocalBtn").GetComponent<Button>();
        homeTog = skinTrans.Find("rightRoot/toggleGroup/toggleHome").GetComponent<Toggle>();
        editTog = skinTrans.Find("rightRoot/toggleGroup/toggleEdit").GetComponent<Toggle>();

        videoRoot = skinTrans.Find("videoListRoot").gameObject;
        tagsDG = videoRoot.transform.Find("topRoot/toggleGroup").GetComponent<DataGrid>();
        videoDG= videoRoot.transform.Find("videoPageRoot/videoPagetemp/contenView").GetComponent<DataGrid>();
        toggleParent = skinTrans.Find("videoListRoot/leftRoot/toggleGroup/Viewport/Content");
        toggleTemp = toggleParent.Find("toggleItem").gameObject;
        videoParent = skinTrans.Find("videoListRoot/videoPageRoot");
        pageVideoTemp = videoParent.Find("videoPagetemp").gameObject;
        greenNum = skinTrans.Find("rightRoot/detialRoot/green/Text").GetComponent<Text>();
        yellowNumt = skinTrans.Find("rightRoot/detialRoot/yellow/Text").GetComponent<Text>();
        redNum = skinTrans.Find("rightRoot/detialRoot/red/Text").GetComponent<Text>();
        registNumt = skinTrans.Find("rightRoot/detialRoot/regist/Text").GetComponent<Text>();
        openNum = skinTrans.Find("rightRoot/detialRoot/open/Text").GetComponent<Text>();
        connectNum = skinTrans.Find("rightRoot/detialRoot/conect/Text").GetComponent<Text>();
        comeinNum = skinTrans.Find("rightRoot/detialRoot/comein/Text").GetComponent<Text>();
        AddUIEvent();
    }

    void AddUIEvent()
    {
        playAllNetBtn.onClick.AddListener(PlayAllNet);
        playLocalBtn.onClick.AddListener(PlayLocal);
        homeTog.onValueChanged.AddListener(OnClickHomeToggle);
        editTog.onValueChanged.AddListener(OnClickEditTogglr);
    }

    private void PlayAllNet()
    {
        if (currentChooseVideoUrl == null)
        {
            PanManager.ShowToast("您还没选择视频");
            return;
        }
        AdminMessage msg = new AdminMessage();
        msg.Type = DataType.AdminEvent;
        msg.Data.Control = ControlState.Play;
        msg.Data.Resource.Id = currentChooseVideoId;
        msg.Data.Resource.Uri = currentChooseVideoUrl;
        msg.Data.Resource.FileType.Id = currentChooseFileType;
        NetManager.SendMessage(Util.ObjectToJson(msg));
        PanManager.OpenPanel<VideoPlayPanel>(PanelName.VideoPlayPanel, currentChooseVideoUrl);
        currentChooseItem.isOn = false;
        currentChooseVideoUrl = null;

        PanManager.AllHidenWithout(PanelName.VideoPlayPanel);
    }

    private void PlayLocal()
    {
        if (currentChooseVideoUrl == null)
        {
            PanManager.ShowToast("您还没选择视频");
            return;
        }
        PanManager.OpenPanel<VideoPlayPanel>(PanelName.VideoPlayPanel, currentChooseVideoUrl);
        currentChooseItem.isOn = false;
        currentChooseVideoUrl = null;
        PanManager.AllHidenWithout(PanelName.VideoPlayPanel);
    }

    void OnClickHomeToggle(bool isOn)
    {
        if (isOn)
        {
            videoRoot.SetActive(true);
            PanManager.ClosePanel(PanelName.ControlPanel);
        }
    }

    void OnClickEditTogglr(bool isOn)
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

    void GetToggleListReq()
    {
        string url = string.Format("{0}{1}", AppConst.IP,NetMessageConst.GetToggleInfoList);
        NetManager.HttpGetReq(url, GetToggleListResp);
    }

    void GetToggleListResp(string msg)
    {
        JSONNode jsonNode = JSON.Parse(msg);
        if (toggleData.Count==0)
        {
            for (int i = 0; i < jsonNode.Count; i++)
            {
                GameObject obj = GameObject.Instantiate(toggleTemp);
                obj.transform.SetParent(toggleParent,false);
                obj.transform.localScale = Vector3.one;
                obj.SetActive(true);

                toggleData.Add(int.Parse(jsonNode[i]["Id"]), obj);
                SetToggleItemDate(obj, i,jsonNode[i]);
            }
        }
    }

    void SetToggleItemDate(GameObject obj,int id,JSONNode json)
    {
        if (id == 0)
        {
            SetVideoSubPage(int.Parse(json["Id"]));
            obj.transform.GetComponent<Toggle>().isOn = true;
        }
        obj.transform.Find("Background/Text").GetComponent<Text>().text =( json["Name"].ToString()).Trim('"');
        obj.transform.Find("Background/Checkmark/Text").GetComponent<Text>().text = (json["Name"].ToString()).Trim('"');
        obj.transform.GetComponent<Toggle>().onValueChanged.AddListener((bool isOn)=> {
            if (isOn)
                SetVideoSubPage(int.Parse( json["Id"]));
        });
    }

    void SetVideoSubPage(int id)
    {
        //if (!videoPageDate.ContainsKey(id))
        //{
        //    GameObject obj = GameObject.Instantiate(pageVideoTemp);
        //    obj.transform.SetParent(videoParent,false);
        //    obj.transform.localScale = Vector3.one;
        //    obj.SetActive(true);
        //    videoDG = obj.transform.Find("contenView").GetComponent<DataGrid>();
        //    videoPageDate.Add(id, obj);
        //    NetManager.HttpGetReq(string.Format("{0}{1}{2}", AppConst.IP, NetMessageConst.GetVideoListByMenu,id), SetVideoItem);
        //}

        //foreach (var item in videoPageDate)
        //{
        //    if (item.Key == id)
        //        item.Value.SetActive(true);
        //    else
        //        item.Value.SetActive(false);
        //}
        GetTagsListByToggleReq(id);
    }

    void GetTagsListByToggleReq(int toggleID)
    {
        currentChooseMenuId = toggleID;
        string url = string.Format("{0}{1}{2}", AppConst.IP, NetMessageConst.GetTagsListByToggle, toggleID);
        NetManager.HttpGetReq(url, GetTagsListByToggleResp);
    }

    void GetTagsListByToggleResp(string msg)
    {
        JSONNode jsonNode = JSON.Parse(msg);
        Debug.Log("88888888888888");
        Debug.Log(jsonNode);
        //tagsDG.Destroy();
        tagsDG.MaxLength = jsonNode.Count;
        ItemRender[] dgirs = tagsDG.getItemRenders();
        for (int i = 0; i < dgirs.Length; i++)
        {
            dgirs[i].AddItemSetDataFunc((int index) =>
            {
                SetTagsItem(dgirs[i].gameObj,i, jsonNode[index]);
            });
            int idx = dgirs[i].m_renderData;
            SetTagsItem(dgirs[i].gameObj, i,jsonNode[idx]);
        }
    }

    void  SetTagsItem(GameObject go, int index, JSONNode msg)
    {
        go.transform.Find("Background/Text").GetComponent<Text>().text = msg["Name"];
        go.GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
        {
            if(isOn)
                NetManager.HttpGetReq(string.Format("{0}{1}", AppConst.IP, string.Format( NetMessageConst.GetVideoListByMenuTag, currentChooseMenuId, (msg["Id"].ToString()).Trim('"'))), SetVideoItem);

        });
        if (index == 0)
        {
            NetManager.HttpGetReq(string.Format("{0}{1}", AppConst.IP, string.Format(NetMessageConst.GetVideoListByMenuTag, currentChooseMenuId, (msg["Id"].ToString()).Trim('"'))), SetVideoItem);
            go.GetComponent<Toggle>().isOn = true;
        }
    }

    void SetVideoItem(string msg)
    {

        JSONNode jsonNode = JSON.Parse(msg);
        Debug.Log(jsonNode);
        videoDG.Destroy();
        videoDG.MaxLength = jsonNode.Count;
        ItemRender[] dgirs = videoDG.getItemRenders();
        for (int i = 0; i < dgirs.Length; i++)
        {
            dgirs[i].AddItemSetDataFunc((int index) =>
            {
                SetVideoItemData(dgirs[i].gameObj, jsonNode[index]);
            });
            int idx = dgirs[i].m_renderData;
            SetVideoItemData(dgirs[i].gameObj, jsonNode[idx]);
        }
    }

    void SetVideoItemData(GameObject obj ,JSONNode json)
    {
        try
        {
            Debug.Log(json);
            JSONNode icon = json["Icon"];
            if (icon["Uri"] != null)
                NetManager.HttpDownImageReq((icon["Uri"].ToString()).Trim('"'), obj.transform.Find("icon").GetComponent<RawImage>());
            obj.transform.Find("name").GetComponent<Text>().text = (json["Name"].ToString()).Trim('"');
            obj.GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) => {
                if (isOn)
                {
                    currentChooseVideoId = (int.Parse(json["Id"]));
                    currentChooseVideoUrl = json["Uri"].ToString().Trim('"');
                    currentChooseFileType= (int.Parse(json["FileType"]["Id"]));
                    //obj.transform.Find("mark").gameObject.SetActive(true);
                    if (currentChooseItem != null && currentChooseItem != obj.GetComponent<Toggle>())
                        currentChooseItem.isOn = false;
                    currentChooseItem = obj.GetComponent<Toggle>();
                }
            });
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    void StatusEvent(string msg)
    {
        Debug.Log("99999999999");
        Debug.Log(msg);
        JSONNode jsonNode = JSON.Parse(msg);
        JSONNode userEvents = jsonNode["UserEvents"];
        if (userEvents== null)
            return;
        redCount = 0;
        greenCount = 0;
        yellowCount = 0;
        string num = userEvents.Count.ToString();
        openNum.text = num;
        connectNum.text = num;
        comeinNum.text = num;
        for (int i = 0; i < userEvents.Count; i++)
        {
            int powerNum = int.Parse(userEvents[i]["PowerState"]);
            if (powerNum < 33)
                redCount++;
            else if (powerNum >= 33 && powerNum < 66)
                yellowCount++;
            else if (powerNum >=66)
                greenCount++;
        }
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
        UpdateAllDeviceInfo(devicesData.Count.ToString());
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
