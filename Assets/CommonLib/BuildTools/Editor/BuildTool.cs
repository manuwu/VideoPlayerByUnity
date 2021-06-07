using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Xml;
using System.Text;

public class BuildTool
{
    private const string USING_VR9 = "SVR_VR9";
    private const string USING_SVR = "SVR";
    private const string USING_SVR_LEGACY = "SVR_LEGACY";
    private const string DebugKey = "ENABLE_LOG";
    static void PerformAndroidBuild()
    {
#if UNITY_ANDROID
        bool SwitchActiveBuildTarget = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        bool SwitchActiveBuildTargetAsync = EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
#else
        //EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        Debug.Log("xxxxx "+EditorUserBuildSettings.activeBuildTarget);
#endif
        //EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
        //Debug.Log("SwitchActiveBuildTarget:" + SwitchActiveBuildTarget);
        //Debug.Log("SwitchActiveBuildTargetAsync:" + SwitchActiveBuildTargetAsync);
        Debug.Log("androidBuildSystem:" + EditorUserBuildSettings.androidBuildSystem);

        buildTarget(Environment.GetCommandLineArgs());
    }
    private static void buildTarget(params string[] systemparms)
    {
        string java_home = "";
        string ANDROID_SDK = "";
        string apkpath = "";
        string apkName = "";
        string platform = "901";
        int versionCode = 1;
        string commit = "";
        bool usgradle = false;
        string branch = "";
        int allLength = systemparms.Length;

        string executeMethod = "BuildTool.PerformAndroidBuild";
        //EditorPrefs.SetString("AndroidSdkRoot", androidsdk);
        int startindex = 0;
        for (int i = 0; i < allLength; i++)
        {
            if (systemparms[i] == executeMethod)
            {
                startindex = i;
                break;
            }
        }
        if (startindex < allLength)
        {
            java_home = systemparms[startindex + 1];
            ANDROID_SDK = systemparms[startindex + 2];
            apkpath = systemparms[startindex + 3];
            apkName = systemparms[startindex + 4];
            platform = systemparms[startindex + 5];
            versionCode = int.Parse(systemparms[startindex + 6]);
            commit = systemparms[startindex + 7];
            Debug.Log("usgradle:" + systemparms[startindex + 8]);
            usgradle = bool.Parse(systemparms[startindex + 8]);
            if (startindex + 9 < systemparms.Length)
            {
                branch = systemparms[startindex + 9];
            }
            if (!string.IsNullOrEmpty(ANDROID_SDK))
                EditorPrefs.SetString("AndroidSdkRoot", ANDROID_SDK);
            if (!string.IsNullOrEmpty(java_home))
                EditorPrefs.SetString("JdkPath", java_home);
            Debug.Log("java_Home:" + java_home);
            Debug.Log("ANDROID_SDK:" + ANDROID_SDK);
            Debug.Log("apkpath:" + apkpath);
            Debug.Log("platform:" + platform);
            Debug.Log("commit:" + commit);
            Debug.Log("usgradle:" + usgradle);
#if UNITY_ANDROID
            if (usgradle)
                EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            else
                EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
#endif
        }
        apkpath = apkpath + "/" + platform;
        if (!Directory.Exists(apkpath))
            Directory.CreateDirectory(apkpath);
        CreatSfxConfigFile(apkpath, apkName);
#if UNITY_ANDROID
        apkpath = apkpath + "/" + apkName + "_" + branch + "_" + commit + ".apk";
        if (File.Exists(apkpath))
            File.Delete(apkpath);
#else
        apkpath = apkpath + "/" + apkName;
        if (!Directory.Exists(apkpath))
            Directory.CreateDirectory(apkpath);
        else
            Directory.Delete(apkpath,true);
        
        apkpath += "/"+ apkName + ".exe";
#endif
#if UNITY_ANDROID
        if (platform == "901")
        {
            apply901();
        }
        else if (platform == "mobile")
        {
            apply901();
            //UpdateAar();
            UpdateAndroidManifest();
        }
        else
        {
            applyVR9();
        }
        setsplash();
        Debug.Log("PackageName:" + PlayerSettings.applicationIdentifier);
        if (PlayerSettings.applicationIdentifier.ToLower() == "com.ssnwt.newskyui")
        {
            setKeystor(Application.dataPath + "/Plugins/Android/android.keystore", "android.keystore", "wubuandroid123");
        }
        else
        {
            setKeystor(Application.dataPath + "/CommonLib/BuildTools/signkeystore/ssnwt.keystore", "ssnwt", "anndroid_ssnwt");
        }
#endif
        var scenes = getScenes(platform);
        if (!string.IsNullOrEmpty(branch))
        {
            if (branch.ToLower().Contains("release"))
                DisableLog();
            else
                EnableLog();

            Debug.Log("LogUtile:" + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android));
        }
        buildApk(apkpath, versionCode, commit, scenes);
    }
    private static void setKeystor(string keysotrepath, string key, string password)
    {
        //string keysotrepath = path;// Application.dataPath + "/Plugins/Android/android.keystore";
        //string key =  "android.keystore";
        //string password =  "wubuandroid123";
        if (!File.Exists(keysotrepath))
        {
            throw new UnityException("The signature file does not exist in path = " + keysotrepath);
        }
        PlayerSettings.Android.keystoreName = keysotrepath;
        PlayerSettings.Android.keystorePass = password;
        PlayerSettings.Android.keyaliasName = key;
        PlayerSettings.Android.keyaliasPass = password;
    }
    private static void setsplash()
    {
        string configpath = Application.dataPath + "/Plugins/Android/assets/splash.cfg";
        if (!Directory.Exists(Application.dataPath + "/Plugins")) Directory.CreateDirectory(Application.dataPath + "/Plugins");
        if (!Directory.Exists(Application.dataPath + "/Plugins/Android")) Directory.CreateDirectory(Application.dataPath + "/Plugins/Android");
        if (!Directory.Exists(Application.dataPath + "/Plugins/Android/assets")) Directory.CreateDirectory(Application.dataPath + "/Plugins/Android/assets");
        if (File.Exists(configpath))
        {
            File.Delete(configpath);
        }
        StreamWriter sw = File.CreateText(configpath);
        sw.WriteLine("UNITY_USE_SPLASH=0");
        sw.WriteLine("USE_SVR_SPLASH=1");
        sw.Close();
    }
    private static void buildTest()
    {
        string executeMethod = "BuildTool.PerformAndroidBuild";
        string java_home = "";
        string androidsdk = "";
        string apkpath = Directory.GetParent(Application.dataPath) + "/signapk";
        string apkName = PlayerSettings.productName;
        string platform = "801";
        string commit = "1.0";
        string usgradle = "false";
        buildTarget(new string[] { executeMethod, java_home, androidsdk, apkpath, apkName, platform, commit, usgradle });
    }

    static bool FindVR9()
    {
        string macros = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        string[] macrosList = macros.Split(';');
        for (int i = 0; i < macrosList.Length; i++)
        {
            if (macrosList[i] == USING_VR9)
            {
                return true;
            }
        }
        return false;
    }
    static bool FindSVR()
    {
        string macros = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        string[] macrosList = macros.Split(';');
        for (int i = 0; i < macrosList.Length; i++)
        {
            if (macrosList[i] == USING_SVR)
            {
                return true;
            }
        }
        return false;
    }
    static void SetSVRConfig()
    {
        if (!FindSVR())
        {
            string macros = DeleteAll();
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, macros + ";" + USING_SVR);
            //Svr.SVRLoad.ApplaySVRSettings();
        }
    }
    static void SetVR9Config()
    {
        if (!FindVR9())
        {

            string macros = DeleteAll();
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, macros + ";" + USING_VR9);
        }
    }
    private static string DeleteAll()
    {
        string macros = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        string[] macrosList = macros.Split(';');
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < macrosList.Length; i++)
        {

            if (macrosList[i] != USING_VR9 && macrosList[i] != USING_SVR && macrosList[i] != USING_SVR_LEGACY)
            {
                sb.Append(macrosList[i]);
                if (i < macrosList.Length - 1)
                    sb.Append(";");
            }
        }
        return sb.ToString();
    }
    static void EnableLog()
    {
        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
        bool found = false;
        string macros = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);

        string[] macrosList = macros.Split(';');
        for (int i = 0; i < macrosList.Length; i++)
        {
            if (macrosList[i] == DebugKey)
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            Debug.Log("EnableLog:" + macros + ";" + DebugKey);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, macros + ";" + DebugKey);
        }
    }
    static void DisableLog()
    {

        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        bool found = false;
        string macros = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        string[] macrosList = macros.Split(';');
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < macrosList.Length; i++)
        {
            if (macrosList[i] != DebugKey)
            {
                sb.Append(macrosList[i]);
                if (i < macrosList.Length - 1)
                    sb.Append(";");
            }
            else
            {
                found = true;
            }
        }
        if (found)
        {
            Debug.Log("DisableLog:" + sb.ToString());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, sb.ToString());
        }
    }
    static void applyVR9()
    {
        SetVR9Config();
        PlayerSettings.Android.blitType = AndroidBlitType.Never;
        PlayerSettings.virtualRealitySupported = false;
    }
    static void apply901()
    {
        SetSVRConfig();
        PlayerSettings.Android.blitType = AndroidBlitType.Always;
        PlayerSettings.virtualRealitySupported = true;
        PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
        PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);
    }
    static List<string> getScenes(string platform)
    {
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            //if (scene.enabled)
            //{
#if UNITY_ANDROID
            if (platform == "mobile")
            {
                if (scene.path.Contains("801") || scene.path.Contains("901"))
                {
                    if (scene.path.Contains("901"))
                        scenes.Add(scene.path);
                }
                else
                {
                    if (scene.enabled)
                        scenes.Add(scene.path);
                }
            }
            else
            {
                if (scene.path.Contains("801") || scene.path.Contains("901"))
                {
                    if (scene.path.Contains(platform))
                        scenes.Add(scene.path);
                }
                else
                {
                    if (scene.enabled)
                        scenes.Add(scene.path);
                }
            }
#else
            if (scene.enabled)
                scenes.Add(scene.path);
#endif
            //}

        }
        return scenes;
    }
    static void buildApk(string path, int versioncode, string versionName, List<string> scenes)
    {
        GC.Collect();
        if (scenes == null || scenes.Count == 0)
        {
            throw new UnityException("错误:未添加场景");
        }
        else
        {
            foreach (var item in scenes)
            {
                Debug.Log("Scenes:" + item);
            }
        }
        AssetDatabase.Refresh();

        BuildOptions buildOptions = BuildOptions.None;

        if (EditorUserBuildSettings.development)
        {
            buildOptions |= BuildOptions.Development | BuildOptions.ConnectWithProfiler;
        }

        PlayerSettings.SplashScreen.show = false;
#if UNITY_ANDROID
        PlayerSettings.Android.bundleVersionCode = versioncode;
#endif
        PlayerSettings.bundleVersion = versionName;

        AssetDatabase.SaveAssets();
        Debug.Log("signapkDirectory:" + path);
        
#if UNITY_ANDROID
        BuildTarget buildTarget = BuildTarget.Android;
#else
        BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
#endif
        var result = BuildPipeline.BuildPlayer(scenes.ToArray(), path, buildTarget, buildOptions);
#if UNITY_2018
        Debug.Log("Build Result : " + result.summary.result);
        if (result.summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
        {
            Debug.Log("Errors " + result.summary.totalErrors);
            foreach (var item in result.steps)
            {
                Debug.LogFormat("{0}:{1}", item.name, item.messages);
                foreach (var item_msg in item.messages)
                {
                    if (item_msg.type == LogType.Warning || item_msg.type == LogType.Log) continue;
                    Debug.LogFormat("log:{0},content:{1}", item_msg.type, item_msg.content);
                }
            }
        }
#endif
#if UNITY_2017
        if(result != null)
            Debug.Log("Build Result : " + result);
        else
            Debug.Log("Build Result : Success");
#endif
    }
    #region AAR
    //[MenuItem("SkyworthVR/Tools/Update aar")]
    public static void UpdateAar()
    {
        string PluginPath = "Assets/CommonLib/Plugins/Android/";
        string androidinterface = "svr_plugin_android_interface.aar";
        string skyupgrade = "skyupgrade.jar";
        string apiinterface = "svr_plugin_android_api.aar.bac";
        if (File.Exists(Path.Combine(PluginPath, androidinterface)))
        {
            File.Delete(Path.Combine(PluginPath, androidinterface));
            Debug.Log("Delete svr_plugin_android_interface.aar");
        }
        if (File.Exists(Path.Combine(PluginPath, skyupgrade)))
        {
            File.Delete(Path.Combine(PluginPath, skyupgrade));
            Debug.Log("Delete skyupgrade.jar");
        }
        if (File.Exists(Path.Combine(PluginPath, apiinterface)))
        {
            var FileSouce = File.ReadAllBytes(Path.Combine(PluginPath, apiinterface));
            File.Delete(Path.Combine(PluginPath, apiinterface));
            File.WriteAllBytes(Path.Combine(PluginPath, "svr_plugin_android_api.aar"), FileSouce);
            Debug.Log("Replace svr_plugin_android_api.aar.bac to svr_plugin_android_api.aar");
        }
        AssetDatabase.Refresh();
    }
    #endregion
    #region Mainfest
    //[MenuItem("SkyworthVR/Tools/Update AndroidManifest.xml")]
    public static void UpdateAndroidManifest()
    {
        string manifestFile = "Assets/Plugins/Android/AndroidManifest.xml";

        if (!File.Exists(manifestFile))
        {
            Debug.LogError("Unable to update manifest because it does not exist! Run \"Create store-compatible AndroidManifest.xml\" first");
            return;
        }

        PatchAndroidManifest(manifestFile, skipExistingAttributes: false);
        AssetDatabase.Refresh();
    }
    public static void PatchAndroidManifest(string sourceFile, string destinationFile = null, bool skipExistingAttributes = true, bool enableSecurity = false)
    {
        if (destinationFile == null)
        {
            destinationFile = sourceFile;
        }

        bool modifyIfFound = !skipExistingAttributes;

        try
        {

            // Load android manfiest file
            XmlDocument doc = new XmlDocument();
            doc.Load(sourceFile);

            string androidNamepsaceURI;
            string sharedUserId;
            XmlElement element = (XmlElement)doc.SelectSingleNode("/manifest");
            if (element == null)
            {
                UnityEngine.Debug.LogError("Could not find manifest tag in android manifest.");
                return;
            }

            // Get android namespace URI from the manifest
            androidNamepsaceURI = element.GetAttribute("xmlns:android");
            if (string.IsNullOrEmpty(androidNamepsaceURI))
            {
                UnityEngine.Debug.LogError("Could not find Android Namespace in manifest.");
                return;
            }
            sharedUserId = element.GetAttribute("android:sharedUserId");
            if (!string.IsNullOrEmpty(androidNamepsaceURI))
            {
                UnityEngine.Debug.Log("Remove Android sharedUserId in manifest.");
                element.RemoveAttribute("android:sharedUserId");
            }

            doc.Save(destinationFile);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }
    private static void AddOrRemoveTag(XmlDocument doc, string @namespace, string path, string elementName, string name, bool required, bool modifyIfFound, params string[] attrs) // name, value pairs	
    {
        var nodes = doc.SelectNodes(path + "/" + elementName);
        XmlElement element = null;
        foreach (XmlElement e in nodes)
        {
            if (name == null || name == e.GetAttribute("name", @namespace))
            {
                element = e;
                break;
            }
        }

        if (required)
        {
            if (element == null)
            {
                var parent = doc.SelectSingleNode(path);
                element = doc.CreateElement(elementName);
                element.SetAttribute("name", @namespace, name);
                parent.AppendChild(element);
            }

            for (int i = 0; i < attrs.Length; i += 2)
            {
                if (modifyIfFound || string.IsNullOrEmpty(element.GetAttribute(attrs[i], @namespace)))
                {
                    if (attrs[i + 1] != null)
                    {
                        element.SetAttribute(attrs[i], @namespace, attrs[i + 1]);
                    }
                    else
                    {
                        element.RemoveAttribute(attrs[i], @namespace);
                    }
                }
            }
        }
        else
        {
            if (element != null && modifyIfFound)
            {
                element.ParentNode.RemoveChild(element);
            }
        }
    }
    #endregion
    #region SFX Config
    private static void CreatSfxConfigFile(string path,string DIST_NAME)
    {
        string filename = Path.Combine(path,"info.config");
        if (File.Exists(filename))
        {
            Debug.Log(File.ReadAllText(filename));
            return;
        }
        string setup = "Setup="+DIST_NAME+"\\CenterControl.exe";
        File.WriteAllLines(filename, new string[] { setup, "TempMode", "Silent=1", "Overwrite=1", "Update=U", "SetupCode" });
        Debug.Log("Create File:"+File.ReadAllText(filename));
    }
    #endregion
}
