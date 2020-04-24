using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.UI;
using UnityEditor;

public class UGUIGenUtil 
{
	public static void InitNormalClassInfo(GenClassInfo ci, string name)
	{
		ci.notes.Add("尝试一下！！");
		ci.usings.Add("UnityEngine");
		ci.usings.Add("UnityEngine.UI");

		ci.flags |= GenBindingFlags.Public;
		ci.name = name;
	}

	public static GenMethodInfo AddOrGetResetMethod(GenClassInfo ci)
	{
		string name = "Reset";
		GenMethodInfo mi;
		if(!ci.methods.TryGetValue(name,out mi))
		{
			GenMethodInfo awakeGenMethod = new GenMethodInfo();
			awakeGenMethod.name = name;
			awakeGenMethod.returnType = "void";
			ci.methods.Add(name,awakeGenMethod);
			mi = ci.methods[name];
		}
		return mi;
	}

	public static void AddErrMsgInResetMethod(GenClassInfo genClassInfo)
	{
		var awakeMethod = UGUIGenUtil.AddOrGetResetMethod(genClassInfo);
		List<GenFieldInfo> props = new List<GenFieldInfo>();
		for(int i=0;i<genClassInfo.fields.Count;i++)
		{
			if(genClassInfo.fields[i].type != "UIDele.Dele")
			{
				props.Add(genClassInfo.fields[i]);
			}
		}

		if(props.Count == 0)
		{
			return;
		}

		StringBuilder sb = new StringBuilder();

		sb.Append("if(");
		bool first = true;
		foreach (var prop in props)
		{
			if(first)
			{
				first = false;
			}
			else
			{
				sb.Append("||");
			}
			sb.Append(prop.name + "== null");
		}
		sb.Append(")");
		awakeMethod.codeLines.Add(sb.ToString());
		awakeMethod.codeLines.Add(" Debug.LogError(\"节点有null，请重新生成代码\");");


	}

	public static GenFieldInfo AddPublicField(GenClassInfo classInfo ,string type , string name)
	{
		GenFieldInfo fieldInfo = new GenFieldInfo();
		fieldInfo.flags |= GenBindingFlags.Public;
		fieldInfo.type = type;
		fieldInfo.name = name;

		for(int i=0;i<classInfo.fields.Count;i++)
		{
			if(classInfo.fields[i].name == name)
			{
				Debug.LogError("当前类中包含相同的type" + type + "name" + name + "的字段");
				return null;
			}
		}
		classInfo.fields.Add(fieldInfo);
		return fieldInfo;

	}

	public static string GetNodeNameNoSuffixNum(string objName)
	{

		var name = GetNodeName(objName);
		name = Regex.Replace(name,"[0-9]+$","");

		return name;
	}


	public static string GetNamespaceByTwoWords(string objName)
	{
		string name = GetNodeName(objName);
		var match = Regex.Match(name,"^[A-Z][a-z]*[A-Z][a-z]*");
		Debug.Log(match.Value);
		return match.Value;
	}

	public static string GetNodeName(string objName)
	{
		int last = objName.LastIndexOf(GenGlobal.SPLIT_CHAR);
		var name = objName.Substring(last + 1);
		name = name.Replace(" ","").Replace("(","").Replace(")","");
		return name;
	}

	public static GenMethodInfo GetAwakeMethod(GenClassInfo ci)
	{
		string name = "Awake";
		GenMethodInfo methodInfo;
		if(!ci.methods.TryGetValue(name, out methodInfo))
		{
			methodInfo = new GenMethodInfo();
			methodInfo.name = name;
			methodInfo.returnType = "void";
			ci.methods.Add(name,methodInfo);
		}
		return methodInfo;
	}

}


public static class GenGlobal
{
	public static readonly char SPLIT_CHAR = '_';
	private static bool debug = true;

	public static void Log(object msg)
	{
		if(debug)
			Debug.Log(msg);
	}
	

}