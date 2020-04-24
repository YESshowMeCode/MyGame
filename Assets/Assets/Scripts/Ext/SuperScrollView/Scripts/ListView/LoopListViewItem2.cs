using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperScrollView
{

    public class LoopListViewItem2 : MonoBehaviour
    {
        // indicates the item’s index in the list
        //if itemTotalCount is set -1, then the mItemIndex can be from –MaxInt to +MaxInt.
        //If itemTotalCount is set a value >=0 , then the mItemIndex can only be from 0 to itemTotalCount -1.
        public int mItemIndex = -1;

        //ndicates the item’s id. 
        //This property is set when the item is created or fetched from pool, 
        //and will no longer change until the item is recycled back to pool.

        int mItemId = -1;

        LoopListView2 mParentListView = null;      //父物体表  
        bool mIsInitHandlerCalled = false;
        string mItemPrefabName;              //预设体名称
        RectTransform mCachedRectTransform; //缓存RectTransform
        float mPadding;          //填充
        float mDistanceWithViewPortSnapCenter = 0;//与视图端口对齐中心的距离
        int mItemCreatedCheckFrameCount = 0;     //项目创建检查帧计数
        float mStartPosOffset = 0;       //开始印刷位置

        object mUserObjectData = null;
        int mUserIntData1 = 0;   //用户数据 1
        int mUserIntData2 = 0;   //用户数据 2
        string mUserStringData1 = null;
        string mUserStringData2 = null;

        //得到
        public object UserObjectData
        {
            get { return mUserObjectData; }
            set { mUserObjectData = value; }
        }
        public int UserIntData1
        {
            get { return mUserIntData1; }
            set { mUserIntData1 = value; }
        }
        public int UserIntData2
        {
            get { return mUserIntData2; }
            set { mUserIntData2 = value; }
        }
        public string UserStringData1
        {
            get { return mUserStringData1; }
            set { mUserStringData1 = value; }
        }
        public string UserStringData2
        {
            get { return mUserStringData2; }
            set { mUserStringData2 = value; }
        }

        public float DistanceWithViewPortSnapCenter
        {
            get { return mDistanceWithViewPortSnapCenter; }
            set { mDistanceWithViewPortSnapCenter = value; }
        }


        public float StartPosOffset
        {
            get { return mStartPosOffset; }
            set { mStartPosOffset = value; }
        }

        public int ItemCreatedCheckFrameCount
        {
            get { return mItemCreatedCheckFrameCount; }
            set { mItemCreatedCheckFrameCount = value; }
        }

        public float Padding
        {
            get { return mPadding; }
            set { mPadding = value; }
        }

        //得到缓存RectTransform
        public RectTransform CachedRectTransform
        {
            get
            {
                if (mCachedRectTransform == null)
                {
                    mCachedRectTransform = gameObject.GetComponent<RectTransform>();
                }
                return mCachedRectTransform;
            }
        }

        //名称
        public string ItemPrefabName
        {
            get
            {
                return mItemPrefabName;
            }
            set
            {
                mItemPrefabName = value;
            }
        }
        //索引
        public int ItemIndex
        {
            get
            {
                return mItemIndex;
            }
            set
            {
                mItemIndex = value;
            }
        }
        //id
        public int ItemId
        {
            get
            {
                return mItemId;
            }
            set
            {
                mItemId = value;
            }
        }


        public bool IsInitHandlerCalled
        {
            get
            {
                return mIsInitHandlerCalled;
            }
            set
            {
                mIsInitHandlerCalled = value;
            }
        }

        public LoopListView2 ParentListView
        {
            get
            {
                return mParentListView;
            }
            set
            {
                mParentListView = value;
            }
        }

        //得到顶部坐标的Y值
        public float TopY
        {
            get
            {
                ListItemArrangeType arrageType = ParentListView.ArrangeType;//滚动类型
                if (arrageType == ListItemArrangeType.TopToBottom)
                {//从上往下
                    return CachedRectTransform.anchoredPosition3D.y;
                }
                else if (arrageType == ListItemArrangeType.BottomToTop)
                {
                    return CachedRectTransform.anchoredPosition3D.y + CachedRectTransform.rect.height;
                }
                return 0;
            }
        }

        //得到底部的Y轴坐标
        public float BottomY
        {
            get
            {
                ListItemArrangeType arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.TopToBottom)
                {
                    return CachedRectTransform.anchoredPosition3D.y - CachedRectTransform.rect.height;
                }
                else if (arrageType == ListItemArrangeType.BottomToTop)
                {
                    return CachedRectTransform.anchoredPosition3D.y;
                }
                return 0;
            }
        }


        public float LeftX
        {
            get
            {
                ListItemArrangeType arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.LeftToRight)
                {
                    return CachedRectTransform.anchoredPosition3D.x;
                }
                else if (arrageType == ListItemArrangeType.RightToLeft)
                {
                    return CachedRectTransform.anchoredPosition3D.x - CachedRectTransform.rect.width;
                }
                return 0;
            }
        }

        public float RightX
        {
            get
            {
                ListItemArrangeType arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.LeftToRight)
                {
                    return CachedRectTransform.anchoredPosition3D.x + CachedRectTransform.rect.width;
                }
                else if (arrageType == ListItemArrangeType.RightToLeft)
                {
                    return CachedRectTransform.anchoredPosition3D.x;
                }
                return 0;
            }
        }

        //得到表的大小
        public float ItemSize
        {
            get
            {
                if (ParentListView.IsVertList)//如果是垂直滚动
                {
                    return CachedRectTransform.rect.height;
                }
                else
                {
                    return CachedRectTransform.rect.width;
                }
            }
        }

        //得到单个item的大小以及下面的填充
        public float ItemSizeWithPadding
        {
            get
            {
                return ItemSize + mPadding;
            }
        }

    }
}
