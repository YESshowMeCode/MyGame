// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/03 21:16:32
// FileName：Assets/Assets/Editor/UIAtlasOptimize/UIAtlasLookOptimizeTool.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class UIAtlasLookOptimizeTool : EditorWindow
{
    private List<string> checkType = new List<string>();
    private Dictionary<string, List<string>> mapReferenceOtherModulePref = new Dictionary<string, List<string>>();
    private Vector2 scrollPos = Vector2.zero;
    private GameObject target;
    private bool showAll = false;

    [MenuItem("Tools/图集工具/图集查看工具")]
    private static void Init()
    {
        UIAtlasLookOptimizeTool wnd = EditorWindow.GetWindow(typeof(UIAtlasLookOptimizeTool)) as UIAtlasLookOptimizeTool;

    }

    private void StartCheck()
    {
        mapReferenceOtherModulePref.Clear();
        string strFile = AssetDatabase.GetAssetPath(target);
        string[] dependencesFile = AssetDatabase.GetDependencies(strFile);

        List<string> OtherFIle = null;
        string txt = null;
        foreach (string depFile in dependencesFile)
        {
            bool isNeedShow = false;
            foreach (string type in checkType)
            {

                //存在设置类型，需要显示
                if (depFile.IndexOf(type) > -1)
                {
                    isNeedShow = true;
                    break;
                }
            }

            if (isNeedShow == false)
            {
                continue;
            }

            if (OtherFIle == null)
            {
                if (!mapReferenceOtherModulePref.TryGetValue(strFile, out OtherFIle))
                {
                    OtherFIle = new List<string>();
                    mapReferenceOtherModulePref.Add(strFile, OtherFIle);
                }
            }

            if (txt == null)
            {
                string fullPath = Path.GetFullPath(strFile);
                txt = File.ReadAllText(fullPath);
            }

            string ugui = AssetDatabase.AssetPathToGUID(depFile);
            int ugui_index = txt.IndexOf(ugui);

            while (ugui_index > 0)
            {
                //8是"m_=Name："的长度
                int fileIDIndex = 8 + txt.IndexOf("fileID:", txt.LastIndexOf("m_GameObject:", ugui_index));
                int fileIDendIndex = txt.IndexOf("}", fileIDIndex);
                string fileID = txt.Substring(fileIDIndex, fileIDendIndex - fileIDIndex);
                int nameIndex = 8 + txt.IndexOf("m_Name:", txt.IndexOf("&" + fileID));
                int endNameIndex = txt.IndexOf("\n", nameIndex);
                string gameObjectName = txt.Substring(nameIndex, endNameIndex - nameIndex);
                string totalStr = gameObjectName + "#" + depFile;
                string[] totalStrArr = totalStr.Split('/');
                int maxCount = 0;
                int index = -1;
                int targetIndex = 0;

                foreach (string nowFile in OtherFIle)
                {
                    index++;
                    string[] splitArr = nowFile.Split('/');
                    int sameCount = 0;
                    for (var i = 0; i < splitArr.Length; i++)
                    {
                        if (i == 0)
                        {
                            continue;

                        }

                        for (int j = 0; j < totalStrArr.Length; j++)
                        {
                            if (j == 0)
                            {
                                continue;
                            }
                            if (splitArr[i] == totalStrArr[j])
                            {
                                sameCount++;
                            }
                        }
                    }

                    if (sameCount > maxCount)
                    {
                        targetIndex = index;
                        maxCount = sameCount;
                    }

                }

                OtherFIle.Insert(targetIndex, totalStr);
                if (showAll)
                {
                    ugui_index = txt.IndexOf(ugui, ugui_index + 1);
                }
                else
                {
                    break;
                }

            }
            
        }
        EditorUtility.ClearProgressBar();
    }


        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("拖拽UIPerfab：", GUILayout.Width(80));
                    GameObject nObject = (GameObject)EditorGUILayout.ObjectField(target, typeof(GameObject), false);
                    if (nObject != target)
                    {
                        target = nObject;
                    }

                    if (GUILayout.Button("[Png 和 JPG]", GUILayout.Width(100)))
                    {
                        checkType.Clear();
                        checkType.Add(".jpg");
                        checkType.Add(".png");
                        StartCheck();
                    }


                    if (GUILayout.Button("[字体]", GUILayout.Width(100)))
                    {
                        checkType.Clear();
                        checkType.Add(".fontsettings");
                        checkType.Add(".ttf");
                        StartCheck();
                    }


                    if (GUILayout.Button("[C#脚本文件]", GUILayout.Width(100)))
                    {
                        checkType.Clear();
                        checkType.Add(".cs");
                        StartCheck();
                    }
                    GUILayout.Space(50);
                    showAll = GUILayout.Toggle(showAll, "显示全部obj", GUILayout.Width(100));
                }
                EditorGUILayout.EndHorizontal();
            }


            EditorGUILayout.BeginVertical();
            {
                EditorGUI.EndChangeCheck();
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                int count = 0;
                foreach (var kv in mapReferenceOtherModulePref)
                {
                    count++;
                    string prefabName = kv.Key;
                    EditorGUILayout.LabelField("" + count + ":" + prefabName);
                    foreach (string strIllegalRes in kv.Value)
                    {
                        int spliterIndex = strIllegalRes.IndexOf("#");
                        string gameObjectName = strIllegalRes.Substring(0, spliterIndex);
                        string resPath = strIllegalRes.Substring(spliterIndex + 1);

                        var obj = AssetDatabase.GetMainAssetTypeAtPath(resPath);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(gameObjectName, GUILayout.Width(200));
                        EditorGUILayout.TextField(resPath, GUILayout.Width(600));
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndScrollView();
            }
        }

    


}
