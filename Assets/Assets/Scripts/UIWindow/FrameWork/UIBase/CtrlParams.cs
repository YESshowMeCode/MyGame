using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlParams {

    public UIWindowEnum winName;
    public string strDesign = string.Empty;
    public object objProgram = null;


    public CtrlParams(UIWindowEnum winName = UIWindowEnum.eNone,string strDesign = "", object objProgram = null)
    {
        this.winName = winName;
        this.strDesign = strDesign;
        this.objProgram = objProgram;
    }


    public virtual void Clear()
    {
        strDesign = string.Empty;
        objProgram = null;
        winName = UIWindowEnum.eNone;
    }
}
