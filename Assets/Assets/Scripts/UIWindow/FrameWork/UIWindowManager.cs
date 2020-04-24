using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIWindowManager : MonoSingleton<UIWindowManager> {

    class WinNameToParam
    {
        public UIWindowEnum WinName;
        public CtrlParams Params;
    }



    private static int MAX_LOADED_WIN = 15;

    Dictionary<int, UIWindowInfo> winInfoDict;

    Dictionary<int, UIWindowBase> winLoadedDict = new Dictionary<int, UIWindowBase>();

    LRUCache<int> winLRUCache = new LRUCache<int>(MAX_LOADED_WIN);

    HashSet<int> winOpenedSet = new HashSet<int>();

    HashSet<int> hideWinNames = new HashSet<int>();

    LinkedList<WinNameToParam> popWinQueue = new LinkedList<WinNameToParam>();

    //事件
    Action<UIWindowEnum, GameObject> WinOpenedEvent;
    Action<UIWindowEnum, GameObject> WinClosedEvent;




    //UI框架常用信息
    Transform uiRoot;
    Transform popupWindowRoot; 
     Transform normalWindowRoot;
    Transform screenClickEffect;
    Transform padWindowRoot;

    public Transform popupMask;
    public Transform blackMask;
    public Transform blurMask;
    public Transform noTouchMask;

    public Transform ClickEffect;

    //public NoTouchMaskClip = 

    Transform screenEffect;
    public Camera UICamera;
    public Camera BlueCamera;
    public Canvas canvas;
    public RawImage CameraBg;

    
    public void Init()
    {
        uiRoot = transform.Find("UIRoot");

        winInfoDict = UIwindowEnumUtil.GetRegisterUIWindow();

        winLRUCache.ModifyCapacity(MAX_LOADED_WIN);
    }


    #region 事件注册
    public void RegisterWinOpenedEvent(Action<UIWindowEnum, GameObject> action)
    {
        WinOpenedEvent -= action;
        WinOpenedEvent += action;
    }

    public void UnregisterWinOpenedEvent(Action<UIWindowEnum, GameObject> action)
    {
        WinOpenedEvent -= action;
    }


    public void RegisterWinClosedEvent(Action<UIWindowEnum, GameObject> action)
    {
        WinClosedEvent -= action;
        WinClosedEvent += action;
    }
    public void UnregisterWinClosedEvent(Action<UIWindowEnum, GameObject> action)
    {
        WinClosedEvent -= action;
    }

    #endregion

    #region window判断、获取、设置等方法

    public bool IsAlreadyOpened(UIWindowEnum name)
    {
        return winOpenedSet.Contains((int)name) || GetPopWinQueueFirstItem(name) != null;
    }

    WinNameToParam GetPopWinQueueFirstItem(UIWindowEnum name)
    {
        foreach (var item in popWinQueue)
        {
            if (item.WinName == name)
            {
                return item;
            }
        }
        return null;
    }


    public bool WinOpenedSetContains(UIWindowEnum name)
    {
        return winOpenedSet.Contains((int)name);
    }


    public UIWindowInfo FetchWinInfo(UIWindowEnum name)
    {
        UIWindowInfo result = null;
        winInfoDict.TryGetValue((int)name, out result);
        return result;
    }

    public bool IsOpenning(UIWindowEnum name)
    {
        UIWindowBase WinBase = null;
        if(winLoadedDict.TryGetValue((int)name,out WinBase))
        {
            return WinBase.gameObject.activeSelf;
        }

        return false;
    }

    public Dictionary<int ,UIWindowInfo> FetchWindowInfoDict()
    {
        return winInfoDict;
    }


    public T GetUIWindow<T>(UIWindowEnum name) where T :UIWindowBase
    {
        return GetUIWindow(name) as T;
    }

    public UIWindowBase GetUIWindow(UIWindowEnum name)
    {
        UIWindowBase win = null;
        winLoadedDict.TryGetValue((int)name, out win);
        return win;
    }

    public void PrepareLoadUIWindow(UIWindowEnum name)
    {
        UIWindowInfo winInfo = winInfoDict[(int)name];
        var win = GetOrLoadUiWindow(winInfo);

        if(win!= null && win.gameObject!= null)
        {
            win.gameObject.SetActive(true);
        }
    }

    public UIWindowBase GetOrLoadUiWindow(UIWindowInfo winInfo)
    {
        UIWindowBase win;

        if(winLoadedDict.TryGetValue((int)winInfo.windowEnum,out win))
        {
            return win;
        }

        GameObject obj = Resources.Load(winInfo.PrefabFullPath) as GameObject;
        if(obj = null)
        {
            Debug.LogError("加载win出错");
            return null;
        }

        GameObject ui = obj;
        UIWindowBase uiWindow = ui.GetComponent<UIWindowBase>();
        if(uiWindow == null)
        {
            Debug.LogError("perfab doesn't have window Script");
            return null;
        }

        winLoadedDict[(int)winInfo.windowEnum] = uiWindow;

        if(winInfo.windowType == UIWindowType.oNormal)
        {
            uiWindow.transform.SetParent(normalWindowRoot);
        }
        else
        {
            uiWindow.transform.SetParent(popupWindowRoot);
        }


        var cvs = ui.GetComponent<Canvas>();

        if(cvs != null && ! cvs.overrideSorting)
        {
            cvs.overrideSorting = true;
            cvs.sortingOrder = 3;
        }

        //描点处理
        //TODO..

        return uiWindow;
    }

    #endregion

    #region 窗口函数封装  loading、Animation函数封装
     
    public  void PopupUIWindowImmdiate(UIWindowEnum name ,CtrlParams par = null)
    {
        if(!isPopup(winInfoDict[(int)name].windowType))
        {
            return;
        }

        GameObject mask = GetMaskByType(winInfoDict[(int)name].windowType);
        
    }


    public void ShowUIWindow(UIWindowEnum name,bool ctrlEvent = false,CtrlParams par = null)
    {
        if(isPopup(winInfoDict[(int)name].windowType))
        {

        }
        else
        {
            ShowUIWindowReal(name, ctrlEvent, null, par);
        }
    }


    private void ShowUIWindowReal(UIWindowEnum name, bool ctrlEvent,GameObject mask,CtrlParams par = null)
    {
        bool isAlreadyLoad = winLoadedDict.ContainsKey((int)winInfoDict[(int)name].windowEnum);

        UIWindowBase uiWindow = GetOrLoadUiWindow(winInfoDict[(int)name]);
        UIWindowInfo winInfo = winInfoDict[(int)name];

        if(!winOpenedSet.Contains((int)name))
        {
            winOpenedSet.Add((int)name);
        }
        else
        {
            Debug.LogWarning("window is opened twice");
        }

        TrySetWindowActive(name, !hideWinNames.Contains((int)name));
        TryClearLoadedWindowLRU(name);
        return;

    }

    private void TryClearLoadedWindowLRU(UIWindowEnum name)
    {
        do
        {
            int removeWindow = 0 ;
            bool ret = winLRUCache.VisitAndTryRemove((int)name, out removeWindow);
            if (ret)
            {
                ClearLoadedWindow((UIWindowEnum)removeWindow);

            }
        } while (winLRUCache.Count > MAX_LOADED_WIN);
    }


    public void PopupUIWindowQueue(UIWindowEnum name,CtrlParams par=null)
    {
        if (!isPopup(winInfoDict[(int)name].windowType))
            return;

        GameObject mask = null;

        popWinQueue.AddLast(new WinNameToParam() { WinName = name, Params = par });

        if(popWinQueue.Count == 1 )
        {
            mask = GetMaskByType(winInfoDict[(int)name].windowType);

            ShowUIWindowReal(name, false, mask, par);
        }

        if (popWinQueue.Count > 1)
        {
            return;
        }

        mask = GetMaskByType(winInfoDict[(int)name].windowType);

        ShowUIWindowReal(name, false, mask, par);
    }

    void ClearLoadedWindow(UIWindowEnum name ,bool forceRemoveCast = false)
    {
        DestroyImmediate(winLoadedDict[(int)name].gameObject);
        winLoadedDict.Remove((int)name);
    }



    #endregion


    #region 窗口打开状态

    public void SetWindowActive(UIWindowEnum name,bool active)
    {
        if(!active && !hideWinNames.Contains((int)name))
        {
            hideWinNames.Add((int)name);
            TrySetWindowActive(name, false);
            return;
        }
        
        if(active && hideWinNames.Contains((int)name))
        {
            hideWinNames.Remove((int)name);
            TrySetWindowActive(name, true);
            return;
        }
    }

    private void TrySetWindowActive(UIWindowEnum name, bool active)
    {
        if (!winOpenedSet.Contains((int)name))
        {
            return;
        }

        var win = GetUIWindow(name);
        if (win != null && win.gameObject.activeSelf != active)
        {
            win.gameObject.SetActive(true);
        }
    }


    #endregion  

    #region 弹出窗口的Mask处理
    public bool isPopup(UIWindowType type)
    {
        if (type != UIWindowType.oNormal)
            return true;
        return false;
    }

    public GameObject GetMaskByType(UIWindowType type)
    {
        GameObject mask = null;

        if(type == UIWindowType.ePopup)
        {
            mask = GameObject.Instantiate(popupMask.gameObject);
        }
        else if(type == UIWindowType.ePopup_Black)
        {
            mask = GameObject.Instantiate(blackMask.gameObject);
        }
        else if(type == UIWindowType.ePopup_Blue)
        {
            mask = GameObject.Instantiate(blurMask.gameObject);
            var rawImages = mask.GetComponentsInChildren<RawImage>();

            var rawImage = rawImages[rawImages.Length - 1];

            rawImage.texture = null;

            rawImage.gameObject.SetActive(true);
        }
        if(mask != null)
        {
            mask.SetActive(true);
        }
        return mask;
    }



    #endregion
}
