using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//**********************************  数据  *******************************
namespace SuperScrollView
{

    public class ItemData               //数据
    {
        public int mId;
        public string mName;        //数据名称
        public int mFileSize;      
        public string mDesc;        //大小
        public string mIcon;        //图集
        public int mStarCount;      //星级等级
        public bool mChecked;       //是否点击
        public bool mIsExpand;      //是否扩大
    }

    public class DataSourceMgr : MonoBehaviour
    {

        List<ItemData> mItemDataList = new List<ItemData>();        //数据表
        System.Action mOnRefreshFinished = null;                    //刷新委托                  
        System.Action mOnLoadMoreFinished = null;                   //加载完成委托
        int mLoadMoreCount = 20;                                       
        float mDataLoadLeftTime = 0;
        float mDataRefreshLeftTime = 0;
        bool mIsWaittingRefreshData = false;                        //是否等待刷新数据
        bool mIsWaitLoadingMoreData = false;                        //是否等待加载更多数据
        public int mTotalDataCount = 10000;                         //数据个数

        static DataSourceMgr instance = null;

        public static DataSourceMgr Get//单例
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindObjectOfType<DataSourceMgr>();
                }
                return instance;
            }

        }

        void Awake()
        {
            Init();
        }


        public void Init()
        {
            //刷新数据
            DoRefreshDataSource();
        }


        //根据索引得到ItenmDate
        public ItemData GetItemDataByIndex(int index)
        {
            if (index < 0 || index >= mItemDataList.Count)
            {
                return null;
            }
            return mItemDataList[index];
        }

        //根据ID得到ItenmDate
        public ItemData GetItemDataById(int itemId)
        {
            int count = mItemDataList.Count;
            for (int i = 0; i < count; ++i)
            {
                if(mItemDataList[i].mId == itemId)
                {
                    return mItemDataList[i];
                }
            }
            return null;
        }


        //得到变得数量
        public int TotalItemCount
        {
            get
            {
                return mItemDataList.Count;
            }
        }

        //刷新数据
        public void RequestRefreshDataList(System.Action onReflushFinished)
        {
            mDataRefreshLeftTime = 1;
            mOnRefreshFinished = onReflushFinished;
            mIsWaittingRefreshData = true;
        }

        //加载更多数据
        public void RequestLoadMoreDataList(int loadCount,System.Action onLoadMoreFinished)
        {
            mLoadMoreCount = loadCount;
            mDataLoadLeftTime = 1;
            mOnLoadMoreFinished = onLoadMoreFinished;
            mIsWaitLoadingMoreData = true;
        }

        public void Update()
        {
            if (mIsWaittingRefreshData)
            {
                //
                mDataRefreshLeftTime -= Time.deltaTime;
                if (mDataRefreshLeftTime <= 0)
                {
                    mIsWaittingRefreshData = false;
                    DoRefreshDataSource();      //重新刷新10000个数据
                    if (mOnRefreshFinished != null)
                    {
                        mOnRefreshFinished();
                    }
                }
            }
            if (mIsWaitLoadingMoreData)
            {
                mDataLoadLeftTime -= Time.deltaTime;
                if (mDataLoadLeftTime <= 0)
                {
                    mIsWaitLoadingMoreData = false;
                    DoLoadMoreDataSource();//加载更多数据
                    if (mOnLoadMoreFinished != null)
                    {
                        mOnLoadMoreFinished();
                    }
                }
            }

        }

        //重新定义list长度并且重新赋值
        public void SetDataTotalCount(int count)
        {
            mTotalDataCount = count;//重新赋值数据个数
            DoRefreshDataSource();//得到当前个数的数据
        }

        //交换数据
        public void ExchangeData(int index1,int index2)
        {
            ItemData tData1 = mItemDataList[index1];
            ItemData tData2 = mItemDataList[index2];
            mItemDataList[index1] = tData2;
            mItemDataList[index2] = tData1;
        }

        //移除索引为index的数据
        public void RemoveData(int index)
        {
            mItemDataList.RemoveAt(index);
        }

        //插入数据   在Index位置处  插入data
        public void InsertData(int index,ItemData data)
        {
            mItemDataList.Insert(index,data);
        }

        //得到10000个数据
        void DoRefreshDataSource()
        {
            mItemDataList.Clear();
            for (int i = 0; i < mTotalDataCount; ++i)
            {
                ItemData tData = new ItemData();
                tData.mId = i;
                tData.mName = "Item" + i;
                tData.mDesc = "Item Desc For Item " + i;
                tData.mIcon = ResManager.Get.GetSpriteNameByIndex(Random.Range(0, 24));
                tData.mStarCount = Random.Range(0, 6);
                tData.mFileSize = Random.Range(20, 999);
                tData.mChecked = false;
                tData.mIsExpand = false;
                mItemDataList.Add(tData);
            }
        }

        //加载更多的数据
        void DoLoadMoreDataSource()
        {
            int count = mItemDataList.Count;
            for (int k = 0; k < mLoadMoreCount; ++k)
            {
                int i = k + count;
                ItemData tData = new ItemData();
                tData.mId = i;
                tData.mName = "Item" + i;
                tData.mDesc = "Item Desc For Item " + i;
                tData.mIcon = ResManager.Get.GetSpriteNameByIndex(Random.Range(0, 24));
                tData.mStarCount = Random.Range(0, 6);
                tData.mFileSize = Random.Range(20, 999);
                tData.mChecked = false;
                tData.mIsExpand = false;
                mItemDataList.Add(tData);
            }
            mTotalDataCount = mItemDataList.Count;
        }

        //设置所i有的为点击
        public void CheckAllItem()
        {
            int count = mItemDataList.Count;
            for (int i = 0; i < count; ++i)
            {
                mItemDataList[i].mChecked = true;
            }
        }
        
        //设置所有的为没点击
        public void UnCheckAllItem()
        {
            int count = mItemDataList.Count;
            for (int i = 0; i < count; ++i)
            {
                mItemDataList[i].mChecked = false;
            }
        }

        //删除所有点击的数据
        public bool DeleteAllCheckedItem()
        {
            int oldCount = mItemDataList.Count;
            mItemDataList.RemoveAll(it => it.mChecked);//mCkecked为ture的数据全部删除
            return (oldCount != mItemDataList.Count);
        }

    }

}