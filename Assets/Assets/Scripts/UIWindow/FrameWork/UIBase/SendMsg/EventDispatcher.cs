using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public delegate void OnEvent(params object[] param);

public class EventDispatcher : MonoSingleton<EventDispatcher>
{
    private readonly Dictionary<int, ListenerWarp> m_AllListenerMap = new Dictionary<int, ListenerWarp>(50);

    public bool IsRecycled { get; set; }

    private EventDispatcher() { }



    #region 内部结构

    private class ListenerWarp
    {
        private List<OnEvent> m_EventList;

        public bool Fire(int key,params object[] param)
        {
            if(m_EventList == null)
            {
                return false;
            }

            for(int i = 0; i < m_EventList.Count; i++)
            {
                if(m_EventList[i]==null || m_EventList[i].Target.Equals(null))
                {
                    if(m_EventList[i] == null)
                    {
                        Debug.LogWarning("OnEvent is null");
                    }
                    else
                    {
                        Debug.LogWarning("OnEvent.target is null");
                    }

                    m_EventList.RemoveAt(i);
                    i--;
                    continue;

                }

                if(m_EventList[i] != null)
                {
                    m_EventList[i](param);
                }
            }

            return true;
        }

        public bool Add(OnEvent listener)
        {
            if(m_EventList == null)
            {
                m_EventList = new List<OnEvent>();

            }

            if (m_EventList.Contains(listener))
            {
                return false;
            }

            m_EventList.Add(listener);
            return true;
        }


        public void Remove(OnEvent listener)
        {
            if(m_EventList == null)
            {
                return;
            }

            m_EventList.Remove(listener);
        }

        public void RemoveAll()
        {
            if(m_EventList == null)
            {
                return;
            }

            m_EventList.Clear();
        }


    }

    #endregion


    #region 功能函数

    public bool Register<T>(T key,OnEvent func)where T :IConvertible
    {
        var kv = key.GetHashCode();
        ListenerWarp warp;

        if(!m_AllListenerMap.TryGetValue(kv,out warp))
        {
            warp = new ListenerWarp();
            m_AllListenerMap.Add(kv, warp);
        }

        if (warp.Add(func))
        {
            return true;
        }

        return false;

    }


    public void UnRegister<T>(T key,OnEvent func)where T :IConvertible
    {
        ListenerWarp warp;

        if(m_AllListenerMap.TryGetValue(key.GetHashCode(),out warp))
        {
            warp.RemoveAll();
            warp = null;
        }

        m_AllListenerMap.Remove(key.GetHashCode());
    }


    public bool Send<T>(T key,params object[] param)where T :IConvertible
    {
        int kv = key.GetHashCode();
        ListenerWarp warp;
        if(m_AllListenerMap.TryGetValue(kv,out warp))
        {
            return warp.Fire(kv, param);
        }

        return false;
    }

    public void Onrecycled()
    {
        m_AllListenerMap.Clear();
    }


    #endregion

    #region 常用APi

    public static bool SendEvent<T>(T key, params object[] param) where T : IConvertible
    {
        return Instance.Send(key, param);
    }

    public static bool RegisterEvent<T>(T key,OnEvent func)where T : IConvertible
    {
        return Instance.Register(key, func);
    }

    public static void UnRegisterEvent<T>(T key,OnEvent func)where T:IConvertible
    {
        Instance.UnRegister(key, func);
    }

    #endregion

}


