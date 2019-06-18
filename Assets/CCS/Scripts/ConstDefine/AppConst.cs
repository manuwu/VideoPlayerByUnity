using UnityEngine;
using System;

namespace CCS
{
    public class AppConst
    {
        public static string AppName = "CCS";                   //素材扩展名
        public static string ExtName = ".unity3d";                   //素材扩展名
        public static string AssetDir = "StreamingAssets";           //素材目录 

        public static string MatBundleName = "Mat"; //预加载材质球
        public static string SoundBundleName = "sound"; //音效资源包名
        public const bool LuaByteMode = false;
        public const bool LuaBundleMode = false;
        public static string LoadAssetBundlePath = "";
#if DEBUG_TEST
        public const bool DebugMode = true;
#else
        public const bool DebugMode = false;
#endif
        public const string LuaTempDir = "Lua/";                    //临时目录

        //public const string IP = "http://223.87.179.82:1555/";
        //public const string WebSocketAdd = "ws://223.87.179.82:1555/ccweb/ws/join?sn=admin001";
        //隋彪
        public static string WebSocketAdd = "ws://{0}/ccweb/ws/join?sn=admin001";
        public static string IP = "http://{0}/";
        public static string Port = string.Empty;

        //统一路径符
        public static string dirSep = "/";
        public static char splitSep = '|';
    }
}