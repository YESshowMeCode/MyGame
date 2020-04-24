using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class UIWindowInfo
{
    public UIWindowEnum windowEnum;
    public UIWindowType windowType;

    public string windowName;
    public string assetPath;

    private static StringBuilder sb = new StringBuilder();


    public string PrefabFullPath
    {
        get
        {
            sb.Remove(0, sb.Length);
            sb.Append(assetPath);
            sb.Append(".prefab");
            return sb.ToString();
        }
    }

    public string ControllerName
    {
        get
        {
            return windowName.Replace("Window", "Controller");
        }
    }

    public UIWindowInfo(UIWindowEnum name , UIWindowType type ,string path)
    {
        this.windowEnum = name;
        this.windowType = type;
        this.assetPath = path;

        windowName = assetPath.Substring(path.LastIndexOf('/') + 1);
    }
	
}
