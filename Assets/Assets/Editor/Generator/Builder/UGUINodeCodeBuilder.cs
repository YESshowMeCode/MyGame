using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEditor;

public class UGUINodeCodeBuilder : CodeBuilder {

	public static UGUINodeCodeBuilder CreateBuilder()
	{
		return new UGUINodeCodeBuilder();
	}

	public override StringBuilder Build(GenClassInfo genClassInfo)
	{
		StringBuilder sb = new StringBuilder();

		foreach (var note in genClassInfo.notes)
		{
			sb.Append(@"//");
			sb.Append(note);
			sb.Append("\n");
		}
		sb.Append("\n");

		foreach (var us in genClassInfo.usings)
		{
			sb.Append(string.Format("using {0};",us));
			sb.Append("\n");
		}
		sb.Append("\n");

		if(genClassInfo.namespaceStr != null && genClassInfo.namespaceStr.Length != 0)
		{
			sb.Append("namespace " + genClassInfo.namespaceStr);
			sb.Append("\n");
			sb.Append("{");
			sb.Append("\n");
		}

		foreach (var attr in genClassInfo.attrs)
		{
			sb.Append("[");
			sb.Append(attr);
			sb.Append("]");
			sb.Append("\n");
		}

		if((genClassInfo.flags & GenBindingFlags.Public) != 0)
		{
			sb.Append("public ");
		}
		if((genClassInfo.flags & GenBindingFlags.Static) != 0)
		{
			sb.Append("static ");
		}

		sb.Append("class ");
		sb.Append(genClassInfo.name);

		if(genClassInfo.parent!=null && genClassInfo.parent.Length != 0)
		{
			sb.Append(": ");
			sb.Append(genClassInfo.parent);
		}

		sb.Append("\n");
		sb.Append("{");

		foreach (var field in genClassInfo.fields)
		{
			sb.Append(tabs(1));
			sb.Append(FieldFmt(field));
		}

		sb.Append("\n");

		foreach (var prop in genClassInfo.propertys)
		{
			sb.Append(tabs(1));
			sb.Append(PropertyFmt(prop));
		}
		sb.Append("\n");

		foreach (var pair in genClassInfo.methods)
		{
			sb.Append("\n");
			var method = pair.Value;
			sb.Append(tabs(1));
			sb.Append(MethodHeadFmt(method));
			sb.Append(tabs(1));
			sb.Append("{");
			foreach (var line in method.codeLines)
			{
				sb.Append(tabs(2));
				sb.Append(line);				
			}

			sb.Append(tabs(1));
			sb.Append("}");
		}

		sb.Append("\n");
		sb.Append("}");
		sb.Append("\n");

		if(genClassInfo.namespaceStr != null && genClassInfo.namespaceStr.Length !=0)
		{
			sb.Append("}");
			sb.Append("\n");
		}


		return sb;

	}

	string MethodHeadFmt(GenMethodInfo info)
	{
		StringBuilder sb = new StringBuilder();
		if((info.flags & GenBindingFlags.Public) != 0)
		{
			sb.Append("public ");
		}
		else if((info.flags & GenBindingFlags.Protected) != 0)
		{
			sb.Append("protect ");
		}
		if((info.flags & GenBindingFlags.Private) != 0)
		{
			sb.Append("private ");
		}
		if((info.flags & GenBindingFlags.Override) != 0)
		{
			sb.Append("override ");
		}
		else if((info.flags & GenBindingFlags.Static) != 0)
		{
			sb.Append("static ");
		}

		string reType = info.returnType;
		sb.Append(reType + " ");
		sb.Append(info.name);
		sb.Append("(");
		bool first = true;

		foreach (var arg in info.args)
		{
			if(!first){
				sb.Append(",");
			}
			else
			{
				first = false;
			}

			sb.Append(arg.name);
			sb.Append(" ");
			sb.Append(arg.name);
		}
		sb.Append(")");
		return sb.ToString();
	}

	string PropertyFmt(GenPropertyInfo info)
	{
		StringBuilder sb = new StringBuilder();
		string fmt = null;
		if(info.getSetType == GenPropertyInfo.Type.private_set_public_get)
		{
			fmt = "public {0} {1} {2} {{ get; private set;}}";
		}

		return string.Format(fmt,info.isStatic?" static" : "", info.type,info.name);
	}

	string FieldFmt(GenFieldInfo info)
	{
		StringBuilder sb = new StringBuilder();
		if((info.flags & GenBindingFlags.Public) != 0)
		{
			sb.Append("public ");
		}

		if((info.flags & GenBindingFlags.Static) != 0)
		{
			sb.Append("static ");
		}

		sb.Append(info.type + " ");
		sb.Append(info.name);

		if(info.def != null)
		{
			sb.Append(" = ");
			sb.Append(info.def);
		}

		sb.Append(";");
		return sb.ToString();
	}


}
