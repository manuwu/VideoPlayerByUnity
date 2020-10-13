using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR && ASSET
using System.IO;
using UnityEditor;
#endif

namespace CCS
{
    public class TPAtlasManager : Manager
    {
        //图集的精灵
        private Dictionary<string, Dictionary<string, Sprite>> mAtlasMap = new Dictionary<string, Dictionary<string, Sprite>>();
        //图集的贴图
        private Dictionary<string, Texture2D> mTexMap = new Dictionary<string, Texture2D>();
        //纯Texture
        private Dictionary<string, Dictionary<string, Texture2D>> mPureTexMap = new Dictionary<string, Dictionary<string, Texture2D>>();

        #region 图集相关
        /// <summary>
        /// 从纹理集中获取一个Sprite
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="sprName"></param>
        /// <returns></returns>
        public Sprite GetSprite(string abname, string sprName)
        {
            abname = abname.ToLower();
            Dictionary<string, Sprite> sprDic = null;
            if (!mAtlasMap.ContainsKey(abname))
                sprDic = LoadAtlas(abname);
            else
                sprDic = mAtlasMap[abname];

            if (sprDic == null)
            {
                Util.LogError("can't load find atlas " + abname);
                return null;
            }

            if (sprDic.ContainsKey(sprName))
                return sprDic[sprName];
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 加载一个纹理集
        /// </summary>
        /// <param name="abName"></param>
        /// <returns></returns>
        public Dictionary<string, Sprite> LoadAtlas(string abname)
        {
            abname = abname.ToLower();
            if (mAtlasMap.ContainsKey(abname))
                return mAtlasMap[abname];

            Dictionary<string, Sprite> sprDic = null;

            ResourceManager resMgr = facade.GetManager<ResourceManager>(ManagerName.Resource);
            Sprite[] sprs = null;
#if UNITY_EDITOR && ASSET
            CsvItem item = null;
            if (resMgr.CsvList.TryGetValue(abname, out item))
            {
                string[] paths = Directory.GetFiles(item.path,"*."+item.ext, SearchOption.AllDirectories);
                List<Sprite> _sptList = new List<Sprite>();
                for(int i=0;i<paths.Length;++i)
                {
                    _sptList.Add(AssetDatabase.LoadAssetAtPath<Sprite>(paths[i]));
                }
                sprs = _sptList.ToArray();
            }
            
#else
            AssetBundle ab = resMgr.LoadAssetBundle(abname);
            sprs = ab.LoadAllAssets<Sprite>();
#endif

            Texture2D tex2D = null;
            if (sprs != null && sprs.Length > 0)
            {
                sprDic = new Dictionary<string, Sprite>();
                string tempStr = "";
                int index = 0;
                Sprite spr;
                for (int i = 0; i < sprs.Length; ++i)
                {
                    spr = sprs[i];
                    tex2D = spr.texture;
                    tempStr = spr.name;
                    index = tempStr.LastIndexOf('.');
                    if (index > 0)
                        tempStr = tempStr.Substring(0, index);
                    if (sprDic.ContainsKey(tempStr))
                    {
                        Util.LogError("同名图片 : " + tempStr);
                        continue;
                    }
                    sprDic.Add(tempStr, spr);
                }
                mAtlasMap.Add(abname, sprDic);
                if (tex2D != null)
                    mTexMap.Add(abname, tex2D);
            }
            return sprDic;
        }

        /// <summary>
        /// 卸载一张纹理集
        /// </summary>
        /// <param name="atlasPath"></param>
        public void UnloadAtlas(string atlasPath)
        {
            if (mAtlasMap.ContainsKey(atlasPath))
            {
                Dictionary<string, Sprite> sprDic = mAtlasMap[atlasPath];
                mAtlasMap.Remove(atlasPath);

                if (sprDic != null)
                {
                    foreach (KeyValuePair<string, Sprite> kv in sprDic)
                        Resources.UnloadAsset(kv.Value);
                    sprDic.Clear();
                    sprDic = null;
                }

                if (mTexMap.ContainsKey(atlasPath))
                {
                    Texture2D tex = mTexMap[atlasPath];
                    if (tex != null)
                        Resources.UnloadAsset(tex);
                    mTexMap.Remove(atlasPath);
                }
            }
            else
            {
                Util.LogError("can not find " + atlasPath + " to be unload");
            }
        }

        /// <summary>
        /// 卸载所有纹理集
        /// </summary>
        public void UnloadAllAtlas()
        {
            for (int a = mAtlasMap.Count - 1; a >= 0; --a)
            {
                KeyValuePair<string, Dictionary<string, Sprite>> kv = mAtlasMap.ElementAt(a);
                UnloadAtlas(kv.Key);
            }
            mAtlasMap.Clear();
        }
        #endregion

        #region 非图集的Texture相关
        public Texture GetTexture(string abname, string texName)
        {
            abname = abname.ToLower();
            Dictionary<string, Texture2D> dic = null;
            if (!mPureTexMap.ContainsKey(abname))
                dic = LoadTexture(abname);
            else
                dic = mPureTexMap[abname];

            if (null == dic)
            {
                Util.LogError("can't load find atlas " + abname);
                return null;
            }

            if (dic.ContainsKey(texName))
                return dic[texName];
            else
            {
                return null;
            }
        }

        private Dictionary<string, Texture2D> LoadTexture(string abname)
        {
            if (mPureTexMap.ContainsKey(abname))
            {
                return mPureTexMap[abname];
            }

            ResourceManager resMgr = facade.GetManager<ResourceManager>(ManagerName.Resource);
            Texture[] textures = null;
#if UNITY_EDITOR && ASSET
            CsvItem item = null;
            if (resMgr.CsvList.TryGetValue(abname, out item))
            {
                string[] paths = Directory.GetFiles(item.path, "*." + item.ext, SearchOption.AllDirectories);
                List<Texture> _sptList = new List<Texture>();
                for (int i = 0; i < paths.Length; ++i)
                {
                    _sptList.Add(AssetDatabase.LoadAssetAtPath<Texture>(paths[i]));
                }
                textures = _sptList.ToArray();
            }
#else
            AssetBundle ab = resMgr.LoadAssetBundle(abname);
            textures = ab.LoadAllAssets<Texture>();
#endif

            Dictionary<string, Texture2D> result = null;
            Texture2D texture;
            if (textures != null && textures.Length > 0)
            {
                result = new Dictionary<string, Texture2D>();
                for (int i = 0; i < textures.Length; ++i)
                {
                    texture = textures[i] as Texture2D;
                    if (!result.ContainsKey(texture.name))
                    {
                        result.Add(texture.name, texture);
                    }
                }
                mPureTexMap.Add(abname, result);
            }
            return result;
        }

        public void UnloadTexture(string path)
        {
            if (mPureTexMap.ContainsKey(path))
            {
                Dictionary<string, Texture2D> dic = mPureTexMap[path];
                mPureTexMap.Remove(path);

                if (null != dic)
                {
                    foreach (KeyValuePair<string, Texture2D> kv in dic)
                        Resources.UnloadAsset(kv.Value);
                    dic.Clear();
                    dic = null;
                }
            }
            else
            {
                Util.LogError("can not find " + path + " to be unload");
            }
        }

        public void UnloadAllTexture()
        {
            for (int a = mPureTexMap.Count - 1; a >= 0; --a)
            {
                KeyValuePair<string, Dictionary<string, Texture2D>> kv = mPureTexMap.ElementAt(a);
                UnloadAtlas(kv.Key);
            }
            mPureTexMap.Clear();
        }
        #endregion
    }
}