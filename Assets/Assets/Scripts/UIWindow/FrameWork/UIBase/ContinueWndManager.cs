// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/24 10:48:16
// FileName：Assets/Scripts/UIWindow/FrameWork/UIBase/ContinueWndManager.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EM_CHECK_UNLOCK_CFG_TYPE
{
    CONTINUEWND,
    SYSTEMOPEN,
}

public class ContinueWndManager  {

    private ContinueWndModule m_ComtinueWndModule;
    private static ContinueWndManager m_Instance;

    private ContinueWndManager()
    {
        //m_ComtinueWndModule初始化
    }

   public static ContinueWndManager Instance
    {
        get
        {
            if(m_Instance == null)
            {
                m_Instance = new ContinueWndManager();
            }
            return m_Instance;
        }
    }

    //public bool OpenTargetWnd(int configID,object dynamicVal)
    //{

    //}



}


public class ContinueWndModule
{

}