using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UI;

public class UguiWindowCodeStrategy : CodeStrategy<UGUIProcessData> {

	protected override void processBeforeIn(UGUIProcessData data)
	{
		UGUIGenUtil.InitNormalClassInfo(data.classInfo,GetInitClassName(data.GameObjectName,data.NodeTypeStr));

		data.classInfo.parent = "UGUIRefNode";

		
		data.classInfo.namespaceStr = UGUIGenUtil.GetNamespaceByTwoWords(data.BaseGameObjectName) + "Ref";
		UGUIGenUtil.GetAwakeMethod(data.classInfo);
	}

	protected override void processIn(UGUIProcessData data)
	{
		var classInfo = data.classInfo;

		if(data.Nodetype == typeof(GenWordType))
		{
			return;
		}
		var fieldInfo = UGUIGenUtil.AddPublicField(classInfo,data.ComponentName,data.NodeTypeStr + "_" + UGUIGenUtil.GetNodeName(data.GameObjectName));

		var resetMethod = UGUIGenUtil.AddOrGetResetMethod(data.classInfo);
		
		StringBuilder sb = new StringBuilder();
		sb.Append(fieldInfo.name);
		sb.Append(" = transform");
		foreach (var node in data.Orders)
		{
			sb.Append(string.Format(".GetChild({0})",node.ToString()));
		}

		sb.Append(string.Format(".GetComponent<{0}>();",data.ComponentName));			
		resetMethod.codeLines.Add(sb.ToString());

		if(data.Nodetype == typeof(Button))
		{
			var eventFieldInfo = UGUIGenUtil.AddPublicField(classInfo,"UIDele.Dele","On" + 
			UGUIGenUtil.GetNodeName(data.GameObjectName) + "Callback");
			if(eventFieldInfo == null)
			{
				return ;
			}

			GenMethodInfo clickMethod = new GenMethodInfo();
			clickMethod.returnType = "void";
			clickMethod.name = "On" + UGUIGenUtil.GetNodeName(data.GameObjectName) + "Click";
			clickMethod.flags |= GenBindingFlags.Private;
			clickMethod.args.Add(new GenArgumentInfo("go","GameObject"));
			clickMethod.codeLines.Add(string.Format("if ({0} != null)", eventFieldInfo.name));
			clickMethod.codeLines.Add(string.Format("   {0}();", eventFieldInfo.name));
			classInfo.methods.Add(clickMethod.name,clickMethod);

			var openMethod = UGUIGenUtil.GetAwakeMethod(classInfo);
			openMethod.codeLines.Add(string.Format("UIEventListener_UGUI.Get({0}.gameObject).onClick = {1};",fieldInfo.name,clickMethod.name));

		}


	}


	protected override void processAfterIn(UGUIProcessData data)
	{
		UGUIGenUtil.AddOrGetResetMethod(data.classInfo);
	}

	public override string GetInitClassName(string objName, string nodeTypeStr)
	{
		if(!objName.EndsWith("Window"))
		{
			objName = objName + "GenWindow";
		}
		return objName + "Ref";
	}
}
