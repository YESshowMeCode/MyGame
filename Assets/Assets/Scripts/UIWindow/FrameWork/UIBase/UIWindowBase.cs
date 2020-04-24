using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWindowBase<C,REF> : UIWindowBase
    where C:UIBaseController
    where REF : UGUIRefNode
{
    public C Ctrler { get { return base.Ctrler as C; } }
    public REF Ref { get; private set; }


    protected override void  Awake()
    {
        base.Awake();
        Ref = GetComponent<REF>();
    }

}

public class UIWindowBase<C> :UIWindowBase where C: UIWindowBase
{
    public C Ctriler { get { return base.Ctrler as C; } }
}


public class UIWindowBase : UIBaseWidget
{
    
    public GameObject ImmediatePopupMaskGo;

    public UIWindowInfo WindowInfo;

    public UIBaseController Ctrler;

    private bool updated = false;

    public string WindowName
    {
        get { return WindowInfo.windowName; }
    }


    public UIWindowEnum WindowEnum
    {
        get { return WindowInfo.windowEnum; }
    }

    #region 生命周期

    protected virtual void Awake()
    {

    }

    protected virtual void Update()
    {
        if (!updated)
            return;

        int deltaTime = 1;
            OnUpdateCall(deltaTime);
    }

    protected virtual void OnDestory()
    {

    }


    public virtual IEnumerator PrepareOpen()
    {
        return null;
    }


    protected virtual bool Open()
    {
        updated = true;
        return true;
    }


    protected virtual bool Close()
    {
        updated = true;
        return true;
    }


    #endregion
    

    public virtual void ReturnBack()
    {

    }


    protected int baseDeltaTime = 0;
    protected int baseMinConstDeltaTime = 50;
    protected int baseMaxConstDeltaTime = 200;
    protected int baseMinExeDeltaTime = 0;

    /// <summary>
    /// 界面统一使用这个限制对界面的update进行限制（只能在update继承时使用）
    /// </summary>
    /// <param name="deltaTimeMs"></param>
    /// <returns></returns>
    protected virtual bool OnUpdateCall(int deltaTimeMs)
    {
        if(deltaTimeMs > baseMinConstDeltaTime)
        {
            baseDeltaTime += deltaTimeMs;
            if(baseDeltaTime < baseMinExeDeltaTime)
            {
                return false;
            }

            baseMinExeDeltaTime = (deltaTimeMs > baseMaxConstDeltaTime ? baseMaxConstDeltaTime : deltaTimeMs);
            baseDeltaTime = 0;
        }
        return true;
    }


    #region 供UI框架使用

    public bool OpenPublic()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        return Open();
    }


    public bool ClosePublic()
    {
        return Close();
    }

    #endregion
}
