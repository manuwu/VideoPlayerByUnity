
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UObject = UnityEngine.Object;
using System.Text;
using System;
#if UNITY_EDITOR && ASSET
using UnityEditor;
#endif

namespace CCS
{
#if UNITY_EDITOR && ASSET
    public class CsvItem
    {
        public string name;
        public string ext;
        public string path;
    }
#endif
    public class ResourceManager : Manager
    {
        private string[] m_Variants = { };
        private AssetBundleManifest manifest;
        public  AssetBundle shared, assetbundle;
        private Dictionary<string, AssetBundle> bundles;
        private Dictionary<string, AssetBundleManifest> manifestMap = new Dictionary<string, AssetBundleManifest>();

#if UNITY_EDITOR && ASSET
        public Dictionary<string, CsvItem> CsvList = new Dictionary<string, CsvItem>();

        void Awake()
        {
            string path = Application.dataPath + "/InfoAssetBundles";
            string[] files = Directory.GetFiles(path, "*.csv");

            for (int i = 0; i < files.Length; i++)
            {
                string fileText = Util.GetFileText(files[i]);

                string[] lines = fileText.Split('\n');

                for (int j = 0; j < lines.Length; j++)
                {
                    string[] content = lines[j].Split(',');

                    if (content.Length > 2)
                    {
                        string name = content[0].Substring(0, content[0].IndexOf(".unity3d"));

                        CsvItem item = new CsvItem();
                        item.name = name;
                        item.ext = content[1].Split('.')[1];
                        item.path = content[2].Trim();

                        CsvList.Add(name, item);
                    }
                }
            }
        }
#endif

        public void Initialize()
        {
            initBundlesCache();
            string uri = Util.DataPath + AppConst.AssetDir;
            //if (!File.Exists(uri)) return;
            if (null != assetbundle)
            {
                assetbundle.Unload(true);
            }
            assetbundle = AssetBundle.LoadFromFile(uri);
            manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        void initBundlesCache()
        {
            if(bundles == null)
                bundles = new Dictionary<string, AssetBundle>();
            if (bundles.Count != 0)
            {
                foreach (var item in bundles)
                {
                    item.Value.Unload(false);
                }
            }
            Resources.UnloadUnusedAssets();
            bundles.Clear();
        }
        /// <summary>
        /// 载入素材
        /// </summary>
        public T LoadAsset<T>(string abname, string assetname) where T : UnityEngine.Object
        {
            abname = abname.ToLower();
#if UNITY_EDITOR && ASSET
            CsvItem item = null;
            if (CsvList.TryGetValue(abname, out item))
            {
                var path = item.path + AppConst.dirSep + assetname + "." + item.ext;

                Util.Log(path);
                T temp = AssetDatabase.LoadAssetAtPath<T>(path);
                if (temp != null)
                {
                    return temp;
                }
                Util.Log("AssetDatabase.LoadAssetAtPath<T> is null");

            }
#endif         
            AssetBundle bundle = LoadAssetBundle(abname);
            if (bundle == null)
            {
                Util.LogError("加载失败 : " + abname + "  " + assetname);
                return null;
            }
            return bundle.LoadAsset<T>(assetname);
        }

#if UNITY_EDITOR && ASSET
        private List<T> LoadMore<T>(string abname, string[] assetNames) where T : UObject
        {
            List<T> res = new List<T>();
            for (int i = 0; i < assetNames.Length; i++)
            {
                res.Add(LoadAsset<T>(abname, assetNames[i]));
            }

            return res;
        }
#endif
        public void LoadMaterial(string abname, string[] assetNames)
        {
#if UNITY_EDITOR && ASSET
            var res = LoadMore<Material>(abname, assetNames);
            if (!abname.Equals(abname.ToLower()))
            {
                throw new System.Exception("LoadAssetBundle 方法传参 必须用小写！！！" + abname);
            }
#else
            abname = abname.ToLower();
            List<Material> result = new List<Material>();
            for (int i = 0; i < assetNames.Length; i++)
            {
                Material mat = LoadAsset<Material>(abname, assetNames[i]);
                if (mat != null)
                {
                    result.Add(mat);
                }
            }
#endif
        }

        public void LoadPrefab(string abname, string[] assetNames)
        {
#if UNITY_EDITOR && ASSET
            var res = LoadMore<GameObject>(abname, assetNames);
#else
            abname = abname.ToLower();
            List<UObject> result = new List<UObject>();
            for (int i = 0; i < assetNames.Length; i++)
            {
                UObject go = LoadAsset<UObject>(abname, assetNames[i]);
                if (go != null) result.Add(go);
            }
#endif
        }

        public List<UObject> loadPrefabList;
        public void LoadPrefab(string abname, string[] assetNames, Action func)
        {
            abname = abname.ToLower();
            List<UObject> result = new List<UObject>();
            for (int i = 0; i < assetNames.Length; i++)
            {
                UObject go = LoadAsset<UObject>(abname, assetNames[i]);
                if (go != null) result.Add(go);
            }
            loadPrefabList = result;
        }
        /// <summary>
        /// 载入AssetBundle
        /// </summary>
        /// <param name="abname"></param>
        /// <returns></returns>
        public AssetBundle LoadAssetBundle(string abname)
        {
#if UNITY_EDITOR
            if (!abname.Equals(abname.ToLower()))
            {
                throw new System.Exception("LoadAssetBundle 方法传参 必须用小写！！！" + abname);
            }
#endif
            abname = abname.ToLower();
            if (!abname.EndsWith(AppConst.ExtName))
            {
                abname += AppConst.ExtName;
            }
            AssetBundle bundle = null;
            if (!bundles.ContainsKey(abname))
            {
                if (asynclock)
                {
                    //异步同步混用报错，后果自负。
                    Util.LogWarning("有异步加载ab在执行，异步同步混用,不要加载到同一个ab，否则后果自负");
                }
                string uri = Util.DataPath + abname;
#if DEBUG_TEST
                string _uri = Util.DataPath + AppConst.LoadAssetBundlePath + AppConst.dirSep + abname;
                if (File.Exists(_uri))
                {
                    uri = _uri;
                }
#endif
                LoadDependencies(abname);
                bundle = AssetBundle.LoadFromFile(uri); //关联数据的素材绑定
                bundles.Add(abname, bundle);
            }
            else
            {
                bundles.TryGetValue(abname, out bundle);
            }
            return bundle;
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="abname"></param>
        public void UnloadAssetBundle(string abname)
        {
#if UNITY_EDITOR
            if (!abname.Equals(abname.ToLower()))
            {
                throw new System.Exception("LoadAssetBundle 方法传参 必须用小写！！！" + abname);
            }
#endif
            abname = abname.ToLower();
            if (!abname.EndsWith(AppConst.ExtName))
            {
                abname += AppConst.ExtName;
            }
            AssetBundle bundle = null;
            if (bundles.ContainsKey(abname))
            {
                bundles.TryGetValue(abname, out bundle);
                bundles.Remove(abname);
                bundle.Unload(true);
                Resources.UnloadUnusedAssets();
                ////卸载相关依赖，排除大厅公用
                //UnLoadDependencies(abname);
            }
        }

        /// <summary>
        /// 载入依赖
        /// </summary>
        /// <param name="name"></param>
        void LoadDependencies(string abname)
        {
            if (manifest == null)
            {
                Util.Log("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                return;
            }
            // Get dependecies from the AssetBundleManifest object..
            string[] dependencies = manifest.GetAllDependencies(abname);
            if (dependencies.Length == 0) return;

            for (int i = 0; i < dependencies.Length; i++)
                dependencies[i] = RemapVariantName(dependencies[i]);

            // Record and load all dependencies.
            for (int i = 0; i < dependencies.Length; i++)
            {
                LoadAssetBundle(dependencies[i]);
            }
        }

        // Remaps the asset bundle name to the best fitting asset bundle variant.
        string RemapVariantName(string assetBundleName)
        {
            string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();

            // If the asset bundle doesn't have variant, simply return.
            if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
                return assetBundleName;

            string[] split = assetBundleName.Split('.');

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for (int i = 0; i < bundlesWithVariant.Length; i++)
            {
                string[] curSplit = bundlesWithVariant[i].Split('.');
                if (curSplit[0] != split[0])
                    continue;

                int found = System.Array.IndexOf(m_Variants, curSplit[1]);
                if (found != -1 && found < bestFit)
                {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }
            if (bestFitIndex != -1)
                return bundlesWithVariant[bestFitIndex];
            else
                return assetBundleName;
        }

        /// <summary>
        /// 加载场景资源
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public AssetBundle LoadSceneBundle(string sceneName)
        {
            if (!sceneName.EndsWith(AppConst.ExtName))
                sceneName += AppConst.ExtName;

            byte[] stream = null;
            string uri = Util.DataPath + sceneName;
            LoadDependencies(sceneName);
            return AssetBundle.LoadFromFile(uri);
        }
        ///// <summary>
        ///// 加载单个游戏的所有资源
        ///// </summary>
        ///// <param name="cvsFileName">要加载的单个游戏的文件夹名称</param>
        //public void LoadSignGameAllAssetBundle(string cvsFileName)
        //{
        //    Util.LogError("cvsFileName:" + cvsFileName);
        //    GameManager tmpGame = AppFacade.Instance.GetManager<GameManager>(ManagerName.Game);
        //    tmpGame.CheckExtractSignGameResource(cvsFileName);
        //}
        public void LoadMatAndMatImage(string matName)
        {
            LoadAsset<Material>(AppConst.MatBundleName, matName);
        }
        /// <summary>
        /// 销毁资源
        /// </summary>
        void OnDestroy()
        {
            if (shared != null) shared.Unload(true);
            if (manifest != null) manifest = null;
            Util.Log("~ResourceManager was destroy!");
        }
        /// <summary>
        /// 异步从ab中构建Prefab。yzs
        /// </summary>
        /// <param name="abname"></param>
        /// <param name="assetNames"></param>
        /// <param name="callBack"></param>
        public void LoadPrefabAsync(string abname, string[] assetNames)
        {
            StartCoroutine(LoadPrefabAsyncC(abname, assetNames));
        }
        private IEnumerator LoadPrefabAsyncC(string abname, string[] assetNames)
        {
            abname = abname.ToLower();
            if (!abname.EndsWith(AppConst.ExtName))
            {
                abname += AppConst.ExtName;
            }
            AssetBundle bundle = null;
            if (!bundles.ContainsKey(abname))
            {
                yield return StartCoroutine(LoadAssetBundleAsyncC(abname));
            }
            bundle = bundles[abname];
            List<UObject> result = new List<UObject>();
            for (int i = 0; i < assetNames.Length; i++)
            {
                AssetBundleRequest async = bundle.LoadAssetAsync<UObject>(assetNames[i]);
                yield return async;
                if (async.isDone && async.asset != null)
                {
                    result.Add(async.asset as UObject);
                }
            }
        }
        /// <summary>
        /// 异步加载ab。yzs
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="abname"></param>
        public void LoadAssetBundleAsync(string abname)
        {
            StartCoroutine(LoadAssetBundleAsyncC(abname));
        }
        private bool asynclock = false;
        private IEnumerator LoadAssetBundleAsyncC(string abname)
        {
            abname = abname.ToLower();
            while (asynclock)
                yield return null;
            if (!abname.EndsWith(AppConst.ExtName))
            {
                abname += AppConst.ExtName;
            }
            if (!bundles.ContainsKey(abname))
            {
                asynclock = true;
                string uri = Util.DataPath + abname;

                AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(uri);
                yield return abcr;
                if (abcr.isDone)
                {
                    bundles.Add(abname, abcr.assetBundle);
                }
                else
                {
                    Util.LogError(uri + "    error");
                }
                asynclock = false;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        ////////////////////////////异步加载 By LiuYang///////////////////////////
        //////////////////////////////////////////////////////////////////////////
        public void LoadPrefabWithPathAsync(string path, string abname, string[] assetNames)
        {
            StartCoroutine(LoadPrefabWithPathAsyncC(path, abname, assetNames));
        }
        private IEnumerator LoadPrefabWithPathAsyncC(string path, string abname, string[] assetNames)
        {
            abname = abname.ToLower();
            if (!abname.EndsWith(AppConst.ExtName))
            {
                abname += AppConst.ExtName;
            }
            AssetBundle bundle = null;
            if (!bundles.ContainsKey(abname))
            {
                yield return StartCoroutine(LoadAssetBundleWithPathAsyncC(path, abname));
            }
            bundle = bundles[abname];
            List<UObject> result = new List<UObject>();
            for (int i = 0; i < assetNames.Length; i++)
            {
                AssetBundleRequest async = bundle.LoadAssetAsync<UObject>(assetNames[i]);
                yield return async;
                if (async.isDone && async.asset != null)
                {
                    result.Add(async.asset as UObject);
                }
            }
        }

        public void LoadAssetBundleWithPathAsync(string path, string abname)
        {
            StartCoroutine(LoadAssetBundleWithPathAsyncC(path, abname));
        }
        private IEnumerator LoadAssetBundleWithPathAsyncC(string path, string abname)
        {
            abname = abname.ToLower();
            while (asynclock)
                yield return null;
            if (!abname.EndsWith(AppConst.ExtName))
            {
                abname += AppConst.ExtName;
            }
            if (!bundles.ContainsKey(abname))
            {
                asynclock = true;
#if DEBUG_TEST
                string uri = string.Format("{0}{1}/{2}", Util.DataPath, path, abname);
#else
                string uri = string.Format("{0}{1}", Util.DataPath,abname);
#endif
                AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(uri);
                yield return abcr;
                if (abcr.isDone)
                {
                    bundles.Add(abname, abcr.assetBundle);
                }
                else
                {
                    Util.LogError(uri + "    error");
                }
                asynclock = false;
            }
        }
        /// <summary>
        /// 获取ab相关依赖。getAll 要不要排除大厅公用。yzs
        /// </summary>
        /// <param name="abname"></param>
        /// <param name="getAll">true 不要排除，false 要排除</param>
        /// <returns></returns>
        public string[] GetAllDependencies(string abname, bool getAll = true)
        {
            if (!manifestMap.ContainsKey(abname))
            {
                if (!manifestMap.ContainsKey(AppConst.LoadAssetBundlePath))
                {
                    byte[] stream = null;
                    if ("".Equals(AppConst.LoadAssetBundlePath))
                        stream = File.ReadAllBytes(Util.DataPath + AppConst.AssetDir);
                    else if (AppConst.DebugMode)
                        stream = File.ReadAllBytes(Util.DataPath + AppConst.LoadAssetBundlePath + AppConst.dirSep + AppConst.LoadAssetBundlePath);
                    else
                        stream = File.ReadAllBytes(Util.DataPath + AppConst.LoadAssetBundlePath);
                    assetbundle = AssetBundle.LoadFromMemory(stream);
                    manifestMap[AppConst.LoadAssetBundlePath] = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    stream = null;
                }
                manifestMap[abname] = manifestMap[AppConst.LoadAssetBundlePath];
            }
            AssetBundleManifest abm = manifestMap[abname];
            if (abm == null)
            {
                Util.LogError("Cannot find abname= " + abname + "  AssetBundleManifest= " + AppConst.LoadAssetBundlePath);
                return new string[0];
            }
            if (getAll || abm.Equals(manifest))
            {
                return abm.GetAllDependencies(abname);
            }
            //排除大厅，，，想了想，，，其实没啥用。。大厅重新加载ab时候检查 关联文件那个方法我挪到 if外面。。每次都检查一下关联有没有掉了的。了事。。
            string[] md = manifest.GetAllAssetBundles();
            string[] td = abm.GetAllDependencies(abname);
            List<string> reList = new List<string>();
            string s;
            for (int i=0;i<td.Length;++i)
            {
                s = td[i];
                if (System.Array.IndexOf(md, s) == -1)
                    reList.Add(s);
            }
            return reList.ToArray();
        }

        /// <summary>
        /// 卸载依赖，排除大厅的。yzs
        /// </summary>
        /// <param name="name"></param>
        public void UnLoadDependencies(string abname)
        {
            //if (manifest == null) {
            //    Util.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
            //    return;
            //}
            string[] dependencies = this.GetAllDependencies(abname, false);
            if (dependencies.Length == 0) return;

            for (int i = 0; i < dependencies.Length; i++)
                dependencies[i] = RemapVariantName(dependencies[i]);

            // Record and load all dependencies.
            for (int i = 0; i < dependencies.Length; i++)
            {
                UnloadAssetBundle(dependencies[i]);
            }
        }
    }
}
