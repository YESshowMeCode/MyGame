// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/03 21:18:51
// FileName：Assets/Assets/Editor/UIAtlasOptimize/UIReferenceReplacer.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class UIReferenceReplacer : EditorWindow
{

    private Vector2 scrollPosition = Vector2.zero;
    private string fromTexture;
    private Texture2D fromTextureObj;
    private string toTexture;
    private Texture2D toTextureObj;
    private List<GameObject> prefabObjList = new List<GameObject>();
    private bool processed = false;

    [MenuItem("Tools/图集工具/引用替换")]
    private static void ReferenceReplacer()
    {
        GetWindow<UIReferenceReplacer>(typeof(UIReferenceReplacer));
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space(50);
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Space(20);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("把", GUILayout.Width(50));
                    EditorGUILayout.BeginVertical();
                    {
                        Texture2D newTextureObj = EditorGUILayout.ObjectField(fromTextureObj, typeof(Texture2D), false) as Texture2D;
                        if (newTextureObj != fromTextureObj)
                        {
                            fromTextureObj = newTextureObj;
                            fromTexture = AssetDatabase.GetAssetPath(fromTextureObj);
                            prefabObjList.Clear();
                            processed = false;

                        }

                        EditorGUILayout.TextField(fromTexture);
                        EditorGUILayout.TextField(fromTextureObj ? fromTextureObj.width + "*" + fromTextureObj.height : string.Empty);
                        Rect rect = GUILayoutUtility.GetRect(100, 100);
                        if (Event.current.type == EventType.Repaint)
                        {
                            GUI.DrawTexture(rect, fromTextureObj, ScaleMode.ScaleToFit);
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                }


                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("替换成", GUILayout.Width(50));
                    EditorGUILayout.BeginVertical();
                    {
                        Texture2D newTextureObj = EditorGUILayout.ObjectField(toTextureObj, typeof(Texture2D), false) as Texture2D;
                        if(newTextureObj != toTextureObj)
                        {
                            toTextureObj = newTextureObj;
                            toTexture = AssetDatabase.GetAssetPath(toTextureObj);
                            processed = false;
                        }

                        EditorGUILayout.TextField(toTexture);
                        EditorGUILayout.TextField(toTextureObj ? toTextureObj.width + "*" + toTextureObj.height : string.Empty);
                        Rect rect = GUILayoutUtility.GetRect(100, 100);


                        if (Event.current.type == EventType.Repaint)
                        {
                            GUI.DrawTexture(rect, toTextureObj, ScaleMode.ScaleToFit);
                        }
                        EditorGUILayout.EndVertical();

                    }
                    EditorGUILayout.EndHorizontal();
                }


                if (GUILayout.Button("查找目前所有引用"))
                {
                    FindReference();
                }
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    for(int i = 0; i < prefabObjList.Count; i++)
                    {
                        prefabObjList[i] = EditorGUILayout.ObjectField("prefab:", prefabObjList[i], typeof(GameObject), false) as GameObject;
                    }

                    GameObject anotherSubPrefab = EditorGUILayout.ObjectField("prefab:", null, typeof(GameObject), false) as GameObject;

                    if (anotherSubPrefab != null)
                    {
                        prefabObjList.Add(anotherSubPrefab);
                    }
                    EditorGUILayout.EndScrollView();

                }
                if (processed)
                {
                    EditorGUILayout.HelpBox("已替换", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("替换上述所有prefab的引用", MessageType.Info);
                    if (GUILayout.Button("替换"))
                    {
                        Replace();
                    }
                }
                GUILayout.Space(20);
                EditorGUILayout.EndVertical();
            }
            GUILayout.Space(50);
            EditorGUILayout.EndHorizontal();
        }
    }


    private void Replace()
    {
        if (fromTextureObj != null)
        {
            foreach(GameObject prefabObj in prefabObjList)
            {
                string prefab = AssetDatabase.GetAssetPath(prefabObj);
                UIUtility.ChangeReference(prefab, fromTexture, toTexture);
            }
            processed = true;
        }
    }

    private void FindReference()
    {
        if (fromTextureObj != null)
        {
            prefabObjList = UIUtility.FindReferencePrefab(fromTexture).Select(prefab => AssetDatabase.LoadAssetAtPath<GameObject>(prefab)).ToList();
        }
    }

}
