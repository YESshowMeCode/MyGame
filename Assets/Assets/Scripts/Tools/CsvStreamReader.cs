// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/06 13:00:38
// FileName：Assets/Assets/Editor/ConfigManager/CsvStreamReader.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CsvReader  {

    private string[][] data;
    private string fileName;
    private int row, column;

    public CsvReader(string fileName)
    {
        this.fileName = fileName;
        LoadCsvFile();
    }

    public string FileName
    {
        get
        {
            return fileName;
        }
    }

    private string FileRealName
    {
        get
        {
            return fileName.Substring(7);
        }
    }

    public int Row
    {
        get
        {
            return row;
        }
    }

    public int Column
    {
        get
        {
            return column;
        }
    }

    public string this[int row,int col]
    {
        get
        {
            if((row<this.row) && (col < this.column))
            {
                return data[row][column];
            }
            else
            {
                Debug.Log("index out of range when reading" + this.fileName);
                return "";
            }
        }
    }

    private void LoadCsvFile()
    {

        string[] lineArray = null;
        TextAsset binAsset = null;
        if (ConfigManager.Instance.bIntervene)
        {
            string curFileText = System.IO.File.ReadAllText(Application.dataPath + "/GameData/" + fileName + ".csv");
            lineArray = curFileText.Split('\n');
        }
        else
        {
            // string curFileText = System.IO.File.ReadAllText(Application.dataPath + "/Assets/GameData/" + fileName + ".csv");
            // lineArray = curFileText.Split('\n');
            string path = string.Concat("Assets/GameData/",fileName+".csv");
            //binAsset = Resources.Load(path,typeof(TextAsset)) as TextAsset;
            binAsset = Resources.Load<TextAsset>(string.Concat("Assets/GameData/",fileName+".csv"));
            if(binAsset == null)
            {
                Debug.LogError(path);
                Debug.LogError("asset is null");
                return;
            }
            lineArray = binAsset.text.Split('\n');
        }

        this.row = lineArray.Length - 1;
        data = new string[lineArray.Length - 1][];
        for(int i = 0; i < lineArray.Length - 1; i++)
        {
            data[i] = lineArray[i].Split(',');
        }
        this.column = data[0].Length;
        if (!ConfigManager.Instance.bIntervene)
        {

        }

    }



}
