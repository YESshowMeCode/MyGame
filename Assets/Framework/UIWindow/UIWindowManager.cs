using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using GameTool;
using GameFramework.Scene;



public class UIWindowManager : GameTool.MonoSingleton<UIWindowManager> 
{
	Dictionary<int, UIWindowInfo> windowInfoDict;

	public void Init()
	{
		windowInfoDict = UIWindowEnumUtil.GetRegisterUIWindows();
		ShowUIWindow(UIWindowEnum.eStart);
		// SceneManager sceneManager = new SceneManager();
		// sceneManager.SetResourceManager();
		// sceneManager.LoadScene("Main");
	}


	private void ShowUIWindow(UIWindowEnum @enum)
	{
		UIWindowInfo info = windowInfoDict[(int)@enum];
		GameObject obj = (GameObject)Instantiate(AssetDatabase.LoadAssetAtPath(info.PrefabFullPath,typeof(GameObject)));
		obj.transform.SetParent(transform);
	}

}



public enum  UIWindowEnum
{
	eStart = 0,
}


public class UIWindowEnumUtil
{



	public static Dictionary<int, UIWindowInfo> GetRegisterUIWindows()
	{
		var dict = new Dictionary<int,UIWindowInfo>();
		RegisterUIWindowInfoToDict(dict,UIWindowEnum.eStart,"TextWindow");

		return dict;

	}

	private static void RegisterUIWindowInfoToDict(Dictionary<int,UIWindowInfo> dict, UIWindowEnum name,string path)
	{
		UIWindowInfo info = new UIWindowInfo(name,path);
		dict.Add((int)name,info);
	}


}


public class UIWindowInfo
{
	public UIWindowEnum windowEnum;
	public string windowName;
	public string assetPath;

	private static StringBuilder m_SB = new StringBuilder();


	public string PrefabFullPath
	{
		get
		{
			m_SB.Remove(0,m_SB.Length);
			m_SB.Append("Assets/GameData/Prefabs/");
			m_SB.Append(assetPath);
			m_SB.Append(".prefab");
			return m_SB.ToString();
		}
	}


	public UIWindowInfo(UIWindowEnum @enum , string path)
	{
		windowEnum = @enum ;
		assetPath = path;
		windowName = assetPath.Substring(path.LastIndexOf('/')+1);
	}
}