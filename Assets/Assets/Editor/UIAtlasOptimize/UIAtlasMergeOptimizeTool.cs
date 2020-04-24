// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/03 21:16:57
// FileName：Assets/Assets/Editor/UIAtlasOptimize/UIAtlasMergeOptimizeTool.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class UIAtlasMergeOptimizeTool : EditorWindow {

	
    private string UIPath = "Assets/GameDate/UGUI/StaticUI";
    private GameObject[] nObjectArray = new GameObject[12];
    private List<string> MergeFileDepList = new List<string>();

    [MenuItem("Tools/图集工具/图集合并工具")]
    private static void Init()
    {
        UIAtlasMergeOptimizeTool wnd = EditorWindow.GetWindow(typeof(UIAtlasMergeOptimizeTool)) as UIAtlasMergeOptimizeTool;
    }

     void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space(200);
            if (GUILayout.Button("开始合并", GUILayout.Width(200)))
            {
                StartMarge();
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginVertical();
        {
            for(int i = 0; i < nObjectArray.Length; i++)
            {
                nObjectArray[i] = (GameObject)EditorGUILayout.ObjectField(nObjectArray[i], typeof(GameObject), false);
            }
            EditorGUILayout.EndVertical();
        }
    }

    private void StartMarge()
    {
        if (nObjectArray[0] == null)
            return;
        MergeFileDepList.Clear();
        string MergePrefabPath = AssetDatabase.GetAssetPath(nObjectArray[0]);
        for(int i = 0; i < nObjectArray.Length; i++)
        {
            if (nObjectArray[i] != null)
            {
                string perfabPath = AssetDatabase.GetAssetPath(nObjectArray[i]);
                string[] dependenceFile = AssetDatabase.GetDependencies(perfabPath);
                
                foreach(string oldPath in dependenceFile)
                {
                    if (oldPath.IndexOf(".jpg") < 0 && oldPath.IndexOf(".png") < 0)
                    {
                        continue;
                    }

                    if(oldPath.Contains("StaticUI") && !oldPath.Contains("StaticUI/Common"))
                    {
                        string name_1 = GetName(perfabPath);
                        string name_2 = GetName(MergePrefabPath);
                        string newPath = oldPath.Replace(name_1, name_2);
                        CopyFileAndRest(oldPath, newPath, perfabPath);
                    }

                    string filename = UIPath + GetName(perfabPath);
                    DirectoryInfo dir = new DirectoryInfo(filename); ;
                    DeleteDirs(dir);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

    }

    private string GetName(string strFile)
    {
        int start = strFile.LastIndexOf("/") + 1;
        int end = strFile.LastIndexOf(".");
        return strFile.Substring(start, end - start);
    }

    private FileInfo GetFile(string prefabName)
    {
        DirectoryInfo dir = new DirectoryInfo(UIPath);
        FileInfo[] fil = dir.GetFiles();
        DirectoryInfo[] dii = dir.GetDirectories();

        foreach(FileInfo f in fil)
        {
            int end = f.Name.LastIndexOf(".");
            string name = f.Name.Substring(0, end);
            if (name == prefabName)
            {
                Debug.LogError("f.Name: " + name);
                return f;
            }
        }
        return null;
    }


    /// <summary>
    /// 拷贝并且重置资源引用
    /// </summary>
    /// <param name="oldPath"></param>
    /// <param name="newPath"></param>
    /// <param name="prefabPath"></param>
    private void CopyFileAndRest(string oldPath,string newPath,string prefabPath)
    {
        string fullPath = Path.GetFullPath(newPath);
        if (!File.Exists(fullPath))
        {
            AssetDatabase.CopyAsset(oldPath, newPath);
        }

        string oldGuild = AssetDatabase.AssetPathToGUID(oldPath);
        string newGuild = AssetDatabase.AssetPathToGUID(newPath);
        string fullPrefabPath = Path.GetFullPath(prefabPath);
        FileStream filestream = new FileStream(fullPrefabPath, FileMode.Open, FileAccess.ReadWrite);
        StreamReader sr = new StreamReader(filestream);
        string bufStr = sr.ReadToEnd();
        filestream.SetLength(0);
        bufStr = bufStr.Replace(oldGuild, newGuild);
        StreamWriter sw = new StreamWriter(filestream);
        sw.Write(bufStr);
        sw.Flush();
        sw.Close();
        sr.Close();
        filestream.Close();
    }

    private void DeleteDirs(DirectoryInfo dirs)
    {
        if (dirs == null || dirs.Exists)
        {
            return;
        }

        DirectoryInfo[] subDir = dirs.GetDirectories();
        if(subDir != null)
        {
            for(int i = 0; i < subDir.Length; i++)
            {
                if (subDir[i] != null)
                {
                    DeleteDirs(subDir[i]);
                }
            }
        }

        FileInfo[] files = dirs.GetFiles();

        if (files != null)
        {
            for(int i = 0; i < files.Length; i++)
            {
                if (files[i] != null)
                {
                    files[i].Delete();
                    files[i] = null;
                }
            }
            files = null;
        }
        dirs.Delete();
    }
}
