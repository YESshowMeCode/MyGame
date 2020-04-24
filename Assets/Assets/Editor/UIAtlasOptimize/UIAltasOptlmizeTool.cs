// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/03 21:18:05
// FileName：Assets/Assets/Editor/UIAtlasOptimize/UIAltasOptlmizeTool.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using FreeImageAPI;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;


public class UIAltasOptlmizeTool {

	[MenuItem("Tools/图集工具/自定义批处理 %#1")]
    private static void Batch()
    {

    }

    [MenuItem("Assets/图集工具/打印引用到的资源/所有类型")]
    private static void PrintReferenceAll()
    {
        PrintReference();
    }

    [MenuItem("Assets/图集工具/打印引用到的资源/图片")]
    private static void PrintReferenceTexture()
    {
        PrintReference(typeof(Texture2D));
    }

    [MenuItem("Assets/图集工具/打印引用到的资源/Prefab")]
    private static void PrintReferencePrefab()
    {
        PrintReference(typeof(GameObject));
    }


    private static void PrintReference(System.Type type = null,bool deepDependence = false)
    {
        List<string> assetList = Selection.GetFiltered<Object>(SelectionMode.Deep).Select(asset => AssetDatabase.GetAssetPath(asset)).ToList();
        foreach(string asset in assetList)
        {
            List<string> dependencyList = UIUtility.GetReference(asset, type, deepDependence);
            Debug.LogFormat("[UIAtlasOptimizeTool.PrintReference]<color=cyan>{0}</color>找到{1}个{2}依赖:", asset, dependencyList.Count, type != null ? type + "类型的" : "");

            foreach(string dependency in dependencyList)
            {
                Debug.LogFormat("[UIAtlasOptimizeTool.PrintReference]{0}", dependency);
            }
        }

        
    }

    [MenuItem("Assets/资源检索引用")]
    private static void GlobalReference()
    {
        List<string> extensions = new List<string>() { ".prefab", ".unity", ".asset", "mat" };
        Dictionary<string, List<string>> referenceDict = new Dictionary<string, List<string>>();

        if (Selection.assetGUIDs.Count() > 0)
        {
            if(!EditorUtility.DisplayDialog("资源检索引用",string.Format("开始为{0}个资源检索引用，确定继续吗？",Selection.assetGUIDs.Count()),"确定","取消"))
            {
                return;
            }
        }

        foreach(string guid in Selection.assetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            referenceDict.Add(path, new List<string>());
            string[] files = Directory.GetFiles(UIUtility.PATH_ASSETS, "*.*", SearchOption.AllDirectories)
                .Where(s => extensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            int index = 0;
            EditorApplication.update = delegate ()
            {
                bool isCancle = EditorUtility.DisplayCancelableProgressBar("资源引用检索中", files[index], (float)index / files.Length);
                if (Regex.IsMatch(File.ReadAllText(files[index]), guid))
                {
                    Debug.LogFormat("[UIAtlasOptimizeTool.GlobalFindReference]{0}", files[index]);
                }

                index++;
                if (isCancle || index >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    index = 0;
                    Debug.LogFormat("[UIAtlasOPtimizelTool.GlobalFindReference]匹配结束{0}", files.Length);
                }
            };
        }
    }

    [MenuItem("Assets/图集工具/判断公共归并")]
    private static void CommonMerge()
    {
        List<string> textureList = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets)
            .Select(texture2d => AssetDatabase.GetAssetPath(texture2d)).Where(path => path.StartsWith(UIUtility.PATH_S_UI_STATIC)).ToList();

        if (textureList.Count <= 0)
        {
            Debug.LogFormat("[UIAtlasOPtimizeTool.CommonMerge]没有选中任何静态资源，注意在project的左侧列表中选中文件夹是无效的");
            return;
        }
        else
        {
            if (!EditorUtility.DisplayDialog("判断公共归并", string.Format("开始为{0}个图片判断公共归并，确定继续吗？", textureList.Count), "确定", "取消"))
            {
                return;
            }
        }
        bool verbose = false;
        int num = 0;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Dictionary<string, string> prefabRelationDict;
        Dictionary<string, List<string>> prefabRelationFolderDict;
        Dictionary<string, List<string>> prefabDynamicUIPathDict;
        UIUtility.LoadPrefabRelation(out prefabRelationDict, out prefabRelationFolderDict, out prefabDynamicUIPathDict);
        foreach(KeyValuePair<string,List<string>> pair in prefabRelationFolderDict)
        {
            foreach(string folder in pair.Value)
            {
                List<string> prefabList = UIUtility.GetAllPrefab(folder);
                foreach(string prefab in prefabList)
                {
                    if (!prefabRelationDict.ContainsKey(prefab))
                    {
                        prefabRelationDict.Add(prefab, pair.Key);
                    }
                }
            }
        }

        bool succeed = false;
        foreach(string texture in textureList)
        {
            if (!File.Exists(texture)) continue;
            if (texture.StartsWith(UIUtility.PATH_UI_STATIC_COMMON))
            {
                List<string> sameTextureList, referencePrefabList;
                if(!UIUtility.IsTextureShouldMerge(texture,out sameTextureList,out referencePrefabList, prefabRelationDict))
                {
                    Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]检测到需要反向归并的资源：{0}", prefabRelationDict);
                    num++;
                    List<string> referencePrefab = UIUtility.FindReferencePrefab(texture);
                    foreach(string prefab in referencePrefab)
                    {
                        string rootPrefab = prefab;
                        if (prefabRelationDict.ContainsKey(prefab))
                        {
                            rootPrefab = prefabRelationDict[prefab];
                        }
                        string findSameTexture = UIUtility.FindSameTexture(texture, UIUtility.PATH_S_UI_STATIC + Path.GetFileNameWithoutExtension(rootPrefab));
                        if (findSameTexture != null)
                        {
                            succeed = UIUtility.ChangeReference(prefab, texture, findSameTexture);
                            if(succeed&& verbose)
                            {
                                Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]修改了prefab：{0}", prefab);
                            }
                        }
                        else
                        {
                            string newPath = UIUtility.PATH_S_UI_STATIC + Path.GetFileNameWithoutExtension(rootPrefab) + UIUtility.PATH_S + Path.GetFileName(texture);
                            newPath = UIUtility.UniquePath(newPath);
                            succeed = CopyFile(texture, newPath);
                            if(succeed && verbose)
                            {
                                Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]创建了资源：{0}", newPath);
                            }
                            succeed = UIUtility.ChangeReference(prefab, texture, newPath);
                            if (succeed && verbose)
                            {
                                Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]修改了prefab：{0}", prefab);
                            }
                        }
                    }
                    succeed = RemoveFile(texture);
                    if (succeed)
                    {
                        if (verbose)
                        {
                            Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]移除了资源：{0}", texture);
                        }
                        else
                        {
                            Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]资源移除失败：{0}", texture);
                        }
                    }

                }
                
            }
            else
            {
                string findCommonTexture = UIUtility.FindSameTexture(texture, UIUtility.PATH_UI_STATIC_COMMON);
                if (findCommonTexture != null)
                {
                    Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]检测到相同的公共资源：{0}->{1}", texture, findCommonTexture);
                    num++;
                    List<string> prefabList = UIUtility.FindReferencePrefab(texture);
                    foreach (string prefab in prefabList)
                    {
                        succeed = UIUtility.ChangeReference(prefab, texture, findCommonTexture);
                        if (succeed && verbose)
                        {
                            Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]修改了prefab：{0}", prefab);
                        }
                    }
                    succeed = RemoveFile(texture);
                    if (succeed)
                    {
                        if (verbose)
                        {
                            Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]移除了资源：{0}", texture);
                        }
                    }
                    else
                    {
                        Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]资源移除失败：{0}", texture);
                    }
                }
                else
                {
                    List<string> sameTextureList, referencePrefabList;
                    if (UIUtility.IsTextureShouldMerge(texture, out sameTextureList, out referencePrefabList, prefabRelationDict))
                    {
                        string newPath = UIUtility.PATH_UI_STATIC_COMMON + Path.GetFileName(texture);
                        newPath = UIUtility.UniquePath(newPath);
                        List<string> allSameTextureList = new List<string>(sameTextureList) { texture };
                        Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]检测到需要归并的资源：{0}->{1}", allSameTextureList.ToString(), newPath);
                        num++;
                        succeed = CopyFile(texture, newPath);
                        if (succeed && verbose)
                        {
                            Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]创建了资源：{0}", newPath);
                        }

                        foreach (string sameTexture in allSameTextureList)
                        {
                            List<string> referencePrefab = UIUtility.FindReferencePrefab(sameTexture);
                            foreach (string prefab in referencePrefab)
                            {
                                succeed = UIUtility.ChangeReference(prefab, sameTexture, newPath);
                                if (succeed && verbose)
                                {
                                    Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]修改了prefab：{0}", prefab);
                                }
                            }
                            succeed = RemoveFile(sameTexture);
                            if (succeed)
                            {
                                if (verbose)
                                {
                                    Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]移除了资源：{0}", texture);
                                }
                            }
                            else
                            {
                                Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]资源移除失败：{0}", texture);
                            }
                        }
                    }
                }
            }
        }
        sw.Stop();
        Debug.LogFormat("[UIAtlasOptimizeTool.CommonMerge]判断公共归并执行结束，处理了{0}/{1}个资源，共耗时{2}毫秒", num, textureList.Count, sw.ElapsedMilliseconds);

    }

    [MenuItem("Tools/图集工具/查找Missing/当前场景")]
    private static void FindMissingCurrentScene()
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        Object[] previousSelection = Selection.objects;
        Selection.objects = transforms.Cast<Transform>().Where(x => x != null).Select(x => x.gameObject).Cast<Object>().ToArray();
        Transform[] selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
        Selection.objects = previousSelection;
        List<GameObject> gameObjectList = selectedTransforms.Select(tr => tr.gameObject).ToList();
        foreach(GameObject gameObject in gameObjectList)
        {
            Component[] components = gameObject.GetComponents<Component>();
            foreach(Component component in components)
            {
                if (!component)
                {
                    Debug.LogFormat("[UIAtlasOptimizeTool.FindMissionCurrentScene].找到为空的component：{0}", gameObject);
                }
                else
                {
                    SerializedObject so = new SerializedObject(component);
                    SerializedProperty iter = so.GetIterator();
                    while (iter.NextVisible(true))
                    {
                        if (iter.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if(iter.objectReferenceValue == null && iter.hasChildren) 
                            {
                                SerializedProperty fileId = iter.FindPropertyRelative("m_FileID");
                                if(fileId != null & fileId.intValue != 0)
                                {
                                    Debug.LogFormat("[UIAtlasOptimizeTool.FindMissiongCurrentScene]找到Missing的引用：{0}>>{1}>>{2}",
                                        gameObject, component.GetType().Name, iter.propertyPath);
                                }
                            }
                        }
                    }
                }
            }
            Debug.LogFormat("[UIAtlasOptimizeTool.FindMissingCurrentScene]查找结束");
        }

    }

    [MenuItem("Assets/图集工具/清理没有引用的静态资源")]
    private static void RemoveUnusedStaticTexture()
    {
        List<string> textureList = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets)
            .Select(texture2d => AssetDatabase.GetAssetPath(texture2d)).Where(path => path.StartsWith(UIUtility.PATH_S_UI_STATIC)).ToList();

        if (textureList.Count > 0)
        {
            Debug.LogFormat("[UIAtlasOptimizeTool.RemoveUnusedStaticTexture]没有选中任何静态资源，注意在project的左侧列表中选中文件夹是无效的");
            return;
        }
        else
        {
            if (!EditorUtility.DisplayDialog("判断引用清理", string.Format("开始为{0}个图片判断引用及清理，确定继续吗？", textureList.Count), "确定", "取消"))
            {
                return;
            }
        }

        int num = 0;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        foreach(string texture in textureList)
        {
            List<string> prefabList = UIUtility.FindReferencePrefab(texture);
            if (prefabList.Count <= 0)
            {
                Debug.LogFormat("[UIAtlasOptimizeTool.RemoveUnusedStaticTexture]检测到没有被引用的资源：{0}", texture);
                num++;
                bool remove = RemoveFile(texture);
                if (!remove)
                {
                    Debug.LogErrorFormat("[UIAtlasOptimizeTool.RemoveUnusedStaticTexture]资源移除失败：{0}", texture);
                }
            }
        }
        sw.Stop();
        Debug.LogFormat("[UIAtlasOptimizeTool.RemoveUnusedStaticTexture]清理没有引用的静态资源执行结束，处理了{0}/{1}个资源，共耗时{2}毫秒",
            num, textureList.Count, sw.ElapsedMilliseconds);
    }


    [MenuItem("Assets/图集工具/资源查找prefab引用/查找UI prefab")]
    private static void FindReferencePrefabUI()
    {
        FindReferencedPrefab(UIUtility.PATH_UI_PREFAB);
    }

    [MenuItem("Assets/图集工具/资源查找prefab引用/查找所有 prefab")]
    private static void FindReferencePrefabAll()
    {
        FindReferencedPrefab(UIUtility.PATH_ASSETS);
    }

    private static void FindReferencedPrefab(string inPath)
    {
        List<string> assetsList = Selection.GetFiltered<Object>(SelectionMode.DeepAssets)
            .Select(asset => AssetDatabase.GetAssetPath(asset)).ToList();

        if (assetsList.Count <= 0)
        {
            Debug.LogFormat("[UIAtlasOptimizeTool.FindReferencedPrefab]没有选中任何静态资源，注意在project的左侧列表中选中文件夹是无效的");
            return;
        }
        else
        {
            if (!EditorUtility.DisplayDialog("资源查找prefab引用", string.Format("开始为{0}个资源查找引用，确定继续吗？", assetsList.Count), "确定", "取消"))
            {
                return;
            }
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();

        Dictionary<string, List<string>> prefabListDict = UIUtility.FindReferencedPrefabByGroup(assetsList, inPath);

        sw.Stop();

        foreach(KeyValuePair<string,List<string>> prefabList in prefabListDict)
        {
            Debug.LogFormat("[UIAtlasOptimizeTool.FindReferencedPrefab]资源<color=cyan>{0}</color>被{1}个prefab引用",
                prefabList.Key, prefabList.Value.Count);

            foreach(string referencePrefabPath in prefabList.Value)
            {
                Debug.LogFormat("[UIAtlasOptimizeTool.FindReferencedPrefab]<color=cyan>{0}</color>", referencePrefabPath);
            }
        }

        Debug.LogFormat("[UIAtlasOptimizeTool.FindReferencedPrefab]查找引用执行结束，查找了{0}个资源，耗时{1}毫秒", assetsList.Count, sw.ElapsedMilliseconds);
    }


    [MenuItem("Assets/图集工具/查找相同图片/查找静态UI图片")]
    private static void FindSameStatic()
    {
        FindSame(UIUtility.PATH_UI_STATIC);
    }

    [MenuItem("Assets/图集工具/查找相同图片/查找UI图片")]
    private static void FindSameUI()
    {
        FindSame(UIUtility.PATH_UI);
    }

    [MenuItem("Assets/图集工具/查找相同图片/查找所有图片")]
    private static void FindSameAll()
    {
        FindSame(UIUtility.PATH_ASSETS);
    }

    private static void FindSame(string inPath)
    {
        List<string> textureList = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets)
            .Select(texture2d => AssetDatabase.GetAssetPath(texture2d)).ToList();

        if (textureList.Count <= 0)
        {
            Debug.LogFormat("[UIAtlasOptimizeTool.FindSame]没有选中任何静态资源，注意在project的左侧列表中选中文件夹是无效的");
            return;
        }
        else
        {
            if (!EditorUtility.DisplayDialog("查找相同图片", string.Format("开始为{0}个图片查找相同图片，确定继续吗？", textureList.Count), "确定", "取消"))
            {
                return;
            }
        }

        Dictionary<string, List<string>> sameTextureListDict = new Dictionary<string, List<string>>();
        Stopwatch sw = new Stopwatch();
        sw.Start();

        foreach(string texture in textureList)
        {
            sameTextureListDict.Add(texture, UIUtility.FindAllSameTexture(texture, inPath));
        }

        sw.Stop();

 
        foreach (KeyValuePair<string, List<string>> sameTextureList in sameTextureListDict)
        {
            Debug.LogFormat("[UIAtlasOptimizeTool.FindSame]资源<color=cyan>{0}</color>查找到{1}个相同图片",
                sameTextureList.Key, sameTextureList.Value.Count);

            foreach (string sameTexture in sameTextureList.Value)
            {
                Debug.LogFormat("[UIAtlasOptimizeTool.FindSame]<color=cyan>{0}</color>", sameTexture);
            }
        }

        Debug.LogFormat("[UIAtlasOptimizeTool.FindSame]查找相同图片执行结束，查找了{0}个资源，耗时{1}毫秒", textureList.Count, sw.ElapsedMilliseconds);
    }


    [MenuItem("Assets/图集工具/判断所选图片是否相同")]
    private static void IsTextureTheSame()
    {
        List<string> textureList = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets)
           .Select(texture2d => AssetDatabase.GetAssetPath(texture2d)).ToList();

        List<List<string>> sameTextureListList = new List<List<string>>();
        Stopwatch sw = new Stopwatch();
        sw.Start();

        foreach(string texture in textureList)
        {
            if (sameTextureListList.Count <= 0)
            {
                sameTextureListList.Add(new List<string>() { texture });
            }
            else
            {
                bool findSame = false;
                foreach(List<string> sameTextureList in sameTextureListList)
                {
                    if (UIUtility.IsSameTexture(texture, sameTextureList[0]))
                    {
                        sameTextureList.Add(texture);
                        findSame = true;
                        break;
                    }
                }
                if (!findSame)
                {
                    sameTextureListList.Add(new List<string>() { texture });
                }
            }
        }
        sw.Stop();
        sameTextureListList = sameTextureListList.Where(list => list.Count > 1).ToList();
        if (sameTextureListList.Count > 0)
        {
            foreach(List<string> sameTextureList in sameTextureListList)
            {
                Debug.LogFormat("[UIAtlasLookOptimizeTool.IsTextureTheSame]相同图片：<color=cyan>{0}</color>", sameTextureList.ToString());
            }

            Debug.LogFormat("[UIAtlasLookOptimizeTool.IsTextureTheSame]查找相同图片执行结束，查找了{0}个图片，耗时{1}毫秒", textureList.Count, sw.ElapsedMilliseconds);
        }
        else
        {
            Debug.LogFormat("[UIAtlasLookOptimizeTool.IsTextureTheSame]没有找到任何相同图片，查找了{0}个图片，耗时{1}毫秒", textureList.Count, sw.ElapsedMilliseconds);
        }
    }

    [MenuItem("Assets/图集工具/像素检查")]
    private static void PixelCheck()
    {
        List<string> textureList = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets)
           .Select(texture2d => AssetDatabase.GetAssetPath(texture2d)).ToList();

        if (textureList.Count <= 0)
        {
            Debug.LogFormat("[UIAtlasOptimizeTool.PixelCheck]没有选中任何资源，注意在project的左侧列表中选中文件夹是无效的");
            return;
        }
        else
        {
            if (!EditorUtility.DisplayDialog("像素检查", string.Format("开始为{0}个图片进行检查，确定继续吗？", textureList.Count), "确定", "取消"))
            {
                return;
            }
        }
        Dictionary<string, int[,]> repeatDic = new Dictionary<string, int[,]>();
        Dictionary<string, int[]> loopDict = new Dictionary<string, int[]>();
        Dictionary<string, bool[]> summetryDict = new Dictionary<string, bool[]>();
        Stopwatch sw = new Stopwatch();
        sw.Start();

        foreach(string texture in textureList)
        {
            int[,] repeat;
            int[] loop;
            bool[] summetry;
            UIUtility.TexturePixelCheck(texture, out repeat, out loop, out summetry);
            repeatDic.Add(texture, repeat);
            loopDict.Add(texture, loop);
            summetryDict.Add(texture, summetry);
        }

        sw.Stop();
        foreach(string texture in textureList)
        {
            Debug.LogFormat("[UIAtlasOptimizeTool.PixelCheck]图片<color=cyan>{0}<color>在<color=cyan>(({1}{2}),({3},{4}))</color>像素上重复,"
                + "从<color=cyan>({5},{6})</color>像素开始循环，对称值为<color=cyan>({7},{8})</color>", texture, repeatDic[texture][0, 0],
                repeatDic[texture][0, 0] + repeatDic[texture][0, 1], repeatDic[texture][1, 0], repeatDic[texture][1, 0] + repeatDic[texture][1, 1],
                loopDict[texture][0], loopDict[texture][1], summetryDict[texture][0], summetryDict[texture][1]);
        }
        Debug.LogFormat("[UIAtlasLookOptimizeTool.PixelCheck]查找相同图片执行结束，查找了{0}个图片，耗时{1}毫秒", textureList.Count, sw.ElapsedMilliseconds);
    }

    [MenuItem("Assets/图集工具/图片处理成4的倍数")]
    private static void ToMultipleOf4()
    {
        List<string> textureList = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets)
           .Select(texture2d => AssetDatabase.GetAssetPath(texture2d)).ToList();

        if (textureList.Count <= 0)
        {
            Debug.LogFormat("[UIAtlasOptimizeTool.PixelCheck]没有选中任何资源，注意在project的左侧列表中选中文件夹是无效的");
            return;
        }
        Stopwatch sw = new Stopwatch();
        sw.Start();
        int num = 0;
        foreach(string texture in textureList)
        {
            int[] size = UIUtility.GetTextureCompressedSize(texture);
            if(size[0]%4!=0 || size[1] % 4 != 0)
            {
                int[] newSize = new int[] { size[0] + ((size[0] % 4 == 0) ? 0 : (4 - size[0] % 4)),
                size[1] + ((size[1] % 4 == 0) ? 0 : (4 - size[1] % 4))};
                //AssetCheckerLocal
            }
        }
    }









    private static bool CopyFile(string oldPath,string newPath)
    {
        bool created = false;
        if (!File.Exists(newPath))
        {
            bool directoryExist = false;
            string newPathDirectory = Path.GetDirectoryName(newPath);
            if (Directory.Exists(newPathDirectory))
            {
                directoryExist = true;
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(newPathDirectory);
                    directoryExist = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogErrorFormat("[UIAtlasOptimizeTool.CopyFile]文件夹{0}创建失败：{1}", newPathDirectory, e);
                }
            }

            if (directoryExist)
            {
                created = AssetDatabase.CopyAsset(oldPath, newPath);
            }
            
        }
        return created;
    }

    private static bool RemoveFile(string path)
    {
        bool removed = AssetDatabase.DeleteAsset(path);
        if (removed && path.StartsWith(UIUtility.PATH_S_UI))
        {
            string removeDirectory = null;
            string parent = path.Substring(0, path.LastIndexOf(UIUtility.PATH_S));
            while (parent.Length > UIUtility.PATH_UI.Length && Directory.GetFiles(parent, "*", SearchOption.AllDirectories).Length <= 0)
            {
                removeDirectory = parent;
                parent = removeDirectory.Substring(0, removeDirectory.LastIndexOf(UIUtility.PATH_S));
            }
            if (removeDirectory != null)
            {
                AssetDatabase.DeleteAsset(removeDirectory);
            }
        }
        return removed;
    }
}
