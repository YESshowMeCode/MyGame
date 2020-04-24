// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/06 09:49:00
// FileName：Assets/Assets/Scripts/Ext/ConfigManager/ExeclToCsv.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using System.Diagnostics;
using ExcelDataReader;
using System.Text;

class ExcelReader
{
    const int maxColumn = 128;
    static string excelPath = Application.dataPath + "/Excel";
    static string csvPath = Application.dataPath + "/Assets/GameData/config/";
    static string codePath = Application.dataPath + "/Assets/Scripts/Config/";
    static List<string> explictList = new List<string>
    {
        "PPRule.xlsx","BattleStateGraph.xlsx","PlayerLines.xlsx","ThreatSwitch.xlsx"
    };

    static Dictionary<string, string> typeCast = new Dictionary<string, string>
    {
        {"Vector2","ReadVector2" },{"Vector3","ReadVector3" },{"Vector4","ReadVector4" },{"int","ReadInt" },{"long","ReadLong" },
        {"int[]","ReadIntArray" },{"float","ReadFloat" },{"float[]","ReadFloatArray" },{"string","ReadString" },
        {"bool","ReadBool" },{"string[]","ReadStringArray" },{"string[][]","ReadStringArrayArray" },{"int[][]","ReadIntArrayArray" },
    };

    static string[] types = new string[maxColumn];
    static string[] variableNames = new string[maxColumn];
    static int column;

    [MenuItem("Tools/ConfigLoader/ExcelToCsv")]
    static void Convert()
    {
        foreach(string path in Directory.GetFiles(excelPath, "*", SearchOption.AllDirectories))
        {
            if (System.IO.Path.GetExtension(path) == ".xlsx")
            {
                string name = Path.GetFileName(path);

                if (name.StartsWith("~$"))
                {
                    continue;
                }

                if(name == "Localization.xlsx")
                {
                    ConvertToCsv(path, true);
                }
                else 
                {
                    ConvertToCsv(path);
                }
            }
        }

        EditorUtility.DisplayDialog("ConfifLoader", "Convert Finished", "OK");
        AssetDatabase.Refresh();
    }


    [MenuItem("Tools/ConfigLoader/ConverAndGenerateCode")]
    public static void ConvertAndGenerateCode()
    {
        className.Clear();
        foreach(string path in Directory.GetFiles(excelPath, "*.*", SearchOption.AllDirectories))
        {
            if (Path.GetExtension(path) == ".xlsx")
            {
                string name = Path.GetFileName(path);
                if(name == "Localization.xlsx")
                {
                    GenerateCode(path, false);
                }
                if (!explictList.Contains(name))
                {
                    if (name.StartsWith("~$")) continue;
                    GenerateCode(path);
                }
            }
        }
        className.Clear();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/ConfigLoader/LocalizationExcelToCsv")]
    static void LocalizationExcelToCsv()
    {
        className.Clear();
        foreach(string path in Directory.GetFiles(excelPath))
        {
            if (Path.GetExtension(path) == ".xlsx")
            {
                string name = Path.GetFileName(path);
                if (!explictList.Contains(name))
                {
                    GenerateCode(path, true);
                }
            }
        }

        className.Clear();
        EditorUtility.DisplayDialog("ConfigLoader", "Convert Finished", "OK");
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/ConfigLoader/ExcelToCsv")]
    static void ConvertSelected()
    {
        Object[] selectedObjects = Selection.objects;

        for(int i = 0; i < selectedObjects.Length; i++)
        {
            string path = AssetDatabase.GetAssetPath(selectedObjects[i]);

            if(System.IO.Path.GetExtension(path) == ".xlsx")
            {
                string name = Path.GetFileName(path);

                if (name.StartsWith("~$"))
                {
                    continue;
                }

                if (name == "Localization.xlsx")
                {
                    ConvertToCsv(path, true);
                }
                else
                {
                    ConvertToCsv(path);
                }
            }
        }


        EditorUtility.DisplayDialog("ConfifLoader", "Convert Finished", "OK");
        AssetDatabase.Refresh();
    }


    [MenuItem("Assets/ConfigLoader/ConverAndGenerateCode")]
    public static void ConvertAndGenerateCodeSelected()
    {
        className.Clear();

        Object[] selectedObjects = Selection.objects;

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            string path = AssetDatabase.GetAssetPath(selectedObjects[i]);

            if (Path.GetExtension(path) == ".xlsx")
            {
                string name = Path.GetFileName(path);
                if (name == "Localization.xlsx")
                {
                    GenerateCode(path, true);
                }
                if (!explictList.Contains(name))
                {
                   //if (name.StartsWith("~$")) continue;
                    GenerateCode(path);
                }
            }
        }

        className.Clear();
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/ConfigLoader/LocalizationExcelToCsv")]
    static void LocalizationExcelToCsvSelected()
    {
        className.Clear();

        Object[] selectedObjects = Selection.objects;

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            string path = AssetDatabase.GetAssetPath(selectedObjects[i]);
            string name = selectedObjects[i].name;
            if (Path.GetExtension(path) == ".xlsx" || name=="Localization")
            {

                    GenerateCode(path, true);
            }
        }
        className.Clear();
        EditorUtility.DisplayDialog("ConfigLoader", "Convert Finished", "OK");
        AssetDatabase.Refresh();
    }


    [MenuItem("Assets/ConfigLoader/ExcelToCsv",true)]
    static bool ValidateConvertSelected()
    {
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            string path = AssetDatabase.GetAssetPath(selectedObjects[i]);

            if (System.IO.Path.GetExtension(path) != ".xlsx")
            {
                return false;
            }
        }

        return true;
    }


    // [MenuItem("Assets/ConfigLoader/ConverAndGenerateCode",true)]
    // public static bool ValiableConvertAndGenerateCodeSelected()
    // {


    //     Object[] selectedObjects = Selection.objects;
    //     if (selectedObjects == null || selectedObjects.Length == 0)
    //     {
    //         return false;
    //     }

    //     for (int i = 0; i < selectedObjects.Length; i++)
    //     {
    //         string path = AssetDatabase.GetAssetPath(selectedObjects[i]);

    //         if (Path.GetExtension(path) != ".xlsx")
    //         {
    //             return false;
    //         }
    //     }
    //     return true;
    // }

    [MenuItem("Assets/ConfigLoader/LocalizationExcelToCsv",true)]
    static bool LocalizationValiableExcelToCsvSelected()
    {

        Object[] selectedObjects = Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            return false;
        }
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            string path = AssetDatabase.GetAssetPath(selectedObjects[i]);
            string name = selectedObjects[i].name;
            if (name != "Localization" || Path.GetExtension(path) != ".xlsx")
            {
                return false;
            }
        }
        return true;
    }

    static void ConvertToCsv(string file ,bool Localizetion = false,bool isNotSkip  = false)
    {
        FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader;

        if (file.EndsWith("xlsx"))
        {
            excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        }
        else
        {
            excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
        }

        StringBuilder output = new StringBuilder();
        int rowNum = -1;
        while (excelReader.Read())
        {
            ++rowNum;
            if (!isNotSkip)
            {
                if ((rowNum == 2) || (rowNum == 3))
                {
                    continue;
                }
            }
            column = excelReader.FieldCount;

            if (rowNum == 0)
            {
                for (int i = 0; i < column; i++)
                {
                    if (Localizetion == true)
                    {
                        if ((i == 2) || (i == 3))
                        {
                            continue;
                        }
                    }
                    object value = excelReader.IsDBNull(i) ? "" : excelReader.GetValue(i);
                    string strValue = value.ToString();
                    output.Append(strValue.Replace(",", "，"));
                    if (i < excelReader.FieldCount - 1)
                    {
                        output.Append(",");
                    }
                    types[i] = strValue;    
                }
                output.Append("\n");
                continue;
            }

            object id = excelReader.IsDBNull(0) ? "" : excelReader.GetValue(0);
            string strID = id.ToString();
            if (strID.Trim() == "")
            {
                continue;
            }

            for(int i = 0; i < column; i++)
            {
                if (Localizetion == true)
                {
                    if ((i == 2) || (i == 3)) continue;
                }
                if (string.IsNullOrEmpty(types[i]))
                {
                    output.Append(",");
                    continue;
                }
                object value = excelReader.IsDBNull(i) ? "" : excelReader.GetValue(i);
                string strValue = value.ToString();
                if(rowNum > 3 && types[i]!="string" && types[i] != "bool" && types[i]!= "string[]" && types[i] != "string[][]")
                {
                    strValue = strValue.Replace("_", "—");
                }
                output.Append(strValue.Replace(",", "，").Replace("\n", "\\n"));
                if (i < excelReader.FieldCount - 1)
                {
                    output.Append(",");
                }
            }
            output.Append("\n");
        }
        string outputFile = csvPath + System.IO.Path.GetFileNameWithoutExtension(file) + "Config.csv";

        StreamWriter csv = new StreamWriter(@outputFile, false);
        csv.Write(output);
        csv.Close();
        excelReader.Close();
    }

    static HashSet<string> className = new HashSet<string>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <param name="isLocalization" 为true表示ui语言表格特殊处理></param>
    static void GenerateCode(string file,bool isLocalization = false)
    {
        string name = Path.GetFileNameWithoutExtension(file);

        name = Regex.Replace(name, "[0-9]+", "");

        if (className.Contains(name))
        {
            return;
        }

        className.Add(name);
        FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader;

        if (file.EndsWith(".xlsx"))
        {
            excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        }
        else
        {
            excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
        }

        StringBuilder code = new StringBuilder
        (
            "/***************************************************************************************\n" +
            "*本代码有ExcelToCsv.cs自动生成，请勿修改\n" +
            "*如需改变量请参考ExcelToCsv的注释修改Excel文件重新生成代码\n" +
            "*如有特殊需要可以重新封装\n" +
            "*或者修改ExcelToCsv与ConfigLoader增加新类型新内容\n" +
            "***************************************************************************************/\n\n\n" +
            "using UnityEngine;\n" +
            "using System.Collections.Generic;\n\n" +
            "public class " + name + "Template : ConfigTemplate \n{\n");


        int rowNum = -1;
        StringBuilder output = new StringBuilder();
        while (excelReader.Read())
        {
            rowNum++;
            if (rowNum == 3) continue;
            column = excelReader.FieldCount;

            for(int i = 0; i < column; i++)
            {
                if (isLocalization)
                {
                    if ((i == 2) || (i == 3)) continue;
                }
                object value = excelReader.IsDBNull(i) ? "" : excelReader.GetValue(i);
                string strValue = value.ToString();
                if (rowNum == 0)
                {
                    output.Append(strValue);
                    if (i < column - 1)
                    {
                        output.Append(",");
                    }
                    types[i] = strValue;
                }

                if(rowNum == 1)
                {
                    output.Append(strValue);
                    if (i < column - 1)
                    {
                        output.Append(",");

                    }
                    variableNames[i] = strValue;
                }

                if (rowNum == 2) continue;

                if (rowNum > 3)
                {
                    if (string.IsNullOrEmpty(types[i]))
                    {
                        output.Append(",");
                        continue;
                    }

                    if(types[i]!= "string"&& types[i]!="bool" && types[i] != "string[]" && types[i]!= "string[][]")
                    {
                        strValue = strValue.Replace("_", "—");
                    }
                    output.Append(strValue);
                    if (i < column - 1)
                    {
                        output.Append(",");
                    }
                }
            }
            if(rowNum != 2)
            {
                output.Append("\n");
            }

        }

        for (int i = 0; i < column; i++)
        {
            if (isLocalization)
            {
                if ((i == 2) || (i == 3)) continue;
            }
            if (types[i] == "") continue;
            if (variableNames[i] == "id") continue;
            code.Append("    public " + types[i] + " " + variableNames[i] + ";\n");
            
        }
        code.Append("}\n\n");

        // code.Append("public class " + name + "Container\n{\n");
        // code.Append("    public Dictionary<int," + name + "Template> data;\n{\n");
        // code.Append("    {\n");

        code.Append("public class " + name + "Config : ConfigLoader<" + name + "Template>\n{\n");
        code.Append("    protected override void readNode(CsvReader reader, int row, " + name + "Template tmpl)\n");
        code.Append("    {\n");

        for (int i = 0; i < column; i++)
        {
            if (isLocalization)
            {
                if ((i == 2) || (i == 3)) continue;
            }
            if (string.IsNullOrEmpty(types[i])) continue;
            if (variableNames[i] == "id") continue;

            try
            {
                if (isLocalization)
                {
                    if (i > 3)
                    {
                        code.Append("      tmpl." + variableNames[i] + " = " + typeCast[types[i]] + "(reader, row, " + (i - 2).ToString() + ");");
                    }
                    else
                    {
                        code.Append("      tmpl." + variableNames[i] + " = " + typeCast[types[i]] + "(reader, row, " + (i).ToString() + ");\n");
                    }
                }
                else
                {
                    code.Append("      tmpl." + variableNames[i] + " = " + typeCast[types[i]] + "(reader, row, " + (i).ToString() + ");\n");
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
                UnityEngine.Debug.Log(e + "   |-------" + types[i]);
            }

        }

        for(int i = 0; i < column; i++)
        {
            if (isLocalization)
            {
                if ((i == 2) || (i == 3)) continue;
            }
        }
        code.Append("    }\n");
        code.Append("}\n");
        excelReader.Close();


        string csvOutpurFile = csvPath + name + "Config.csv";
        StreamWriter csvWirter;
        if (isLocalization)
        {
            csvWirter = new StreamWriter(csvOutpurFile, false, Encoding.UTF8);
        }
        else
        {
            csvWirter = new StreamWriter(@csvOutpurFile, false);
        }
        csvWirter.Write(output);
        csvWirter.Close();

        string codeOutpurFile = codePath + name + "Config.cs";
        StreamWriter codeWirter;
        if (isLocalization)
        {
            codeWirter = new StreamWriter(@codeOutpurFile, false, Encoding.UTF8);
        }
        else
        {
            codeWirter = new StreamWriter(@codeOutpurFile, false);
        }
        codeWirter.Write(code);
        codeWirter.Close();

    }


    

}
