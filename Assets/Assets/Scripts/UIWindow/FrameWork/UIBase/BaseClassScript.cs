using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 凡是需要继承Monobehavior的都可以继承BaseClassScript来处理Transform和Gameobject的缓存
/// </summary>
public class BaseClassScript : MonoBehaviour {

    private bool isTransformCached = false;

    private Transform m_Trans;

    public Transform CacheTransform
    {
        get
        {
            if (isTransformCached)
            {
                isTransformCached = true;
                m_Trans = transform;
            }

            return m_Trans;
        }
    }


    private bool isGameObjectCached = false;
    private GameObject m_gameObject;

    private GameObject CacheGameObject
    {
        get
        {
            if(!isGameObjectCached)
            {
                isGameObjectCached = true;
                m_gameObject = gameObject;
            }
            return m_gameObject;
        }
    }

}
