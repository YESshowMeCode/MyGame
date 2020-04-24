using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;

public class UIGenHierarchyMenu  {
	[MenuItem("GameObject/脚本生成/Window脚本",false,20)]
	public 	static void GenWindowScript()
	{
		if(Selection.activeTransform == null || Selection.activeGameObject == null)
		{
			Debug.LogError("未选中节点！");
			return;
		}

		if(!Selection.activeGameObject.name.EndsWith("Window"))
		{
			Debug.LogError("后缀必须为Window！");
			return;
		}

		var go = Selection.activeGameObject;
		if(go.GetComponent<RectTransform>() == null)
		{
			Debug.LogError("改功能仅支持UGUI！");
			return;
		}

		var nodeName = go.name.Substring(0,go.name.IndexOf("Window"));
		GenGlobal.Log("开始生成！");

		CodeGenerator.UGUIWindowInit(Selection.activeTransform);
		GenGlobal.Log("完成生成");
	}
	

	[MenuItem("Asset/折叠所有文件夹 &c",false,30)]
	public static void SetProjectBrowserCollapseFolders()
	{
		// var assembly = Assembly.GetAssembly(typeof(Editor));
		// var type = assembly.GetType("UnityEditor.ProectBrowser");

		Assembly editorAssembly = typeof(Editor).Assembly;
    	System.Type type = editorAssembly.GetType("UnityEditor.ProjectBrowser");
		if(type == null)
		{
			Debug.LogError("type == null");
			return;
		}

		var browserField = type.GetField("s_LastInteractedProjectBrowser",BindingFlags.Public|BindingFlags.Static);
		var browser = browserField.GetValue(null);

		if(browser == null)
		{
			Debug.LogError("browser == null");
			return;
		}

		var modeField = type.GetField("m_ViewMode",BindingFlags.NonPublic|BindingFlags.Instance);
		bool isOne = (int)modeField.GetValue(browser) == 0;

		var treeField = type.GetField(isOne? "m_AssetTree":"m_FolderTree",BindingFlags.NonPublic|BindingFlags.Instance);
		var tree = treeField.GetValue(browser);

		var dataProperty = treeField.FieldType.GetProperty("data",BindingFlags.Instance|BindingFlags.Public);
		var data = dataProperty.GetValue(tree,null);

		var getRowMethod = dataProperty.PropertyType.GetMethod("GetRows",BindingFlags.Instance|BindingFlags.Public);
		var setExpandedMethods = dataProperty.PropertyType.GetMethods(BindingFlags.Instance|BindingFlags.Public).ToList().FindAll(method => method.Name == "SetExpanded");
		var setExpandedMethod = setExpandedMethods[0];

		var rows = ((IEnumerable)getRowMethod.Invoke(data,null));
		var rowList = new List<object>();

		foreach (var row in rows)
		{
			rowList.Add(row);
		}

		bool first = true;

		for(int i=rowList.Count-1;i>=0;i--)
		{
			var obj = rowList[i];
			if(first && !isOne)
			{
				var itemType = obj.GetType();
				var nameField = itemType.GetField("m_DisplayName",BindingFlags.Instance|BindingFlags.NonPublic);

				if(nameField != null)
				{
					string name = (string)nameField.GetValue(obj);
					if(name == "Assets")
					{
						first = false;
						setExpandedMethod.Invoke(data,new object[] {obj,true});
						continue;
					}
				}
			}

			setExpandedMethod.Invoke(data,new object[] {obj,false});
		}

		AssetDatabase.Refresh();
		



	}

}
