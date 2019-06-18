using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
using DG.Tweening;
using Object = UnityEngine.Object;

namespace CCS
{
    public enum PanelLayer
    {
        Layer1 = 1,
        Layer2 = 2,
        Layer3 = 3,
        Layer4 = 4
    }
    public class PanelName
    {
        public const string LobbyPanel = "LobbyPanel";
        public const string ControlPanel = "ControlPanel";
        public const string VideoPlayPanel = "VideoPlayPanel";
        public const string ChooseServePanel = "ChooseServePanel";

    }

    public class PanelManager : Manager
    {
        //Trans
        private Transform Canvas;
        private Transform Toast;
        private Text toastTxt;
        private Transform Layer3;
        public Transform GlobalUIRoot;
        //Data
        public Dictionary<string, PanelBase> dict;
        private Vector3 toastPos;
        private Dictionary<string,GameObject> uiRes;
        private void Awake()
        {

            dict = new Dictionary<string, PanelBase>();
            uiRes = new Dictionary<string, GameObject>();
            //InitLayer();
            Canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            Toast = Canvas.Find("Toast");
            Layer3 = Canvas.Find("Layer3");
            GlobalUIRoot = Canvas.parent;
        }

        private GameObject CreatePanel(string name, PanelLayer layer)
        {
            string assetName = name;
            GameObject prefab;
            if (!uiRes.TryGetValue(name, out prefab))
            { 
                prefab = ResManager.LoadAsset<GameObject>(name, assetName);
                if (prefab == null)
                    return null;
            }
            GameObject go = Instantiate(prefab) as GameObject;
            go.name = assetName;
            SetLayer(go.transform, layer);
            return go;
        }

        public void SetLayer(Transform trans, PanelLayer layer)
        {
            trans.SetParent(GetLayer(layer), false);
        }

        private Transform GetLayer(PanelLayer layer)
        {
            return Layer3;
        }

        //private void InitLayer()
        //{
        //    dict.Clear();
        //    Object[] objs = FindObjectsOfType(typeof(PanelBase));
        //    if (objs!=null)
        //    {
        //        foreach ( Object obj in objs )
        //        {
        //            PanelBase pb = obj as PanelBase;
        //            pb.skin = pb.gameObject;
        //            pb.skin.SetActive(false);
        //            pb.OnCloseing();
        //            dict.Add(pb.gameObject.name, pb);
        //        }
        //    }
        //    ClosePanel(PanelName.ControlPanel);
        //    OpenPanel<LobbyPanel>(PanelName.LobbyPanel);
        //}

        public void OpenPanel<T>(string name, params object[] args) where T : PanelBase
        {

            PanelBase panel;
            if (dict.TryGetValue(name, out panel))
            {
                panel.Init(args);
                panel.skin.SetActive(true);
                panel.isShow = true;
                return;
            }
            GameObject skin = CreatePanel(name,PanelLayer.Layer3);
            panel = skin.AddComponent<T>();
            dict.Add(name, panel);
            panel.Init(args);
            panel.isShow = true;
            panel.skin = panel.gameObject;
            panel.skin.SetActive(true);
            panel.OnShowing();
            //Anima
            panel.OnShowed();
            panel.AddEvent();
        }

        public void ClosePanel(string name)
        {

            PanelBase panel ;
            if (!dict.TryGetValue(name,out panel))
                return;

            panel.isShow = false;
            panel.RemoveEvent();
            panel.OnCloseing();
            panel.OnClosed();
            DestroyObject(panel.skin);
            Component.Destroy(panel);
            dict.Remove(name);
            panel.skin = null;
            panel.args = null;
        }

        public void HidePanel(string name)
        {
            PanelBase panel;
            if (!dict.TryGetValue(name, out panel))
                return;
            panel.isShow = false;
            panel.skin.SetActive(false);
        }

        public void AllHidenWithout(string name)
        {
            foreach (var item in dict)
            {
                if (item.Key != name)
                {
                    item.Value.isShow = false;
                    item.Value.skin.SetActive(false);
                }
            }
        }

        public void AllOpenWithout(string name)
        {
            foreach (var item in dict)
            {
                if (item.Key != name)
                {
                    item.Value.isShow = true;
                    item.Value.skin.SetActive(true);
                }
            }
        }

        public void  ShowToast(string msg)
        {
            if (toastTxt == null)
                toastTxt = Toast.Find("Text").GetComponent<Text>();
            Toast.gameObject.SetActive(true);
            toastTxt.text = msg;
            Toast.DOLocalMoveY(150,1f);
            if (IsInvoking("HideToast"))
                CancelInvoke("HideToast");
            Invoke("HideToast", 1f);
        }

        void  HideToast()
        {
            Toast.gameObject.SetActive(false);
            Toast.localPosition = Vector3.zero;
        }
    }
}