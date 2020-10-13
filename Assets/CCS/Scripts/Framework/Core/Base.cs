using UnityEngine;
using System.Collections;
using CCS;
using System.Collections.Generic;

public class Base : MonoBehaviour
{
    private AppFacade m_Facade;
    private NetworkManager m_NetMgr;
    private PanelManager m_PanelMgr;
    private SceneLoadManager m_SceneManager;
    private ResourceManager m_ResManager;
    private TPAtlasManager m_TPManager;
    private PlayerManager m_PlayerManager;

    protected AppFacade facade
    {
        get
        {
            if (m_Facade == null)
            {
                m_Facade = AppFacade.Instance;
            }
            return m_Facade;
        }
    }

    protected NetworkManager NetManager
    {
        get
        {
            if (m_NetMgr == null)
            {
                m_NetMgr = facade.GetManager<NetworkManager>(ManagerName.Network);
            }
            return m_NetMgr;
        }
    }

    protected ResourceManager ResManager
    {
        get
        {
            if (m_ResManager == null)
            {
                m_ResManager = facade.GetManager<ResourceManager>(ManagerName.Resource);
            }
            return m_ResManager;
        }
    }

    protected TPAtlasManager TPManager
    {
        get
        {
            if (m_TPManager == null)
            {
                m_TPManager = facade.GetManager<TPAtlasManager>(ManagerName.TPManager);
            }
            return m_TPManager;
        }
    }

    protected PanelManager PanManager
    {
        get
        {
            if (m_PanelMgr == null)
            {
                m_PanelMgr = facade.GetManager<PanelManager>(ManagerName.Panel);
            }
            return m_PanelMgr;
        }
    }   
    
    protected PlayerManager PlayerManager
    {
        get
        {
            if (m_PlayerManager == null)
            {
                m_PlayerManager = facade.GetManager<PlayerManager>(ManagerName.PLayer);
            }
            return m_PlayerManager;
        }
    }

    protected SceneLoadManager ScenesManager
    {
        get
        {
            if (m_SceneManager == null)
            {
                m_SceneManager = facade.GetManager<SceneLoadManager>(ManagerName.Scene);
            }
            return m_SceneManager;
        }
    }
}
