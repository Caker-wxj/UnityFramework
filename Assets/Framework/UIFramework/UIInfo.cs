﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ViewInfo
{
    public string resName;
    public string resFolder;
    public DisplayType showType;
    public DisplayType hideType;
    public ViewInfo(string resName, string resFolder = "", DisplayType showType = DisplayType.Normal, DisplayType hideType = DisplayType.Normal)
    {
        this.resName = resName;
        this.resFolder = resFolder;
        this.showType = showType;
        this.hideType = hideType;
    }
}

public class StageInfo
{
    //ab依赖
}

public static class UIInfo
{
    public static Dictionary<Type, ViewInfo> viewInfoDict = new Dictionary<Type, ViewInfo>
    {
        {typeof(TestView), new ViewInfo("testUI","test", DisplayType.Pop, DisplayType.Pop) }
    };

    public static Dictionary<Type, StageInfo> stageInfoDict = new Dictionary<Type, StageInfo> {
        {typeof(TestStage), new StageInfo() },
    };
}