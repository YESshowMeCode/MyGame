using System;
using System.Collections.Generic;
using UnityEngine;

//对象池
namespace SuperScrollView
{
    public class ItemPool
    {
        GameObject mPrefabObj;      //预设体物体
        string mPrefabName;         //预设体名称
        int mInitCreateCount = 1;   //初始化数量
        float mPadding = 0;
        float mStartPosOffset = 0;
        List<LoopListViewItem2> mTmpPooledItemList = new List<LoopListViewItem2>();
        List<LoopListViewItem2> mPooledItemList = new List<LoopListViewItem2>();
        static int mCurItemIdCount = 0;     //Item数量
        RectTransform mItemParent = null;   //父对象
        public ItemPool()
        {

        }
        public void Init(GameObject prefabObj, float padding, float startPosOffset, int createCount, RectTransform parent)
        {
            mPrefabObj = prefabObj;
            mPrefabName = mPrefabObj.name;
            mInitCreateCount = createCount;
            mPadding = padding;
            mStartPosOffset = startPosOffset;
            mItemParent = parent;
            mPrefabObj.SetActive(false);
            for (int i = 0; i < mInitCreateCount; ++i)
            {
                LoopListViewItem2 tViewItem = CreateItem();
                RecycleItemReal(tViewItem);
            }
        }

        //得到Item
        public LoopListViewItem2 GetItem()
        {
            mCurItemIdCount++;
            LoopListViewItem2 tItem = null;
            if (mTmpPooledItemList.Count > 0)
            {//如果mTmpPooledItemList表中有数据从mTmpPooledItemList表中取出最后一个数据并且显示，并在表中删除此数据

                int count = mTmpPooledItemList.Count;
                tItem = mTmpPooledItemList[count - 1];
                mTmpPooledItemList.RemoveAt(count - 1);
                tItem.gameObject.SetActive(true);
            }
            else
            {//mTmpPooledItemList表中没有数据，则从mPooledItemList表中去除数据，如果mPooledItemList表中没有数据曾创建一个
                int count = mPooledItemList.Count;
                if (count == 0)
                {
                    tItem = CreateItem();
                }
                else
                {
                    tItem = mPooledItemList[count - 1];
                    mPooledItemList.RemoveAt(count - 1);
                    tItem.gameObject.SetActive(true);
                }
            }
            tItem.Padding = mPadding;
            tItem.ItemId = mCurItemIdCount;
            return tItem;

        }

        //删除所有的Item
        public void DestroyAllItem()
        {
            ClearTmpRecycledItem();
            int count = mPooledItemList.Count;
            for (int i = 0; i < count; ++i)
            {
                GameObject.DestroyImmediate(mPooledItemList[i].gameObject);
            }
            mPooledItemList.Clear();
        }
        //创键Item
        public LoopListViewItem2 CreateItem()
        {

            GameObject go = GameObject.Instantiate<GameObject>(mPrefabObj, Vector3.zero, Quaternion.identity, mItemParent);
            go.SetActive(true);
            RectTransform rf = go.GetComponent<RectTransform>();
            rf.localScale = Vector3.one;//位置
            rf.anchoredPosition3D = Vector3.zero;//锚点位置
            rf.localEulerAngles = Vector3.zero;//旋转欧拉角
            LoopListViewItem2 tViewItem = go.GetComponent<LoopListViewItem2>();
            tViewItem.ItemPrefabName = mPrefabName;
            tViewItem.StartPosOffset = mStartPosOffset;
            return tViewItem;
        }
        //回收项目到mPooledItemList表
        void RecycleItemReal(LoopListViewItem2 item)
        {
            item.gameObject.SetActive(false);
            mPooledItemList.Add(item);
        }
        //回收项目到TmpPooledItemList表
        public void RecycleItem(LoopListViewItem2 item)
        {
            mTmpPooledItemList.Add(item);
        }
        //清楚TemRecycledItem表
        public void ClearTmpRecycledItem()
        {
            int count = mTmpPooledItemList.Count;
            if (count == 0)
            {
                return;
            }
            for (int i = 0; i < count; ++i)
            {
                RecycleItemReal(mTmpPooledItemList[i]);
            }
            mTmpPooledItemList.Clear();
        }
    }
}
