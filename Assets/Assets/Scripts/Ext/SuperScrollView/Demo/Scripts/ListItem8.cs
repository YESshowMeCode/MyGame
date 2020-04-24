using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SuperScrollView
{
    public class ListItem8 : MonoBehaviour
    {
        public Text mNameText;              //名称
        public Image mIcon;                 //图片
        public Image[] mStarArray;          //星级
        public Text mDescText;              //KB大小          
        public GameObject mExpandContentRoot;//展开的内容
        public Text mClickTip;              //展开按钮的文字
        public Button mExpandBtn;           //展开按钮
        public Color32 mRedStarColor = new Color32(249, 227, 101, 255);     //黄色
        public Color32 mGrayStarColor = new Color32(215, 215, 215, 255);    //灰色
        int mItemDataIndex = -1;        //数据索引
        bool mIsExpand;     //是否扩大
        public void Init()
        {
            for (int i = 0; i < mStarArray.Length; ++i)
            {
                int index = i;
                ClickEventListener listener = ClickEventListener.Get(mStarArray[i].gameObject);//得到每一个星的点击事件
                listener.SetClickEventHandler(delegate (GameObject obj) { OnStarClicked(index); });//添加星级点击
            }

            mExpandBtn.onClick.AddListener( OnExpandBtnClicked );//展开按钮点击事件
        }

        //改变展开东西的状态
        public void OnExpandChanged()
        {
            RectTransform rt = gameObject.GetComponent<RectTransform>();
            if (mIsExpand)//为true展开
            {
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 284f);//改变RectTransform大小
                mExpandContentRoot.SetActive(true);
                mClickTip.text = "Shrink";
            }
            else
            {
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 143f);
                mExpandContentRoot.SetActive(false);
                mClickTip.text = "Expand";
            }

        }


        //展开按钮点击
        void OnExpandBtnClicked()
        {
            ItemData data = DataSourceMgr.Get.GetItemDataByIndex(mItemDataIndex);
            if (data == null)
            {
                return;
            }
            mIsExpand = !mIsExpand;
            data.mIsExpand = mIsExpand;
            OnExpandChanged();
            LoopListViewItem2 item2 = gameObject.GetComponent<LoopListViewItem2>();
            item2.ParentListView.OnItemSizeChanged(item2.ItemIndex);
        }


        //星点击处理
        void OnStarClicked(int index)
        {
            ItemData data = DataSourceMgr.Get.GetItemDataByIndex(mItemDataIndex);
            if (data == null)
            {
                return;
            }
            if (index == 0 && data.mStarCount == 1)
            {
                data.mStarCount = 0;
            }
            else
            {
                data.mStarCount = index + 1;
            }
            SetStarCount(data.mStarCount);
        }

        //改变星的颜色
        public void SetStarCount(int count)
        {
            int i = 0;
            for (; i < count; ++i)
            {
                mStarArray[i].color = mRedStarColor;
            }
            for (; i < mStarArray.Length; ++i)
            {
                mStarArray[i].color = mGrayStarColor;
            }
        }

        //设置数据
        public void SetItemData(ItemData itemData, int itemIndex)
        {
            mItemDataIndex = itemIndex;
            mNameText.text = itemData.mName;
            mDescText.text = itemData.mFileSize.ToString() + "KB";
            mIcon.sprite = ResManager.Get.GetSpriteByName(itemData.mIcon);
            SetStarCount(itemData.mStarCount);
            mIsExpand = itemData.mIsExpand;
            OnExpandChanged();
        }


    }
}
