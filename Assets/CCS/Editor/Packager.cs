using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CCS;
using System;

public class Packager
{
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();
    public static List<AssetBundleBuild> maps = new List<AssetBundleBuild>();
    static string[] exts = { ".txt", ".xml", ".lua", ".assetbundle", ".json" };
    static bool CanCopy(string ext)
    {   //能不能复制
        foreach (string e in exts)
        {
            if (ext.Equals(e)) return true;
        }
        return false;
    }


#if UNITY_IPHONE
    [MenuItem("CCS/Build iPhone Resource", false, 100)]
    public static void BuildiPhoneResource()
    {
        BuildTarget target;
#if UNITY_5
        target = BuildTarget.iOS;
#else
        target = BuildTarget.iOS;
#endif
        BuildAssetResource(target);
    }
#elif UNITY_ANDROID
    [MenuItem("CCS/Build Android Resource", false, 101)]
    public static void BuildAndroidResource()
    {
        BuildAssetResource(BuildTarget.Android);
    }
#else
    [MenuItem("CCS/Build Windows Resource", false, 102)]
    public static void BuildWindowsResource()
    {
        BuildAssetResource(BuildTarget.StandaloneWindows);
    }
#endif

    /// <summary>
    /// 生成绑定素材
    /// </summary>
    public static void BuildAssetResource(BuildTarget target)
    {
        string sourcefile = Application.dataPath + "/InfoAssetBundles/AssetBundleInfo.csv";
        if (Directory.Exists(Application.dataPath + AppConst.dirSep + AppConst.AssetDir + AppConst.dirSep))
        {
            Directory.Delete(Application.dataPath + AppConst.dirSep + AppConst.AssetDir + AppConst.dirSep, true);
        }
        string streamPath = Application.streamingAssetsPath;
        if (Directory.Exists(streamPath))
        {
            Directory.Delete(streamPath, true);
        }
        Directory.CreateDirectory(streamPath);
        AssetDatabase.Refresh();

        maps.Clear();
        //if (AppConst.LuaBundleMode)
        //{
        //    HandleLuaBundle(AppConst.LuaByteMode, target);
        //}
        //else
        //{
        //    HandleLuaFile(AppConst.LuaByteMode);
        //}
        HandleExampleBundle(target);
        //string curPath = System.Environment.CurrentDirectory;
        //string srcPath = curPath + @"/工程资源/CSVTool/CSVTool/bin/Release/Bytes";
        //string destPath = curPath + @"/Assets/StreamingAssets";
        //if (Directory.Exists(srcPath) && Directory.Exists(destPath))
        //{
        //    CopyDir(srcPath, destPath);
        //}

        string streamDir = Application.dataPath + AppConst.dirSep + AppConst.LuaTempDir;
        if (Directory.Exists(streamDir)) Directory.Delete(streamDir, true);
        AssetDatabase.Refresh();
    }

    public static void CopyDir(string srcPath, string destPath)
    {
        try
        {
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)     //判断是否文件夹
                {
                    if (!Directory.Exists(destPath + AppConst.dirSep + i.Name))
                    {
                        Directory.CreateDirectory(destPath + AppConst.dirSep + i.Name);   //目标目录下不存在此文件夹即创建子文件夹
                    }
                    CopyDir(i.FullName, destPath + AppConst.dirSep + i.Name);    //递归调用复制子文件夹
                }
                else
                {
                    File.Copy(i.FullName, destPath + AppConst.dirSep + i.Name, true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
                }
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }
    public static void AddBuildMap(string bundleName, string pattern, string path)
    {
        string[] files = Directory.GetFiles(path, pattern);
        if (files.Length == 0) return;

        for (int i = 0; i < files.Length; i++)
        {
            files[i] = files[i].Replace('\\', '/');
        }

        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = bundleName;
        build.assetNames = files;
        maps.Add(build);
    }

    /// <summary>
    /// 处理Lua代码包
    /// </summary>
    public static void HandleLuaBundle(bool LuaByteMode, BuildTarget target)
    {
        //string streamDir = Application.dataPath + AppConst.dirSep + AppConst.LuaTempDir;
        //if (!Directory.Exists(streamDir)) Directory.CreateDirectory(streamDir);

        //Debugger.LogError("路径：" + CustomSettings.luaDir);
        //Debugger.LogError("路径：" + CustomSettings.FrameworkPath);

        //string[] srcDirs = { CustomSettings.luaDir, CustomSettings.FrameworkPath + "/ToLua/Lua" };
        //for (int i = 0; i < srcDirs.Length; i++)
        //{
        //    if (LuaByteMode)
        //    {
        //        string sourceDir = srcDirs[i];
        //        string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
        //        int len = sourceDir.Length;

        //        if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
        //        {
        //            --len;
        //        }
        //        for (int j = 0; j < files.Length; j++)
        //        {
        //            string str = files[j].Remove(0, len);
        //            string dest = streamDir + str + ".bytes";
        //            string dir = Path.GetDirectoryName(dest);
        //            Directory.CreateDirectory(dir);
        //            EncodeLuaFile(files[j], dest);
        //        }
        //    }
        //    else
        //    {
        //        ToLuaMenu.CopyLuaBytesFiles(srcDirs[i], streamDir);
        //    }
        //}
        //string[] dirs = Directory.GetDirectories(streamDir, "*", SearchOption.AllDirectories);
        //for (int i = 0; i < dirs.Length; i++)
        //{
        //    string name = dirs[i].Replace(streamDir, string.Empty);
        //    name = name.Replace('\\', '_').Replace('/', '_');
        //    name = "lua/lua_" + name.ToLower() + AppConst.ExtName;

        //    string path = "Assets" + dirs[i].Replace(Application.dataPath, "");
        //    AddBuildMap(name, "*.bytes", path);
        //}
        //AddBuildMap("lua/lua" + AppConst.ExtName, "*.bytes", "Assets/" + AppConst.LuaTempDir);

        ////-------------------------------处理非Lua文件将项目中的lua文件复制到StreamingAssets下面----------------------------------
        //string luaPath = AppDataPath + "/StreamingAssets/lua/";
        //for (int i = 0; i < srcDirs.Length; i++)
        //{
        //    paths.Clear(); files.Clear();
        //    string luaDataPath = srcDirs[i].ToLower();
        //    Recursive(luaDataPath);
        //    foreach (string f in files)
        //    {
        //        if (f.EndsWith(".meta") || f.EndsWith(".lua") || f.StartsWith(".")) continue;
        //        string newfile = f.Replace(luaDataPath, "");
        //        string path = Path.GetDirectoryName(luaPath + newfile);
        //        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        //        string destfile = path + AppConst.dirSep + Path.GetFileName(f);
        //        File.Copy(f, destfile, true);
        //    }
        //}
        //AssetDatabase.Refresh();
        //string resPath = AppDataPath + AppConst.dirSep + AppConst.AssetDir;
        //BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle;
        //BuildPipeline.BuildAssetBundles(resPath, maps.ToArray(), options, target);
    }

    /// <summary>
    /// 处理框架实例包
    /// </summary>
    static void HandleExampleBundle(BuildTarget target)
    {
        string resPath = AppDataPath + AppConst.dirSep + AppConst.AssetDir + AppConst.dirSep;
        if (!Directory.Exists(resPath)) Directory.CreateDirectory(resPath);


        //查找所有的csv
        string[] filesNames = Directory.GetFiles(Application.dataPath + "/InfoAssetBundles", "*.csv");
        foreach (string filename in filesNames)
        {
            string csvName = filename.Replace('\\', '/');
            csvName = csvName.Substring(csvName.LastIndexOf('/') + 1);
            csvName = csvName.Substring(0, csvName.IndexOf('.'));
            //此方法有可能会有问题，需要修改
            maps.Clear();

            string tmpFileName = "";
            tmpFileName = filename.Replace('\\', '/');
            string content = File.ReadAllText(tmpFileName);

            string[] contents = content.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < contents.Length; i++)
            {
                string[] a = contents[i].Split(',');
                AddBuildMap(a[0], a[1], a[2]);
            }
            CurCVSToAssetBundle(csvName, target);
            //开始创建版本文件（txt文件）
            if (csvName.IndexOf("AssetBundleInfo") != -1)
            {
                BuildGameHallFile(csvName);
            }
            else
            {
                string cvspath = BuildSignGameFile(csvName);
                string cvsTxtName = csvName + ".txt";
                GameVersionFileTxtToFiles(cvspath, cvsTxtName);
            }
        }
    }
    //将当前cvs中的所有文件打包成ab
    public static void CurCVSToAssetBundle(string csvName, BuildTarget target)
    {
        string resPath = AppDataPath + AppConst.dirSep + AppConst.AssetDir;

        if (csvName != "AssetBundleInfo")
        {
            resPath = AppDataPath + AppConst.dirSep + AppConst.AssetDir + AppConst.dirSep + csvName + AppConst.dirSep;
            if (!Directory.Exists(resPath)) Directory.CreateDirectory(resPath);
        }
        BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle;

        BuildPipeline.BuildAssetBundles(resPath, maps.ToArray(), options, target);
    }
    /// <summary>
    /// 处理Lua文件
    /// </summary>
    public static void HandleLuaFile(bool LuaByteMode)
    {
        string resPath = AppDataPath + "/StreamingAssets/";
        string luaPath = resPath + "/lua/";

        //----------复制Lua文件----------------
        if (!Directory.Exists(luaPath))
        {
            Directory.CreateDirectory(luaPath);
        }
        string[] luaPaths = { AppDataPath + "/DDZ/lua/",
                              AppDataPath + "/DDZ/Tolua/Lua/" };

        for (int i = 0; i < luaPaths.Length; i++)
        {
            paths.Clear(); files.Clear();
            string luaDataPath = luaPaths[i].ToLower();
            Recursive(luaDataPath);
            int n = 0;
            foreach (string f in files)
            {
                if (f.EndsWith(".meta") || (f.EndsWith(".proto")) || f.Contains(".manifest") || f.StartsWith(".")) continue;
                string newpath = f.Replace(luaDataPath, luaPath);
                string path = Path.GetDirectoryName(newpath);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                if (File.Exists(newpath))
                {
                    File.Delete(newpath);
                }

                byte[] str = File.ReadAllBytes(f);
                if (str[0] == 255)
                    Util.LogError("文件编码可能存在问题,检查一下。" + f);

                if (LuaByteMode)
                {
                    EncodeLuaFile(f, newpath);
                }
                else
                {
                    File.Copy(f, newpath, true);
                }
                UpdateProgress(n++, files.Count, newpath);
            }
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 将单个游戏的版本文件cvs，txt文件添加到files.txt中，使用游戏在启动时候被下载
    /// </summary>
    /// <param name="gameVersionFilePath">要添加的版本文件的全路径名称</param>
    public static void GameVersionFileTxtToFiles(string gameVersionFilePath, string cvsTxtName)
    {
        string filesTxtPath = AppDataPath + AppConst.dirSep + AppConst.AssetDir + AppConst.dirSep + "files.txt";
        using (FileStream fs = new FileStream(filesTxtPath, FileMode.Append))
        {
            string md5 = Util.md5file(gameVersionFilePath);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(cvsTxtName + "|" + md5);
            sw.Flush();
            sw.Close();
        }
    }

    /// <summary>
    /// 加载游戏大厅及龙珠探宝游戏（初始游戏）的版本检查文件
    /// </summary>
    public static void BuildGameHallFile(string csvName)
    {
        string resPath = AppDataPath + AppConst.dirSep + AppConst.AssetDir + AppConst.dirSep;
        string resLuaPath = resPath + "lua/";
        if (!csvName.Equals("AssetBundleInfo"))
        {
            resPath = resPath + csvName + AppConst.dirSep;
            string luaPath = resPath + "lua/";
            if (File.Exists(luaPath))
            {
                File.Delete(luaPath);
            }

            paths.Clear(); files.Clear();
            Recursive(resLuaPath);
            foreach (string f in files)
            {
                string newpath = f.Replace(resLuaPath, luaPath);
                string path = Path.GetDirectoryName(newpath);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                File.Copy(f, newpath, true);
            }
        }

        ///----------------------记录一下打包时间-----------------------
        string verFilePath = resPath + "ver.txt";
        if (File.Exists(verFilePath)) File.Delete(verFilePath);
        using (FileStream fsV = new FileStream(verFilePath, FileMode.CreateNew))
        {
            StreamWriter swV = new StreamWriter(fsV);
            DateTime date = DateTime.Now;
            string dateStr = date.ToString("yyyy-MM-dd HH:mm:ss");
            swV.WriteLine("Build Time: " + dateStr);
            swV.Flush();
        }

        ///----------------------创建文件列表-----------------------
        string newFilePath = resPath + "files.txt";
        Util.LogError("newFilePath:" + newFilePath);
        if (File.Exists(newFilePath)) File.Delete(newFilePath);
        paths.Clear(); files.Clear();
        Recursive(resPath);
        using (FileStream fs = new FileStream(newFilePath, FileMode.CreateNew))
        {
            StreamWriter sw = new StreamWriter(fs);
            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                string ext = Path.GetExtension(file);
                if (file.EndsWith(".meta") || file.Contains(".DS_Store") || file.Contains(".manifest") || file.Contains(".suo")) continue;

                string md5 = Util.md5file(file);
                string value = file.Replace(resPath, string.Empty);
                sw.WriteLine(value + "|" + md5);
            }
            sw.Flush();
        }
    }
    /// <summary>
    /// 创建单独加载游戏的版本检查文件
    /// </summary>
    public static string BuildSignGameFile(string csvName)
    {
        string resPath = AppDataPath + AppConst.dirSep + AppConst.AssetDir + AppConst.dirSep + csvName + AppConst.dirSep;
        ///----------------------创建文件列表-----------------------
        string newFilePath = AppDataPath + AppConst.dirSep + AppConst.AssetDir + AppConst.dirSep + csvName + ".txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        paths.Clear(); files.Clear();
        Recursive(resPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);

        string newFilePath1 = AppDataPath + AppConst.dirSep + AppConst.AssetDir + AppConst.dirSep + csvName + AppConst.dirSep + csvName + ".txt";
        FileStream fs1 = new FileStream(newFilePath1, FileMode.CreateNew);
        StreamWriter sw1 = new StreamWriter(fs1);

        string abValue = "";
        string abMd5 = "";
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            string ext = Path.GetExtension(file);
            if (file.EndsWith(".meta") || file.Contains(".DS_Store") || file.Contains(".manifest") || file.Contains(".suo")) continue;

            string md5 = Util.md5file(file);
            string value = file.Replace(resPath, string.Empty);
            if (string.IsNullOrEmpty(abValue))
            {
                abValue = value;
                abMd5 = md5;
            }
            sw.WriteLine(value + "|" + md5);
            sw1.WriteLine(value + "|" + md5);
        }
        sw.Close(); fs.Close();
        sw1.Close(); fs1.Close();
        return newFilePath;
    }
    /// <summary>
    /// lua
    /// </summary>
    public static string LuaFileMD5()
    {
        string resPath = AppDataPath + AppConst.dirSep + AppConst.AssetDir + "/lua/";
        ///----------------------创建文件列表-----------------------
        string newFilePath = resPath + "lua.txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        paths.Clear(); files.Clear();
        Recursive(resPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);

        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            string ext = Path.GetExtension(file);
            if (file.EndsWith(".meta") || file.Contains(".DS_Store") || file.Contains(".manifest") || file.Contains(".suo")) continue;

            string md5 = Util.md5file(file);
            string value = file.Replace(resPath, string.Empty);
            sw.WriteLine(value + "|" + md5);
        }
        sw.Close(); fs.Close();

        return newFilePath;
    }
    /// <summary>
    /// 数据目录
    /// </summary>
    public static string AppDataPath
    {
        get { return Application.dataPath.ToLower(); }
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path, bool isGen = false,bool findInSub = true)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (isGen)
            {
                if (ext.EndsWith(".meta")) continue;
            }
            else
            {
                if (ext.EndsWith(".meta") || (ext.EndsWith(".proto")) || ext.Contains(".manifest")) continue;
            }
            files.Add(filename.Replace('\\', '/'));
        }

        if (findInSub)
        {
            foreach (string dir in dirs)
            {
                paths.Add(dir.Replace('\\', '/'));
                Recursive(dir, isGen);
            }
        }
    }

    static void UpdateProgress(int progress, int progressMax, string desc)
    {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / (float)progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }

    public static void EncodeLuaFile(string srcFile, string outFile)
    {
        if (!srcFile.ToLower().EndsWith(".lua"))
        {
            File.Copy(srcFile, outFile, true);
            return;
        }
        bool isWin = true;
        string luaexe = string.Empty;
        string args = string.Empty;
        string exedir = string.Empty;
        string currDir = Directory.GetCurrentDirectory();
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            isWin = true;
            luaexe = "luajit.exe";
            args = "-b " + srcFile + " " + outFile;
            exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit/";
        }
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            isWin = false;
            luaexe = "./luac";
            args = "-o " + outFile + " " + srcFile;
            exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luavm/";
        }
        Directory.SetCurrentDirectory(exedir);
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = luaexe;
        info.Arguments = args;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.ErrorDialog = true;
        info.UseShellExecute = isWin;
        Util.Log(info.FileName + " " + info.Arguments);

        Process pro = Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    private static int HaveToRePlace(string f, string[] str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if ((f.EndsWith(str[i] + ".proto")))
                return i;
        }
        return -1;
       
    }

    [MenuItem("CCS/Build Protobuf-lua-gen File")]
    public static void BuildProtobufFile()
    {
        string dir = AppDataPath + "/DDZ/Lua/Logic";
        paths.Clear();
        files.Clear();
        Recursive(dir, true,false);

        string protoc = "d:/protobuf-master/src/protoc.exe";
        string protoc_gen_dir = "\"d:/protoc-gen-lua-master/plugin/protoc-gen-lua.bat\"";

        float count = files.Count - 1;
        int index = 0;

        StringBuilder sb = new StringBuilder();
        UTF8Encoding utf8 = new UTF8Encoding(false, false);
        bool hasBom = false;
        foreach (string f in files)
        {

            index++;
            string name = Path.GetFileName(f);
            string ext = Path.GetExtension(f);
            if (!ext.Equals(".proto")) continue;

            using (FileStream fs = new FileStream(f, FileMode.Open, FileAccess.ReadWrite))
            {
                hasBom = !IsUTF8(fs);
            }

            if (hasBom)
            {
                string content;
                using (StreamReader sr = new StreamReader(f, Encoding.Default, false))
                {
                    content = sr.ReadToEnd();
                    Debugger.LogError("协议不是UTF8-无bom" + f + ":" + sr.CurrentEncoding);

                }

                using (StreamWriter sw = new StreamWriter(f, false, utf8))
                {
                    sw.Write(content);
                }
            }

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = protoc;
            info.Arguments = " --lua_out=./ --plugin=protoc-gen-lua=" + protoc_gen_dir + " " + name;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = true;
            info.UseShellExecute = false;
            info.WorkingDirectory = dir;
            info.ErrorDialog = true;
            Util.Log(info.FileName + " " + info.Arguments);
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.CreateNoWindow = true;

            Process pro = Process.Start(info);
            pro.WaitForExit();
            string error = pro.StandardError.ReadToEnd();
            //if(f)
            //    StreamReader reader = new StreamReader(path);

            // index到error , except就报错
            if (error.Length > 500 || error.IndexOf("rror") != -1 || error.IndexOf("xcept") != -1)
            {
                sb.Append(name);
                sb.Append("\n");
                sb.Append("order :");
                sb.Append(info.FileName);
                sb.Append(info.Arguments);
                sb.Append("\n");
                sb.Append(error);
                sb.Append("\n");
            }

            string[] ReplaceCount = { "Room", "DDZ_MID", "Arena", "PDK" };
            var nameIndex = HaveToRePlace(f, ReplaceCount);
            if (nameIndex != -1)
            {
                string str = f.Replace((ReplaceCount[nameIndex] + ".proto"), (ReplaceCount[nameIndex] + "_pb"));
                try
                {
                    StreamReader reader = new StreamReader(str + ".lua");
                    LinkedList<string> linelist = new LinkedList<string>();
                    int conti = 0;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (line.StartsWith("local "))
                        {
                            if (++conti > 200) 
                            line = line.Replace("local ", "");
                        }
                        linelist.AddLast(line);                        
                    }
                    
                    reader.Close();                  
                    //存储;  
                    StreamWriter writer = new StreamWriter(str + ".lua",false);
                    foreach (var item in linelist)
                    {
                        writer.WriteLine(item);
                    }
                    writer.Flush();
                    writer.Close();
                }
                catch (Exception ex)
                {
                    Util.Log(ex);
                    EditorUtility.DisplayDialog("警告", ReplaceCount[nameIndex] + ".proto生成失败", "OK");
                    EditorUtility.ClearProgressBar();
                    return;
                }
            }
            EditorUtility.DisplayProgressBar("Building .... ", name, index / count);
        }
        EditorUtility.ClearProgressBar();
        string s = sb.ToString();
        if (s != "")
        {
            Util.LogError(s);
            EditorUtility.DisplayDialog("警告", "可能有生成失败，请查看日志", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("提示", "成功生成所有的protobuf文件", "OK");
        }

        AssetDatabase.Refresh();
    }

    //0000 0000-0000 007F - 0xxxxxxx  (ascii converts to 1 octet!)
    //0000 0080-0000 07FF - 110xxxxx 10xxxxxx    ( 2 octet format)
    //0000 0800-0000 FFFF - 1110xxxx 10xxxxxx 10xxxxxx (3 octet format) 
    private static bool IsUTF8(FileStream sbInputStream)
    {
        int i;
        byte cOctets;  // octets to go in this UTF-8 encoded character
        byte chr;
        bool bAllAscii = true;
        long iLen = sbInputStream.Length;

        cOctets = 0;
        for (i = 0; i < iLen; i++)
        {
            chr = (byte)sbInputStream.ReadByte();

            if ((chr & 0x80) != 0) bAllAscii = false;

            if (cOctets == 0)
            {
                if (chr >= 0x80)
                {
                    do
                    {
                        chr <<= 1;
                        cOctets++;
                    }
                    while ((chr & 0x80) != 0);

                    cOctets--;
                    if (cOctets == 0) return false;
                }
            }
            else
            {
                if ((chr & 0xC0) != 0x80)
                {
                    return false;
                }
                cOctets--;
            }
        }

        if (cOctets > 0)
        {
            return false;
        }

        if (bAllAscii)
        {
            return false;
        }
        return true;
    }

}