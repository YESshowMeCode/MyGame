// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/03 21:19:55
// FileName：Assets/Assets/Editor/UIAtlasOptimize/UITextureSizeFilter.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Linq;

public class UITextureSizeFilter : EditorWindow
{

    private Vector2 scrollPosition = Vector2.zero;
    private bool[] widthSymbol = new bool[] { false, false, true };
    private bool[] heightSymbol = new bool[] { false, false, true };
    private string widthStr = "1920";
    private string heightStr = "1080";
    private bool and = false;
    private bool mergeSame = false;
    private bool afterCompress = false;
    private List<TextureInfo> textureList = new List<TextureInfo>();
    private IEnumerator coroutine;


    private class TextureInfo :System.IComparable<TextureInfo>
    {
        public string Path { get; set; }

        public Texture2D Obj { get; private set; }

        public int[] Size { get; private set; }

        public TextureInfo(string path)
        {
            Path = path;
            Obj = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Size = UIUtility.GetTextureSize(path);
        }

        int System.IComparable<TextureInfo>.CompareTo(TextureInfo other)
        {
            return 0 + Path.CompareTo(other.Path);
        }

    }


    [MenuItem("Tools/图集工具/尺寸筛选")]
    private static void SizeFilter()
    {
        GetWindow<UITextureSizeFilter>(typeof(UITextureSizeFilter));
    }

    private void Update()
    {
        if (coroutine != null)
        {
            if(!coroutine.MoveNext())
            {
                coroutine = null;
            }
        }
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
                    EditorGUILayout.LabelField("宽度", GUILayout.Width(50));
                    widthSymbol[0] = GUILayout.Toggle(widthSymbol[0], "<", GUILayout.Width(50));
                    widthSymbol[1] = GUILayout.Toggle(widthSymbol[1], "=", GUILayout.Width(50));
                    widthSymbol[2] = GUILayout.Toggle(widthSymbol[2], ">", GUILayout.Width(50));
                    widthStr = EditorGUILayout.TextField(widthStr);
                    EditorGUILayout.EndHorizontal();
                }


                EditorGUILayout.BeginHorizontal();
                {
                    and = GUILayout.Toggle(and, "并且", GUILayout.Width(50));
                    and = !GUILayout.Toggle(!and, "或者", GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("高度", GUILayout.Width(50));
                    heightSymbol[0] = GUILayout.Toggle(heightSymbol[0], "<", GUILayout.Width(50));
                    heightSymbol[1] = GUILayout.Toggle(heightSymbol[1], "=", GUILayout.Width(50));
                    heightSymbol[2] = GUILayout.Toggle(heightSymbol[2], ">", GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                }

                mergeSame = GUILayout.Toggle(mergeSame, "合并同样的图片");
                afterCompress = GUILayout.Toggle(afterCompress, "压缩后尺寸");
                if (GUILayout.Button("搜索"))
                {
                    ResetSeach();
                }

                EditorGUILayout.HelpBox("筛选出满足条件的静态图片", MessageType.Info);
                if (coroutine != null || textureList.Count > 0)
                {
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                    {
                        foreach(TextureInfo textureInfo in textureList)
                        {
                            EditorGUILayout.ObjectField(textureInfo.Obj, typeof(Texture2D), false);
                            EditorGUILayout.TextField(textureInfo.Path);


                            if (afterCompress)
                            {
                                EditorGUILayout.TextField(textureInfo.Obj.width + "*" + textureInfo.Obj.height);
                            }
                            else
                            {
                                EditorGUILayout.TextField(textureInfo.Size[0] + "*" + textureInfo.Size[1]);
                            }
                            Rect rect = GUILayoutUtility.GetRect(100, 100);

                            if(Event.current.type == EventType.Repaint)
                            {
                                GUI.DrawTexture(rect, textureInfo.Obj, ScaleMode.ScaleToFit);
                            }
                           
                        }
                        EditorGUILayout.EndScrollView();
                        

                    }
                    if (coroutine != null)
                    {
                        EditorGUILayout.HelpBox("正在检查资源。。。", MessageType.Info);
                    }
                }
                else
                {
                    GUILayout.Space(50);
                    EditorGUILayout.LabelField("没有找到任何东西");
                    GUILayout.Space(50);
                }

                GUILayout.Space(20);
                EditorGUILayout.EndVertical();
            }
            GUILayout.Space(50);
            EditorGUILayout.EndHorizontal();
        }
        
       
    }


    private void ResetSeach()
    {
        int width, height = 0;
        if(int.TryParse(widthStr,out width)&& int.TryParse(heightStr,out height))
        {
            coroutine = SeachCoroutine(width, height);
        }
    }


    private IEnumerator SeachCoroutine(int width,int height)
    {
        textureList.Clear();
        yield return null;

        List<string> pathList = Selection.objects.Where(obj => obj is DefaultAsset).Select(obj => AssetDatabase.GetAssetPath((DefaultAsset)obj)).ToList();
        pathList = pathList.Where(selectionPath => Directory.Exists(selectionPath)).ToList();

        if (pathList.Count <= 0)
        {
            pathList.Add(UIUtility.PATH_UI);

        }

        List<string> allTextureList = new List<string>();
        foreach(string path in pathList)
        {
            allTextureList.AddRange(UIUtility.GetAllTexture(path));
        }

        foreach(string texture in allTextureList)
        {
            int[] size = afterCompress ? UIUtility.GetTextureCompressedSize(texture) : UIUtility.GetTextureSize(texture);
            bool widthTrue = widthSymbol[0] && size[0] < width || widthSymbol[1] && (size[0] == width) || widthSymbol[2] && size[0] > width;
            bool heightTrue = heightSymbol[0] && size[1] < height || heightSymbol[1] && (size[1] == height) || heightSymbol[2] && size[1] > height;

            if (and ? widthTrue && heightTrue : widthTrue || heightTrue)
            {
                if (!mergeSame || !textureList.Exists(_texture => UIUtility.IsSameTexture(_texture.Path, texture))){
                    textureList.Add(new TextureInfo(texture));
                }
            }
            yield return null;
         }
        textureList.Sort();
    }

}
