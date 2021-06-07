using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleJSON;
using System.Runtime.InteropServices;
using UnityEngine.Networking;


namespace CCS
{

    public enum DataType     { AdminEvent = 1, AdminResourceManageEvent, StatusEvent, UserResourceManageEvent, DeviceInfo }

    public class PlayState
    {
        public const string Idle="0";
        public const string Play="1";
        public const string Pause = "2";
    }
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
        public Devices[] Devices;
    }

    public class Devices
    {
        public int Id;
        public string SerialNumber;
        public Devices(int id,string seriNum)
        {
            this.Id = id;
            this.SerialNumber = seriNum;
        }
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
        public bool IsLive;
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
    public class NetworkManager : Manager
    {

        #region Private Fields

        private WebData _webData;
        private Dictionary<string, Sprite> downSprites = new Dictionary<string, Sprite>();
        //private Texture2D tempTexture;
        #endregion

        #region Unity Events

        private void Awake()
        {
            NetMsgHandler.AddListener(NetMessageConst.SocketServerClosed,SocketServerClosedEvent);
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
                NetMsgHandler.SendMsg(json["type"], json["data"].ToString());
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

        void SocketServerClosedEvent(string msg)
        {
            Shutdown();
            SendConnect();
        }
        #endregion

        /// <summary>
        /// ������������
        /// </summary>
        public void SendConnect()
        {
            _webData.Connect();
        }

        /// <summary>
        /// ����SOCKET��Ϣ
        /// </summary>
        public void SendMessage(string msg)
        {
            Debug.Log("manu msg "+msg);
            // Send message to the server  
            if (_webData != null)
            {
                _webData.WebSocket.Send(msg);
            }
        }

        /// <summary>
        /// ����SOCKET��Ϣ
        /// </summary>
        public void SendMessage(byte[] msg)
        {
            // Send message to the server  
            _webData.WebSocket.Send(msg);
        }

        /// <summary>
        /// �Ͽ�����
        /// </summary>
        public void Shutdown()
        {
            // Close the connection  
            _webData.WebSocket.Close(1000, "Bye!");
        }

        public void Unload()
        {
            downSprites.Clear();
            if (_webData!=null && _webData.WebSocket != null)
                _webData.WebSocket.Close();
            NetMsgHandler.ClearAllListeners();
            Util.LogWarning("~NetworkManager was destroy");
        }


        public void HttpGetReq(string url, Action<string> callBack)
        {
//            Debug.LogError("manu url "+url);
            StartCoroutine(StartHttpGetReq(url, callBack));
        }

        /// <summary>
        /// http GET����
        /// </summary>
        IEnumerator StartHttpGetReq(string url, Action<string> callBack)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.Send();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(url+"error"+ www.error);
                }
                else
                {
                    if (callBack != null)
                    {
                        callBack(www.downloadHandler.text);
                    }
                }
            }
        }

        public void HttpPostReq(string url, Dictionary<string, string> post, Action<string> callBack)
        {
            StartCoroutine(StartHttpPostReq(url, post, callBack));
        }

        /// <summary>
        /// http Post����
        /// </summary>
        IEnumerator StartHttpPostReq(string url, Dictionary<string, string> post, Action<string> callBack)
        {
            WWWForm form = new WWWForm();
            foreach (KeyValuePair<string, string> post_arg in post)
                form.AddField(post_arg.Key, post_arg.Value);

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    if (callBack != null)
                        callBack(www.downloadHandler.text);
                }
            }
        }

        public void HttpDownImageReq(string url, RawImage img)
        {
            StartCoroutine(HttpDownImageResp(url, img));
        }

        /// <summary>
        /// http����
        /// </summary>
        IEnumerator HttpDownImageResp(string url, RawImage img)
        {
            if (img != null)
            {
                if (url.StartsWith(AppConst.dirSep))
                {
                    url = string.Format("{0}{1}", AppConst.IP, url);
                }
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
                yield return www.SendWebRequest();
                if(www.isNetworkError || www.isHttpError) {
                    Debug.Log(url+ www.error);
                }
                else {
                    img.texture=DownloadHandlerTexture.GetContent(www);
                }
            }
        }
        
        public void HttpDownImageReq(string url, Action<Texture> callBack)
        {
            StartCoroutine(HttpDownImageResp(url, callBack));
        }

        /// <summary>
        /// http����
        /// </summary>
        IEnumerator HttpDownImageResp(string url, Action<Texture> callBack)
        {
            if (url.StartsWith(AppConst.dirSep))
            {
                url = string.Format("{0}{1}", AppConst.IP, url);
            }

            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(url + www.error);
            }
            else
            {
                Texture myTexture = DownloadHandlerTexture.GetContent(www);
                if (callBack != null)
                    callBack(myTexture);
            }
        }

        /// <summary>
        /// ���ֽ�����ת��Ϊ�ṹ��
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object ByteaToStruct(byte[] bytes, Type type)
        {
            //�õ��ṹ���С
            int size = Marshal.SizeOf(type);
            Math.Log(size, 1);

            if (size > bytes.Length)
                return null;
            //����ṹ��С���ڴ�ռ�
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //��BYTE���鿽��������õ��ڴ�ռ�
            Marshal.Copy(bytes, 0, structPtr, size);
            //���ڴ�ռ�ת��ΪĿ��ṹ
            object obj = Marshal.PtrToStructure(structPtr, type);
            //�ͷ����ݿռ�
            Marshal.FreeHGlobal(structPtr);
            return obj;
        }

        /// <summary>
        /// ���ṹת��Ϊ�ֽ�����
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public byte[] StructTOBytes(object obj)
        {
            int size = Marshal.SizeOf(obj);
            //����byte����
            byte[] bytes = new byte[size];
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //���ṹ�忽��������õ��ڴ�ռ�
            Marshal.StructureToPtr(obj, structPtr, false);
            //���ڴ�ռ俽����byte����
            Marshal.Copy(structPtr, bytes, 0, size);
            //�ͷ��ڴ�ռ�
            Marshal.FreeHGlobal(structPtr);
            return bytes;
        }
    }
}