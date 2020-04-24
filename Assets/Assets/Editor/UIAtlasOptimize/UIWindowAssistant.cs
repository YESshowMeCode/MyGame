// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/03 21:20:14
// FileName：Assets/Assets/Editor/UIAtlasOptimize/UIWindowAssistant.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;


public class UIWindowAssistant : EditorWindow
{

    private readonly bool processMergeRevert = false;

    private enum Status
    {
        None,
        WindowAssign,
        SubPrefabAssign,
        PrefabCheck,
        FolderCommonCheck,
        FolderReference,
        Remove,
        Standard,
        Merge,
        MergeRevert,
        Summary,
    }
    private Status status = Status.None;

    private GameObject window;//窗口prefab
    private string windowPath;//窗口路径
    private string windowUIPath;//窗口静态路径
    private List<GameObject> subPrefabList = new List<GameObject>();//子prefab
    private List<string> subPrefabPathList = new List<string>();//子prefab路径
    private List<string> subPrefabUIPathLsit = new List<string>();//子prefab静态UI路径，规范后该文件夹应该不存在
    private List<string> subPrefabFolderPathList = new List<string>();//子prefab文件夹，该文件夹内所有prefab自动指定成子prefab
    private List<string> dynamicUIPathList = new List<string>();//动态资源路径

    private List<TextureInfo> prefabCheckList = new List<TextureInfo>();
    private List<TextureInfo> folderCommonCheckList = new List<TextureInfo>();
    private List<TextureInfo> folderReferenceList = new List<TextureInfo>();
    private List<TextureInfo> removeList = new List<TextureInfo>();
    private List<TextureInfo> standardList = new List<TextureInfo>();
    private List<TextureInfo> mergeList = new List<TextureInfo>();
    private List<TextureInfo> mergeCommonList = new List<TextureInfo>();
    private List<TextureInfo> mergeRevertList = new List<TextureInfo>();

    private List<string> toRemoveFileList = new List<string>();
    private List<string> modifiedFileList = new List<string>();
    private List<string> addedFileList = new List<string>();
    private List<string> removedFileList = new List<string>();


    private Dictionary<string, string> prefabRelationDict = new Dictionary<string, string>();
    private Dictionary<string, List<string>> prefabRelationFolderDict = new Dictionary<string, List<string>>();
    private Dictionary<string, List<string>> prefabDynamicUIPathDict = new Dictionary<string, List<string>>();

    private Vector2 scrollPosition = Vector2.zero;
    private IEnumerator checkCoroutine;
    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();


    private class TextureInfo : System.IComparable<TextureInfo>
    {
        public string Path { get; private set; }

        public Texture2D obj { get; private set; }

        /// <summary>
        /// 0 无需修改  1 修改引用，标记删除原图片  2 复制到新路径并修改引用，标记移除原图片
        /// 3, 查找并修改引用，标记移除原图片  4 对prefabLIst复制到各自对应的路径并修改引用 
        /// 5 移除   6 有优化内容  7 检索相同图片，修改所有引用并移除源文件  8 查找引用，复制到各自对应的路径，修改引用并移除原文件
        /// </summary>
        public int Flag { get; private set; }

        public bool Check { get;  set; }

        public string NewPath { get; private set; }

        private Texture2D newObj;
        public Texture2D NewObj
        {
            get
            {
                return newObj;
            }

            set
            {
                if (newObj != value)
                {
                    newObj = value;
                    if (newObj != null)
                    {
                        NewPath = AssetDatabase.GetAssetPath(newObj);
                        Flag = 1;
                        Check = true;
                    }
                    else
                    {
                        NewPath = string.Empty;
                        Flag = 0;
                        Check = false;
                    }
                }
            }
        }

        public List<string> PrefabList { get; private set; }

        public List<GameObject> PrefabObjList { get; private set; }

        public List<string>  TextureList { get; private set; }

        public bool NameIllegal { get; private set; }

        public bool[] SizeIllegal { get; private set; }

        public int PixelType { get; private set; }
        
        public Texture2D TargetObj { get; private set; }

        private int[,] newRepeat;

        public int[,] NewRepeat
        {
            get
            {
                return newRepeat;
            }

            set
            {
                newRepeat = value;
            }
        }


        public bool Cut { get;  set; }
        

        private TextureInfo()
        {

        }

        public static TextureInfo NewReplaceInfo(string path,int flag,string newPath)
        {
            TextureInfo info = new TextureInfo
            {
                Path = path,
                obj = AssetDatabase.LoadAssetAtPath<Texture2D>(newPath),
                Flag = flag,
            };

            switch (flag)
            {
                case 1:
                case 3:
                    info.NewPath = newPath;
                    info.newObj = AssetDatabase.LoadAssetAtPath<Texture2D>(newPath);
                    info.Check = true;
                    break; 
                case 2:
                    info.NewPath = newPath;
                    info.Check = true; 
                    break;
                default:
                    info.Check = false;
                    break; 
            }
            return info; ;
        }



        public static TextureInfo NewReferenceInfo(string path,List<string> prefabList)
        {
            TextureInfo info = new TextureInfo
            {
                Path = path,
                obj = AssetDatabase.LoadAssetAtPath<Texture2D>(path),
                Flag = 4,
                PrefabList = prefabList,
                Check = prefabList.Count > 0,
            };

            info.PrefabObjList = new List<GameObject>();
            foreach(string prefab in prefabList)
            {
                info.PrefabObjList.Add(AssetDatabase.LoadAssetAtPath<GameObject>(prefab));
            }
            return info;
        }

        public static TextureInfo NewMergeInfo(string path ,int flag,string newPath,List<string> textureList ,List<string> prefabList)
        {
            TextureInfo info = new TextureInfo
            {
                Path = path,
                obj = AssetDatabase.LoadAssetAtPath<Texture2D>(path),
                Flag = flag,
                NewPath = newPath,
                Check = true,
                TextureList = textureList,
                PrefabList = prefabList,
            };

            return info;
        }


        public static TextureInfo NewRemoveInfo(string path,List<string> prefabList)
        {
            TextureInfo info = new TextureInfo
            {
                Path = path,
                obj = AssetDatabase.LoadAssetAtPath<Texture2D>(path),
                Flag = 5,
                PrefabList = prefabList,
                Check = path.StartsWith(UIUtility.PATH_S_UI_STATIC) && prefabList.Count <= 0

            };

            return info;
        }

        public static TextureInfo NewStandardInfo(string path,int pixelType,int[,] repeat,int[] loop, bool[] summetry,bool nameIllegal,bool[] sizeIllegal)
        {
            TextureInfo info = new TextureInfo
            {
                Path = path,
                obj = AssetDatabase.LoadAssetAtPath<Texture2D>(path),
                Flag = 0,
                PixelType = pixelType,
                Check = true,
                NameIllegal = nameIllegal,
                SizeIllegal = sizeIllegal
            };

            switch (pixelType)
            {
                case 1:
                    info.obj = UIUtility.TextureMaskRepeat(path, repeat);
                    info.TargetObj = UIUtility.TextureCut(path, out info.newRepeat);
                    info.NewObj = UIUtility.TextureMaskRepeat(info.TargetObj, info.newRepeat);
                    break;
                case 2:
                    info.newObj = UIUtility.TextureMarkLoop(path, loop);
                    break;
            }
            return info;
        }

        int System.IComparable<TextureInfo>.CompareTo(TextureInfo other)
        {
            return 0 + Path.CompareTo(other.Path);
        }

    }

    [MenuItem("Tools/图集助手/窗口助手 %#m")]
    private static void WindowAssistantStart()
    {
        var window = GetWindow<UIWindowAssistant>(typeof(UIWindowAssistant));
        window.status = Status.None;
    }


    private void OnDestroy()
    {
        
    }

    private void Update()
    {
        if (checkCoroutine != null)
        {
            bool hasNext = checkCoroutine.MoveNext();
            if (!hasNext)
            {
                checkCoroutine = null;
            }
        }
    }

    private void OnGUI()
    {
        switch (status)
        {
            case Status.None:
                CommonLayoutStart("窗口助手", string.Empty);
                CommonLayoutEnd();
                NextState();
                break;
            case Status.WindowAssign:
                OnWindowAssginUpdate();
                break;
            case Status.SubPrefabAssign:
                OnSubPrefabAssignUpdate();
                break;
            case Status.PrefabCheck:
                OnPrefabCheckUpdate();
                break;
            case Status.FolderCommonCheck:
                OnFolderCommonCheckUpdate();
                break;
            case Status.FolderReference:
                OnFolderReferenceUpdate();
                break;
            case Status.Remove:
                OnRemoveUpdate();
                break;
            case Status.Standard:
                OnStandardUpdate();
                break;
            case Status.Merge:
                OnMergeUpdate();
                break;
            case Status.MergeRevert:
                OnMergeRevertUpdate();
                break;
            case Status.Summary:
                OnSummaryUpdate();
                break;
            default:
                status = Status.None;
                break;
        }
    }


    void NextState(bool skip = false)
    {
        checkCoroutine = null;

        switch (status)
        {
            case Status.None:
                status = Status.WindowAssign;
                OnWindowAssignStart();
                break;
            case Status.WindowAssign:
                OnPrefabAssignEnd(skip);
                status = Status.SubPrefabAssign;
                OnSubPrefabAssignStart();
                break;
            case Status.SubPrefabAssign:
                OnSubPrefabAssignEnd(skip);
                status = Status.PrefabCheck;
                OnPrefabCheckStart();
                break;
            case Status.PrefabCheck:
                OnPrefabCheckEnd(skip);
                status = Status.FolderCommonCheck;
                OnFolderCommonCheckStart();
                break;
            case Status.FolderCommonCheck:
                OnFolderCommonCheckEnd(skip);
                status = Status.FolderReference;
                OnFolderReferenceStart();
                break;
            case Status.FolderReference:
                OnFolderReferenceEnd(skip);
                status = Status.Remove;
                OnRemoveStart();
                break;
            case Status.Remove:
                OnRemoveEnd(skip);
                status = Status.Standard;
                OnStandardStart();
                break;
            case Status.Standard:
                OnStandardEnd(skip);
                status = Status.Merge;
                OnMergeStart();
                break;
            case Status.Merge:
                OnMergeEnd(skip);
                if (processMergeRevert)
                {
                    status = Status.MergeRevert;
                    OnMergeRevertStart();
                }
                else
                {
                    status = Status.Summary;
                    OnSummaryStart();
                }
                break;
            case Status.MergeRevert:
                OnMergeRevertEnd(skip);
                status = Status.Summary;
                OnSummaryStart();
                break;
            case Status.Summary:
                OnSummaryEnd(skip);
                status = Status.None;
                break;
            default:
                status = Status.None;
                break;

                
        }
    }

    #region 状态开始、结束和更新

    private void OnWindowAssignStart()
    {
        toRemoveFileList.Clear();
        modifiedFileList.Clear();
        addedFileList.Clear();
        removedFileList.Clear();
        windowPath = string.Empty;
        windowUIPath = string.Empty;
    }

    private void OnWindowAssginUpdate()
    {
        CommonLayoutStart("窗口助手", "指定窗口：指定一个窗口来进行整理");
        window = EditorGUILayout.ObjectField("窗口prefab：", window, typeof(GameObject), false) as GameObject;
        GUILayout.Space(50);
        CommonLayoutEnd(true, false, false);
    }

    private void OnPrefabAssignEnd(bool skip)
    {
        if (skip)
        {
            window = null;
        }
        else
        {
            if(window != null)
            {
                windowPath = AssetDatabase.GetAssetPath(window);
                windowUIPath = UIUtility.PATH_S_UI_STATIC + window.name;
            }
        }
    }

    private void OnSubPrefabAssignStart()
    {
        scrollPosition = Vector2.zero;
        subPrefabList.Clear();
        subPrefabFolderPathList.Clear();
        subPrefabPathList.Clear();
        subPrefabUIPathLsit.Clear();
        dynamicUIPathList.Clear();

        UIUtility.LoadPrefabRelation(out prefabRelationDict, out prefabRelationFolderDict, out prefabDynamicUIPathDict);
        if (prefabRelationFolderDict.ContainsKey(windowPath))
        {
            subPrefabFolderPathList = prefabRelationFolderDict[windowPath];
        }
        if (prefabDynamicUIPathDict.ContainsKey(windowPath))
        {
            dynamicUIPathList = prefabDynamicUIPathDict[windowPath];
        }

        foreach(KeyValuePair<string,string> prefabRelation in prefabRelationDict)
        {
            if(prefabRelation.Value == windowPath)
            {
                GameObject subPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabRelation.Key);
                if (subPrefab != null)
                {
                    subPrefabList.Add(subPrefab);
                }
            }
        }
    }

    private void OnSubPrefabAssignUpdate()
    {
        CommonLayoutStart("窗口助手", "指定其他项：指定窗口附属prefab和窗口用到动态图片文件夹");
        if(window == null)
        {
            EditorGUILayout.HelpBox("没有知道任何窗口", MessageType.Error);
            CommonLayoutEnd(false, false);
        }
        else
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                GUILayout.Space(50);
                EditorGUILayout.HelpBox("指定窗口附属prefab，兵在后面的步骤把这些prefab和窗口合并到同一文件夹（图集）", MessageType.Info);
                for(int i = 0; i < subPrefabList.Count; i++)
                {
                    subPrefabList[i] = EditorGUILayout.ObjectField("prefab:", subPrefabList[i], typeof(GameObject), false) as GameObject;
                }
                GameObject anotherSubPrefab = EditorGUILayout.ObjectField("prefab:", null, typeof(GameObject), false) as GameObject;
                if (anotherSubPrefab != null)
                {
                    subPrefabList.Add(anotherSubPrefab);
                }
                GUILayout.Space(50);
                EditorGUILayout.HelpBox("指定包含附属prefab的文件夹。文件夹下的所有prefab都会被认为是窗口附属prefab", MessageType.Info);
                for(int i = 0; i < subPrefabFolderPathList.Count; i++)
                {
                    subPrefabFolderPathList[i] = EditorGUILayout.TextField(subPrefabFolderPathList[i]);
                }
                EditorGUILayout.BeginHorizontal();
                {
                    string newSubPrefabFolderPath = EditorGUILayout.TextField(null);
                    if (newSubPrefabFolderPath != null)
                    {
                        while (newSubPrefabFolderPath.EndsWith(UIUtility.PATH_S))
                        {
                            newSubPrefabFolderPath = newSubPrefabFolderPath.Substring(0, newSubPrefabFolderPath.Length - 1);
                        }
                        subPrefabFolderPathList.Add(newSubPrefabFolderPath);
                    }
                    if (GUILayout.Button("..."))
                    {
                        string path = EditorUtility.OpenFolderPanel("选择prefab文件夹", UIUtility.PATH_S_UI_PREFAB, null);
                        if (path.Contains(UIUtility.PATH_ASSETS))
                        {
                            subPrefabFolderPathList.Add(path);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.Space(50);
                EditorGUILayout.HelpBox("指定窗口使用到的动态图片文件夹，动态图片不会做任何整理，只会检查是否符合规范", MessageType.Info);
                for(int i = 0; i < dynamicUIPathList.Count; i++)
                {
                    dynamicUIPathList[i] = EditorGUILayout.TextField(dynamicUIPathList[i]);
                }
                EditorGUILayout.BeginHorizontal();
                {
                    string newDynamicUIPath = EditorGUILayout.TextField(null);
                    if (newDynamicUIPath != null)
                    {
                        while (newDynamicUIPath.EndsWith(UIUtility.PATH_S))
                        {
                            newDynamicUIPath = newDynamicUIPath.Substring(0, newDynamicUIPath.Length - 1);
                        }
                        dynamicUIPathList.Add(newDynamicUIPath);
                    }
                    if (GUILayout.Button("..."))
                    {
                        string path = EditorUtility.OpenFolderPanel("选择动态图片文件夹", UIUtility.PATH_S_UI, null);
                        if (path.Contains(UIUtility.PATH_ASSETS))
                        {
                            dynamicUIPathList.Add(path);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            CommonLayoutEnd();
        }
    }

    private void OnSubPrefabAssignEnd(bool skip)
    {
        if (skip)
        {
            subPrefabList.Clear();
            dynamicUIPathList.Clear();
            ClearPrefabRelation(windowPath);
        }
        else
        {
            //移除旧关系
            ClearPrefabRelation(windowPath);
            foreach(GameObject subPrefab in subPrefabList)
            {
                //移除子prefab的旧关系
                if (subPrefab != null)
                {
                    string subPrefabPath = AssetDatabase.GetAssetPath(subPrefab);
                    ClearPrefabRelation(subPrefabPath);
                }
            }
            //建立新的prefab关系
            foreach(GameObject subPrefab in subPrefabList)
            {
                if(subPrefab != null)
                {
                    string subPrefanPath = AssetDatabase.GetAssetPath(subPrefab);
                    subPrefabPathList.Add(subPrefanPath);
                    subPrefabUIPathLsit.Add(UIUtility.PATH_S_UI_STATIC + Path.GetFileNameWithoutExtension(subPrefanPath));
                    AddPrefabRelation(subPrefanPath, windowPath);
                }
            }

            if(prefabRelationFolderDict.ContainsKey(windowPath) || subPrefabFolderPathList.Count > 0)
            {
                prefabRelationFolderDict[windowPath] = subPrefabFolderPathList;
            }

            if(prefabDynamicUIPathDict.ContainsKey(windowPath) || dynamicUIPathList.Count > 0)
            {
                prefabDynamicUIPathDict[windowPath] = dynamicUIPathList;
            }

            UIUtility.SavePrefabRelation(prefabRelationDict, prefabRelationFolderDict, prefabDynamicUIPathDict);
            //计算prefabRelationFolderDict路径下subprefab
            foreach(KeyValuePair<string,List<string>> pair in prefabRelationFolderDict)
            {
                if(pair.Key == windowPath)
                {
                    
                    foreach(string folder in pair.Value)
                    {
                        List<string> prefabList = UIUtility.GetAllPrefab(folder);
                        foreach(string prefab in prefabList)
                        {
                            prefabRelationDict.Add(prefab, pair.Key);
                            GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
                            string subPrefabPath = AssetDatabase.GetAssetPath(prefabObj);
                            subPrefabPathList.Add(subPrefabPath);
                            subPrefabUIPathLsit.Add(UIUtility.PATH_S_UI_STATIC + Path.GetFileNameWithoutExtension(subPrefabPath));
                        }
                    }
                }
            }

        }
    }

    private void OnPrefabCheckStart()
    {
        if (window == null)
        {
            status = Status.None;
            return;
        }
        scrollPosition = Vector2.zero;
        checkCoroutine = PrefabCheckCoroutine();
    }

    private void OnPrefabCheckUpdate()
    {
        CommonLayoutStart("窗口助手", "检查prefab：下面列出了窗口和prefab中引用到的图片资源。默认勾选说明不在窗口文件夹内，右侧是整理后的结果。点击下一步来整理");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        {
            if(checkCoroutine!=null || prefabCheckList.Count > 0)
            {
                foreach(TextureInfo textureInfo in prefabCheckList)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        textureInfo.Check = GUILayout.Toggle(textureInfo.Check, string.Empty);
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.ObjectField(textureInfo.obj, typeof(Texture2D), false);
                            EditorGUILayout.TextField(textureInfo.Path);
                            EditorGUILayout.TextField(textureInfo.obj.width + "*" + textureInfo.obj.height);
                            Rect rect = GUILayoutUtility.GetRect(100, 100);
                            if(Event.current.type == EventType.Repaint)
                            {
                                GUI.DrawTexture(rect, textureInfo.obj, ScaleMode.ScaleToFit);
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.BeginVertical();
                        {
                            switch (textureInfo.Flag)
                            {
                                case 1:
                                    textureInfo.NewObj = EditorGUILayout.ObjectField(textureInfo.NewObj, typeof(Texture2D), false) as Texture2D;
                                    EditorGUILayout.TextField(textureInfo.NewPath);
                                    EditorGUILayout.TextField(textureInfo.NewObj.width + "*" + textureInfo.NewObj.height);
                                    Rect rect = GUILayoutUtility.GetRect(100, 100);
                                    if (Event.current.type == EventType.Repaint)
                                    {
                                        GUI.DrawTexture(rect, textureInfo.NewObj, ScaleMode.ScaleToFit);
                                    }
                                    break;
                                case 2:
                                    textureInfo.NewObj = EditorGUILayout.ObjectField(textureInfo.NewObj, typeof(Texture2D), false) as Texture2D;
                                    EditorGUILayout.TextField(textureInfo.NewPath);
                                    break;
                                default:
                                    textureInfo.NewObj = EditorGUILayout.ObjectField(textureInfo.NewObj, typeof(Texture2D), false) as Texture2D;
                                    EditorGUILayout.TextField(textureInfo.NewPath);
                                    break;
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    GUILayout.Space(50);
                }
                
            }
            else
            {
                GUILayout.Space(50);
                EditorGUILayout.LabelField("没有找到任何被引用的资源");
                GUILayout.Space(50);
            }
        }
        if(checkCoroutine != null)
        {
            EditorGUILayout.HelpBox("正在检查资源...", MessageType.Info);
        }
        CommonLayoutEnd(checkCoroutine == null);
    }

    private void OnPrefabCheckEnd(bool skip)
    {
        if (skip)
        {
            ProcessTextureInfo(prefabCheckList);
        }
    }


    private void OnFolderCommonCheckStart()
    {
        scrollPosition = Vector2.zero;
        checkCoroutine = FolderCommonCheckCoroutine();
    }

    private void OnFolderCommonCheckUpdate()
    {
        CommonLayoutStart("窗口助手", "检查公共：遍历相关文件夹，检查是否有可替换的公共资源，有则替换，点击下一步来整理");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        {
            if (checkCoroutine != null || folderCommonCheckList.Count > 0)
            {
                foreach (TextureInfo textureInfo in folderCommonCheckList)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        textureInfo.Check = GUILayout.Toggle(textureInfo.Check, string.Empty);
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.ObjectField(textureInfo.obj, typeof(Texture2D), false);
                            EditorGUILayout.TextField(textureInfo.Path);
                            EditorGUILayout.TextField(textureInfo.obj.width + "*" + textureInfo.obj.height);
                            Rect rect = GUILayoutUtility.GetRect(100, 100);
                            if (Event.current.type == EventType.Repaint)
                            {
                                GUI.DrawTexture(rect, textureInfo.obj, ScaleMode.ScaleToFit);
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.BeginVertical();
                        {
                            switch (textureInfo.Flag)
                            {
                                case 3:
                                    textureInfo.NewObj = EditorGUILayout.ObjectField(textureInfo.NewObj, typeof(Texture2D), false) as Texture2D;
                                    EditorGUILayout.TextField(textureInfo.NewPath);
                                    EditorGUILayout.TextField(textureInfo.NewObj.width + "*" + textureInfo.NewObj.height);
                                    Rect rect = GUILayoutUtility.GetRect(100, 100);
                                    if (Event.current.type == EventType.Repaint)
                                    {
                                        GUI.DrawTexture(rect, textureInfo.NewObj, ScaleMode.ScaleToFit);
                                    }
                                    break;
                                default:
                                    textureInfo.NewObj = EditorGUILayout.ObjectField(textureInfo.NewObj, typeof(Texture2D), false) as Texture2D;
                                    EditorGUILayout.TextField(textureInfo.NewPath);
                                    break;
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    GUILayout.Space(50);
                }

            }
            else
            {
                GUILayout.Space(50);
                EditorGUILayout.LabelField("没有找到任何被引用的资源");
                GUILayout.Space(50);
            }
        }
        if (checkCoroutine != null)
        {
            EditorGUILayout.HelpBox("正在检查资源...", MessageType.Info);
        }
        CommonLayoutEnd(checkCoroutine == null);
    }

    private void OnFolderCommonCheckEnd(bool skip)
    {
        if (!skip)
        {
            ProcessTextureInfo(folderCommonCheckList);
        }
    }



    private void OnFolderReferenceStart()
    {
        scrollPosition = Vector2.zero;
        checkCoroutine = FolderReferenceCoroutine();
    }

    private void OnFolderReferenceUpdate()
    {
        CommonLayoutStart("窗口助手", "检查其他引用：遍历相关文件夹，检查资源是否被其他界面引用，有则复制到自己的文件夹并修改引用，点击下一步来整理");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        {
            if (checkCoroutine != null || folderReferenceList.Count > 0)
            {
                foreach (TextureInfo textureInfo in folderReferenceList)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        textureInfo.Check = GUILayout.Toggle(textureInfo.Check, string.Empty);
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.ObjectField(textureInfo.obj, typeof(Texture2D), false);
                            EditorGUILayout.TextField(textureInfo.Path);
                            EditorGUILayout.TextField(textureInfo.obj.width + "*" + textureInfo.obj.height);
                            Rect rect = GUILayoutUtility.GetRect(100, 100);
                            if (Event.current.type == EventType.Repaint)
                            {
                                GUI.DrawTexture(rect, textureInfo.obj, ScaleMode.ScaleToFit);
                            }
                            EditorGUILayout.LabelField("被以下界面引用");
                            for(int i = 0; i < textureInfo.PrefabList.Count; i++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.ObjectField(textureInfo.PrefabObjList[i], typeof(GameObject), false);
                                    EditorGUILayout.TextField(textureInfo.PrefabList[i]);
                                    EditorGUILayout.EndHorizontal();
                                }
                             }
                            GUILayout.Space(50);
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    GUILayout.Space(50);
                }

            }
            else
            {
                GUILayout.Space(50);
                EditorGUILayout.LabelField("没有找到任何被引用的资源");
                GUILayout.Space(50);
            }
        }
        if (checkCoroutine != null)
        {
            EditorGUILayout.HelpBox("正在检查资源...", MessageType.Info);
        }
        CommonLayoutEnd(checkCoroutine == null);
    }

    private void OnFolderReferenceEnd(bool skip)
    {
        if (!skip)
        {
            ProcessTextureInfo(folderReferenceList);
        }
    }



    private void OnRemoveStart()
    {
        scrollPosition = Vector2.zero;
        checkCoroutine = RemoveCoroutine();
    }

    private void OnRemoveUpdate()
    {
        CommonLayoutStart("窗口助手", "移除资源：遍历相关文件夹，移除修改过引用的旧资源，移除相关文件夹内没有被引用的资源，点击下一步来整理");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        {
            if (checkCoroutine != null || removeList.Count > 0)
            {
                foreach (TextureInfo textureInfo in removeList)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        textureInfo.Check = GUILayout.Toggle(textureInfo.Check, string.Empty);
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.ObjectField(textureInfo.obj, typeof(Texture2D), false);
                            EditorGUILayout.TextField(textureInfo.Path);
                            if (textureInfo.PrefabList.Count > 0)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.LabelField("该图片被以下prefan所引用");
                                    EditorGUILayout.BeginVertical();
                                    {
                                        foreach(string referencePrefab in textureInfo.PrefabList)
                                        {
                                            EditorGUILayout.TextField(referencePrefab);
                                        }
                                        EditorGUILayout.EndVertical();
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            EditorGUILayout.EndVertical();


                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    GUILayout.Space(50);
                }

            }
            else
            {
                GUILayout.Space(50);
                EditorGUILayout.LabelField("不需要移除任何资源");
                GUILayout.Space(50);
            }
        }
        if (checkCoroutine != null)
        {
            EditorGUILayout.HelpBox("正在检查资源...", MessageType.Info);
        }
        CommonLayoutEnd(checkCoroutine == null);
    }

    private void OnRemoveEnd(bool skip)
    {
        if (!skip)
        {
            ProcessTextureInfo(removeList);
        }
    }


    private void OnStandardStart()
    {
        scrollPosition = Vector2.zero;
        checkCoroutine = StandardCoroutine();
    }

    private void OnStandardUpdate()
    {
        CommonLayoutStart("窗口助手", "检查规范：检查资源是否符合规范，可优化切割的图片，点击切割会自动处理，点击下一步来整理");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        {
            if (checkCoroutine != null || removeList.Count > 0)
            {
                foreach (TextureInfo textureInfo in removeList)
                {
                    EditorGUILayout.ObjectField(textureInfo.obj, typeof(Texture2D), false);
                    EditorGUILayout.TextField(textureInfo.Path);
                    if (textureInfo.NameIllegal)
                    {
                        EditorGUILayout.HelpBox("资源名称内含义不符合规范的字符", MessageType.Warning);
                    }
                    if(textureInfo.SizeIllegal[0] && textureInfo.SizeIllegal[1])
                    {
                        string str = string.Empty;
                        if(textureInfo.SizeIllegal[0] && textureInfo.SizeIllegal[1])
                        {
                            str = "宽和高";
                        }
                        else if(textureInfo.SizeIllegal[0])
                        {
                            str = "宽";

                        }
                        else
                        {
                            str = "高";
                        }
                        EditorGUILayout.HelpBox(string.Format("资源的{0}不是4的倍数", str), MessageType.Warning);
                    }

                    switch (textureInfo.PixelType)
                    {
                        case 1:
                            EditorGUILayout.HelpBox("资源在红框范围内像素重复，可优化九宫格切割", MessageType.Warning);
                            break;
                        case 2:
                            EditorGUILayout.HelpBox("资源在红框范围内像素循环，可优化平铺", MessageType.Warning);
                            break;
                        case 3:
                            EditorGUILayout.HelpBox("资源横向像素对称", MessageType.Warning);
                            break;
                        case 4:
                            EditorGUILayout.HelpBox("资源纵向像素对称", MessageType.Warning);
                            break;
                        default:
                            break;
                    }

                    if(textureInfo.NewObj == null)
                    {
                        EditorGUILayout.TextField(textureInfo.obj.width + "*" + textureInfo.obj.height);
                        Rect rect = GUILayoutUtility.GetRect(100, 100);
                        if(Event.current.type == EventType.Repaint)
                        {
                            GUI.DrawTexture(rect, textureInfo.obj, ScaleMode.ScaleToFit);
                        }
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.TextField(textureInfo.obj.width + "*" + textureInfo.obj.height);
                                Rect rect = GUILayoutUtility.GetRect(100, 100);
                                if (Event.current.type == EventType.Repaint)
                                {
                                    GUI.DrawTexture(rect, textureInfo.obj, ScaleMode.ScaleToFit);
                                }
                                EditorGUILayout.EndVertical();
                            }

                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.TextField(textureInfo.NewObj.width + "*" + textureInfo.NewObj.height);
                                Rect rect = GUILayoutUtility.GetRect(100, 100);
                                if (Event.current.type == EventType.Repaint)
                                {
                                    GUI.DrawTexture(rect, textureInfo.NewObj, ScaleMode.ScaleToFit);
                                }
                                EditorGUILayout.EndVertical();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    if(textureInfo.PixelType == 1)
                    {
                        if (textureInfo.Cut)
                        {
                            EditorGUILayout.HelpBox("已切割", MessageType.Info);
                        }
                        else
                        {
                            if (GUILayout.Button("切割"))
                            {
                                if (UIUtility.SaveTextureTo(textureInfo.TargetObj, textureInfo.Path))
                                {
                                    textureInfo.Cut = true;
                                    TextureImporter importer = AssetImporter.GetAtPath(textureInfo.Path) as TextureImporter;
                                    int left = 0, right = 0, top = 0, bottom = 0;
                                    if (textureInfo.NewRepeat[0, 1] > 0)
                                    {
                                        left = textureInfo.NewRepeat[0, 0] + 1;
                                        right = textureInfo.NewObj.width - textureInfo.NewRepeat[0, 0] - textureInfo.NewRepeat[0, 1];
                                    }

                                    if (textureInfo.NewRepeat[1, 1] > 0)
                                    {
                                        top = textureInfo.NewRepeat[1, 0] + 1;
                                        bottom = textureInfo.NewObj.height - textureInfo.NewRepeat[1, 0] - textureInfo.NewRepeat[1, 1];
                                    }
                                    importer.spriteBorder = new Vector4(left, bottom, right, top);
                                    importer.SaveAndReimport();
                                    AssetDatabase.Refresh();
                                    modifiedFileList.Add(textureInfo.Path);
                                    modifiedFileList.Add(textureInfo.Path + ".meta");
                                }
                            }
                        }
                    }
                    GUILayout.Space(50);
                }

                

            }
            else
            {
                GUILayout.Space(50);
                EditorGUILayout.LabelField("不需要移除任何资源");
                GUILayout.Space(50);
            }
        }
        if (checkCoroutine != null)
        {
            EditorGUILayout.HelpBox("正在检查资源...", MessageType.Info);
        }
        else
        {
            if (GUILayout.Button("重新检查"))
            {
                checkCoroutine = StandardCoroutine();
            }
        }
        CommonLayoutEnd(checkCoroutine == null);
    }

    private void OnStandardEnd(bool skip)
    {

    }


    private void OnMergeStart()
    {
        scrollPosition = Vector2.zero;
        checkCoroutine = MergeCoroutine();
    }

    private void OnMergeUpdate()
    {
        CommonLayoutStart("窗口助手", "检查公共归并：根据引用次数和分辨率来判断资源是否能够归并到公共资源中，有则修改相同资源的所有引用，点击下一步来整理");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        {
            if (checkCoroutine != null || mergeList.Count > 0 || mergeCommonList.Count>0)
            {
                foreach (TextureInfo textureInfo in mergeList)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        textureInfo.Check = GUILayout.Toggle(textureInfo.Check, string.Empty);
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.ObjectField(textureInfo.obj, typeof(Texture2D), false);
                            EditorGUILayout.TextField(textureInfo.Path);
                            EditorGUILayout.TextField(textureInfo.obj.width + "*" + textureInfo.obj.height);
                            Rect rect = GUILayoutUtility.GetRect(100, 100);
                            if (Event.current.type == EventType.Repaint)
                            {
                                GUI.DrawTexture(rect, textureInfo.obj, ScaleMode.ScaleToFit);
                            }
                            EditorGUILayout.EndVertical();
                        }

                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.ObjectField(textureInfo.NewObj, typeof(Texture2D), false);
                            EditorGUILayout.TextField(textureInfo.NewPath);
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    if (textureInfo.TextureList.Count > 0)
                    {
                        EditorGUILayout.LabelField("其他地方相同的图片");
                        foreach(string texture in textureInfo.TextureList)
                        {
                            EditorGUILayout.TextField(texture);
                        }
                    }
                    if (textureInfo.PrefabList.Count > 0)
                    {
                        EditorGUILayout.LabelField(string.Format("总共被{0}个prefab引用过，被引用到的prefab", textureInfo.PrefabList.Count));
                        foreach (string prefab in textureInfo.PrefabList)
                        {
                            EditorGUILayout.TextField(prefab);
                        }
                    }
                    GUILayout.Space(50);
                }
                if (mergeCommonList.Count > 0)
                {
                    EditorGUILayout.HelpBox("胰腺癌资源有公共资源（但未使用）因此不做判断：", MessageType.Info);
                    foreach(TextureInfo textureInfo in mergeCommonList)
                    {
                        EditorGUILayout.ObjectField(textureInfo.obj, typeof(Texture2D), false);
                        EditorGUILayout.TextField(textureInfo.Path);
                        EditorGUILayout.TextField(textureInfo.obj.width + "*" + textureInfo.obj.height);
                        Rect rect = GUILayoutUtility.GetRect(100, 100);
                        if (Event.current.type == EventType.Repaint)
                        {
                            GUI.DrawTexture(rect, textureInfo.obj, ScaleMode.ScaleToFit);
                        }
                        GUILayout.Space(50);
                    }
                }
            }
            else
            {
                GUILayout.Space(50);
                EditorGUILayout.LabelField("没有找到任何被引用的资源");
                GUILayout.Space(50);
            }
        }
        if (checkCoroutine != null)
        {
            EditorGUILayout.HelpBox("正在检查资源...", MessageType.Info);
        }
        CommonLayoutEnd(checkCoroutine == null);
    }

    private void OnMergeEnd(bool skip)
    {
        if (!skip)
        {
            ProcessTextureInfo(mergeList);
        }
    }


    private void OnMergeRevertStart()
    {
        scrollPosition = Vector2.zero;
        checkCoroutine = MergeRevertCoroutine();
    }

    private void OnMergeRevertUpdate()
    {
        CommonLayoutStart("窗口助手", "检查公共反向归并：检查已归并的资源，根据引用次数和分辨率来判断资源是否符合条件，如有不符合的则复制到各个引用的prefab的文件夹下，点击下一步来整理");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        {
            if (checkCoroutine != null || mergeRevertList.Count > 0)
            {
                foreach (TextureInfo textureInfo in mergeRevertList)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        textureInfo.Check = GUILayout.Toggle(textureInfo.Check, string.Empty);
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.ObjectField(textureInfo.obj, typeof(Texture2D), false);
                            EditorGUILayout.TextField(textureInfo.Path);
                            EditorGUILayout.TextField(textureInfo.obj.width + "*" + textureInfo.obj.height);
                            Rect rect = GUILayoutUtility.GetRect(100, 100);
                            if (Event.current.type == EventType.Repaint)
                            {
                                GUI.DrawTexture(rect, textureInfo.obj, ScaleMode.ScaleToFit);
                            }
                            if (textureInfo.TextureList.Count > 0)
                            {
                                EditorGUILayout.LabelField("其他地方相同的图片");
                                foreach (string texture in textureInfo.TextureList)
                                {
                                    EditorGUILayout.TextField(texture);
                                }
                            }
                            if (textureInfo.PrefabList.Count > 0)
                            {
                                EditorGUILayout.LabelField(string.Format("总共被{0}个prefab引用过，被引用到的prefab", textureInfo.PrefabList.Count));
                                foreach (string prefab in textureInfo.PrefabList)
                                {
                                    EditorGUILayout.TextField(prefab);
                                }
                            }

                            EditorGUILayout.EndVertical();
                        }


                        EditorGUILayout.EndHorizontal();
                    }

                    GUILayout.Space(50);
                }
            }
            else
            {
                GUILayout.Space(50);
                EditorGUILayout.LabelField("没有找到任何被引用的资源");
                GUILayout.Space(50);
            }
        }
        if (checkCoroutine != null)
        {
            EditorGUILayout.HelpBox("正在检查资源...", MessageType.Info);
        }
        CommonLayoutEnd(checkCoroutine == null);
    }

    private void OnMergeRevertEnd(bool skip)
    {
        if (!skip)
        {
            ProcessTextureInfo(mergeRevertList);
        }
    }


    private void OnSummaryStart()
    {
        scrollPosition = Vector2.zero;
        modifiedFileList.Sort();
        List<string> addedAndRemveFileList = new List<string>();
        foreach(string addedFIle in addedFileList)
        {
            if (removedFileList.Contains(addedFIle))
            {
                addedAndRemveFileList.Add(addedFIle);
            }
        }

        foreach(string file in addedAndRemveFileList)
        {
            addedFileList.Remove(file);
            removedFileList.Remove(file);
        }

        addedFileList.Sort();
        removedFileList.Sort();
    }

    private void OnSummaryUpdate()
    {

        CommonLayoutStart("窗口助手", "整理结束L下面列出了本次整理涉及到的prefab和资源");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        {
            if (modifiedFileList.Count > 0)
            {
                EditorGUILayout.LabelField("以下文件被修改：");
                foreach(string modefiedFile in modifiedFileList)
                {
                    EditorGUILayout.TextField(modefiedFile);
                }
                GUILayout.Space(50);
            }

            if (addedFileList.Count > 0)
            {
                EditorGUILayout.LabelField("以下资源或者文件夹被新建：");
                foreach (string addedFile in addedFileList)
                {
                    EditorGUILayout.TextField(addedFile);
                }
                GUILayout.Space(50);
            }

            if (removedFileList.Count > 0)
            {
                EditorGUILayout.LabelField("以下资源或者文件夹被移除：");
                foreach (string removeFile in removedFileList)
                {
                    EditorGUILayout.TextField(removeFile);
                }
                GUILayout.Space(50);
            }

            EditorGUILayout.EndScrollView();
        }
        CommonLayoutEnd(false, false);
    }

    private void OnSummaryEnd(bool skip)
    {

    }

    #endregion


    #region 资源检索协程


    private IEnumerator PrefabCheckCoroutine()
    {
        sw.Reset();
        sw.Start();
        int checkNumber = 0;
        prefabCheckList.Clear();
        List<string> newPathList = new List<string>();
        yield return null;
        List<string> prefabList = new List<string> { windowPath };
        prefabList.AddRange(subPrefabPathList);
        
        foreach(string prefab in prefabList)
        {
            foreach(string texture in UIUtility.GetReference(prefab, typeof(Texture2D)))
            {
                if (!prefabCheckList.Exists(textureInfo => textureInfo.Path == texture))
                {
                    int flag = 0;
                    string newPath = string.Empty;
                    if(!texture.StartsWith(windowUIPath+UIUtility.PATH_S)&&!texture.StartsWith(UIUtility.PATH_UI_STATIC_COMMON)&&
                        !texture.StartsWith(UIUtility.PATH_BACKGROUND_UI) && !texture.StartsWith(UIUtility.PATH_BACKGROUND_SCENE) &&
                        !texture.StartsWith(UIUtility.PATH_S_BUILD_IN_ASSETS))
                    {
                        string findWindowTexture = UIUtility.FindSameTexture(texture, windowUIPath);
                        if (findWindowTexture != null)
                        {
                            flag = 1;
                            newPath = findWindowTexture;
                        }
                        else
                        {
                            TextureInfo findCopyTextureInfo = null;
                            foreach(TextureInfo textureInfo in prefabCheckList)
                            {
                                if(!string.IsNullOrEmpty(textureInfo.NewPath) && UIUtility.IsSameTexture(textureInfo.Path, texture))
                                {
                                    findCopyTextureInfo = textureInfo;
                                    break;
                                }
                            }
                            if (findCopyTextureInfo != null)
                            {
                                flag = 2;
                                newPath = findCopyTextureInfo.NewPath;
                            }
                            else
                            {
                                flag = 2;
                                newPath = windowUIPath + UIUtility.PATH_S + Path.GetFileName(texture);
                                newPath = UIUtility.UniquePath(newPath, newPathList);
                                newPathList.Add(newPath);
                            }
                        }
                        yield return null;
                    }
                    prefabCheckList.Add(TextureInfo.NewReplaceInfo(texture, flag, newPath));
                    checkNumber++;
                }
            }
            
        }
        prefabCheckList.Sort();
        Focus();
        sw.Stop();
        long averageTime = checkNumber == 0 ? 0 : sw.ElapsedMilliseconds / checkNumber;
        Debug.LogFormat("窗口助手{0}阶段检索{1}个资源，共耗时{2}毫秒，平均耗时{3}毫秒", status, checkNumber, sw.ElapsedMilliseconds, averageTime);
    }

    private IEnumerator FolderCommonCheckCoroutine()
    {
        sw.Reset();
        sw.Start();
        int checkNumber = 0;
        folderReferenceList.Clear();
        yield return null;
        foreach(string texture in GetAllRelativeTexture())
        {
            string findCommonTexture = UIUtility.FindSameTexture(texture, UIUtility.PATH_UI_STATIC_COMMON);
            if (findCommonTexture != null)
            {
                folderCommonCheckList.Add(TextureInfo.NewReplaceInfo(texture, 3, findCommonTexture));
            }
            checkNumber++;
            yield return null;
        }
        folderCommonCheckList.Sort();
        Focus();
        sw.Stop();
        long averageTime = checkNumber == 0 ? 0 : sw.ElapsedMilliseconds / checkNumber;
        Debug.LogFormat("窗口助手{0}阶段检索{1}个资源，共耗时{2}毫秒，平均耗时{3}毫秒", status, checkNumber, sw.ElapsedMilliseconds, averageTime);
    }


    private IEnumerator FolderReferenceCoroutine()
    {
        sw.Reset();
        sw.Start();
        int checkNumber = 0;
        folderReferenceList.Clear();
        yield return null;
        foreach (string texture in GetAllRelativeTexture())
        {
            List<string> otherReferenceList = new List<string>();
            List<string> prefabList = UIUtility.FindReferencePrefab(texture);
            foreach(string prefab in prefabList)
            {
                if(prefab!= windowPath && !subPrefabPathList.Contains(prefab))
                {
                    otherReferenceList.Add(prefab);
                }
            }

            if (otherReferenceList.Count > 0)
            {
                folderReferenceList.Add(TextureInfo.NewReferenceInfo(texture, otherReferenceList));
            }
        }
        folderCommonCheckList.Sort();
        Focus();
        sw.Stop();
        long averageTime = checkNumber == 0 ? 0 : sw.ElapsedMilliseconds / checkNumber;
        Debug.LogFormat("窗口助手{0}阶段检索{1}个资源，共耗时{2}毫秒，平均耗时{3}毫秒", status, checkNumber, sw.ElapsedMilliseconds, averageTime);
    }

    private IEnumerator MergeCoroutine()
    {
        sw.Reset();
        sw.Start();
        int checkNumber = 0;
        mergeList.Clear();
        mergeCommonList.Clear();
        List<string> newPathList = new List<string>();
        yield return null;
        foreach (string texture in GetAllRelativeTexture())
        {
            string sameCommonTexture = UIUtility.FindSameTexture(texture, UIUtility.PATH_UI_STATIC_COMMON);
            if(sameCommonTexture!= null)
            {
                mergeCommonList.Add(TextureInfo.NewReplaceInfo(texture, 0, string.Empty));
            }
            else
            {
                List<string> sameTextureList, referencePrefabList;
                if(UIUtility.IsTextureShouldMerge(texture,out sameTextureList,out referencePrefabList, prefabRelationDict))
                {
                    string newPath = UIUtility.PATH_UI_STATIC_COMMON + Path.GetFileName(texture);
                    newPath = UIUtility.UniquePath(newPath, newPathList);
                    newPathList.Add(newPath);
                    mergeCommonList.Add(TextureInfo.NewMergeInfo(texture, 7, newPath, sameTextureList, referencePrefabList));
                }
            }

            checkNumber++;
            yield return null;
        }
        mergeList.Sort();
        Focus();
        sw.Stop();
        long averageTime = checkNumber == 0 ? 0 : sw.ElapsedMilliseconds / checkNumber;
        Debug.LogFormat("窗口助手{0}阶段检索{1}个资源，共耗时{2}毫秒，平均耗时{3}毫秒", status, checkNumber, sw.ElapsedMilliseconds, averageTime);
    }


    private IEnumerator MergeRevertCoroutine()
    {
        sw.Reset();
        sw.Start();
        int checkNumber = 0;
        mergeRevertList.Clear();
        yield return null;
        foreach (string texture in GetAllRelativeTexture())
        {
            List<string> sameTextureList, referencePrefabList;
            if (UIUtility.IsTextureShouldMerge(texture, out sameTextureList, out referencePrefabList, prefabRelationDict))
            {
                mergeRevertList.Add(TextureInfo.NewMergeInfo(texture, 8, null, sameTextureList, referencePrefabList));
            }
            checkNumber++;
            yield return null;
        }
        mergeRevertList.Sort();
        Focus();
        sw.Stop();
        long averageTime = checkNumber == 0 ? 0 : sw.ElapsedMilliseconds / checkNumber;
        Debug.LogFormat("窗口助手{0}阶段检索{1}个资源，共耗时{2}毫秒，平均耗时{3}毫秒", status, checkNumber, sw.ElapsedMilliseconds, averageTime);
    }

    private IEnumerator RemoveCoroutine()
    {
        sw.Reset();
        sw.Start();
        int checkNumber = 0;
        removeList.Clear();
        yield return null;
        foreach (string texture in toRemoveFileList)
        {
            List<string> prefabList = UIUtility.FindReferencePrefab(texture);
            removeList.Add(TextureInfo.NewRemoveInfo(texture, prefabList));
            checkNumber++;
            yield return null;
        }
        foreach(string texture in GetAllRelativeTexture())
        {
            if (!toRemoveFileList.Contains(texture))
            {
                List<string> prefabList = UIUtility.FindReferencePrefab(texture);
                if (prefabList.Count <= 0)
                {
                    removeList.Add(TextureInfo.NewRemoveInfo(texture, prefabList));
                }
                checkNumber++;
                yield return null;
            }
        }
        
        removeList.Sort();
        Focus();
        sw.Stop();
        long averageTime = checkNumber == 0 ? 0 : sw.ElapsedMilliseconds / checkNumber;
        Debug.LogFormat("窗口助手{0}阶段检索{1}个资源，共耗时{2}毫秒，平均耗时{3}毫秒", status, checkNumber, sw.ElapsedMilliseconds, averageTime);
    }



    private IEnumerator StandardCoroutine()
    {
        sw.Reset();
        sw.Start();
        int checkNumber = 0;
        standardList.Clear();
        yield return null;
        foreach (string texture in GetAllRelativeTexture())
        {
            int[,] repeat = new int[2, 2];
            int[] loop = new int[2];
            bool[] summetry = new bool[2];
            int checkResult = UIUtility.TexturePixelCheck(texture, out repeat, out loop, out summetry, false, true);
            int pixelType = 0;
            if ((checkResult & 1) > 0)
            {
                pixelType = 1;
            }
            else if ((checkResult & 2) > 0) 
            {
                pixelType = 2;
            }
            else if((checkResult & 4) > 0)
            {
                if (summetry[0])
                {
                    pixelType = 3;
                }
                else
                {
                    pixelType = 4;
                }
            }
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9_\-]+$");
            bool nameIllegal = !regex.IsMatch(Path.GetFileNameWithoutExtension(texture));
            bool[] sizeIllegal = new bool[2];
            bool inAtlas = UIUtility.IsTextureShouldAtlas(texture);
            if (!inAtlas)
            {
                int[] size = UIUtility.GetTextureCompressedSize(texture);
                if(size[0] % 4 != 0)
                {
                    sizeIllegal[0] = true;
                }
                if (size[1] % 4 != 0)
                {
                    sizeIllegal[1] = true;
                }
            }
            if (pixelType > 0 || nameIllegal || sizeIllegal[0] || sizeIllegal[1])
            {
                standardList.Add(TextureInfo.NewStandardInfo(texture, pixelType, repeat, loop, summetry, nameIllegal, sizeIllegal));
            }
            checkNumber++;
            yield return null;

        }

        List<string> dynamicTextureList = new List<string>();
        foreach(string dynamicUIPath in dynamicUIPathList)
        {
            dynamicTextureList.AddRange(UIUtility.GetAllTexture(dynamicUIPath));
        }
        foreach(string texture in dynamicTextureList)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9_\-]+$");
            bool nameIllegal = !regex.IsMatch(Path.GetFileNameWithoutExtension(texture));
            bool[] sizeIllegal = new bool[2];
            bool inAtlas = UIUtility.IsTextureShouldAtlas(texture);
            if (!inAtlas)
            {
                int[] size = UIUtility.GetTextureCompressedSize(texture);
                if (size[0] % 4 != 0)
                {
                    sizeIllegal[0] = true;
                }
                if (size[1] % 4 != 0)
                {
                    sizeIllegal[1] = true;
                }
            }
            if ( nameIllegal || sizeIllegal[0] || sizeIllegal[1])
            {
                standardList.Add(TextureInfo.NewStandardInfo(texture, 0, new int[2,2], new int[2], new bool[2], nameIllegal, sizeIllegal));
            }
            checkNumber++;
            yield return null;
        }
        standardList.Sort();
        Focus();
        sw.Stop();
        long averageTime = checkNumber == 0 ? 0 : sw.ElapsedMilliseconds / checkNumber;
        Debug.LogFormat("窗口助手{0}阶段检索{1}个资源，共耗时{2}毫秒，平均耗时{3}毫秒", status, checkNumber, sw.ElapsedMilliseconds, averageTime);
    }

    #endregion


    #region 功能函数


    private void CommonLayoutStart(string title,string helpContent)
    {
        titleContent = new GUIContent(title);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(50);
        EditorGUILayout.BeginVertical();
        GUILayout.Space(20);
        if(status!=Status.None && status != Status.WindowAssign && windowPath != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("正在整理窗口：");
            EditorGUILayout.BeginVertical();
            EditorGUILayout.ObjectField(window, typeof(GameObject), false);
            EditorGUILayout.TextField(windowPath);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.HelpBox(helpContent, MessageType.Info);
    }


    private void CommonLayoutEnd(bool showNext = true,bool showSkip = true,bool showReset = true)
    {
        if(showNext && GUILayout.Button("下一步"))
        {
            NextState();
        }

        if(showSkip && GUILayout.Button("跳过"))
        {
            NextState(true);
        }

        if(showReset && GUILayout.Button("重新开始"))
        {
            if(status == Status.Summary || EditorUtility.DisplayDialog("重新开始", "即使重新开始，你在之前的步骤所做的修改也不会回退，确定要重新开始吗", "确定", "取消"))
            {
                status = Status.None;
            }
        }

        GUILayout.Space(50);
        EditorGUILayout.EndVertical();
        GUILayout.Space(20);
        EditorGUILayout.EndHorizontal();
    }

    private void ProcessTextureInfo(List<TextureInfo> textureList)
    {
        AssetDatabase.StartAssetEditing();
        foreach(TextureInfo textureInfo in textureList)
        {
            if (textureInfo.Check)
            {
                switch (textureInfo.Flag)
                {
                    case 1:
                        ChangeReference(windowPath, textureInfo.Path, textureInfo.NewPath);
                        foreach(string subPrefabPath in subPrefabPathList)
                        {
                            ChangeReference(subPrefabPath, textureInfo.Path, textureInfo.NewPath);
                        }
                        toRemoveFileList.Add(textureInfo.Path);
                        break;
                    case 2:
                        CopyFile(textureInfo.Path, textureInfo.NewPath);
                        ChangeReference(windowPath, textureInfo.Path, textureInfo.NewPath);
                        foreach (string subPrefabPath in subPrefabPathList)
                        {
                            ChangeReference(subPrefabPath, textureInfo.Path, textureInfo.NewPath);
                        }
                        toRemoveFileList.Add(textureInfo.Path);
                        break;
                    case 3:
                        List<string> referencedPrefabList = UIUtility.FindReferencePrefab(textureInfo.Path);
                        foreach(string prefab in referencedPrefabList)
                        {
                            ChangeReference(prefab, textureInfo.Path, textureInfo.NewPath);
                        }
                        toRemoveFileList.Add(textureInfo.Path);
                        break;
                    case 4:
                        foreach(string prefab in textureInfo.PrefabList)
                        {
                            string rootPrefab = prefab;
                            if (prefabRelationDict.ContainsKey(prefab))
                            {
                                rootPrefab = prefabRelationDict[prefab];
                            }
                            string findSameTexture = UIUtility.FindSameTexture(textureInfo.Path, UIUtility.PATH_S_UI_STATIC + Path.GetFileNameWithoutExtension(rootPrefab));
                            if (findSameTexture != null)
                            {
                                ChangeReference(prefab, textureInfo.Path, textureInfo.NewPath);
                            }
                            else
                            {
                                string newPath = UIUtility.PATH_S_UI_STATIC + Path.GetFileNameWithoutExtension(rootPrefab) + UIUtility.PATH_S + Path.GetFileName(textureInfo.Path);
                                newPath = UIUtility.UniquePath(newPath);
                                CopyFile(textureInfo.Path, newPath);
                                ChangeReference(prefab, textureInfo.Path, newPath);
                            }
                        }
                        break;
                    case 5:
                        RemoveFile(textureInfo.Path);
                        break;
                    case 7:
                        CopyFile(textureInfo.Path, textureInfo.NewPath);
                        List<string> referencedPrefab = UIUtility.FindReferencePrefab(textureInfo.Path);
                        foreach (string prefab in referencedPrefab)
                        {
                            ChangeReference(prefab, textureInfo.Path, textureInfo.NewPath);
                        }
                        bool removed = RemoveFile(textureInfo.Path);
                        if (!removed)
                        {
                            Debug.LogErrorFormat("资源移除失败：{0}", textureInfo.Path);
                        }
                        foreach(string texture in textureInfo.TextureList)
                        {
                            referencedPrefab = UIUtility.FindReferencePrefab(texture);
                            foreach(string prefab in referencedPrefab)
                            {
                                ChangeReference(prefab, texture, textureInfo.NewPath);
                            }
                            removed = RemoveFile(texture);
                            if (!removed)
                            {
                                Debug.LogErrorFormat("资源移除失败：{0}", textureInfo.Path);
                            }
                        }
                        break;
                    case 8:
                        referencedPrefab = UIUtility.FindReferencePrefab(textureInfo.Path);
                        foreach(string prefab in referencedPrefab)
                        {
                            string rootPrefab = prefab;
                            if (prefabRelationDict.ContainsKey(prefab))
                            {
                                rootPrefab = prefabRelationDict[prefab];
                            }
                            string findSameTexture = UIUtility.FindSameTexture(textureInfo.Path, UIUtility.PATH_S_UI_STATIC + Path.GetFileNameWithoutExtension(rootPrefab));
                            if (findSameTexture != null)
                            {
                                ChangeReference(prefab, textureInfo.Path, textureInfo.NewPath);
                            }
                            else
                            {
                                string newPath = UIUtility.PATH_S_UI_STATIC + Path.GetFileNameWithoutExtension(rootPrefab) + UIUtility.PATH_S + Path.GetFileName(textureInfo.Path);
                                newPath = UIUtility.UniquePath(newPath);
                                CopyFile(textureInfo.Path, newPath);
                                ChangeReference(prefab, textureInfo.Path, newPath);
                            }
                        }
                        RemoveFile(textureInfo.Path);
                        break;
                    default:
                        break;
                }
            }
        }

        AssetDatabase.StopAssetEditing();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private List<string> GetAllRelativeTexture()
    {
        List<string> textureList = new List<string>();
        if (Directory.Exists(windowUIPath))
        {
            textureList.AddRange(UIUtility.GetAllTexture(windowUIPath));
        }
        foreach(string subPrefabUIPath in subPrefabUIPathLsit)
        {
            textureList.AddRange(UIUtility.GetAllTexture(subPrefabUIPath));
        }
        return textureList;
    }

    private bool ChangeReference(string prefab,string oldPath,string newPath,bool addToList = true)
    {
        bool changed = UIUtility.ChangeReference(prefab, oldPath, newPath);
        if (changed)
        {
            if(addToList&& !modifiedFileList.Contains(prefab))
            {
                modifiedFileList.Add(prefab);
            }
        }
        return changed;
    }

    private bool CopyFile(string oldPath,string newPath,bool addToList = true)
    {
        bool created = false;
        if (!File.Exists(newPath))
        {
            bool directoryExist = false;
            string newPathDirectory = Path.GetDirectoryName(newPath);
            newPathDirectory = UIUtility.UniteSlash(newPathDirectory);
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
                    if(addToList && !addedFileList.Contains(newPathDirectory))
                    {
                        addedFileList.Add(newPathDirectory);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogErrorFormat("[UIAtlasOptimizeTool.CopyFile]文件夹{0}创建失败：{1}", newPathDirectory, e);
                }
            }

            if (directoryExist)
            {
                created = AssetDatabase.CopyAsset(oldPath, newPath);
                if (created && addToList && !addedFileList.Contains(newPathDirectory))
                {
                    addedFileList.Add(newPathDirectory);
                }
            }

        }
        return created;
    }


    private bool RemoveFile(string path , bool addToList = true)
    {
        bool removed = AssetDatabase.DeleteAsset(path);
        if (removed && path.StartsWith(UIUtility.PATH_S_UI))
        {
            if(addToList && removedFileList.Contains(path))
            {
                removedFileList.Add(path);
            }

            string removeDirectory = null;
            string parent = path.Substring(0, path.LastIndexOf(UIUtility.PATH_S));
            while (parent.Length > UIUtility.PATH_UI.Length && Directory.GetFiles(parent, "*", SearchOption.AllDirectories).Length <= 0)
            {
                removeDirectory = parent;
                parent = removeDirectory.Substring(0, removeDirectory.LastIndexOf(UIUtility.PATH_S));
            }
            if (removeDirectory != null)
            {
                bool directorRemoved = AssetDatabase.DeleteAsset(removeDirectory);

                if (directorRemoved && addToList && removedFileList.Contains(path))
                {
                    removedFileList.Add(path);
                }
            }
        }
        return removed;
    }

    private void ClearPrefabRelation(string window)
    {
        List<string> removeKeyList = new List<string>();
        foreach(string key in prefabRelationDict.Keys)
        {
            if(prefabRelationDict[key] == window)
            {
                removeKeyList.Add(key);
            }
        }


        foreach(string removeKey in removeKeyList)
        {
            prefabRelationDict.Remove(removeKey);
        }
    }

    private void AddPrefabRelation(string subPrefab ,string window)
    {
        if (prefabRelationDict.ContainsKey(subPrefab))
        {
            Debug.LogErrorFormat("prefab关系添加出错，{0}已归属于{1}，尝试将其归属于{2}", subPrefab, prefabRelationDict[subPrefab], window);
        }
        else
        {
            prefabRelationDict.Add(subPrefab, window);
        }
    }

    #endregion


}
