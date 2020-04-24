using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class UGUIRefNode : MonoBehaviour {

    Dictionary<Type, Component> comDic = new Dictionary<Type, Component>();

    public T GetCom<T>() where T : Component
    {
        Type key = typeof(T);
        if(!comDic.ContainsKey(key))
        {
            var com = transform.GetComponent<T>();
            if (com == null)
            {
                com = transform.gameObject.AddComponent<T>();
            }
            comDic.Add(key, com);
        }
        return comDic[key] as T;
    }

    private CanvasGroup m_CanvasGroup;
    public CanvasGroup CanvasGroup
    {
        get
        {
            return m_CanvasGroup;
        }
        private set
        {
            m_CanvasGroup = value;
        }

    }
}
