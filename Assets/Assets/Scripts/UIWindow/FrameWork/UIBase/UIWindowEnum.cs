using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum UIWindowEnum
{

    eNone = 0,
    eMainStart = 1,
}


public enum UIWindowType
{
    oNormal,

    ePopup,

    ePopup_Black,

    ePopup_Blue,

    ePopup_None,
}


public class UIwindowEnumUtil
{
    public static Dictionary<int, UIWindowInfo> GetRegisterUIWindow()
    {

        var dict = new Dictionary<int, UIWindowInfo>();
        RegisterWinInfoToDict(dict, UIWindowEnum.eNone, UIWindowType.oNormal, "");
        RegisterWinInfoToDict(dict, UIWindowEnum.eMainStart, UIWindowType.oNormal, "GameData/Prefabs/MainStartWindow");
        return dict;
    }

    private static void RegisterWinInfoToDict(Dictionary<int,UIWindowInfo> dict,UIWindowEnum name,UIWindowType type,string path)
    {
        UIWindowInfo info = new UIWindowInfo(name, type, path);
        dict.Add((int)name, info);
    }

}