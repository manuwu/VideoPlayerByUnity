using CCS;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UMP;
using UnityEngine;
using UnityEngine.UI;


public class ChooseServePanel : PanelBase
{
    //Trans
    private Button connectBtn;
    private InputField serveInput;

    //Data
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
        base.OnShowing();
        connectBtn = skin.transform.Find("connectBtn").GetComponent<Button>();
        serveInput = skin.transform.Find("ServeInputField").GetComponent<InputField>();
        InitIp();
        AddUIEvent();
    }

    void InitIp()
    {
        if (PlayerPrefs.HasKey(AppConst.IPSaveKey))
        {
            serveInput.text = PlayerPrefs.GetString(AppConst.IPSaveKey);
        }
        else
        {
            serveInput.text = string.Empty;
        }
    }

    void AddUIEvent()
    {
        connectBtn.onClick.AddListener(OnClickConnect);
    }

    void OnClickConnect()
    {
        if(string.IsNullOrEmpty(serveInput.text))
        {
            PanManager.ShowToast("请输入服务器IP+端口");
            return;
        }
        PlayerPrefs.SetString(AppConst.IPSaveKey,serveInput.text);
        AppConst.IP = string.Format("{0}{1}",AppConst.Http, serveInput.text);
        AppConst.WebSocketAdd = string.Format(AppConst.WebSocketHost, serveInput.text,Util.GetMacAddress());
        NetManager.InitNet();
        PanManager.OpenPanel<LobbyPanel>(PanelName.LobbyPanel);
        PanManager.ClosePanel(PanelName.ChooseServePanel);
    }
}
