/**************************************
*Module：添加C#代码 自动添加注释                                           
*Author：ChenGaoshuang                                        
*Time: 2018.05.22                                                     
**************************************/
using System.Collections;
using System.IO;


namespace CLB
{
    public class ChangeScriptTemplate : UnityEditor.AssetModificationProcessor
    {

        // 添加脚本注释模板
        private static string str =
        "// ========================================================\r\n"
        + "// Author：ChenGaoshuang \r\n"
        + "// CreateTime：#CreateTime#\r\n"
        + "// FileName：#file#\r\n"
        + "// ========================================================\r\n" + "\r\n" + "\r\n" + "\r\n";

        // 创建资源调用
        public static void OnWillCreateAsset(string path)
        {
            // 只修改C#脚本
            path = path.Replace(".meta", "");
            if (path.EndsWith(".cs"))
            {
                string allText = str;
                allText += File.ReadAllText(path);
                // 替换字符串为系统时间
                allText = allText.Replace("#CreateTime#", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                allText = allText.Replace("#file#", path);
                File.WriteAllText(path, allText);
            }
        }
    }
}