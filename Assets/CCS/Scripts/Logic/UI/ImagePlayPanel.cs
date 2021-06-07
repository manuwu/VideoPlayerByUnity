using System.Collections;
using CCS;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class ImagePlayPanel : PanelBase
{
    //Trans
    private Button backBtn;
    private Button deviceBtn;
    private Transform videoCameraTran;
    private GameObject videoControlRoot;
    
    //Data
    private Quaternion cameraRow;
    private Vector3 lastMousePosition;
    private float   mouseTime;
    
    public override void Init(params object[] args)
    {
        base.Init(args);
        videoCameraTran  = PlayerManager.GetPlayerCamera();
        videoCameraTran.localPosition=Vector3.zero;
        videoCameraTran.localRotation=Quaternion.Euler(0,0,0);
        PlayerManager.ShowPlayerRoot();
        PanManager.ShowLoading();
        
        NetManager.HttpDownImageReq(args[0].ToString(),LoadImageCallBack);
    }

    void LoadImageCallBack(Texture tex)
    {
        PanManager.HideLoading();

        PlayerManager.PlayImage(tex);
    }
    
    public override void OnShowing()
    {
        base.OnShowing();
        if (isInit)
        {
            return;
        }
        isInit = true;
        base.OnShowing();
        backBtn         = skin.transform.Find("BtnRoot/LeftCorner/Corner/backBtn").GetComponent<Button>();
        deviceBtn       = skin.transform.Find("BtnRoot/LeftCorner/deviceBtn").GetComponent<Button>();

        videoControlRoot=skin.transform.Find("BtnRoot/LeftCorner").gameObject;
        AddUIEvent();
        InvokeRepeating("SetCursorVisiable",1f,1f);
    }
    
    void SetCursorVisiable()
    {
        if (Input.mousePosition != lastMousePosition)
        {
            lastMousePosition = Input.mousePosition;

            videoControlRoot.SetActive(true);
            Cursor.visible = true;
            mouseTime = 0;
        }
        else
        {
            mouseTime += 1;

            if (mouseTime >= 5)
            {
                Cursor.visible = false;
                videoControlRoot.SetActive(false);
            }
        } 
    }

    private void AddUIEvent()
    {
        backBtn.onClick.AddListener(OnClickBack);
        deviceBtn.onClick.AddListener(OnClickDeviceBtn);
    }
    
    public override void AddEvent()
    {
        base.AddEvent();
        NetMsgHandler.AddListener(NetMessageConst.SameCamera, UpdateCameraRotation);
    }

    public override void RemoveEvent()
    {
        base.RemoveEvent();
        NetMsgHandler.RemoveListener(NetMessageConst.SameCamera, UpdateCameraRotation);
    }

    private void UpdateCameraRotation(string msg)
    {
        JSONNode json = JSON.Parse(msg);
        cameraRow.x = json["x"].AsFloat;
        cameraRow.y = json["y"].AsFloat;
        cameraRow.z = json["z"].AsFloat;
        cameraRow.w = json["w"].AsFloat;
        videoCameraTran.rotation = Quaternion.Lerp(videoCameraTran.rotation,cameraRow, 10f * Time.deltaTime);
    }
    
    void OnClickDeviceBtn()
    {
        PanManager.OpenPanel<ChooseSameSceneDevicePanel>(PanelName.ChooseSameSceneDevicePanel);
    }
    
    private void OnClickBack()
    {
        PlayerManager.HidePlayerRoot();
        PlayerManager.ResetImagePlayer();
        PanManager.AllOpenWithout(PanelName.ImagePlayPanel);
        PanManager.ClosePanel(PanelName.ImagePlayPanel);

        Cursor.visible = true;
    }
}
