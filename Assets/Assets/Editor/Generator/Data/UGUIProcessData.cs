using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UGUIProcessData : ProcessData {

	public GenClassInfo classInfo;

	public Transform BaseTransform;
	public string BaseGameObjectName
	{
		get{return BaseTransform.gameObject.name;}
	}

	public Transform transform;

	public string GameObjectName
	{
		get{return transform.gameObject.name;}
	}

	public bool IsRoot()
	{
		return BaseTransform == transform;
	}

	public Type Nodetype;
	public string NodeTypeStr;
	public string ComponentName;

	public List<int> Orders;
}
