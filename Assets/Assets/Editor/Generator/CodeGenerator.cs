using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Text;
using System.IO;

public class CodeGenerator {

	private static string m_CodeSavePath;

	public static string  CodeSavePath
	{
		get{return m_CodeSavePath??(m_CodeSavePath = Application.dataPath + "/Assets/UIExt/GeneratedUIScripts/");}
	}

	public static void UGUIWindowInit(Transform transform)
	{
		var classes = UGUIProcessor.CreateProcessor(new UguiChildWindowCodeStrategy(),new UguiWindowCodeStrategy()).Process(transform);
		WriteClsesToDirectory(classes,UGUINodeCodeBuilder.CreateBuilder());
	}


	public static void WriteClsesToDirectory(List<GenClassInfo> clese ,CodeBuilder codeBuilder,bool writeToDir = true)
	{

		if(clese == null && clese.Count == 0)
		{
			Debug.LogError("clese为空");
			return;
		}

		//统一写入
		foreach (var cls in clese)
		{
			string path = CodeSavePath + (cls.namespaceStr + "/");

			if(!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			var sb = codeBuilder.Build(cls);
			if(writeToDir)
			{
				WriteSbToDirectory(path + cls.name + ".cs",sb);
			}

			GenGlobal.Log("\n"+sb.ToString());//测试代码
		}

		AssetDatabase.Refresh();
	}


	private static void WriteSbToDirectory(string file,StringBuilder sb)
	{
		using(StreamWriter write = new StreamWriter(file ,false))
		{
			write.Write(sb);
		}

	}

}
