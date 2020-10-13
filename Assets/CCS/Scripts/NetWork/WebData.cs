
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using BestHTTP;
using BestHTTP.WebSocket;
using System.Text;


namespace CCS
{
    public class WebData
    {
        /// <summary>  
        /// Default text to send  
        /// </summary>  
        private string _msgToHeart = "Heart";

        /// <summary>  
        /// Saved WebSocket instance  
        /// </summary>  
        private WebSocket _webSocket;

        private Queue<string> _msgQueue = new Queue<string>();

        public Queue<string> MsgQueue { get { return _msgQueue; } }
        public WebSocket WebSocket { get { return _webSocket; } }


        public string MsgToSend
        {
            get { return _msgToHeart; }
            set
            {
                _msgToHeart = value;
            }
        }

        public void OpenWebSocket()
        {
            if (_webSocket == null)
            {
                // Create the WebSocket instance  
                _webSocket = new WebSocket(new Uri(AppConst.WebSocketAdd));

                if (HTTPManager.Proxy != null)
                    _webSocket.InternalRequest.Proxy = new HTTPProxy(HTTPManager.Proxy.Address, HTTPManager.Proxy.Credentials, false);

                // Subscribe to the WS events  
                _webSocket.OnOpen += OnOpen;
                _webSocket.OnMessage += OnMessageReceived;
                _webSocket.OnClosed += OnClosed;
                _webSocket.OnError += OnError;

                // Start connecting to the server  
                Connect();
            }
        }

        public void Connect()
        {
            if (_webSocket != null)
            {
                _webSocket.Open();
            }
            else
                OpenWebSocket();
        }

        public void UnInit()
        {
            _webSocket.OnOpen = null;
            _webSocket.OnMessage = null;
            _webSocket.OnError = null;
            _webSocket.OnClosed = null;
            _webSocket = null;
        }

        private byte[] getBytes(string message)
        {

            byte[] buffer = Encoding.Default.GetBytes(message);
            return buffer;
        }

        public void SendMsg(string msg)
        {
            // Send message to the server  
            _webSocket.Send(msg);
        }

        public void CloseSocket()
        {
            // Close the connection  
            _webSocket.Close(1000, "Bye!");
        }

        /// <summary>  
        /// Called when the web socket is open, and we are ready to send and receive data  
        /// </summary>  
        void OnOpen(WebSocket ws)
        {
            Debug.Log("connected");
            StartHeartBeat();
            SendMsg("ok");
        }

        /// <summary>  
        /// Called when we received a text message from the server  
        /// </summary>  
        void OnMessageReceived(WebSocket ws, string message)
        {
            //DataInfo datainfo = JsonUtility.FromJson<DataInfo>(message);
            if (message != null) _msgQueue.Enqueue(message);
        }

        /// <summary>  
        /// Called when the web socket closed  
        /// </summary>  
        void OnClosed(WebSocket ws, UInt16 code, string message)
        {
            Debug.Log(string.Format("-WebSocket closed! Code: {0} Message: {1}\n", code, message));
            UnInit();
        }

        /// <summary>  
        /// Called when an error occured on client side  
        /// </summary>  
        void OnError(WebSocket ws, Exception ex)
        {
            string errorMsg = string.Empty;
#if !UNITY_WEBGL || UNITY_EDITOR

            if (ws.InternalRequest.Response != null)
                errorMsg = string.Format("Status Code from Server: {0} and Message: {1}", ws.InternalRequest.Response.StatusCode, ws.InternalRequest.Response.Message);
#endif
            Debug.Log(string.Format("-An error occured: {0}\n", ex != null ? ex.Message : "Unknown Error " + errorMsg));
            UnInit();
        }

        void StartHeartBeat()
        {
            // 开启心跳线程
            Thread t = new Thread(new ThreadStart(SendHeartbeat));
            t.IsBackground = true;
            t.Start();
        }

        /// <summary>
        /// 发送心跳包
        /// </summary>
        void SendHeartbeat()
        {
            while (_webSocket!=null)
            {
                // 向服务端发送心跳包
                SendMsg(_msgToHeart);
                System.Threading.Thread.Sleep(2000);
            }
        }
    }

    //{"info":[{"area":11,"x":80,"y":50},{"area":5,"x":76,"y":48}]}
    [System.Serializable]
    public class DataInfo
    {
        public Data[] info;
    }

    [System.Serializable]
    public class Data
    {
        public int area;
        public int x;
        public int y;
    }
}
