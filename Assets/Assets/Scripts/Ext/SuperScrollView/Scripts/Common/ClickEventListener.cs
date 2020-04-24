using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SuperScrollView
{
    public class ClickEventListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public static ClickEventListener Get(GameObject obj)
        {
            ClickEventListener listener = obj.GetComponent<ClickEventListener>();
            if (listener == null)
            {
                listener = obj.AddComponent<ClickEventListener>();
            }
            return listener;
        }

        System.Action<GameObject> mClickedHandler = null;
        System.Action<GameObject> mDoubleClickedHandler = null;
        System.Action<GameObject> mOnPointerDownHandler = null;
        System.Action<GameObject> mOnPointerUpHandler = null;
        bool mIsPressed = false;

        public bool IsPressd
        {
            get { return mIsPressed; }
        }
        //是否发生了点击
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                if (mDoubleClickedHandler != null)
                {
                    mDoubleClickedHandler(gameObject);
                }
            }
            else
            {
                if (mClickedHandler != null)
                {
                    mClickedHandler(gameObject);
                }
            }

        }
        //设置点击的处理
        public void SetClickEventHandler(System.Action<GameObject> handler)
        {
            mClickedHandler = handler;
        }

        //设置鼠标双击处理
        public void SetDoubleClickEventHandler(System.Action<GameObject> handler)
        {
            mDoubleClickedHandler = handler;
        }
        //设置鼠标按下处理
        public void SetPointerDownHandler(System.Action<GameObject> handler)
        {
            mOnPointerDownHandler = handler;
        }
        //设置鼠标抬起处理
        public void SetPointerUpHandler(System.Action<GameObject> handler)
        {
            mOnPointerUpHandler = handler;
        }


        //点击
        public void OnPointerDown(PointerEventData eventData)
        {
            mIsPressed = true;
            if (mOnPointerDownHandler != null)
            {
                mOnPointerDownHandler(gameObject);
            }
        }
        //点击释放
        public void OnPointerUp(PointerEventData eventData)
        {
            mIsPressed = false;
            if (mOnPointerUpHandler != null)
            {
                mOnPointerUpHandler(gameObject);
            }
        }

    }

}
