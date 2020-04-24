// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/09 16:00:33
// FileName：Assets/Scripts/Ext/LoopScrollerPageCom/LoopScrollRect2LoopListView2.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;
using UnityEngine.Events;

public class LoopScrollRect2LoopListView2
{
    public LoopListView2 listView;

    public ScrollRect scrollRect;

    string poolName;

    UnityAction<int> scrollRectIndexCallBack;

    public LoopScrollRect2LoopListView2(LoopListView2 lv ,string poolName,int itemCount =0,UnityAction<int> scrollRectIndexCallback = null)
    {
        if(listView == null)
        {
            this.poolName = poolName;
            listView = lv;
            scrollRect = lv.GetComponent<ScrollRect>();
            LoopListViewInitParam initParam = LoopListViewInitParam.CopyDefaultInitParam();
            ItemPrefabConfData data = lv.GetFirstItemPrefabConfData();

            if(data != null)
            {
                Vector2 size = data.mItemPrefab.GetComponent<RectTransform>().sizeDelta;
                bool isVertList = (lv.ArrangeType == ListItemArrangeType.TopToBottom || lv.ArrangeType == ListItemArrangeType.BottomToTop);
                initParam.mItemDefaultWithPaddingSize = isVertList ? size.y : size.x;
                initParam.mItemDefaultWithPaddingSize += data.mPadding;
                
            }

            initParam.mDistanceForNew0 = 100;
            initParam.mDistanceForNew1 = 100;
            listView.InitListView(itemCount, OnGetItemByIndex, initParam);
            this.scrollRectIndexCallBack = scrollRectIndexCallback;
        }
    }

    LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView,int index)
    {
        if(index < 0)
        {
            return null;
        }

        LoopListViewItem2 item = listView.NewListViewItem(poolName);
        LoopScrollRect2LoopListView2Item itemScript = item.GetComponent<LoopScrollRect2LoopListView2Item>();
        itemScript.ScrollCellIndex(index);
        if(scrollRectIndexCallBack != null)
        {
            scrollRectIndexCallBack(index);
        }

        return item;
    }


    public void RefreshAllShowItem(int itemCount,bool resetPos = false)
    {
        listView.SetListItemCount(itemCount, resetPos);
        if (itemCount == listView.ItemTotalCount)
        {
            listView.RefreshAllShownItem();
        }
    }


    public void RefillCells(int itemCount)
    {
        listView.SetListItemCount(itemCount);
        listView.RefreshAllShownItemWithFirstIndex(0);
    }


    public static GameObject LoadScrollRectItem(string itemName)
    {
        try
        {
            return Resources.Load<GameObject>(string.Empty);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
            return null;
        }
    }


    public void PlayItemAnimation(string aniRootName)
    {
        switch (aniRootName)
        {

        }
    }

}

public class LoopScrollRect2LoopListView2Item : MonoBehaviour
{
    public virtual void ScrollCellIndex(int index)
    {

    }
}