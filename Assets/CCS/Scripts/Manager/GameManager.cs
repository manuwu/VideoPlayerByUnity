using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleJson;

namespace CCS
{
    public class GameManager : Manager
    {
        private const string filesStr = "config.txt";
        private Dictionary<string, string> fileDic = new Dictionary<string, string>();

        void Start()
        {
            StartCoroutine(Init());
        }

        IEnumerator Init()
        {
            DontDestroyOnLoad(gameObject);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = 40;
            Application.runInBackground = true;

            string resPath = Util.DataPath;  //数据目录
            string dataPath = Util.AppContentPath(); //
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
                string[] strList =
                {
                        filesStr
                    };
                if (Application.platform == RuntimePlatform.WindowsEditor
                    || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    resPath = "file:///" + resPath;
                }
                string fromUrl, toUrl;
                for (int i = 0; i < strList.Length; ++i)
                {
                    fromUrl = resPath + strList[i];
                    toUrl = dataPath + strList[i];
                    if (Application.platform == RuntimePlatform.Android
                        || Application.platform == RuntimePlatform.WindowsEditor
                        || Application.platform == RuntimePlatform.WindowsPlayer)
                    {
                        if (File.Exists(toUrl)) File.Delete(toUrl);
                        WWW www = new WWW(fromUrl);
                        yield return www;
                        if (www.isDone)
                        {
                            File.WriteAllBytes(dataPath + strList[i], www.bytes);
                            www.Dispose();
                            www = null;
                        }
                    }
                    else
                    {
                        File.Copy(fromUrl, toUrl, true);
                    }
                }
                yield return Yielders.EndOfFrame;
            }
            ConfigFileInit();
            yield return null;
        }

        //        public void OnApplicationPause(bool pause)
        //        {
        //#if UNITY_IPHONE || UNITY_ANDROID

        //#endif
        //        }

        //        private double loseFocusTime = 0;
        //        public void OnApplicationFocus(bool focus)
        //        {
        //#if UNITY_IPHONE || UNITY_ANDROID
        //            if (!focus)
        //            {
        //                //失去焦点
        //                loseFocusTime = Util.GetTimeStamp();
        //            }
        //            else
        //            {
        //                //获得焦点
        //                if (loseFocusTime > 0)
        //                {
        //                    //Util.CallMethod("GlobalListener", "OnApplicationFocusManager", Util.GetTimeStamp() - loseFocusTime);
        //                }
        //            }
        //#endif
        //        }


        private void ConfigFileInit()
        {
            string url = Util.AppContentPath() + filesStr;
            if (File.Exists(url))
            {
                string[] fill = File.ReadAllLines(url);
                string key = null;
                string value = null;
                foreach (var item in fill)
                {
                    var configKeyValue = item.Trim().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    key = configKeyValue[0].Trim();
                    value = configKeyValue[1].Trim();
                    fileDic.Add(key, value);
                }
                fileDic.TryGetValue("ip", out AppConst.Port);
                //
                AppConst.IP = string.Format(AppConst.IP, AppConst.Port);
                AppConst.WebSocketAdd = string.Format(AppConst.WebSocketAdd, AppConst.Port,Util.GetMacAddress());
                NetManager.InitNet();
                ResManager.Initialize();
                PanManager.OpenPanel<LobbyPanel>(PanelName.LobbyPanel);
            }
            else
                StartCoroutine(Init());
        }

        void OnDestroy()
        {
            if (NetManager != null)
            {
                NetManager.Unload();
            }
        }
    }
}