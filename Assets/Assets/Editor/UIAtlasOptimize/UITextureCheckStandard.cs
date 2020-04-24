// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/03 21:19:20
// FileName：Assets/Assets/Editor/UIAtlasOptimize/UITextureCheckStandard.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class UITextureCheckStandard : EditorWindow
{

    private Vector2 scrollPosition = Vector2.zero;
    private List<TextureInfo> textureList = new List<TextureInfo>();
    private IEnumerator checkCoroutine;
    private bool repeatStrict;


    private class TextureInfo : System.IComparable<TextureInfo>
    {
        public string Path { get; set; }

        public Texture2D Obj { get; private set; }

        public bool NameIllegal { get; private set; }

        public bool[] SizeIllegal { get; private set; }

        public int PixelType { get; set; }

        public Texture2D NewObj { get; private set; }

        public Texture2D TargetObj { get; private set; }

        private int[,] newRepeat;

        public int[,] NewRepeat
        {
            get
            {
                return newRepeat;
            }

            private set
            {
                newRepeat = value;
            }
        }

        public bool Cut { get; set; }

        public TextureInfo(string path,int pixelType,int[,] repeat , int[] loop ,bool[] summetry,bool nameIllegal,bool[] sizeIllegal,bool repeatStrict)
        {
            Path = path;
            PixelType = pixelType;
            NameIllegal = nameIllegal;
            SizeIllegal = sizeIllegal;

            switch (pixelType)
            {
                case 1:
                    Obj = UIUtility.TextureMaskRepeat(path, repeat);
                    TargetObj = UIUtility.TextureCut(path, out newRepeat, repeatStrict);
                    NewObj = UIUtility.TextureMaskRepeat(TargetObj, NewRepeat);
                    break;
                case 2:
                    Obj = UIUtility.TextureMarkLoop(path, loop);
                    break;
                default:
                    Obj = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    break;
            }

            
            
        }

        int System.IComparable<TextureInfo>.CompareTo(TextureInfo other)
        {
            return 0 + Path.CompareTo(other.Path);
        }

    }


    [MenuItem("Tools/图集工具/检查规范 %#d")]
    private static void CheckStandard()
    {
        GetWindow<UITextureCheckStandard>(typeof(UITextureCheckStandard));
    }

    private void Update()
    {
        if (checkCoroutine != null)
        {
            if (!checkCoroutine.MoveNext())
            {
                checkCoroutine = null;
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
                if (GUILayout.Button("检查"))
                {
                    ResetSearch();
                }

                repeatStrict = GUILayout.Toggle(repeatStrict, "重复像素严格模式");
                EditorGUILayout.HelpBox("检查图片石佛符合规范，Project视图中选择图片并点击检查，可以多选或者选择文件夹", MessageType.Info);

                if (checkCoroutine != null || textureList.Count > 0)
                {
                    scrollPosition=EditorGUILayout.BeginScrollView(scrollPosition);
                    {
                        foreach(TextureInfo textureInfo in textureList)
                        {
                            EditorGUILayout.ObjectField(textureInfo.Obj, typeof(Texture2D), false);
                            EditorGUILayout.TextField(textureInfo.Path);

                            if (textureInfo.NameIllegal)
                            {
                                EditorGUILayout.HelpBox("资源名称内含义不符合规范的字符", MessageType.Warning);
                          
                            }

                            if (textureInfo.SizeIllegal[0] || textureInfo.SizeIllegal[1])
                            {
                                string str = "";
                                if (textureInfo.SizeIllegal[0] && textureInfo.SizeIllegal[1])
                                {
                                    str = "宽和高";
                                }
                                else if (textureInfo.SizeIllegal[0])
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
                                    EditorGUILayout.HelpBox("资源在红框范围内像素重复，可以优化九宫格切割", MessageType.Warning);
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
                            if(textureInfo.NewObj== null)
                            {
                                EditorGUILayout.TextField(textureInfo.Obj.width + "*" + textureInfo.Obj.height);
                                Rect rect = GUILayoutUtility.GetRect(100, 100);

                                if (Event.current.type == EventType.Repaint)
                                {
                                    GUI.DrawTexture(rect, textureInfo.Obj, ScaleMode.ScaleToFit);
                                }
                            }
                            else
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.BeginVertical();
                                    {
                                        EditorGUILayout.TextField(textureInfo.Obj.width +"*" + textureInfo.Obj.height);
                                        Rect rect = GUILayoutUtility.GetRect(100, 100);

                                        if (Event.current.type == EventType.Repaint)
                                        {
                                            GUI.DrawTexture(rect, textureInfo.Obj, ScaleMode.ScaleToFit);
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

                                                if (textureInfo.NewRepeat[1, 0] > 0)
                                                {
                                                    top = textureInfo.NewRepeat[1, 0] + 1;
                                                    bottom = textureInfo.NewObj.height - textureInfo.NewRepeat[1, 0] - textureInfo.NewRepeat[1, 1];
                                                }
                                                importer.spriteBorder = new Vector4(left, bottom, right, top);
                                                importer.SaveAndReimport();
                                                AssetDatabase.Refresh();
                                            }
                                        }
                                    }
                                }


                                GUILayout.Space(50);
                            }
                            EditorGUILayout.EndScrollView();


                        }

                        
                    }

                    if (checkCoroutine != null)
                    {
                        EditorGUILayout.HelpBox("正在检查资源...", MessageType.Info);
                    }
                }
                else
                {
                    GUILayout.Space(50);
                    EditorGUILayout.LabelField("没有选中任务不符合规范的资源");
                    GUILayout.Space(50);

                }
                GUILayout.Space(20);
                EditorGUILayout.EndVertical();
            }
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();
        }
    }

    private void ResetSearch()
    {
        checkCoroutine = SearchCoroutine();
    }

    private IEnumerator SearchCoroutine()
    {
        List<string> allTextureList = new List<string>();
        Texture2D[] texture2Ds = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets);
        for(int i = 0; i < texture2Ds.Length; i++)
        {
            allTextureList.Add(AssetDatabase.GetAssetPath(texture2Ds[i]));
        }
        textureList.Clear();
        yield return null;
        foreach(string texture in allTextureList)
        {
            //检查像素
            int[,] repeat = new int[2, 2];
            int[] loop = new int[2];
            bool[] summetry = new bool[2];
            int checkResult = UIUtility.TexturePixelCheck(texture, out repeat, out loop, out summetry, repeatStrict, true);
            int pixelType = 0;
            if ((checkResult & 1) > 0)
            {
                pixelType = 1;
            }
            else if ((checkResult & 2) > 0)
            {
                pixelType = 2;
            }
            else if ((checkResult & 4) > 0)
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

            //检查名字
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9_\-]+$");
            bool nameIllegal = !regex.IsMatch(Path.GetFileNameWithoutExtension(texture));

            //检查尺寸
            bool[] sizeIllegal = new bool[] { false, false };
            TextureImporter importer = AssetImporter.GetAtPath(texture) as TextureImporter;
            int[] size = UIUtility.GetTextureCompressedSize(texture);
            bool inAtlas = UIUtility.IsTextureShouldAtlas(texture);
            if(!inAtlas&&importer.npotScale==TextureImporterNPOTScale.None&&importer.textureType== TextureImporterType.Sprite)
            {
                if (size[0] % 4 != 0)
                {
                    sizeIllegal[0] = true;
                }
                if (size[1] % 4 != 0)
                {
                    sizeIllegal[1] = true;
                }

                //保存结果
                if (pixelType > 0 || nameIllegal || sizeIllegal[0] || sizeIllegal[1])
                {
                    textureList.Add(new TextureInfo(texture, pixelType, repeat, loop, summetry, nameIllegal, sizeIllegal, repeatStrict));
                }
                yield return null;

            }

            textureList.Sort();
            Focus();
        }
    }
}
