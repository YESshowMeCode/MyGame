// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/03 21:18:25
// FileName：Assets/Assets/Editor/UIAtlasOptimize/UIEditorTool.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class NoDrawGraphicEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.HelpBox("请将该脚本替换为 NonDrawingGraphic 脚本", MessageType.Error);
    }
}

public class UGUIEditorTool : EditorWindow
{

    static int tab = 0;
    string[] btnName = { "查找空的Image替换为NoDrawGraphics", "显示RaycastTarget或者边框" };

	[MenuItem("Tools/图集工具/批量处理小工具合集")]
    private static void Init()
    {
        UGUIEditorTool wnd = EditorWindow.GetWindow(typeof(UGUIEditorTool)) as UGUIEditorTool;
    }

    private void Enable()
    {
        Selection.selectionChanged += UpdateSelectObj;
        SceneView.onSceneGUIDelegate += RaycastTargetGizmo;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= UpdateSelectObj;
        SceneView.onSceneGUIDelegate -= RaycastTargetGizmo;
    }


    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        tab = GUILayout.Toolbar(tab, btnName);
        switch (tab)
        {
            case 0:FindEmptyImage();break;
            case 1:RaycastTarget();break;
        }

        EditorGUILayout.EndVertical();
    }

    #region 查找空的Image和空的CanvasRender

    Vector2 findEmptyImageScrollVector = Vector2.zero;
    //查找空的Image
    List<Image> findEmptyImages = new List<Image>();
    List<RectTransform> findRectTransform = new List<RectTransform>();

    /// <summary>
    /// 查找一个prefab下的所有空的Image或者Alpha为0的  用来方便替换NoDrawGraph
    /// </summary>
    void FindEmptyImage()
    {
        EditorGUILayout.HelpBox("使用方法： 。\n" +
                                 "1、将UI对象拖进场景中。\n" +
                                 "2、选择需要查找的UIRoot。\n" +
                                 "3、点击重新查找。 \n" +
                                 "4、根据输出列表查找自己的UI并且替换。", MessageType.Warning);

        if (GUILayout.Button("重新查找"))
        {
            Object[] obj = Selection.objects;
            if(obj == null || obj.Length <= 0)
            {
                return;
            }

            findEmptyImages.Clear();
            findRectTransform.Clear();

            for(int i = 0; i < obj.Length; i++)
            {
                findEmptyImages.AddRange((obj[i] as GameObject).GetComponentsInChildren<Image>(true));
                findRectTransform.AddRange((obj[i] as GameObject).GetComponentsInChildren<RectTransform>(true));
            }
        }
        findEmptyImageScrollVector = EditorGUILayout.BeginScrollView(findEmptyImageScrollVector, "box");
        if (findEmptyImages.Count > 0 && findEmptyImages[0] != null)
        {
            for (int i = 0; i < findEmptyImages.Count; i++)
            {
                if(findEmptyImages[i]!=null&&(findEmptyImages[i].sprite == null || findEmptyImages[i].color.a < 1))
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(findEmptyImages[i].name, GUILayout.Width(200)))
                    {
                        EditorGUIUtility.PingObject(findEmptyImages[i]);
                        Selection.activeObject = findEmptyImages[i];
                    }
                    if (GUILayout.Button("Image替换为NonDrawGrapthic", GUILayout.Width(200)))
                    {
                        GameObject obj = findEmptyImages[i].gameObject;
                        DestroyImmediate(obj.GetComponent<Image>());
                        obj.AddComponent<UnityEngine.UI.Extensions.NonDrawingGraphic>();
                    }

                    if (findEmptyImages[i].sprite == null)
                    {
                        GUILayout.Space(5);
                        GUILayout.Label("sprite 为空");
                        GUILayout.Space(5);
                    }

                    if (findEmptyImages[i].color.a < 1)
                    {
                        GUILayout.Space(5);
                        GUILayout.Label("alpha 小于1", findEmptyImages[i].color.a.ToString());
                        GUILayout.Space(5);
                    }

                    EditorGUILayout.EndHorizontal();
                }

            }
        }

        if (findRectTransform.Count > 0 && findRectTransform[0] != null)
        {
            for(int i = 0; i < findRectTransform.Count; i++)
            {
                if(findRectTransform[i]!=null && findRectTransform[i].GetComponent<CanvasRenderer>()!=null && findRectTransform[i].GetComponent<Graphic>() == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(findRectTransform[i].name, GUILayout.Width(200)))
                    {
                        EditorGUIUtility.PingObject(findRectTransform[i]);
                        Selection.activeObject = findRectTransform[i];
                    }

                    if (GUILayout.Button("删除掉canvasRenderer组件", GUILayout.Width(200)))
                    {
                        GameObject obj = findRectTransform[i].gameObject;
                        DestroyImmediate(obj.GetComponent<CanvasRenderer>());
                    }

                    GUILayout.Space(5);
                    GUILayout.Label("CanvasRenderer不为空Graphic为空");
                    GUILayout.Space(5);
                    EditorGUILayout.EndHorizontal();
                }
            }

        }
        EditorGUILayout.EndScrollView();
    }

    #endregion


    #region 显示RaycastTarget或者边框

    bool isOnlyShowSelectBorder = false;
    List<Graphic> noSelectGraphicList = new List<Graphic>();
    List<Graphic> selectGraphicList = new List<Graphic>();

    Vector3[] fourCorners = new Vector3[4];
    Color SelectColor = Color.yellow;
    Color BackColor = Color.green;

    int selectType = 0;
    string[] showTypeStr = new string[] { "显示UI绘制区域", "显示射线检测区域" };


    void RaycastTarget()
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("显示类型：", GUILayout.Width(300));
            selectType = GUILayout.SelectionGrid(selectType, showTypeStr, showTypeStr.Length, GUILayout.Width(300));
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("只显示选择对象的边框：", GUILayout.Width(300));
            isOnlyShowSelectBorder = GUILayout.Toggle(isOnlyShowSelectBorder, "", GUILayout.Width(300));
        }
        EditorGUILayout.EndHorizontal();

        if (!isOnlyShowSelectBorder)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("选中色", GUILayout.Width(300));
                SelectColor = EditorGUILayout.ColorField(SelectColor, GUILayout.Width(300));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("非选中色", GUILayout.Width(300));
                BackColor = EditorGUILayout.ColorField(BackColor, GUILayout.Width(300));
            }
            EditorGUILayout.EndHorizontal();

        }

        SceneView.RepaintAll();
        UpdateSelectObj();
    }



    void UpdateSelectObj()
    {
        selectGraphicList.Clear();
        noSelectGraphicList.Clear();
        //获取选中对象
        if (Selection.gameObjects.Length > 0)
        {
            selectGraphicList.AddRange(Selection.gameObjects[0].GetComponentsInChildren<Graphic>());
        }

        if (isOnlyShowSelectBorder)
        {
            //获取未选中对象
            Graphic[] objs = FindObjectsOfType(typeof(Graphic)) as Graphic[];
            foreach(var obj in objs)
            {
                if (!selectGraphicList.Contains(obj))
                {
                    noSelectGraphicList.Add(obj);
                }
            }
        }
    }

    void RaycastTargetGizmo(SceneView sceneView)
    {
        Debug.LogError("has entered");
        if (tab != 1)
        {
            return;
        }

        Handles.color = isOnlyShowSelectBorder ? Color.green : BackColor;
        foreach (var tar in noSelectGraphicList)
        {
            if (tar != null)
            {
                //显示UI区域
                if (selectType == 0)
                {
                    tar.rectTransform.GetWorldCorners(fourCorners);
                    for (int i = 0; i < 4; i++)
                    {
                        Handles.DrawLine(fourCorners[i], fourCorners[(i + 1) % 4]);
                    }
                }
                else
                {
                    //显示射线检测区域
                    if (tar.raycastTarget)
                    {
                        tar.rectTransform.GetWorldCorners(fourCorners);
                        for (int i = 0; i < 4; i++)
                        {
                            Handles.DrawLine(fourCorners[i], fourCorners[(i + 1) % 4]);
                        }
                    }
                }
            }
        }

        Handles.color = isOnlyShowSelectBorder ? Color.green : SelectColor;
        foreach (var tar in selectGraphicList)
        {
            if (tar != null)
            {
                //显示UI区域
                if (selectType == 0)
                {
                    tar.rectTransform.GetWorldCorners(fourCorners);
                    for (int i = 0; i < 4; i++)
                    {
                        Handles.DrawLine(fourCorners[i], fourCorners[(i + 1) % 4]);
                    }
                }
                else
                {
                    //显示射线检测区域
                    if (tar.raycastTarget)
                    {
                        tar.rectTransform.GetWorldCorners(fourCorners);
                        for (int i = 0; i < 4; i++)
                        {
                            Handles.DrawLine(fourCorners[i], fourCorners[(i + 1) % 4]);
                        }
                    }
                }
            }
        }
    }
}

#endregion