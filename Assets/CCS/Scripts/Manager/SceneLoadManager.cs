using UnityEngine;
using System.Collections;
using System;
 
using CCS;
using UnityEngine.SceneManagement;

namespace CCS
{
    public class SceneLoadManager : Manager
    {
        public class SceneParam
        {
            public static SceneParam Create(string name, Action<int> callback = null)
            {
                SceneParam param = new SceneParam();
                param.SceneName = name;
                param.LoadCallback = callback;
                return param;
            }

            public string SceneName;
            public Action<int> LoadCallback;
        }

        private AsyncOperation m_LoadAsync; //异步加载
        //private LuaFunction m_LuaFunc;
        private bool m_IsShowFinished = false;
        private string m_SceneName;
        private AssetBundle m_SceneBundle;


        /// <summary>
        /// 异步加载场景. 不显示过渡场景
        /// </summary>
        /// <param name="name"></param>
        /// <param name="func"></param>
        public void LoadSceneAsync(string name,  bool showLoading)
        {
            m_IsShowFinished = !showLoading;
            LoadSceneAsync(name);
        }
        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="name"></param>
        public void LoadSceneAsync(string name)
        {
            //if (m_LoadAsync != null)
            //    return;
            //m_LuaFunc = func
            m_SceneName = name;

            if (!m_IsShowFinished)
            {

            }
            else
            {
                BeginLoad();
            }

        }
        private void BeginLoad()
        {
//#if !DEBUG_TEST
//            m_SceneBundle = ResManager.LoadSceneBundle(m_SceneName.ToLower());
//#endif
            m_LoadAsync = SceneManager.LoadSceneAsync(m_SceneName);
        }

        private void OnShowFinished()
        {
            m_IsShowFinished = true;
            BeginLoad();
        }


    }
}