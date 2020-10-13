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
        AddUIEvent();
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
        AppConst.WebSocketAdd = string.Format(AppConst.WebSocketAdd, serveInput.text);
        AppConst.IP = string.Format(AppConst.IP, serveInput.text);
        NetManager.InitNet();
        PanManager.OpenPanel<LobbyPanel>(PanelName.LobbyPanel);

    }
}
