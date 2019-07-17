using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleJSON;
using System.Runtime.InteropServices;

namespace CCS {

    public enum DataType { AdminEvent = 1, AdminResourceManageEvent, StatusEvent, UserResourceManageEvent, DeviceInfo }
    public enum PlayState { Idle, Play, Pause }
    public enum ControlState { NoThing = 0, Play, Pause, Stop, Resume, Loop, NoLoop, TurnOff = 10, Reboot = 20 }

    public struct AdminMessage
    {
        public DataType Type;
        public AdminEvent Data;
    }

    public struct AdminEvent
    {
        public ControlState Control;
        public long Progress;
        public ResourceInfo Resource;
    }

    public struct ResourceInfo
    {
        public int Id;
        public string Name;
        public string Md5;
        public long Size;
        public string CreatedTime;
        public string Description;
        public FileTypeInfo FileType;
        public string Uri;
        public string UriLowRes;
        public string Duration;
        public int Width;
        public int Height;
        public bool Recommend;

    }

    public struct FileTypeInfo
    {
        public int Id;
        public string Name;
        public string Description;
    }

    public struct StatusEvent
    {
        public byte EventType;
        public UserEvent UserEvents;
    }

    public struct UserEvent
    {
        public string UserName;
        public Device UserDevice;
        public PlayState PlayerState;
        public int PowerState; //0-100
        public int SignalStrength; //0-100
    }

    public struct Device
    {
        public int Id;
        public string SerialNumber;
        public string Description;
    }
    public class NetworkManager : Manager {

        #region Private Fields

        private WebData _webData;
        private Dictionary<string, Sprite> downSprites = new Dictionary<string, Sprite>();
        //private Texture2D tempTexture;
        #endregion

        #region Unity Events
        void Start()
        {
            //_webData = new WebData();
            //_webData.OpenWebSocket();
            //InvokeRepeating("UpdateMsg", 0.5f,0.02f);
            //tempTexture = new Texture2D(4, 4, TextureFormat.DXT1, false);
        }

        public void InitNet()
        {
            _webData = new WebData();
            _webData.OpenWebSocket();
            InvokeRepeating("UpdateMsg", 0.5f, 0.02f);
        }

        void UpdateMsg()
        {
            if (_webData.MsgQueue.Count > 0)
            {
                string info = _webData.MsgQueue.Dequeue();
                JSONNode json = JSON.Parse(info);
                NetMsgHandler.SendMsg(json["Type"].ToString().Trim('"'), json["Data"].ToString());
            }
        }

        //void Update()
        //{
        //    if (_webData.MsgQueue.Count > 0)
        //    {
        //        string info = _webData.MsgQueue.Dequeue();
        //        JSONNode json = JSON.Parse(info);
        //        NetMsgHandler.SendMsg(json["Type"].ToString().Trim('"'), json["Data"].ToString());
        //    }
        //    Debug.Log("_webData.MsgQueue.Count " + _webData.MsgQueue.Count);
        //}
        #endregion

        /// <summary>
        /// 发送链接请求
        /// </summary>
        public void SendConnect() {
            _webData.Connect();
        }

        /// <summary>
        /// 发送SOCKET消息
        /// </summary>
        public void SendMessage(string msg) {
            // Send message to the server  
            if(_webData!=null)
                _webData.WebSocket.Send(msg);
        }

        /// <summary>
        /// 发送SOCKET消息
        /// </summary>
        public void SendMessage(byte[] msg)
        {
            // Send message to the server  
            _webData.WebSocket.Send(msg);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Shutdown()
        {
            // Close the connection  
            _webData.WebSocket.Close(1000, "Bye!");
        }

        public void Unload() {
            downSprites.Clear();
            if (_webData.WebSocket != null)
                _webData.WebSocket.Close();
            NetMsgHandler.ClearAllListeners();
            Util.LogWarning("~NetworkManager was destroy");
        }


        public void HttpGetReq(string url,Action<string> callBack)
        {
            StartCoroutine(StartHttpGetReq(url,callBack));
        }

        /// <summary>
        /// http GET请求
        /// </summary>
        IEnumerator StartHttpGetReq(string url, Action<string> callBack)
        {
            WWW www = new WWW(url);
            www.threadPriority = UnityEngine.ThreadPriority.High;
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                if (callBack!=null)
                {
                    callBack(www.text);
                }
                Util.Log(www.text);
            }
        }

        public void HttpPostReq(string url, Dictionary<string, string> post, Action<string> callBack)
        {
            StartCoroutine(StartHttpPostReq(url, post, callBack));
        }

        /// <summary>
        /// http Post请求
        /// </summary>
        IEnumerator StartHttpPostReq(string url, Dictionary<string,string> post, Action<string> callBack)
        {
            WWWForm form = new WWWForm();
            foreach (KeyValuePair<string, string> post_arg in post)
                form.AddField(post_arg.Key, post_arg.Value);
            WWW www = new WWW(url,form);
            www.threadPriority = UnityEngine.ThreadPriority.High;
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                if (callBack != null)
                {
                    callBack(www.text);
                }
                Util.Log(www.text);
            }
        }

        public void HttpDownImageReq(string url, RawImage img)
        {
            StartCoroutine(HttpDownImageResp(url, img));
        }

        /// <summary>
        /// http请求
        /// </summary>
        IEnumerator HttpDownImageResp(string url, RawImage img)
        {
            if (img != null)
            {
                WWW www = new WWW(url);
                www.threadPriority = UnityEngine.ThreadPriority.High;
                yield return www;
                if (www.isDone && string.IsNullOrEmpty(www.error))
                {
                    Texture2D tempTexture = new Texture2D(4, 4, TextureFormat.DXT1, false);
                    www.LoadImageIntoTexture(tempTexture);
                    if(img!=null)
                        img.texture = tempTexture;
                    tempTexture = null;
                }
            }
        }


        /// <summary>
        /// 将字节数组转换为结构体
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object ByteaToStruct(byte[] bytes, Type type)
        {
            //得到结构体大小
            int size = Marshal.SizeOf(type);
            Math.Log(size, 1);

            if (size > bytes.Length)
                return null;
            //分配结构大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将BYTE数组拷贝到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, size);
            //将内存空间转换为目标结构
            object obj = Marshal.PtrToStructure(structPtr, type);
            //释放内容空间
            Marshal.FreeHGlobal(structPtr);
            return obj;
        }

        /// <summary>
        /// 将结构转换为字节数组
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public byte[] StructTOBytes(object obj)
        {
            int size = Marshal.SizeOf(obj);
            //创建byte数组
            byte[] bytes = new byte[size];
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷贝到分配好的内存空间
            Marshal.StructureToPtr(obj, structPtr, false);
            //从内存空间拷贝到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            return bytes;
        }
    }
}