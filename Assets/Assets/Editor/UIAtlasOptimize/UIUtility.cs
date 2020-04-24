// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/04 08:27:55
// FileName：Assets/Assets/Editor/UIAtlasOptimize/UIUtility.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using System.Drawing;

public static class UIUtility
{
    public const string PATH_S = @"/";
    public const string PATH_ASSETS = @"Assets";
    public const string PATH_UI = @"Assets/GameData/UGUI";
    public const string PATH_UI_STATIC = PATH_S_UI + @"StaticUI";
    public const string PATH_UI_STATIC_COMMON = PATH_S_UI_STATIC + @"Common";
    public const string PATH_UI_ATLAS = PATH_S_UI + @"Texture";
    public const string PATH_UI_NO_COMPRESS = PATH_S_UI + @"UnCompress";
    public const string PATH_UI_TEST = PATH_S_UI + @"Test";
    public const string PATH_BACKGROUND_UI = PATH_S_UI + "Background/Scene";
    public const string PATH_UI_PREFAB = @"Assets/GameData/Prefabs/UGUI";
    public const string PATH_BACKGROUND_SCENE = PATH_S_UI + @"Background/Scene";
    public const string PATH_BUILD_IN_ASSETS = @"Assets/GameData/UnityAssetsBuiltin";



    public const string PATH_S_UI = PATH_UI + PATH_S;
    public const string PATH_S_UI_STATIC = PATH_UI_STATIC + PATH_S;
    public const string PATH_S_BUILD_IN_ASSETS = PATH_BUILD_IN_ASSETS + PATH_S;
    public const string PATH_S_UI_PREFAB = PATH_UI_PREFAB + PATH_S;


    public const string FILE_PREFAB_RELATION = @"PrefabRelation.csv";
    public const string PREFIX_PREFAB_RELATION_WINDOW = "[window]";
    public const string PREFIX_PREFAB_RELATION_SUB = "[sub]";
    public const string PREFIX_PREFAB_RELATION_SUB_FLODER = "[folder]";
    public const string PREFIX_PREFAB_RELATION_DYNAMIC = "[dynamic]";

    public const string PLATFORM_ANDROID = "Android";
    public const string PLATFORM_IPHONE = "iPhone";
    public const string PLATFORM_STANDALONE = "Standalone";


    public const int TEXTURE_MAX_SIZE = 2048;
    public const int TEXTURE_SIZE_NO_ATLAS = 512;

    public const int PIXEL_THRESHOLD_REPEAT = 64;
    public const int PIXEL_THRESHOLD_REPEAT_CUT = 8;
    public const int PIXEL_THRESHOLD_LOOP = 4;
    public const int PIXEL_THRESHOLD_SUMMETRY = 512;

    public readonly static bool Verbose = false;

    /// <summary>
    /// 获取所有贴图
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static List<string> GetAllTexture(string path)
    {
        return AssetDatabase.FindAssets("t:texture", new string[] { path }).Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToList(); 
    }


    public static List<string> GetAllPrefab(string path = PATH_UI_PREFAB)
    {
        return AssetDatabase.FindAssets("t:prefab", new string[] { path }).Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToList(); 
    }

    public static string FindSameTexture(string texture ,string inpath)
    {
        string sameTexture = null;
        if (Directory.Exists(inpath))
        {
            foreach (string file in GetAllTexture(inpath))
            {
                if (file != texture && IsSameTexture(file, texture))
                {
                    sameTexture = file;
                    break;
                }
            }
        }
        return sameTexture;
    }


    public static List<string> FindAllSameTexture(string texture,string inPath = PATH_UI_STATIC,bool includeSelf = false)
    {
        List<string> sameTextureList = new List<string>();
        if (includeSelf)
        {
            sameTextureList.Add(texture);
        }

        foreach(string file in GetAllTexture(inPath))
        {
            if(file != null && IsSameTexture(file, texture))
            {
                sameTextureList.Add(file);
            }
        }

        return sameTextureList;
    }

    /// <summary>
    /// 获取图片原始尺寸
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    public static int[] GetTextureSize(string texture)
    {
        int[] size = new int[2];

        TextureImporter importer = AssetImporter.GetAtPath(texture) as TextureImporter;
        if (importer == null)
        {
            Debug.LogErrorFormat("[UIUtility.GetTextureSize]{0} importer为空", texture);
            return size;
        }

        object[] args = new object[2] { 0, 0 };
        MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
        mi.Invoke(importer, args);
        size[0] = (int)args[0];
        size[1] = (int)args[1];

        return size;
    }

    /// <summary>
    /// 获取图片压缩后尺寸
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    public static int[] GetTextureCompressedSize(string texture)
    {
        return GetTextureCompressedSize(AssetDatabase.LoadAssetAtPath<Texture2D>(texture));
    }

    public static int[] GetTextureCompressedSize(Texture2D texture)
    {
        return new int[] { texture.width, texture.height };
    }


    public static bool IsTextureCompressResized(string texture,List<string> platformList = null)
    {
        bool resized = false;
        if(platformList == null)
        {
            platformList = new List<string>() { PLATFORM_ANDROID, PLATFORM_IPHONE };
        }

        TextureImporter importer = AssetImporter.GetAtPath(texture) as TextureImporter;
        int[] size = GetTextureSize(texture);
        TextureImporterPlatformSettings settings = null;

        foreach(string platform in platformList)
        {
            settings = importer.GetPlatformTextureSettings(platform);
            if (size[0] > settings.maxTextureSize || size[1] > settings.maxTextureSize)
            {
                resized = true;
                break;
            }
        }
        return resized;
    }

    /// <summary>
    /// 不打图集文件夹
    /// </summary>
    private static List<string> DontPackingAtlasFolders = new List<string>()
    {

    };

    /// <summary>
    /// 限制尺寸打图集文件夹
    /// </summary>
    private static Dictionary<string, int> DontPackingAtlasSizeLimit = new Dictionary<string, int>()
    {
        //{"",1024 },
        //{ "",100}
    };

    private static List<string> selfManagerFolders = new List<string>()
    {

    };


    public static bool IsTextureShouldAtlas(string texture)
    {
        //如果不在GameData/UGUI下，则不是UI资源，返回false
        if (!texture.StartsWith(PATH_S_UI))
        {
            return false;
        }

        //检查是否是不打图集的文件夹
        if (DontPackingAtlasFolder(texture))
        {
            return false;
        }

        //检查图片大小，过大的图片不打图集以防图集太大
        int[] size = GetTextureSize(texture);
        int limitSize = GetDontPackingAtlasLimit(texture);
        return size[0] <= limitSize && size[1] <= limitSize;
    }


    private static bool DontPackingAtlasFolder(string path)
    {
        foreach(var folderPath in DontPackingAtlasFolders)
        {
            if (path.StartsWith(folderPath))
            {
                return true;
            }
        }
        return false;
    }


    private static int GetDontPackingAtlasLimit(string path)
    {
        foreach(var pair in DontPackingAtlasSizeLimit)
        {
            if (path.StartsWith(pair.Key))
            {
                return pair.Value;
            }
        }

        return TEXTURE_SIZE_NO_ATLAS;
    }

    public static bool IsSelfManagerAtlas(string texture)
    {
        foreach(var folderPath in selfManagerFolders)
        {
            if (texture.StartsWith(folderPath)) return true;
        }
        return false;
    }



    public static bool IsTextureShouldMerge(string texture ,int referenceNumber)
    {
        int[] size = GetTextureCompressedSize(texture);
        float threshold = -1;
        if (IsTextureShouldAtlas(texture))
        {
            threshold = referenceNumber * 250 - Mathf.Sqrt(size[0] * size[1]) - 700;
        }
        else
        {
            threshold = referenceNumber - 2;
        }
        return threshold > 0;
    }

    public static bool IsTextureShouldMerge(string texture,out List<string> sameTextureList,out List<string> referencePrefabList,Dictionary<string,string> prefabRelationDict = null)
    {
        sameTextureList = FindAllSameTexture(texture);
        List<string> allTextureList = new List<string>(sameTextureList)
        {
            texture
        };

        referencePrefabList = FindReferencePrefab(allTextureList, PATH_UI_PREFAB, prefabRelationDict);
        return IsTextureShouldMerge(texture, referencePrefabList.Count);
    }

    public static bool SaveTextureTo(Texture2D obj,string path)
    {
        byte[] bytes = obj.EncodeToPNG();
        if (File.Exists(path))
        {
            FileStream file = File.Open(path, FileMode.Open);
            BinaryWriter writer = new BinaryWriter(file);
            writer.Write(bytes);
            writer.Close();
            file.Close();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 判断两张图片是否是相同的图片，返回true的情况下耗时较长
    /// 一张1920*1080的图片大概耗时一秒
    /// </summary>
    /// <param name="textureA"></param>
    /// <param name="textureB"></param>
    /// <returns></returns>
    public static bool IsSameTexture(string textureA,string textureB)
    {
        if(textureA == textureB)
        {
            return true;
        }

        Bitmap bmpA = null;
        Bitmap bmpB = null;

        try
        {
            bmpA = new Bitmap(textureA);
        }
        catch(System.Exception e)
        {
            if(Verbose)
            {
                Debug.LogWarningFormat("[UIUtility.IsSameTexture]图片{0}解析错误，不做判断。错误信息：{1}", textureA, e);
            }
            return false;
        }

        try
        {
            bmpB = new Bitmap(textureB);
        }
        catch (System.Exception e)
        {
            if (Verbose)
            {
                Debug.LogWarningFormat("[UIUtility.IsSameTexture]图片{0}解析错误，不做判断。错误信息：{1}", textureB, e);
            }
            bmpA.Dispose();
            return false;
        }
        bool isSame = false;

        for(int i=0;i<bmpA.Width; i++)
        {
            for(int j = 0; j < bmpA.Height; j++)
            {
                if (bmpA.GetPixel(i, j) != bmpB.GetPixel(i, j))
                {
                    isSame = false;
                    break;
                }
            }
        }

        bmpB.Dispose();
        bmpA.Dispose();

        return isSame;
    }

    #region 依赖处理

    /// <summary>
    /// 返回一个asset引用到的资源
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="type"></param>
    /// <param name="deepReference"></param>
    /// <returns></returns>
    public static List<string> GetReference(string asset,System.Type type = null,bool deepReference = false)
    {
        List<string> referenceList = AssetDatabase.GetDependencies(asset, deepReference).ToList();

        if(type != null)
        {
            referenceList = referenceList.Where(dependency => AssetDatabase.GetMainAssetTypeAtPath(dependency) == type).ToList();

        }
        return referenceList;

    }


    public static List<string> FindReferencePrefab(string asset, string inPath = PATH_UI_PREFAB, Dictionary<string, string> prefabRelationDict = null)
    {
        return FindReferencePrefab(new List<string> { asset }, inPath, prefabRelationDict);
    }

    public static List<string> FindReferencePrefab(List<string> asset, string inPath = PATH_UI_PREFAB, Dictionary<string, string> prefabRelationDict = null)
    {
        List<string> prefabList = new List<string>();
        foreach(string prefab in GetAllPrefab(inPath))
        {
            string addPrefab = prefab;
            if (prefabRelationDict != null && prefabRelationDict.ContainsKey(prefab))
            {
                addPrefab = prefabRelationDict[prefab];
            }

            if (!prefabList.Contains(addPrefab))
            {
                string[] dependencies = AssetDatabase.GetDependencies(prefab, false);
                bool referenced = false;
                foreach(string dependency in dependencies)
                {
                    if (asset.Contains(dependency))
                    {
                        referenced = true;
                        break;
                    }
                }
                if (referenced)
                {
                    prefabList.Add(addPrefab);
                }
            }
            
        }
        return prefabList;
    }


    public static Dictionary<string,List<string>> FindReferencedPrefabByGroup(List<string> assetList,string inPath = PATH_UI_PREFAB, Dictionary<string, string> prefabRelationDict = null)
    {
        Dictionary<string, List<string>> prefabListDict = new Dictionary<string, List<string>>();

        foreach(string asset in assetList)
        {
            prefabListDict.Add(asset, new List<string>());
        }

        foreach(string prefab in GetAllPrefab(inPath))
        {
            string addPrefab = prefab;
            if(prefabRelationDict != null && prefabRelationDict.ContainsKey(prefab))
            {
                addPrefab = prefabRelationDict[prefab];
            }

            string[] dependencies = AssetDatabase.GetDependencies(prefab, false);
            foreach(string dependency in dependencies)
            {
                if (assetList.Contains(dependency) && !prefabListDict[dependency].Contains(addPrefab))
                {
                    prefabListDict[dependency].Add(addPrefab);
                }
            }
        }
        return prefabListDict;
    }


    public static bool ChangeReference(string prefab,string oldReference,string newReference)
    {
        bool changed = false;

        if (!File.Exists(oldReference))
        {
            Debug.LogErrorFormat("[UIUtility.ChangeReference]修改引用出错，文件{0}不存在", oldReference);
            return false;
        }

        if (!File.Exists(newReference))
        {
            Debug.LogErrorFormat("[UIUtility.ChangeReference]修改引用出错，文件{0}不存在", newReference);
            return false;
        }

        string oldGuid = AssetDatabase.AssetPathToGUID(oldReference);
        string newGuid = AssetDatabase.AssetPathToGUID(newReference);
        string fullPrefabPath = Path.GetFullPath(prefab);

        FileStream fileStream = new FileStream(fullPrefabPath, FileMode.Open, FileAccess.ReadWrite);
        StreamReader sr = new StreamReader(fileStream);
        string bufStr = sr.ReadToEnd();

        if (bufStr.Contains(oldGuid))
        {
            fileStream.SetLength(0);
            bufStr = bufStr.Replace(oldGuid, newGuid);
            changed = true;
            StreamWriter sw = new StreamWriter(fileStream);
            sw.Write(bufStr);
            sw.Flush();
            sw.Close();
        }

        sr.Close();
        fileStream.Close();
        return changed;

    }

    public static void LoadPrefabRelation(out Dictionary<string,string> relationDict,out Dictionary<string ,List<string>> relationFolderDict,out Dictionary<string,List<string>> dynamicUIPathDict)
    {
        relationDict = new Dictionary<string, string>();
        relationFolderDict = new Dictionary<string, List<string>>();
        dynamicUIPathDict = new Dictionary<string, List<string>>();
        StreamReader sr = File.OpenText(PATH_UI_PREFAB + PATH_S + FILE_PREFAB_RELATION);
        string line = sr.ReadLine();

        while(line!= null)
        {
            string[] array = ArrayFromCSV(line);
            string Window = null;

            List<string> subList = new List<string>();
            List<string> folderList = new List<string>();
            List<string> dynamicList = new List<string>();

            for(int i = 0; i < array.Length; i++)
            {
                string item = array[i];
                string itemLower = array[i].ToLower();
                if (itemLower.StartsWith(PREFIX_PREFAB_RELATION_WINDOW))
                {
                    if (Window != null)
                    {
                        Debug.LogErrorFormat("[UIUtility.LoadPrefabRelation] PrefabRelation.csv 非法数据，一行数据找到多个window：{0}", item);
                        continue;
                    }
                    else
                    {
                        Window = item.Substring(PREFIX_PREFAB_RELATION_WINDOW.Length);

                    }
                }
                else if (itemLower.StartsWith(PREFIX_PREFAB_RELATION_SUB))
                {
                    subList.Add(item.Substring(PREFIX_PREFAB_RELATION_SUB.Length));
                }
                else if (itemLower.StartsWith(PREFIX_PREFAB_RELATION_SUB_FLODER))
                {
                    folderList.Add(item.Substring(PREFIX_PREFAB_RELATION_SUB_FLODER.Length));
                }
                else if (itemLower.StartsWith(PREFIX_PREFAB_RELATION_DYNAMIC))
                {
                    dynamicList.Add(item.Substring(PREFIX_PREFAB_RELATION_DYNAMIC.Length));
                }
            }

            if (Window == null)
            {
                continue;
            }

            foreach(string sub in subList)
            {
                if (relationDict.ContainsKey(sub))
                {

                }
                else
                {
                    relationDict.Add(sub, Window);
                }
            }

            foreach(string folder in folderList)
            {
                if (!relationFolderDict.ContainsKey(Window))
                {
                    relationFolderDict.Add(Window, new List<string>());
                }

                if (relationFolderDict[Window].Contains(folder))
                {

                }
                else
                {
                    relationFolderDict[Window].Add(folder);
                }
            }

            foreach(string dynamic in dynamicList)
            {
                if (!dynamicUIPathDict.ContainsKey(Window))
                {
                    dynamicUIPathDict.Add(Window,new List<string>());
                }
                if (dynamicUIPathDict[Window].Contains(dynamic))
                {

                }
                else
                {
                    dynamicUIPathDict[Window].Add(dynamic);
                }
            }
            line = sr.ReadLine();

        }
        sr.Close();
    }

    public static string[] ArrayFromCSV(string str)
    {
        return str.Split(new[]{','}).Select(value => value.Trim()).ToArray();
    }


    public static void SavePrefabRelation(Dictionary<string,string> relationDict,Dictionary<string,List<string>> relationFolderDict, Dictionary<string, List<string>> dynamicUIPathDict)
    {

        Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();

        foreach(string subPrefab in relationDict.Keys)
        {
            if (!dict.ContainsKey(relationDict[subPrefab]))
            {
                dict.Add(relationDict[subPrefab], new List<string>());
            }
            dict[relationDict[subPrefab]].Add(PREFIX_PREFAB_RELATION_SUB + subPrefab);
        }

        foreach(KeyValuePair<string,List<string>> pair in relationFolderDict)
        {
            if (pair.Value.Count > 0)
            {
                if (!dict.ContainsKey(pair.Key))
                {
                    dict.Add(pair.Key, new List<string>());
                }
                foreach(string relationFolder in pair.Value)
                {
                    dict[pair.Key].Add(PREFIX_PREFAB_RELATION_SUB_FLODER + relationFolder);
                }
            }
        }


        foreach(KeyValuePair<string,List<string>> pair in dynamicUIPathDict)
        {
            if (pair.Value.Count > 0)
            {
                if (!dict.ContainsKey(pair.Key))
                {
                    dict.Add(pair.Key, new List<string>());
                }

                foreach(string dynamicUIPath in pair.Value)
                {
                    dict[pair.Key].Add(PREFIX_PREFAB_RELATION_DYNAMIC + dynamicUIPath);
                }
            }
        }

        StreamWriter sw = new StreamWriter(PATH_UI_PREFAB + PATH_S + FILE_PREFAB_RELATION, false);
        foreach(string key in dict.Keys)
        {
            dict[key].Sort();
            string line = PREFIX_PREFAB_RELATION_WINDOW + key + ", " + dict[key].ToArray();
            sw.Write(line);
        }
        sw.Close();
    }

    #endregion


    #region  像素处理

    public static int TexturePixelCheck(string texture, out int[,] repeat, out int[] loop, out bool[] summetry, bool repeatStrict = false, bool efficiently = false)
    {
        int result = 0;
        Bitmap bmp = new Bitmap(texture);
        repeat = new int[2, 2];
        loop = new int[2];
        summetry = new bool[] { true,true };

        int[,] maxIndex = new int[,] { { bmp.Width, bmp.Height }, { bmp.Width, bmp.Height } };
        int[] maxloopIndex = new int[] { Mathf.FloorToInt(bmp.Width / (float)PIXEL_THRESHOLD_LOOP), Mathf.FloorToInt(bmp.Height / (float)PIXEL_THRESHOLD_LOOP) };
        int[] halfIndex = new int[] { bmp.Width / 2, bmp.Height / 2 };

        if (efficiently)
        {
            summetry[0] = bmp.Width > PIXEL_THRESHOLD_SUMMETRY;
            summetry[1] = bmp.Height > PIXEL_THRESHOLD_SUMMETRY;
        }

        for(int i = 0; i < 2; i++)
        {
            int repeatIndex = 0;
            int repeatNumber = 0;
            bool isRepeat = false;
            bool isLoop = false;


            for(int j = 0; j < maxIndex[i, 1]; j++)
            {
                if (j > 0)
                {
                    isRepeat = true;
                    if (!efficiently || j < maxloopIndex[i])
                    {
                        isLoop = true;
                    }
                }

                for(int k = 0; k < maxIndex[i, 1]; k++)
                {
                    System.Drawing.Color pixel = GetPixel(bmp,i, j, k);
                    if(isRepeat && !PixelEquals(pixel, GetPixel(bmp,i, repeatIndex, k)))
                    {
                        isRepeat = false;
                    }

                    if (isLoop)
                    {
                        int loopIndex = 0;
                        if (loop[i] > 0)
                        {
                            loopIndex = j - loop[i];
                        }
                        if (!PixelEquals(pixel, GetPixel(bmp, i, loopIndex, k)))
                        {
                            isLoop = false;
                        }
                    }

                    if(summetry[i] && j>halfIndex[i]&& !PixelEquals(pixel, GetPixel(bmp, i, maxIndex[i, 0] - j - 1, k)))
                    {
                        summetry[i] = false;
                    }
                       
                }

                if (isRepeat)
                {
                    repeatNumber++;
                }
                else
                {
                    if (repeatNumber > repeat[i, 1])
                    {
                        repeat[i, 0] = repeatIndex;
                        repeat[i, 1] = repeatNumber;
                    }

                    repeatNumber = 0;
                    repeatIndex = j;
                }

                if (isLoop)
                {
                    loop[i] = j;
                }
                else
                {
                    loop[i] = 0;
                }
            }

            if (repeatNumber > repeat[i, 1])
            {
                repeat[i, 0] = repeatIndex;
                repeat[i, 1] = repeatNumber;
            }

        }

        if (repeatStrict && (repeat[0, 1] > PIXEL_THRESHOLD_REPEAT_CUT || repeat[1, 1] > PIXEL_THRESHOLD_REPEAT_CUT || repeat[0, 1] > PIXEL_THRESHOLD_REPEAT || repeat[1, 1] > PIXEL_THRESHOLD_REPEAT))
        {
            result |= 1;
        }
        if ((loop[0] > 0 && loop[0] * PIXEL_THRESHOLD_LOOP < bmp.Width) || (loop[1] > 0 && loop[1] * PIXEL_THRESHOLD_LOOP < bmp.Height))
        {
            result |= 2;
        }
        if (summetry[0] || summetry[1])
        {
            result |= 4;
        }
        bmp.Dispose();
        return result;

    }

    private static System.Drawing.Color GetPixel(this Bitmap bmp,int axis,int index0 ,int index1)
    {
        return axis == 0 ? bmp.GetPixel(index0, index1) : bmp.GetPixel(index1,index0);
    }

    private static bool PixelEquals(System.Drawing.Color a,System.Drawing.Color b)
    {
        return a == b || a.A == 0 && b.A == 0;
    }


    public static UnityEngine.Color ToUnityColor(this System.Drawing.Color color)
    {
        float byteMax = byte.MaxValue;
        return new UnityEngine.Color(color.R / byteMax, color.G / byteMax, color.B / byteMax,color.A / byteMax);
    }


    public static Texture2D TextureCut(string texture ,out int[,] repeat,bool repeatStrict = false)
    {
        Texture2D texture2D = null;
        repeat = new int[2, 2];
        int[,] oldRepeat = new int[2, 2];
        int[] loop = new int[2];
        bool[] summetry = new bool[2];

        int checkResult = TexturePixelCheck(texture, out oldRepeat, out loop, out summetry, repeatStrict, true);
        if ((checkResult & 1) > 0)
        {
            int[] oldSize = GetTextureSize(texture);
            int[] size = new int[2];
            for(int i = 0; i < 2; i++)
            {
                repeat[i, 0] = oldRepeat[i, 0];
                repeat[i, 1] = Mathf.Min(oldRepeat[i, 1], PIXEL_THRESHOLD_REPEAT_CUT - 1);
                size[i] = oldSize[i] + repeat[i, 1] - oldRepeat[i, 1];
            }

            Bitmap bmp = new Bitmap(texture);
            texture2D = new Texture2D(size[0], size[1]);
            int oldX = 0;
            int x = 0;
            while (x < size[0])
            {
                int oldY = oldSize[1] - 1;
                int y = 0;
                while (y < size[1])
                {
                    texture2D.SetPixel(x, y, ToUnityColor(bmp.GetPixel(oldX, oldY)));
                    if (y == size[1] - repeat[1, 0] - 1)
                    {
                        oldY = oldRepeat[1, 0];
                    }
                    if (y < size[1] - repeat[1, 0] - repeat[1, 1] - 1 || y >= size[1] - repeat[1, 0] - 1)
                    {
                        oldY--;
                    }
                    y++;
                }

                if (x == repeat[0, 0] + repeat[0, 1])
                {
                    oldX = oldRepeat[0, 0] + oldRepeat[0, 1];
                }

                if (x < repeat[0, 0] || x >= repeat[0, 0] + repeat[0, 1])
                {
                    oldX++;
                }

                x++;
            }
            texture2D.Apply();
            bmp.Dispose();
        }

        return texture2D;
    }

    public static Texture2D TextureMaskRepeat(string texture,int[,] repeat)
    {
        Bitmap bmp = new Bitmap(texture);
        Texture2D markedobj = new Texture2D(bmp.Width, bmp.Height);
        for(int i = 0; i < bmp.Width; i++)
        {
            for(int j = 0; j < bmp.Height; j++)
            {
                markedobj.SetPixel(i, bmp.Height - j - i, ToUnityColor(bmp.GetPixel(i, j)));
            }
        }

        if (repeat[0, 1] > 0)
        {
            for(int i = 0; i < bmp.Height; i++)
            {
                markedobj.SetPixel(i, bmp.Height - repeat[1, 0] - 1, UnityEngine.Color.red);
                markedobj.SetPixel(i, bmp.Height - repeat[1, 0] - repeat[1, 1] - 1, UnityEngine.Color.red);
            }
        }
        bmp.Dispose();
        markedobj.Apply();
        return markedobj;
    }

    public static Texture2D TextureMarkLoop(string texture ,int[] loop)
    {
        Bitmap bmp = new Bitmap(texture);
        Texture2D markedobj = new Texture2D(bmp.Width, bmp.Height);
        for (int i = 0; i < bmp.Width; i++)
        {
            for (int j = 0; j < bmp.Height; j++)
            {
                markedobj.SetPixel(i, bmp.Height - j - i, ToUnityColor(bmp.GetPixel(i, j)));
            }
        }

        if (loop[0] > 0)
        {
            for (int i = 0; i < bmp.Height; i++)
            {
                markedobj.SetPixel(0, i, UnityEngine.Color.red);
                markedobj.SetPixel(loop[0], i, UnityEngine.Color.red);
            }
        }
        bmp.Dispose();
        markedobj.Apply();
        return markedobj;
    }

    public static Texture2D TextureMaskRepeat(Texture2D obj, int[,] repeat)
    {

        Texture2D markedobj = new Texture2D(obj.width, obj.height);
        for (int i = 0; i < obj.width; i++)
        {
            for (int j = 0; j < obj.height; j++)
            {
                markedobj.SetPixel(i, j, obj.GetPixel(i, j));
            }
        }

        if (repeat[0, 1] > 0)
        {
            for (int i = 0; i < obj.height; i++)
            {
                markedobj.SetPixel(repeat[0, 0], i, UnityEngine.Color.red);
                markedobj.SetPixel(repeat[0, 0] + repeat[0, 1], i, UnityEngine.Color.red);
            }
        }

        if (repeat[1, 1] > 0)
        {
            for(int i = 0; i < obj.width; i++)
            {
                markedobj.SetPixel(i, obj.height - repeat[1, 0] - 1, UnityEngine.Color.red);
                markedobj.SetPixel(1, obj.height - repeat[1, 0] - repeat[1, 1] - 1, UnityEngine.Color.red);
            }
        }
        markedobj.Apply();
        return markedobj;
    }


    #endregion



    #region 路径处理

    public static string UniquePath(string path,List<string> excludeList = null)
    {
        string directory = Path.GetDirectoryName(path);
        string extension = Path.GetExtension(path);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        int duplicatePrefixNUmber = 0;

        do
        {
            duplicatePrefixNUmber++;
            path = directory + PATH_S + fileNameWithoutExtension + duplicatePrefixNUmber + extension;
        }
        while (File.Exists(path) || (excludeList != null && excludeList.Contains(path)));

        return path;
    }

    public static string UniteSlash(string path)
    {
        return path.Replace('\\', '/');
    }

    public static string FullPathToRelativePath(string fullPath)
    {
        return fullPath.Substring(fullPath.IndexOf(PATH_ASSETS));
    }

    public static string RelativePathToFullPath(string relativePath)
    {
        return Application.dataPath + relativePath.Replace(PATH_ASSETS, string.Empty);
    }

    #endregion

}
