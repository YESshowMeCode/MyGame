using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.UI;
using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;

public enum  GenBindingFlags
{
	Static = 1<<3,
	Public = 1<<4,
	Protected = 1<<5,
	Private = 1<<6,
	Override = 1<<7,
}

public class GenWordType
{

}

public class UGUIProcessor  {

	static Dictionary<Type,string[]> nodeTypeStr = new Dictionary<Type, string[]>()
	{
		{typeof(RawImage),new[]{"RawImage","RawImg"}},
		{typeof(Image),new[]{"Img","Image"}},
		{typeof(Button),new[]{"Btn","Button"}},
		{typeof(Text),new[]{"Text"}},
		{typeof(Transform),new[]{"TR"}},
	};

	static HashSet<Type> skipCheckTypes = new HashSet<Type>()
	{
		typeof(GenWordType),
	};

	static Dictionary<string,Type> nameToNodeType = new Dictionary<string, Type>();

	static UGUIProcessor()
	{
		foreach (var item in nodeTypeStr)
		{
			foreach (var typeName in item.Value)
			{
				nameToNodeType.Add(typeName,item.Key);

			}
			
		}
	}

	public static Type GetNodeType(string name)
	{
		if(nameToNodeType.ContainsKey(name))
		{
			return nameToNodeType[name];
		}
		return null;
	}

	public static bool TransformHasComponent(Transform transform, Type type)
	{
		if(type == null)
			return false;
		
		if(skipCheckTypes.Contains(type))
			return true;

			return transform.GetComponent<Type>() != null;
	}


	public static bool NeedProcess(string[] types)
	{
		for(int i = 0 ; i<types.Length; ++i)
		{
			var type = types[i];
			if(!nameToNodeType.ContainsKey(type))
				return false;
		}
		return true;
	}

	public static bool CheckAbleProcess(Transform transform, string[] types)
	{
		foreach (var type in types)
		{
			if(!TransformHasComponent(transform,GetNodeType(type)))
				return false;
			
		}
		return true;
	}


	protected UGUIProcessor()
	{

	}

	public CodeStrategy Strategy;
	public CodeStrategy RootStrategy;

	private Transform baseTransform;
	private GenClassInfo rootClassInfo;
	private List<GenClassInfo> classInfos;

	private void ClearTmps()
	{
		rootClassInfo = null;
		baseTransform = null;
		classInfos = new List<GenClassInfo>();
	}

	private GenClassInfo AddClassToTmp(Transform transform , UGUIProcessData data)
	{
		if(data == null)
			throw new Exception("非root节点");

		var ci = new GenClassInfo();
		classInfos.Add(ci);
		data.classInfo = ci;
		Strategy.ProcessBefore(data);
		return ci;

	}

	private GenClassInfo AddRootClassTmp(Transform transform)
	{
		var ci = new GenClassInfo();
		classInfos.Add(ci);
		RootStrategy.ProcessBefore(CreateProcessData(ci,transform));
		return ci;
	}

	public List<GenClassInfo> Process(Transform transform)
	{
		ClearTmps();
		baseTransform = transform;
		rootClassInfo = AddRootClassTmp(transform);
		new TransformRecurser(transform).Recurse((tr,order)=>SingleProcess(tr,order,rootClassInfo));

		RootStrategy.ProcessAfter(CreateProcessData(rootClassInfo,null));
		foreach (var ci in classInfos)
		{
			if(ci != rootClassInfo)
			{
				Strategy.ProcessAfter(CreateProcessData(ci,null));
			}
		}

		return classInfos;

		
	}

	public bool SingleProcess(Transform transform ,string orderStr , GenClassInfo classInfo)
	{
		CodeStrategy strategy = null;
		if(classInfo == rootClassInfo)
		{
			strategy = RootStrategy;
		}
		else
		{
			strategy = Strategy;
		}

		var typeStrs = GetTypeStrs(transform.gameObject.name);
		var order = TransformRecurser.GetOrder(orderStr);

		if(typeStrs == null)
			return true;

		bool continueRecurse = true;

		foreach (var typeStr in typeStrs)
		{
			var type = GetNodeType(typeStr);
			if(type == null)
			{
				throw new Exception("检查程序出错");
			}

			var data = CreateProcessData(classInfo,transform);
			data.NodeTypeStr = typeStr;
			data.Nodetype = type;
			data.Orders = order;
			data.ComponentName = type.FullName;
			
			Debug.Log(data.ComponentName);

			strategy.Process(data);
		}

		return continueRecurse;
	}


	public static IEnumerable<string> GetTypeStrs(string objName)
	{
		int last = objName.LastIndexOf(GenGlobal.SPLIT_CHAR);
		if(last == -1)
		{
			return null;
		}

		var typeStr = objName.Substring(0,last).Split(GenGlobal.SPLIT_CHAR).Where((str) => str != null).ToList();

		foreach (var str in typeStr)
		{
			if(str.Contains(" "))
			{
				Debug.LogError(objName + "Type中有空格");
			}
		}

		for(int i = typeStr.Count-1 ; i>=0;i--)
		{
			if(!nameToNodeType.ContainsKey(typeStr[i]))
			{
				typeStr.RemoveAt(i);
			}
		}

		return typeStr;
	}


	public static UGUIProcessor CreateProcessor(CodeStrategy strategy , CodeStrategy rootStrategy = null)
	{
		var processor = new UGUIProcessor();
		processor.Strategy = strategy;
		processor.RootStrategy = rootStrategy;
		return processor;
	}

	UGUIProcessData CreateProcessData(GenClassInfo ci , Transform transform)
	{
		UGUIProcessData data = new UGUIProcessData();
		data.BaseTransform = baseTransform;
		data.transform = transform;
		data.classInfo = ci;
		return data;
	}



}
