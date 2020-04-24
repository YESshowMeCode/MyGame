// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/07 13:04:09
// FileName：Assets/Assets/Scripts/Config/TestConfig.cs
// ========================================================



/***************************************************************************************
*本代码有ExcelToCsv.cs自动生成，请勿修改
*如需改变量请参考ExcelToCsv的注释修改Excel文件重新生成代码
*如有特殊需要可以重新封装
*或者修改ExcelToCsv与ConfigLoader增加新类型新内容
***************************************************************************************/


using UnityEngine;
using System.Collections.Generic;

public class TestTemplate : ConfigTemplate 
{
    public string name;
    public int[] num;
    public string[] dec;
}

public class TestConfig : ConfigLoader<TestTemplate>
{
    protected override void readNode(CsvReader reader, int row, TestTemplate tmpl)
    {
      tmpl.name = ReadString(reader, row, 1);
      tmpl.num = ReadIntArray(reader, row, 2);
      tmpl.dec = ReadStringArray(reader, row, 3);
    }
}
