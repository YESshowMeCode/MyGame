// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/09 16:00:00
// FileName：Assets/Scripts/Ext/LoopScrollerPageCom/LoopScrollerPageCom.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperScrollView;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using DG.Tweening;


/// <summary>
/// 循环列表 分页组件
/// 注意使用时应该是先刷新再请求，否则会出现错误
/// 如果请求在前  刷新在后的话 相对索引已经发生变化  返回的相对索引会出现错误
/// loopScrollRect在刷新item时调用LoopScrollRectBaseCell中的ScrollCellIndex方法通过
/// 使用时需要同服务器定制的协议中需要包括 page_num（第几页） （收发协议 total_count（总条数）收发协议 pageItem_num（每一页元素的个数）
/// 
/// 
/// </summary>
public class LoopScrollerPageCom
{
    /// <summary>
    /// 是否关闭下拉刷新
    /// </summary>
    public bool DisableReq = false;

    public LoopScrollRect2LoopListView2 loopScrollRect2LoopListView2;

    /// <summary>
    /// 跳转时使用的加速度组件
    /// </summary>
    public AnimationCurve JumpToTargetCurve = null;


    /// <summary>
    /// 跳到目标时使用
    /// </summary>
    public float JumpToTargetTime = 2;

    private bool isJumping = false;

    /// <summary>
    /// 当前在第几页
    /// </summary>
    private int pageNum;
    /// <summary>
    /// 总条数
    /// </summary>
    private int totalCount;
    /// <summary>
    /// 当前页签的最大索引
    /// </summary>
    int currentPageMaxNum
    {
        get
        {
            return GetCurrentPageMaxIndex(pageNum);
        }
    }

    /// <summary>
    /// 当前页签的最小索引
    /// </summary>
    int currentPageMinNum
    {
        get
        {
            return GetCurrentPageMinIndex(pageNum);
        }
    }

    /// <summary>
    /// 元素大小
    /// </summary>
    private float elementSize = 0;

    /// <summary>
    /// 为空不进行更新，非空则向上或者向下
    /// </summary>
    bool? isTop = null;

    #region 分页计算
    /// <summary>
    /// 返回时每个页签的大小
    /// </summary>
    public int pageItemNum = 20;
    /// <summary>
    /// 如果位于这个区间不进行重复请求
    /// 由于ScrollRect不是一个分页的表现 所有当处在请求的区间时  如第一页0-19，第二页20-39处在19上下区间时会导致频繁请求所以加入了一个
    /// 重叠区域不请求的参数 这样玩家在区间上下拖动时数据不再刷新 超出该区域后才能再次请求
    /// </summary>
    public int OverlapReqNum = 5;

    public int GetCurrentPageMinIndex(int pageNum)
    {
        if(pageNum == 0)
        {
            return 0;
        }

        return pageNum * pageItemNum;
    }

    public int GetCurrentPageMaxIndex(int pageNum)
    {
        return (pageNum + 1) * pageItemNum - 1;
    }

    #endregion


    UnityAction<int> reqDataCall;


    bool isOutBottomBounds = false;

    bool isOutTopBounds = false;

    int? targetJumpIndex = null;

    public delegate bool HasLastPage(int index);

    HasLastPage hasLastPage;

    bool isReq = false;

    public LoopScrollerPageCom(LoopListView2 loopListView2,UnityAction<int> reqDataCall , string poolName,int pageItemNum = 20,int overlapReqCount = 5,AnimationCurve jumpToTargetCurve = null,float jumpToTargetTime = 0,HasLastPage hasLastPage = null)
    {
        loopScrollRect2LoopListView2 = new LoopScrollRect2LoopListView2(loopListView2, poolName, 0, ScrollerRectUpdateIndex);
        this.reqDataCall = reqDataCall;
        this.pageItemNum = pageItemNum;
        this.pageItemNum = pageItemNum;
        this.OverlapReqNum = overlapReqCount;
        this.hasLastPage = hasLastPage;
        this.targetJumpIndex = null;
        this.JumpToTargetCurve = jumpToTargetCurve;
        this.JumpToTargetTime = jumpToTargetTime;

    }

    /// <summary>
    /// 当前元素刷新到的索引，需要指定的loopScrollRectBaseCell 通过Controller来刷新这个接口
    /// </summary>
    /// <param name="index"></param>
    void ScrollerRectUpdateIndex(int index)
    {
        if (DisableReq) return;
        if (isReq) return;
        if (index == 0 || index > currentPageMaxNum) return;
        if (targetJumpIndex.HasValue) return;

        if (index < currentPageMinNum || index > currentPageMaxNum)
        {
            pageNum = index / pageItemNum;
            return;
        }

        if(index > currentPageMaxNum - OverlapReqNum && isOutTopBounds)
        {
            isOutBottomBounds = false;
            pageNum += 1;
            isReq = true;
        }
        else if(pageNum!=0 && index <= currentPageMinNum + OverlapReqNum && isOutBottomBounds)
        {
            isOutTopBounds = false;
            pageNum -= 1;
            pageNum = pageNum < 0 ? 0 : pageNum;
            isReq = true;

            if(hasLastPage!=null && !hasLastPage(pageNum))
            {
                loopScrollRect2LoopListView2.listView.ScrollRect.StopMovement();
                loopScrollRect2LoopListView2.listView.ScrollRect.enabled = false;
            }
        }
        if (index > currentPageMinNum + OverlapReqNum)
        {
            isOutBottomBounds = true;
        }

        if (index < currentPageMaxNum - OverlapReqNum)
        {
            isOutTopBounds = true;
        }

        if (isReq)
        {
            ReqData(pageNum);
        }

    }

    void ReqData(int pageIndex)
    {
        reqDataCall(pageIndex);
    }

    public void RspDataCall(int pageIndex,int totalCount,int cacheCount = 0)
    {
        this.totalCount = totalCount;
        pageNum = pageIndex;
        loopScrollRect2LoopListView2.RefreshAllShowItem(cacheCount);
        isReq = false;
    }

    public void RspDataCall<T>(int pageIndex,int totalCount,Dictionary<int,List<T>> dataCache = null)
    {
        loopScrollRect2LoopListView2.listView.ScrollRect.StopMovement();
        int cacheCount = GetVirtualCacheDataCount(dataCache, pageItemNum);
        this.totalCount = totalCount;

        if(targetJumpIndex.HasValue && dataCache != null)
        {
            
        }
    }

    public void InitScrollRect(int count)
    {
        loopScrollRect2LoopListView2.RefreshAllShowItem(count);
    }

    public void Clear()
    {
        loopScrollRect2LoopListView2.RefillCells(0);
    }

    List<int> ReqPageList = new List<int>();

    public void JumpToTargetIndex(int index,Action callBack = null)
    {
        ReqPageList.Clear();
        targetJumpIndex = index;
        LoopListViewItem2 curItem = loopScrollRect2LoopListView2.listView.GetViewportFirstItem();

        if(curItem == null)
        {
            isJumping = false;
            return;
        }

        loopScrollRect2LoopListView2.listView.ScrollRect.StopMovement();

        int targetpage = index / pageItemNum;

        int curPage = curItem.ItemIndex / pageItemNum;

        pageNum = curPage;

        if(index>= GetCurrentPageMaxIndex(targetpage) - OverlapReqNum)
        {
            ReqPageList.Add(targetpage + 1);
        }

        if(pageNum == targetpage&& ReqPageList.Count == 0)
        {
            WaitToTargetPageDataRsp<object>(null, callBack);
        }
        else
        {
            ReqPageList.Add(targetpage);
            for(int i = ReqPageList.Count - 1; i >= 0; i--)
            {
                ReqData(ReqPageList[i]);
            }
        }


    }


    public void WaitToTargetPageDataRsp<T>(Dictionary<int,List<T>> cachaData=null,Action callBack = null)
    {
        if(targetJumpIndex == null)
        {
            return;
        }

        
    }

    public static void ClearData<T>(Dictionary<int,List<T>> dir)
    {
        dir.Clear();
    }

    public static int GetVirtualCacheDataCount<T>(Dictionary<int,List<T>> dir,int pageCount = 20)
    {
        int maxPage = -1;
        List<T> maxPageData = null;
        foreach(KeyValuePair<int,List<T>> kv in dir)
        {
            if(kv.Key > maxPage)
            {
                maxPage = kv.Key;
                maxPageData = kv.Value;
            }
        }
        return maxPage * pageCount + maxPageData.Count;
    }

}
