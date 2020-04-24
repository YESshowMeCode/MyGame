using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class UIInterfaceInfo
{
    public UIInterfaceEnum InterfaceEnum;
    public List<UIWindowEnum> WinEnumList;

    public UIInterfaceInfo(UIInterfaceEnum interfaceEnum,List<UIWindowEnum> winEnumList)
    {
        InterfaceEnum = interfaceEnum;
        WinEnumList = winEnumList;
    }
}


public class UIInterfaceManager : MonoSingleton<UIInterfaceManager>
{
    private UIInterfaceManager()
    {

    }

    List<UIWindowEnum> NullWindowList = new List<UIWindowEnum>();

    public Dictionary<int, UIInterfaceInfo> m_InterfaceDict = new Dictionary<int, UIInterfaceInfo>();

    public Stack<OpenStackItem> OpenStack { get; private set; }

    public UIInterfaceEnum OpenLastInterface { get; private set; }

    public UIInterfaceEnum OpenningInterface { get; private set; }

    public UIInterfaceEnum m_CurInterface { get; private set; }

    public UIInterfaceEnum m_LastInterface { get; private set; }

    public UIInterfaceEnum OpenedInterface { get; private set; }


    public bool Opening { get; private set; }

    private OpenStackItem returnedOpenStackitem;


    #region
    Action<UIInterfaceEnum> EnteredEvent;
    Action<UIInterfaceEnum> EnteredAfterAniEvent;

    public void RegisterEnterEvent(Action<UIInterfaceEnum> action)
    {
        EnteredEvent -= action;
        EnteredEvent += action;
    }

    public void UnregisterEnterEvent(Action<UIInterfaceEnum> action)
    {
        EnteredEvent -= action;

    }

    public void RegisterOnceEnteredEvent(Action<UIInterfaceEnum> action)
    {
        Action<UIInterfaceEnum> callback = null;

        callback = (inter) =>
        {
            UnregisterEnterEvent(callback);
            if (action != null)
            {
                action(inter);
            }
        };
        RegisterEnterEvent(action);
    }

    public void RegisterAfterAniEnterEvent(Action<UIInterfaceEnum> action)
    {
        EnteredAfterAniEvent -= action;
        EnteredAfterAniEvent += action;
    }

    public void UnregisterAfterAniEnterEvent(Action<UIInterfaceEnum> action)
    {
        EnteredAfterAniEvent -= action;

    }
    #endregion



    public void Init()
    {
        m_InterfaceDict = UIInterfaceEnumUtil.GetInterfaceInfoDict();
        OpenStack = new Stack<OpenStackItem>();

        m_CurInterface = UIInterfaceEnum.None;
        m_LastInterface = UIInterfaceEnum.None;
        OpenLastInterface = UIInterfaceEnum.None;
        OpenningInterface = UIInterfaceEnum.None;
        OpenedInterface = UIInterfaceEnum.None;
    }


    public void Clear()
    {
        EnteredEvent = null;
        OpenStack.Clear();

        m_CurInterface = UIInterfaceEnum.None;
        m_LastInterface = UIInterfaceEnum.None;
        OpenLastInterface = UIInterfaceEnum.None;
        OpenningInterface = UIInterfaceEnum.None;
        OpenedInterface = UIInterfaceEnum.None;
    }


     


    #region 获取、遍历、判断方法

    public List<UIWindowEnum> GetWindowList(UIInterfaceEnum name)
    {
        UIInterfaceInfo info;
        if(m_InterfaceDict.TryGetValue((int)name,out info))
        {
            return info.WinEnumList;
        }

        return null;
    }


    private List<UIWindowEnum> GetSurplusWindows(UIInterfaceEnum name,UIInterfaceEnum minus,List<UIWindowEnum> oResult)
    {
        var list = GetWindowList(name);
        foreach(var winName in list)
        {
            oResult.Add(winName);
        }

        if (minus == UIInterfaceEnum.None)
            return oResult;

        var minuses = GetWindowList(minus);
        for(int i= oResult.Count - 1; i >= 0; i--)
        {
            if (minuses.Contains(oResult[i]))
            {
                oResult.RemoveAt(i);
            }
        }

        return oResult;
    }


    private void RecurseWindowList(UIInterfaceEnum name,Action<UIWindowEnum> action)
    {
        var list = GetWindowList(name);
        for(int i = 0; i < list.Count; i++)
        {
            action(list[i]);
        }
    }

    //private void RecurseSurplusWindowsList(UIInterfaceEnum name,UIInterfaceEnum minus,Action<UIWindowEnum> action)
    //{
    //    var tmpList = ListPool
    //}




    #endregion


    #region 返回堆栈

    public class OpenStackItem
    {
        public UIInterfaceEnum name;
        public string SceneName;
        public string MapName;
        public CtrlParams[] pars;

        public OpenStackItem(UIInterfaceEnum name,string sceneName,string mapName)
        {
            this.name = name;
            this.SceneName = sceneName;
            MapName = mapName;
        }

        public override bool Equals(object obj)
        {
            var other = obj as OpenStackItem;
            if (other == null)
                return false;

            return name == other.name;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)name * 397;
                if(SceneName != null)
                {
                    hash ^= SceneName.GetHashCode();
                }
                if(MapName != null)
                {
                    hash ^= MapName.GetHashCode();
                }
                return hash;
            }
        }

    }



    public void PushInOpenStack(UIInterfaceEnum name,string sceneName,string mapName,CtrlParams[] pars = null,List<int> specialreturnChain = null)
    {
        var item = new OpenStackItem(name, sceneName, mapName);
        item.pars = pars;
        if(name != UIInterfaceEnum.None)
        {
            if(UIInterfaceEnumUtil.IsRootUIInterfaceEnum(name))
            {
                OpenStack.Clear();
            }

            if(specialreturnChain!=null && specialreturnChain.Count >0 &&
                UIInterfaceEnumUtil.IsRootUIInterfaceEnum((UIInterfaceEnum)specialreturnChain[specialreturnChain.Count-1]))
            {
                OpenStack.Clear();


                for(int i = specialreturnChain.Count - 1; i >= 0; i--)
                {
                    UIInterfaceEnum specialName = (UIInterfaceEnum)specialreturnChain[i];

                    var specialItem = new OpenStackItem(specialName,
                        UIInterfaceEnumUtil.IsRootUIInterfaceEnum(specialName) ? sceneName : sceneName,
                        mapName);
                    specialItem.pars = pars;
                    OpenStack.Push(specialItem);
                }
            }

            if (OpenStack.Contains(item))
            {
                while (!item.Equals(OpenStack.Pop())) ;
            }
            OpenStack.Push(item);
        }
    }


    public void PopOutOpenStack()
    {
        OpenStack.Pop();
    }

    public void ClearOpenStack()
    {
        var target = UIInterfaceEnum.None;//根窗口

        while(OpenStack.Count!=0&& OpenStack.Peek().name != target)
        {
            OpenStack.Pop();
        }
    }

    #endregion


}
