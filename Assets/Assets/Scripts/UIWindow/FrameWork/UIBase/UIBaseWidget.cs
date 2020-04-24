using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBaseWidget : BaseClassScript {

    protected int widgetDepth;
    protected string widgetName;
    protected GraphicRaycasterSortingOrder canvasAndGraphicRaycaster;

    private bool isRectTranformCached = false;
    private RectTransform m_RectTransform;
    private Dictionary<int, UnityEngine.Object> m_NeedFindVariableDict = new Dictionary<int, Object>();

    public RectTransform CacheRectTransform
    {
        get
        {
            if (!isActiveAndEnabled)
            {
                isRectTranformCached = true;
                m_RectTransform = GetComponent<RectTransform>();
            }

            return m_RectTransform;
        }
    }



	
}
