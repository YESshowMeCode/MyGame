using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SuperScrollView
{

    //Item组
    public class ItemSizeGroup
    {

        public float[] mItemSizeArray = null;//记录Item大小的数组
        public float[] mItemStartPosArray = null;//记录item开始位置的数组
        public int mItemCount = 0;  //组内Item数量
        int mDirtyBeginIndex = ItemPosMgr.mItemMaxCountPerGroup;//暗淡开始索引
        public float mGroupSize = 0;//当前组的总大小
        public float mGroupStartPos = 0;//当前组开始位置
        public float mGroupEndPos = 0;//当前组结束位置
        public int mGroupIndex = 0;//当前组在链表中的索引
        float mItemDefaultSize = 0;//组内每一个Item默认大小
        int mMaxNoZeroIndex = 0;//组内最大   大小不为0的Item的索引
        public ItemSizeGroup(int index,float itemDefaultSize)
        {
            mGroupIndex = index;
            mItemDefaultSize = itemDefaultSize;
            Init();
        }

        //初始化
        public void Init()
        {
            mItemSizeArray = new float[ItemPosMgr.mItemMaxCountPerGroup];//根据最大组数创建float数组
            if (mItemDefaultSize != 0)
            {
                for (int i = 0; i < mItemSizeArray.Length; ++i)
                {
                    mItemSizeArray[i] = mItemDefaultSize;
                }
            }
            mItemStartPosArray = new float[ItemPosMgr.mItemMaxCountPerGroup];//创建Item初始位置数组
            mItemStartPosArray[0] = 0;
            mItemCount = ItemPosMgr.mItemMaxCountPerGroup;//Item数量
            mGroupSize = mItemDefaultSize * mItemSizeArray.Length;//大小
            if (mItemDefaultSize != 0)//默认大小不等于0
            {
                mDirtyBeginIndex = 0;
            }
            else
            {
                mDirtyBeginIndex = ItemPosMgr.mItemMaxCountPerGroup;
            }
        }

        //得到Item开始位置
        public float GetItemStartPos(int index)
        {
            return mGroupStartPos + mItemStartPosArray[index];
        }

        //是否暗淡的
        public bool IsDirty
        {
            get
            {
                return (mDirtyBeginIndex < mItemCount);
            }
        }
        //根据索引index以及大小size更新当前组index索引出的数据
        public float SetItemSize(int index, float size)
        {
            if(index > mMaxNoZeroIndex && size > 0)
            {//如果Index大于当前组内不为零最大Item的索引，则把Index设置为当前组不为零最大Item的索引
                mMaxNoZeroIndex = index;
            }
            float old = mItemSizeArray[index];
            if (old == size)
            {
                return 0;
            }
            mItemSizeArray[index] = size;
            if (index < mDirtyBeginIndex)
            {//
                mDirtyBeginIndex = index;
            }
            float ds = size - old;
            mGroupSize = mGroupSize + ds;//更新当前组的大小
            return ds;
        }

        //设置Item数量
        public void SetItemCount(int count)
        {
            if(count < mMaxNoZeroIndex)
            {
                mMaxNoZeroIndex = count;
            }
            if (mItemCount == count)
            {
                return;
            }
            mItemCount = count;
            RecalcGroupSize();
        }

        //重载大小
        public void RecalcGroupSize()
        {
            mGroupSize = 0;
            for (int i = 0; i < mItemCount; ++i)
            {
                mGroupSize += mItemSizeArray[i];
            }
        }

        //根据位置得到Item索引
        public int GetItemIndexByPos(float pos)
        {
            if (mItemCount == 0)
            {
                return -1;
            }
            
            int low = 0;
            int high = mItemCount - 1;
            if (mItemDefaultSize == 0f)
            {
                if(mMaxNoZeroIndex < 0)
                {
                    mMaxNoZeroIndex = 0;
                }
                high = mMaxNoZeroIndex;
            }
            while (low <= high)//折半查找根据位置坐标求Item索引
            {
                int mid = (low + high) / 2;
                float startPos = mItemStartPosArray[mid];
                float endPos = startPos + mItemSizeArray[mid];
                if (startPos <= pos && endPos >= pos)
                {
                    return mid;
                }
                else if (pos > endPos)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }
            return -1;
        }

        //更新所有的Item开始坐标
        public void UpdateAllItemStartPos()
        {
            if (mDirtyBeginIndex >= mItemCount)
            {
                return;
            }
            int startIndex = (mDirtyBeginIndex < 1) ? 1 : mDirtyBeginIndex;//更新的索引
            for (int i = startIndex; i < mItemCount; ++i)
            {
                mItemStartPosArray[i] = mItemStartPosArray[i - 1] + mItemSizeArray[i - 1];
            }
            mDirtyBeginIndex = mItemCount;
        }

        //删除之前的数据
        public void ClearOldData()
        {
            for (int i = mItemCount; i < ItemPosMgr.mItemMaxCountPerGroup; ++i)
            {
                mItemSizeArray[i] = 0;
            }
        }
    }

    //对Item组的管理
    public class ItemPosMgr
    {
        public const int mItemMaxCountPerGroup = 100;//每组里面Item数量上限
        List<ItemSizeGroup> mItemSizeGroupList = new List<ItemSizeGroup>();//项目大小列表
        int mDirtyBeginIndex = int.MaxValue;//开始暗淡的索引
        public float mTotalSize = 0;//项目中所有Item总大小
        public float mItemDefaultSize = 20;//item默认大小
        int mMaxNotEmptyGroupIndex = 0;//最大不为空的组的索引

        public ItemPosMgr(float itemDefaultSize)
        {
            mItemDefaultSize = itemDefaultSize;
        }

        //设置Item最大数量
        public void SetItemMaxCount(int maxCount)
        {
            mDirtyBeginIndex = 0;
            mTotalSize = 0;
            int st = maxCount % mItemMaxCountPerGroup;
            int lastGroupItemCount = st;//最后一组Itme计数
            int needMaxGroupCount = maxCount / mItemMaxCountPerGroup;//需要的最大组数
            if (st > 0)
            {
                needMaxGroupCount++;
            }
            else
            {
                lastGroupItemCount = mItemMaxCountPerGroup;
            }
            int count = mItemSizeGroupList.Count;
            if (count > needMaxGroupCount)
            {//如果组节点链表的数量大于所需要的数量，则把链表后面不需要的删除
                int d = count - needMaxGroupCount;
                mItemSizeGroupList.RemoveRange(needMaxGroupCount, d);
            }
            else if (count < needMaxGroupCount)
            {//如果组链表节点数量小于所需要的数量，则把已有链表中最后一个组的多余大小元素归零，并在链表中添加所需的组数的节点
                if(count > 0)
                {
                    mItemSizeGroupList[count - 1].ClearOldData();
                }
                int d = needMaxGroupCount - count;
                for (int i = 0; i < d; ++i)
                {
                    ItemSizeGroup tGroup = new ItemSizeGroup(count + i, mItemDefaultSize);
                    mItemSizeGroupList.Add(tGroup);
                }
            }
            else
            {
                //如果组链表的节点数量等于所需要的组数，则把最后一组的多余大小元素归零
                if (count > 0)
                {
                    mItemSizeGroupList[count - 1].ClearOldData();
                }
            }
            count = mItemSizeGroupList.Count;
            if((count-1) < mMaxNotEmptyGroupIndex)
            {//设置最大不为空节点的索引 
                mMaxNotEmptyGroupIndex = count - 1;
            }
            if(mMaxNotEmptyGroupIndex < 0)
            {
                mMaxNotEmptyGroupIndex = 0;
            }
            if (count == 0)
            {
                return;
            }
            for (int i = 0; i < count - 1; ++i)
            {
                //设置每组的Item的数量
                mItemSizeGroupList[i].SetItemCount(mItemMaxCountPerGroup);
            }
            mItemSizeGroupList[count - 1].SetItemCount(lastGroupItemCount);

            for (int i = 0; i < count; ++i)
            {
                //设置总大小
                mTotalSize = mTotalSize + mItemSizeGroupList[i].mGroupSize;
            }

        }

        //根据总Item索引设置size大小
        public void SetItemSize(int itemIndex, float size)
        {
            int groupIndex = itemIndex / mItemMaxCountPerGroup;//Item所在组的索引
            int indexInGroup = itemIndex % mItemMaxCountPerGroup;//在组内的索引
            ItemSizeGroup tGroup = mItemSizeGroupList[groupIndex];
            float changedSize = tGroup.SetItemSize(indexInGroup, size);//更新总索引为itemIndex出的数据-》size，并返回新的Item大小减去之前的大小的差值
            if (changedSize != 0f)
            {
                if (groupIndex < mDirtyBeginIndex)
                {//更新
                    mDirtyBeginIndex = groupIndex;
                }
            }
            mTotalSize += changedSize;//更新大小
            if(groupIndex > mMaxNotEmptyGroupIndex && size > 0)
            {
                //更新最大不为空的组在链表中的索引
                mMaxNotEmptyGroupIndex = groupIndex;
            }
        }

        //根据总索引得到Item的开始位置
        public float GetItemPos(int itemIndex)
        {
            Update(true);
            int groupIndex = itemIndex / mItemMaxCountPerGroup;
            int indexInGroup = itemIndex % mItemMaxCountPerGroup;
            return mItemSizeGroupList[groupIndex].GetItemStartPos(indexInGroup);
        }
        //二分查找根据位置得到索引得到位置
        public bool GetItemIndexAndPosAtGivenPos(float pos, ref int index, ref float itemPos)
        {
            Update(true);
            index = 0;
            itemPos = 0f;
            int count = mItemSizeGroupList.Count;
            if (count == 0)
            {
                return true;
            }
            ItemSizeGroup hitGroup = null;

            int low = 0;
            int high = count - 1;

            if (mItemDefaultSize == 0f)
            {
                if(mMaxNotEmptyGroupIndex < 0)
                {
                    mMaxNotEmptyGroupIndex = 0;
                }
                high = mMaxNotEmptyGroupIndex;
            }
            while (low <= high)//二分查找根据位置得到索引得到位置
            {
                int mid = (low + high) / 2;
                ItemSizeGroup tGroup = mItemSizeGroupList[mid];
                if (tGroup.mGroupStartPos <= pos && tGroup.mGroupEndPos >= pos)
                {
                    hitGroup = tGroup;
                    break;
                }
                else if (pos > tGroup.mGroupEndPos)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }
            int hitIndex = -1;
            if (hitGroup != null)
            {
                hitIndex = hitGroup.GetItemIndexByPos(pos - hitGroup.mGroupStartPos);
            }
            else
            {
                return false;
            }
            if (hitIndex < 0)
            {
                return false;
            }
            index = hitIndex + hitGroup.mGroupIndex * mItemMaxCountPerGroup;//索引 
            itemPos = hitGroup.GetItemStartPos(hitIndex);//坐标
            return true;
        }

        //从暗淡开始组更新每组的坐标
        public void Update(bool updateAll)
        {
            int count = mItemSizeGroupList.Count;
            if (count == 0)
            {
                return;
            }
            if (mDirtyBeginIndex >= count)
            {
                return;
            }
            int loopCount = 0;
            for (int i = mDirtyBeginIndex; i < count; ++i)
            {
                loopCount++;
                ItemSizeGroup tGroup = mItemSizeGroupList[i];
                mDirtyBeginIndex++;
                tGroup.UpdateAllItemStartPos();//更新当前组的所有Item坐标
                if (i == 0)
                {
                    tGroup.mGroupStartPos = 0;
                    tGroup.mGroupEndPos = tGroup.mGroupSize;
                }
                else
                {
                    tGroup.mGroupStartPos = mItemSizeGroupList[i - 1].mGroupEndPos;
                    tGroup.mGroupEndPos = tGroup.mGroupStartPos + tGroup.mGroupSize;
                }
                if (!updateAll && loopCount > 1)
                {
                    return;
                }

            }
        }

    }
}