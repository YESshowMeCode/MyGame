using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIControllerManager : MonoSingleton<UIControllerManager>
{
    private UIControllerManager()
    {

    }


    Dictionary<int, UIBaseController> winCtrlerDict = new Dictionary<int, UIBaseController>();

    public override void OnSingletonInit()
    {
        base.OnSingletonInit();
        InitAllController();
    }

    public override void OnSingletonDestory()
    {
        base.OnSingletonDestory();
        ReleaseAllController();
    }

    //根据名称获取contrller
    public UIBaseController GetUIController(UIWindowEnum name)
    {
        UIBaseController ctrl;
        winCtrlerDict.TryGetValue((int)name, out ctrl);
        return ctrl;
    }

    public T GetUIController<T>(UIWindowEnum name) where T:UIBaseController
    {
        return GetUIController(name) as T;
    }

    public void ResetAllController()
    {
        ReleaseAllController();
        InitAllController();
    }

    #region 事件调用

    public void ControllerOpenEvent(UIWindowEnum name,params CtrlParams[] pars)
    {
        var ctrl = GetUIController(name);
        if(ctrl != null)
        {
            ctrl.PanelOpened(pars[0]);
        }
    }

    public void ControllerOpenAfterAniEvent(UIWindowEnum name)
    {
        var ctrl = GetUIController(name);
        if(ctrl != null)
        {
            ctrl.PanelOpenedAfterAni();
        }
    }


    public void AllControllerReconnect(object[] obj)
    {
        using(var ie = winCtrlerDict.GetEnumerator())
        {
            while(ie.MoveNext())
            {
                ie.Current.Value.ResetWhenReconnect();
            }
        }
    }

    #endregion

    #region

    void InitAllController()
    {
        var winInfoDict = UIWindowManager.Instance.FetchWindowInfoDict();

        var ie = winInfoDict.GetEnumerator();

        while(ie.MoveNext())
        {
           
        }
    }


    private void InitController(UIWindowInfo info)
    {
        UIWindowEnum winName = info.windowEnum;
        string ctrlerName = info.ControllerName;

        Type type = Type.GetType(ctrlerName);

        if(type == null)
        {
            Debug.LogError(winName + "无法获取type ctrlerName" + ctrlerName);
            return;
        }

        UIBaseController controller = Activator.CreateInstance(type) as UIBaseController;

        if(controller == null)
        {
            Debug.LogError(winName + "无法生成controller");
            return;
        }

        winCtrlerDict.Add((int)winName, controller);
        controller.RegisterEvent();

    }

    void ReleaseAllController()
    {
        var ie = winCtrlerDict.GetEnumerator();

        while(ie.MoveNext())
        {
            ie.Current.Value.UnregisterEvent();
        }

        ie.Dispose();
        winCtrlerDict.Clear();
    }


    #endregion

}
