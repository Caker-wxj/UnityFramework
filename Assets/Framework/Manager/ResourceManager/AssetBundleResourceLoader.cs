﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// 对外暴露加载资源接口
/// 对AssetBundleLoader进行管理
/// TODO： 需要检查依赖是否正确
/// </summary>
public class AssetBundleResourceLoader
{
    private Dictionary<string, AssetBundleHandler> handlerDictionary = new Dictionary<string, AssetBundleHandler>();

    private AssetBundleManifest _manifest;
    private AssetBundleManifest manifest
    {
        get
        {
            if (_manifest == null)
            {
                AssetBundle ab = AssetBundle.LoadFromFile(AssetPath.GetResPath(true, AssetPath.StreamingAssetsPath, AssetPath.ResourcePath[ResourceType.Manifest]));
                _manifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
            return _manifest;
        }
    }

    /// <summary>
    /// 同步加载资源
    /// </summary>
    public T LoadAsset<T>(ResourceType resType, string resName, string folder = "") where T : UnityEngine.Object
    {
        folder = folder.Length > 0 ? "/" + folder : folder;
        string path = AssetPath.GetResPath(true, AssetPath.StreamingAssetsPath, AssetPath.ResourcePath[resType] + folder);
        return handlerDictionary[path].LoadAsset<T>(resName);
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <returns>返回AssetBundleRequest自行处理</returns>
    public AssetBundleRequest LoadAssetAsync<T>(ResourceType resType, string resName, string folder = "") where T : UnityEngine.Object
    {
        folder = folder.Length > 0 ? "/" + folder : folder;
        string path = AssetPath.GetResPath(true, AssetPath.StreamingAssetsPath, AssetPath.ResourcePath[resType] + folder);
        return handlerDictionary[path].LoadAssetAsync<T>(resName);
    }

    /// <summary>
    /// 目前依赖处理: 
    /// 系统进入时定义依赖于哪些ab包
    /// 上层进入时 基类 调用LoadHandler增加引用（同时load AssetBundle）
    /// 离开时 基类 调用UnLoadAsset 减少引用
    /// </summary>
    private void LoadHandler(string path)
    {
        if (!handlerDictionary.ContainsKey(path))
        {
            LoadDependAssetBundle(path);
            AssetBundleHandler handler = ObjectPoolManager.Instance.Get<AssetBundleHandler>();
            handler.Init(AssetBundle.LoadFromFile(path));
            handler.IncreaseReference();
            handlerDictionary.Add(path, handler);
        }
        else
        {
            handlerDictionary[path].IncreaseReference();
        }
    }

    private void TryUnloadHandler(string path, AssetBundleHandler handler)
    {
        handler.DecreaseReference();
        if (handler.UnloadAble)
        {
            handler.UnloadAssetBundle(false);
            handlerDictionary.Remove(path);
        }
    }

    private void LoadDependAssetBundle(string targetAssetBundle)
    {
        string[] depends = manifest.GetAllDependencies(targetAssetBundle);
        for (int i = 0; i < depends.Length; i++)
        {
            string target = depends[i];

            LoadDependAssetBundle(target);
            LoadHandler(target);
        }
    }

    private void TryUnloadDependAssetBundle(string targetAssetBundle)
    {
        string[] depends = manifest.GetAllDependencies(targetAssetBundle);
        for (int i = 0; i < depends.Length; i++)
        {
            string target = depends[i];
            TryUnloadDependAssetBundle(target);

            if (handlerDictionary.ContainsKey(target))
            {
                AssetBundleHandler handler = handlerDictionary[target];
                TryUnloadHandler(target, handler);
            }
        }
    }

    public void UnLoadAsset(ResourceType resType, string resName, string folder = "")
    {
        folder = folder.Length > 0 ? "/" + folder : folder;
        string assetPath = AssetPath.GetResPath(true, AssetPath.StreamingAssetsPath, AssetPath.ResourcePath[resType] + folder);
        UnLoadAsset(assetPath);
    }

    private void UnLoadAsset(string assetPath)
    {
        AssetBundleHandler handler = handlerDictionary[assetPath];
        TryUnloadHandler(assetPath, handler);
        TryUnloadDependAssetBundle(assetPath);
    }

    #region Stage相关
    public void StageLoadAB(string[] ab)
    {
        if(ab != null && ab.Length != 0)
        {
            foreach(string assetName in ab)
            {
                string path = AssetPath.GetABPath(assetName);
                LoadHandler(path);
            }
        }
    }

    public void StageUnLoadAB(string[] ab)
    {
        if (ab != null && ab.Length != 0)
        {
            foreach (string assetName in ab)
            {
                string path = AssetPath.GetABPath(assetName);
                UnLoadAsset(path);
            }
        }
    }
    #endregion
}
