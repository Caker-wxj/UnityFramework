﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
/// <summary>
/// 负责游戏中具体逻辑的处理,UI的加载等
/// 拥有一个或多个ViewComponent
/// </summary>
public class StageComponent : BaseComponent
{
    protected override void Init()
    {
        StageManager.Instance.EnterStage(this);
    }

    /// <summary>
    /// Create View
    /// </summary>
    /// <typeparam name="T">View Type</typeparam>
    /// <param name="onViewShowed">Call When View Showed</param>
    /// <param name="parent">View's Parent</param>
    public void CreateView<T>(Action<T> onViewShowed, Transform parent = null) where T : ViewComponent
    {
        Type uiType = typeof(T);
        ViewInfo viewInfo;
        if (UIInfo.viewInfoDict.TryGetValue(uiType, out viewInfo))
        {
            GameObject go = ResourceManager.Instance.LoadUI(viewInfo.resName, viewInfo.resFolder);
            if(parent == null)
            {
                parent = FrameworkRoot.ui;
            }
            go.transform.SetParent(parent, false);
            onViewShowed(go.AddComponent<T>());
        }
        else
        {
            Debug.LogError("View Not Define in UIInfo!");
        }
    }

    /// <summary>
    /// 离开舞台
    /// </summary>
    public virtual void LeaveStage()
    {
        StageManager.Instance.LeaveStage(this);
    }

    /// <summary>
    /// 离开Stage时调用
    /// </summary>
    public override void Clear()
    {
        //清理Stage AB依赖
        //清理UI资源等
        //清理侦听
        ClearAllNotify();
        //清理挂Stage的GO
        Destroy(gameObject);
    }
}
