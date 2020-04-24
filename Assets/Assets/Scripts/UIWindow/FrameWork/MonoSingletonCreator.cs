using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

public sealed class MonoSingletonCreator
{
    private static bool m_IsUnitTestMode;

	public static bool IsUnitTestMode
    {
        get { return m_IsUnitTestMode; }
        set { m_IsUnitTestMode = value; }
    }

    public static T CreateMonoSingleton<T>(bool isDontDestory) where T:MonoBehaviour,ISingleton
    {
        T instance = null;

        if (instance != null || (!m_IsUnitTestMode && !Application.isPlaying))
            return instance;

        instance = GameObject.FindObjectOfType(typeof(T)) as T;

        if (instance != null)
            return instance;

        MemberInfo info = typeof(T);
        var attributes = info.GetCustomAttributes(true);
        foreach(var attribute in attributes)
        {
            var defineAttri = attribute as MonoSingletonPath;
            if(defineAttri == null)
            {
                continue;
            }

            instance = CreateComponentOnGameObject<T>(defineAttri.PathInHierarchy, isDontDestory);
            break;
        }

        if(instance == null)
        {
            var obj = new GameObject("(Singleton)" + typeof(T).Name);
            instance = obj.AddComponent<T>();
        }

        return instance;
    }


    private static T CreateComponentOnGameObject<T>(string path,bool dontDestory) where T : MonoBehaviour
    {
        var obj = FindGameObject(null, path, true, dontDestory);
        if(obj == null)
        {
            obj = new GameObject("(Singleton)" + typeof(T).Name);
            if(dontDestory && !m_IsUnitTestMode)
            {
                //Object.DontDestoryOnload(obj);
            }
        }

        return obj.AddComponent<T>();
    }


    static GameObject FindGameObject(GameObject root , string path,bool build , bool dontDestory)
    {
        if(path == null || path.Length == 0)
        {
            return null;
        }

        string[] subPath = path.Split('/');
        if(subPath == null || subPath.Length == 0)
        {
            return null;
        }
        return FindGameObject(null, subPath, 0, build, dontDestory);
    }

    static GameObject FindGameObject(GameObject root , string[] path , int index ,bool build ,bool dontDestory)
    {
        GameObject client = null;
        if(root == null)
        {
            client = GameObject.Find(path[index]);
        }
        else
        {
            var child = root.transform.Find(path[index]);
            if(child != null)
            {
                client = child.gameObject;
            }
        }

        if(client == null)
        {
            if(build)
            {
                client = new GameObject(path[index]);
                if(root != null)
                {
                    client.transform.SetParent(root.transform);
                }

                if(dontDestory && index == 0&& !m_IsUnitTestMode)
                {
                    GameObject.DontDestroyOnLoad(client);
                }
            }
        }


        if(client == null)
        {
            return null;
        }

        return ++index == path.Length ? client : FindGameObject(client, path, index, build, dontDestory);
    }
}


public class MonoSingletonPath: Attribute
{
    private string m_PathInHierarchy;

    public MonoSingletonPath(string pathInHierarchy)
    {
        m_PathInHierarchy = pathInHierarchy;
    }

    public string PathInHierarchy
    {
        get { return m_PathInHierarchy; }
    }
}