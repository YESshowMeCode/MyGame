using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
//**********************************  图集管理  *******************************
namespace SuperScrollView
{
    public class ResManager : MonoBehaviour
    {
        public Sprite[] spriteObjArray;                             //Sprite集合
        // Use this for initialization
        static ResManager instance = null;

        string[] mWordList;

        Dictionary<string, Sprite> spriteObjDict = new Dictionary<string, Sprite>();    //图集字典

        public static ResManager Get                                //单例
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindObjectOfType<ResManager>();
                }
                return instance;
            }

        }

        //图集字典初始化    <图集名称,图集>
        void InitData()
        {
            spriteObjDict.Clear();
            foreach (Sprite sp in spriteObjArray)
            {
                spriteObjDict[sp.name] = sp;
            }
        }

        void Awake()
        {
            instance = null;
            InitData();
        }

        //根据名称得到对应的图集
        public Sprite GetSpriteByName(string spriteName)
        {
            Sprite ret = null;
            if (spriteObjDict.TryGetValue(spriteName, out ret))
            {
                return ret;
            }
            return null;
        }


        //在图集Length内得到随机图集名称
        public string GetRandomSpriteName()
        {
            int count = spriteObjArray.Length;
            int index = Random.Range(0, count);
            return spriteObjArray[index].name;
        }

        //得到图集数量
        public int SpriteCount
        {
            get
            {
                return spriteObjArray.Length;
            }
        }

        //根据索引得到对应的图集
        public Sprite GetSpriteByIndex(int index)
        {
            if (index < 0 || index >= spriteObjArray.Length)
            {
                return null;
            }
            return spriteObjArray[index];
        }

        //根据索引得到对应图集的名称
        public string GetSpriteNameByIndex(int index)
        {
            if (index < 0 || index >= spriteObjArray.Length)
            {
                return "";
            }
            return spriteObjArray[index].name;
        }
    }
}
