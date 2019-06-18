using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Reflection;
using System.Collections;

namespace CCS
{
    public class Util
    {
        private static StringBuilder sb = new StringBuilder();
        private static System.Security.Cryptography.MD5 m_md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        public static int Int(object o)
        {
            return Convert.ToInt32(o);
        }

        public static float Float(object o)
        {
            return (float)Math.Round(Convert.ToSingle(o), 2);
        }

        public static long Long(object o)
        {
            return Convert.ToInt64(o);
        }

        public static int RandomInt(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public static float RandomFloat(float min, float max)
        {

            return UnityEngine.Random.Range(min, max);
        }

        public static string Uid(string uid)
        {
            int position = uid.LastIndexOf('_');
            return uid.Remove(0, position + 1);
        }

        public static long GetTime()
        {
            TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            return (long)ts.TotalMilliseconds;
        }

        /// <summary>
        /// 搜索子物体组件-GameObject版
        /// </summary>
        public static T Get<T>(GameObject go, string subnode) where T : Component
        {
            if (go != null)
            {
                Transform sub = go.transform.Find(subnode);
                if (sub != null) return sub.GetComponent<T>();
            }
            return null;
        }

        /// <summary>
        /// 搜索子物体组件-Transform版
        /// </summary>
        public static T Get<T>(Transform go, string subnode) where T : Component
        {
            if (go != null)
            {
                Transform sub = go.Find(subnode);
                if (sub != null) return sub.GetComponent<T>();
            }
            return null;
        }

        /// <summary>
        /// 搜索子物体组件-Component版
        /// </summary>
        public static T Get<T>(Component go, string subnode) where T : Component
        {
            return go.transform.Find(subnode).GetComponent<T>();
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        public static T Add<T>(GameObject go) where T : Component
        {
            if (go != null)
            {
                T[] ts = go.GetComponents<T>();
                for (int i = 0; i < ts.Length; i++)
                {
                    if (ts[i] != null) GameObject.Destroy(ts[i]);
                }
                return go.gameObject.AddComponent<T>();
            }
            return null;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        public static T Add<T>(Transform go) where T : Component
        {
            return Add<T>(go.gameObject);
        }

        /// <summary>
        /// 查找子对象
        /// </summary>
        public static GameObject Child(GameObject go, string subnode)
        {
            return Child(go.transform, subnode);
        }

        /// <summary>
        /// 查找子对象
        /// </summary>
        public static GameObject Child(Transform go, string subnode)
        {
            Transform tran = go.Find(subnode);
            if (tran == null) return null;
            return tran.gameObject;
        }

        /// <summary>
        /// 取平级对象
        /// </summary>
        public static GameObject Peer(GameObject go, string subnode)
        {
            return Peer(go.transform, subnode);
        }

        /// <summary>
        /// 取平级对象
        /// </summary>
        public static GameObject Peer(Transform go, string subnode)
        {
            Transform tran = go.parent.Find(subnode);
            if (tran == null) return null;
            return tran.gameObject;
        }

        /// <summary>
        /// 清除所有子节点
        /// </summary>
        public static void ClearChild(Transform go)
        {
            if (go == null) return;
            for (int i = go.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(go.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 清理内存
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            Resources.UnloadUnusedAssets();

        }

        /// <summary>
        /// 取得行文本
        /// </summary>
        public static string GetFileText(string path)
        {
            return File.ReadAllText(path);
        }

        /// <summary>
        /// 是否显示log日志
        /// </summary>
        /// <param name="show"></param>
        public static void ShowLog(bool show)
        {
            GameLogger.WriteLog = show;
        }
        /// <summary>
        /// 是否将日志打印到日志文件
        /// </summary>
        /// <param name="printBol"></param>
        public static void PrintLogToFile(bool printBol)
        {
            GameLogger.WriteToFile = printBol;
        }
        public static void Log(string str)
        {
            GameLogger.Log(str);
        }

        public static void LogWarning(string str)
        {
            GameLogger.LogWarning(str);
        }

        public static void LogError(string str)
        {
            GameLogger.LogError(str);
        }
        public static void Log(object obj)
        {
            GameLogger.Log(obj);
        }

        public static void LogWarning(object obj)
        {
            GameLogger.LogWarning(obj);
        }
        public static void LogError(object obj)
        {
            GameLogger.LogError(obj);
        }

    

        /// <summary>
        /// 网络可用
        /// </summary>
        public static bool NetAvailable
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }

        /// <summary>
        /// 是否是无线
        /// </summary>
        public static bool IsWifi
        {
            get
            {
                return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
            }
        }

        /**************************self add**************************/
        //检查网络是否连接上,1表示未连接网络状态，2表示连接本地网络（网线或者wifi），3表示连接移动网络
        public static int CheckNetWork()
        {
            //当网络不可用时              
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return 1;
            }
            else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                //如果项目需要耗费的流量比较大，可以通过下面的方法判断，并提示用户
                //当用户使用WiFi时 
                return 2;
            }
            else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                //当用户使用移动网络时 
                return 3;
            }
            return 1;
        }
        //判断游戏是否暂停，统一处理方法
        public void OnApplicationPause(bool pause)
        {
#if UNITY_IPHONE || UNITY_ANDROID
            //Util.CallMethod("GlobalListener", "OnApplicationPause", pause);
#endif
        }

        //游戏焦点处理
        //失去焦点的时间
        private double loseFocusTime = 0;
        public void OnApplicationFocus(bool focus)
        {
#if UNITY_IPHONE || UNITY_ANDROID
            if (!focus)
            {
                //失去焦点
                loseFocusTime = Util.GetTimeStamp();
            }
            else
            {
                //获得焦点
                if (loseFocusTime > 0)
                {
                    //Util.CallMethod("GlobalListener", "OnApplicationFocus", Util.GetTimeStamp() - loseFocusTime);
                }
            }
#endif
        }

        public static string DateTimeString
        {
            get
            {
                return DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second;
            }
        }

        public static int DateTimeToInt()
        {
            string nowTime = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
            int value = DateTime.Now.GetHashCode();
            return value;
        }

        public static byte[] StringToBytes(string str)
        {
            return Encoding.GetEncoding("utf-8").GetBytes(str);
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static double GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return double.Parse(Convert.ToInt64(ts.TotalMilliseconds).ToString());
        }

        public static string MillisecondToData(float t)
        {

            TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(t/1000));
            int hour = ts.Hours;
            int minute = ts.Minutes;
            int second = ts.Seconds;
            return string.Format("{0:#00}:{1:#00}:{2:#00}", hour, minute, second);
        }

        // 从一个对象信息生成Json串
        public static string ObjectToJson(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string md5file(string file)
        {
            try
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                byte[] retVal = m_md5.ComputeHash(fs);
                fs.Close();
                fs.Dispose();
                sb.Clear();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }

        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        public static string md5(string source)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            byte[] md5Data = m_md5.ComputeHash(data, 0, data.Length);

            string destString = "";
            for (int i = 0; i < md5Data.Length; i++)
            {
                destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }
            destString = destString.PadLeft(32, '0');
            return destString;
        }

        /// <summary>
        /// 取得数据存放目录
        /// </summary>
        public static string DataPath
        {
            get
            {
                string game = AppConst.AppName.ToLower();
                if (Application.isMobilePlatform)
                {
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        //return Application.persistentDataPath + AppConst.dirSep + game + AppConst.dirSep;
                        return "jar:file://" + Application.dataPath + "!assets/";
                    }
                    else
                    {
                        return Application.temporaryCachePath + AppConst.dirSep + game + AppConst.dirSep;
                    }
                }

#if DEBUG_TEST
                return Application.dataPath + AppConst.dirSep + AppConst.AssetDir + AppConst.dirSep;
#endif
                if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    //int i = Application.dataPath.LastIndexOf('/');
                    //return Application.dataPath.Substring(0, i + 1) + game + AppConst.dirSep;
                    return Application.dataPath + "/Raw/";
                }
                else if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    //return Application.persistentDataPath + AppConst.dirSep + game + AppConst.dirSep;
                    return Application.streamingAssetsPath + AppConst.dirSep;
                }
                return Application.streamingAssetsPath + AppConst.dirSep;
            }
        }

        public static string GetRelativePath()
        {
            if (Application.isEditor)
                return "file://" + System.Environment.CurrentDirectory.Replace("\\", AppConst.dirSep) + "/Assets/" + AppConst.AssetDir + AppConst.dirSep;
            else if (Application.isMobilePlatform || Application.isConsolePlatform)
                return "file:///" + DataPath;
            else // For standalone player.
                return "file://" + Application.streamingAssetsPath + AppConst.dirSep;
        }


        /// <summary>
        /// 应用程序内容路径
        /// </summary>
        public static string AppContentPath()
        {
            string path = string.Empty;
            string game = AppConst.AppName.ToLower();

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    //path = "jar:file://" + Application.dataPath + "!/assets/";
                    path= Application.persistentDataPath + AppConst.dirSep + game + AppConst.dirSep;

                    break;
                case RuntimePlatform.IPhonePlayer:
                    path = Application.dataPath + "/Raw/";
                    break;
                //case RuntimePlatform.WindowsPlayer:

                //    path= Application.persistentDataPath + AppConst.dirSep + game + AppConst.dirSep;
                //    break;
                default:
                    path= "c:/" + game + AppConst.dirSep;
                    break;
            }
            return path;
        }
    }
}