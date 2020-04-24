using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum UIInterfaceEnum
{
    None = 0,

}

public static class UIInterfaceEnumUtil
{
    public static Dictionary<int,UIInterfaceInfo> GetInterfaceInfoDict()
    {
        Dictionary<int, UIInterfaceInfo> dict = new Dictionary<int, UIInterfaceInfo>();

        RegisterInterfaceInfo(dict, UIInterfaceEnum.None, new List<UIWindowEnum> { });
        return dict;
    }


    static void RegisterInterfaceInfo(Dictionary<int,UIInterfaceInfo> dict,UIInterfaceEnum name,List<UIWindowEnum> windowArray)
    {
        if((int)name <0 ||(int)name > 1000)
        {
            return;
        }
        if(!dict.ContainsKey((int)name))
        {
            dict.Add((int)name, new UIInterfaceInfo(name, windowArray));
        }
    }

    /// <summary>
    /// 特殊处理interface窗口----竖屏
    /// </summary>
    static HashSet<int> ProtraitInterfaceName = new HashSet<int>()
    {
        (int)UIInterfaceEnum.None,
    };

    /// <summary>
    /// 特殊处理interface窗口----不会进入OpenStack堆栈
    /// </summary>
    static HashSet<int> NotOpenStackInterfaceName = new HashSet<int>()
    {
        (int)UIInterfaceEnum.None,
    };

    /// <summary>
    /// 特殊处理interface窗口----窗口如果进入根interface，会清空返回堆栈
    /// </summary>
    static HashSet<int> RootInterfaceName = new HashSet<int>()
    {
        (int)UIInterfaceEnum.None,
    };


    public static bool IsRootUIInterfaceEnum(UIInterfaceEnum name)
    {
        return RootInterfaceName.Contains((int)name);
    }
}