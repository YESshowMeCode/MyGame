using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UGUIRefNode : MonoBehaviour {

	Dictionary<Type,Component> comDict = new Dictionary<Type, Component>();

	public T GetCom<T>() where T : Component
	{
		Type key = typeof(T);
		if(!comDict.ContainsKey(key))
		{
			var com = transform.GetComponent<T>();
			if(com == null)
			{
				com = transform.gameObject.AddComponent<T>();
			}
			comDict.Add(key,com);
		}
		return comDict[key] as T;
	}

	private CanvasGroup m_CanvasGroup;

	public CanvasGroup canvasGroup
	{
		get{ return canvasGroup = m_CanvasGroup ?? GetCom<CanvasGroup>();}
		private set{m_CanvasGroup = value;}
	}




}
