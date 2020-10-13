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
    private Vector3 m_LastMousePosition;
    private float   m_MouseTime;
    
    public override void Init(params object[] args)
    {
        base.Init(args);
        videoCameraTran  = PlayerManager.GetPlayerCamera();
        videoCameraTran.localPosition=Vector3.zero;
        videoCameraTran.localRotation=Quaternion.Euler(0,0,0);
        PlayerManager.ShowPlayerRoot();
        PlayerManager.ShowImagePlayerRoot();
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
        if (Input.mousePosition != m_LastMousePosition)
        {
            m_LastMousePosition = Input.mousePosition;

            videoControlRoot.SetActive(true);
            Cursor.visible = true;
            m_MouseTime = 0;
        }
        else
        {
            m_MouseTime += 1;

            if (m_MouseTime >= 5)
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

    void OnClickDeviceBtn()
    {
        PanManager.OpenPanel<ChooseSameSceneDevicePanel>(PanelName.ChooseSameSceneDevicePanel);
    }
    
    private void OnClickBack()
    {
        PlayerManager.HidePlayerRoot();
        PlayerManager.HideImagePlayerRoot();
        PlayerManager.ResetImagePlayer();
        PanManager.AllOpenWithout(PanelName.ImagePlayPanel);
        PanManager.ClosePanel(PanelName.ImagePlayPanel);

        Cursor.visible = true;
    }
}
