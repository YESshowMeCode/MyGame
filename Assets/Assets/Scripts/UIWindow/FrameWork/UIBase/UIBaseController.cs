using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIBaseController<T, D> : UIBaseController where T : UIWindowBase where D : CtrlParams
{
    public T WindowScript { get { return base.WindowScript as T; } }

    public sealed override void PanelOpened(CtrlParams pars)
    {
        base.PanelOpened(pars);
        PanelOpened(pars as D);
    }
    public abstract void PanelOpened(D pars);
}


public class UIBaseController<T>:UIBaseController where T:UIWindowBase
{
    public T WindowScript { get { return base.WindowScript as T; } }
}

public partial class UIBaseController {

    public UIWindowBase WindowScript;
    public CtrlParams param;


    public UIBaseController()
    {

    }


    #region 生命周期方法

    public virtual bool ContinueWnd(string strDesign = "",object objProgram = null)
    {
        return false;
    }

    public virtual void PanelOpened(CtrlParams pars)
    {

    }


    public virtual void PanelOpenedAfterAni()
    {

    }

    public virtual void PanelClose()
    {

    }


    public virtual void RegisterEvent()
    {

    }


    public virtual void UnregisterEvent()
    {

    }

    public virtual void ResetWhenReconnect()
    {

    }

    #endregion
}
