using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;

public class UGUIScriptsCreate : Editor
{
    static private string m_scriptsPath = Application.dataPath + "/Scripts";
    static private Dictionary<string, UITypeSign> m_interactionUIDic = new Dictionary<string, UITypeSign>();
    static private Dictionary<string, string> m_varNameDic = new Dictionary<string, string>();
    static private readonly string m_initUIEventFunctionName = "InitUIEvent";
    static private readonly string m_varPrefix = "m_";
    static private Dictionary<UIType, string> m_EventFunctionNameDic = new Dictionary<UIType, string>();
    static private readonly string m_tabStr = "    ";
    static private Dictionary<UIType, string> m_cacheFunction = new Dictionary<UIType, string>();

    #region 菜单

    [MenuItem("GameObject/UGUITools/AddUIType/UIRoot", priority = 0)]
    static void AddUIType_UIRoot()
    {
        AddUIType(UIType.UIRoot);
    }

    [MenuItem("GameObject/UGUITools/AddUIType/UITransform", priority = 0)]
    static void AddUIType_UITransform()
    {
        AddUIType(UIType.Transform);
    }

    [MenuItem("GameObject/UGUITools/AddUIType/UIImage", priority = 0)]
    static void AddUIType_UIImage()
    {
        AddUIType(UIType.Image);
    }

    [MenuItem("GameObject/UGUITools/AddUIType/UIRawImage", priority = 0)]
    static void AddUIType_UIRawImage()
    {
        AddUIType(UIType.RawImage);
    }

    [MenuItem("GameObject/UGUITools/AddUIType/UIButton", priority = 0)]
    static void AddUIType_UIButton()
    {
        AddUIType(UIType.Button);
    }

    [MenuItem("GameObject/UGUITools/AddUIType/UIToggle", priority = 0)]
    static void AddUIType_Toggle()
    {
        AddUIType(UIType.Toggle);
    }

    [MenuItem("GameObject/UGUITools/AddUIType/UISlider", priority = 0)]
    static void AddUIType_Slider()
    {
        AddUIType(UIType.Slider);
    }

    [MenuItem("GameObject/UGUITools/AddUIType/UIScrollbar", priority = 0)]
    static void AddUIType_Scrollbar()
    {
        AddUIType(UIType.Scrollbar);
    }

    [MenuItem("GameObject/UGUITools/AddUIType/UIDropDown", priority = 0)]
    static void AddUIType_Dropdown()
    {
        AddUIType(UIType.Dropdown);
    }

    [MenuItem("GameObject/UGUITools/AddUIType/UIInputField", priority = 0)]
    static void AddUIType_InputField()
    {
        AddUIType(UIType.InputField);
    }

    [MenuItem("GameObject/UGUITools/AddUIType/UIScrollRect", priority = 0)]
    static void AddUIType_ScrollRect()
    {
        AddUIType(UIType.ScrollRect);
    }

    [MenuItem("GameObject/UGUITools/CreateScripts", priority = 0)]
    static void CreateScripts()
    {
        if (Selection.gameObjects != null && Selection.gameObjects.Length >= 1)
        {
            GameObject go = Selection.gameObjects[0];
            UITypeSign sign = go.GetComponent<UITypeSign>();
            if (sign == null)
            {
                EditorUtility.DisplayDialog("错误", "选中物体并未添加UITypeSign组件！", "确认");
            }
            else
            {
                if (sign.Type == UIType.UIRoot)
                {
                    //开始生成代码
                    CreateScriptsToFile(go.transform);
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "选中物体并不是UI根节点！", "确认");
                }
            }
        }
        else
        {
            EditorUtility.DisplayDialog("错误", "请先选中UI Root，再进行该操作！", "确认");
        }
    }
    #endregion
    
    static void CreateScriptsToFile(Transform root)
    {
        if (root == null)
        {
            return;
        }
        m_interactionUIDic.Clear();
        m_varNameDic.Clear();
        GetUITypeSignDic(root, "");
        InitEventFunctionNameDic();
        string scriptName = root.name;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using UnityEngine.UI;");
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.Append("public class ");
        sb.Append(scriptName);
        sb.Append(" : MonoBehaviour");
        sb.AppendLine();
        sb.AppendLine("{");
        
        if (m_interactionUIDic != null && m_interactionUIDic.Count > 0)
        {
            foreach (var item in m_interactionUIDic)
            {
                string typeName = GetStrByUIType(item.Value.Type);
                if (!string.IsNullOrEmpty(typeName))
                {
                    StringBuilder sb_variable = new StringBuilder();
                    sb_variable.Append("private ");
                    sb_variable.Append(typeName);
                    sb_variable.Append(" ");
                    sb_variable.Append(m_varPrefix);//变量名前缀
                    string name = null;
                    if (item.Key.Contains("/"))
                    {
                        int index = item.Key.LastIndexOf("/");
                        name = item.Key.Substring(index + 1);
                    }
                    else
                    {
                        name = item.Key;
                    }
                    //保存变量名，以path为key
                    m_varNameDic.Add(item.Key, m_varPrefix + name);
                    sb_variable.Append(name);
                    sb_variable.Append(" = null;");
                    sb.Append(m_tabStr);
                    sb.AppendLine(sb_variable.ToString());
                }
            }
        }

        sb.AppendLine();

        //插入Awake函数
        sb.Append(m_tabStr);
        sb.AppendLine("void Awake()");
        sb.Append(m_tabStr);
        sb.AppendLine("{");

        //插入ui变量的FindChild
        if (m_interactionUIDic != null && m_interactionUIDic.Count > 0)
        {
            foreach (var item in m_interactionUIDic)
            {
                sb.Append(m_tabStr);
                sb.Append(m_tabStr);
                sb.Append(m_varNameDic[item.Key]);
                sb.Append(" = transform.FindChild(");
                sb.Append("\"");
                sb.Append(item.Key);
                sb.Append("\").GetComponent<");
                sb.Append(GetStrByUIType(item.Value.Type));
                sb.AppendLine(">();");
            }
        }

        sb.Append(m_tabStr);
        sb.AppendLine("}");

        //插入start函数
        sb.AppendLine();
        sb.Append(m_tabStr);
        sb.AppendLine("void Start()");
        sb.Append(m_tabStr);
        sb.AppendLine("{");

        sb.Append(m_tabStr);
        sb.Append(m_tabStr);
        sb.Append(m_initUIEventFunctionName);
        sb.AppendLine("();");
        sb.Append(m_tabStr);
        sb.AppendLine("}");
        
        //插入InitUIEvent函数
        sb.AppendLine();
        sb.Append(m_tabStr);
        sb.Append("private ");
        sb.Append("void ");
        sb.Append(m_initUIEventFunctionName);
        sb.AppendLine("()");
        sb.Append(m_tabStr);
        sb.AppendLine("{");

        m_cacheFunction.Clear();
        if (m_interactionUIDic != null && m_interactionUIDic.Count > 0)
        {
            foreach (var item in m_interactionUIDic)
            {
                if (m_EventFunctionNameDic.ContainsKey(item.Value.Type))
                {
                    string functionName = GetFunctionNameByTypeVarName(item.Value.Type, m_varNameDic[item.Key].Substring(2));
                    if (null != functionName)
                    {
                        sb.Append(m_tabStr);
                        sb.Append(m_tabStr);
                        sb.Append(m_varNameDic[item.Key]);
                        sb.Append(".");

                        sb.Append(m_EventFunctionNameDic[item.Value.Type]);
                        sb.Append(".AddListener(");
                        sb.Append(functionName);
                        sb.AppendLine(");");

                        //缓存函数名
                        m_cacheFunction.Add(item.Value.Type, functionName);
                    }
                }
            }
        }
        
        sb.Append(m_tabStr);
        sb.AppendLine("}");

        //插入缓存的函数名
        if (m_cacheFunction != null && m_cacheFunction.Count > 0)
        {
            foreach (var item in m_cacheFunction)
            {
                sb.AppendLine();
                sb.Append(m_tabStr);
                sb.Append("private ");
                sb.Append("void ");
                sb.Append(item.Value);
                sb.Append("(");

                //参数
                switch (item.Key)
                {
                    case UIType.InputField:
                        sb.Append("string arg0");
                        break;
                    case UIType.ScrollRect:
                    case UIType.Toggle:
                    case UIType.Slider:
                    case UIType.Scrollbar:
                    case UIType.Dropdown:
                        sb.Append("bool arg0");
                        break;
                }
                sb.AppendLine(")");
                sb.Append(m_tabStr);
                sb.AppendLine("{");
                sb.Append(m_tabStr); sb.Append(m_tabStr);
                sb.AppendLine("throw new NotImplementedException();");
                sb.Append(m_tabStr);
                sb.AppendLine("}");
            }
        }

        sb.AppendLine("}");
        WriteStrToFile(sb.ToString(), m_scriptsPath, scriptName);

        //刷新资源
        AssetDatabase.Refresh();
    }

    static void InitEventFunctionNameDic()
    {
        m_EventFunctionNameDic.Clear();
        m_EventFunctionNameDic.Add(UIType.Button, "onClick");
        m_EventFunctionNameDic.Add(UIType.Toggle, "onValueChanged");
        m_EventFunctionNameDic.Add(UIType.Scrollbar, "onValueChanged");
        m_EventFunctionNameDic.Add(UIType.ScrollRect, "onValueChanged");
        m_EventFunctionNameDic.Add(UIType.Slider, "onValueChanged");
        m_EventFunctionNameDic.Add(UIType.Dropdown, "onValueChanged");
        m_EventFunctionNameDic.Add(UIType.InputField, "onEndEdit");
    }

    /// <summary>
    /// 根据类别和变量名返回事件函数名
    /// </summary>
    /// <param name="type">类别</param>
    /// <param name="varName">变量名</param>
    static string GetFunctionNameByTypeVarName(UIType type,string varName)
    {
        if (string.IsNullOrEmpty(varName))
        {
            return null;
        }
        if (!m_EventFunctionNameDic.ContainsKey(type))
        {
            return null;
        }
        string eventStr = m_EventFunctionNameDic[type];
        //这里的命名规范是On + varName + eventName
        //举例：type = UIType.Button,varName = CloseBtn，结果是OnCloseBtnClick
        return "On" + varName + eventStr.Substring(2);//截掉eventStr开头的on
    }

    static void InsertFunction(StringBuilder sb, string functionName)
    {
        if (sb == null || string.IsNullOrEmpty(functionName))
        {
            return;
        }
        sb.Append(m_tabStr);
        sb.Append("private void ");
        sb.Append(functionName);
        sb.AppendLine("()");
        sb.Append(m_tabStr);
        sb.AppendLine("{");
        sb.AppendLine();
        sb.Append(m_tabStr);
        sb.AppendLine("}");
    }
    
    static string GetStrByUIType(UIType type)
    {
        string str = null;
        switch (type)
        {
            case UIType.Transform:
                str = "Transform";
                break;
            case UIType.Image:
                str = "Image";
                break;
            case UIType.RawImage:
                str = "RawImage";
                break;
            case UIType.Button:
                str = "Button";
                break;
            case UIType.Toggle:
                str = "Toggle";
                break;
            case UIType.Slider:
                str = "Slider";
                break;
            case UIType.Scrollbar:
                str = "Scrollbar";
                break;
            case UIType.Dropdown:
                str = "Dropdown";
                break;
            case UIType.InputField:
                str = "InputField";
                break;
            case UIType.ScrollRect:
                str = "ScrollRect";
                break;
        }
        return str;
    }

    static void GetUITypeSignDic(Transform root, string path)
    {
        if (root == null)
        {
            return;
        }
        UITypeSign sign = root.GetComponent<UITypeSign>();
        if (sign != null && sign.Type != UIType.UIRoot)
        {
            m_interactionUIDic.Add(path, sign);
        }
        if (root.childCount > 0)
        {
            for (int i = 0; i < root.childCount; i++)
            {

                GetUITypeSignDic(root.GetChild(i), string.IsNullOrEmpty(path) ? root.GetChild(i).name : path + "/" + root.GetChild(i).name);
            }
        }
        return;
    }

    static void WriteStrToFile(string txt, string path, string fileName)
    {
        if (string.IsNullOrEmpty(txt) || string.IsNullOrEmpty(path))
        {
            return;
        }
        File.WriteAllText(path + "/" + fileName + ".cs", txt, Encoding.UTF8);
    }

    static void AddUIType(UIType type)
    {
        if (Selection.gameObjects != null && Selection.gameObjects.Length >= 1)
        {
            int count = Selection.gameObjects.Length;
            for (int i = 0; i < count; i++)
            {
                GameObject go = Selection.gameObjects[i];
                UITypeSign sign = go.AddComponent<UITypeSign>();
                switch (type)
                {
                    case UIType.UIRoot:
                        sign.Type = UIType.UIRoot;
                        break;
                    case UIType.Transform:
                        sign.Type = UIType.Transform;
                        break;
                    case UIType.Image:
                        sign.Type = UIType.Image;
                        break;
                    case UIType.RawImage:
                        sign.Type = UIType.RawImage;
                        break;
                    case UIType.Button:
                        sign.Type = UIType.Button;
                        break;
                    case UIType.Toggle:
                        sign.Type = UIType.Toggle;
                        break;
                    case UIType.Slider:
                        sign.Type = UIType.Slider;
                        break;
                    case UIType.Scrollbar:
                        sign.Type = UIType.Scrollbar;
                        break;
                    case UIType.Dropdown:
                        sign.Type = UIType.Dropdown;
                        break;
                    case UIType.InputField:
                        sign.Type = UIType.InputField;
                        break;
                    case UIType.ScrollRect:
                        sign.Type = UIType.ScrollRect;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
