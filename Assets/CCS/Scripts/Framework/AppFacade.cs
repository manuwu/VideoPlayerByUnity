using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using CCS;

public class AppFacade : Facade
{
    private static AppFacade _instance;

    public AppFacade() : base()
    {
    }

    public static AppFacade Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AppFacade();
            }
            return _instance;
        }
    }

    public void StartUp()
    {
        //-----------------初始化管理器-----------------------
        AppFacade.Instance.AddManager<TPAtlasManager>(ManagerName.TPManager);
        AppFacade.Instance.AddManager<NetworkManager>(ManagerName.Network);
        AppFacade.Instance.AddManager<ResourceManager>(ManagerName.Resource);
        AppFacade.Instance.AddManager<GameManager>(ManagerName.Game);
        AppFacade.Instance.AddManager<PanelManager>(ManagerName.Panel);
        AppFacade.Instance.AddManager<PlayerManager>(ManagerName.PLayer);
#if DEBUG_TEST
        AppFacade.Instance.AddManager<ShowFPS>(ManagerName.Fps);
#endif
        AppFacade.Instance.AddManager<SceneLoadManager>(ManagerName.Scene);
    }
}

