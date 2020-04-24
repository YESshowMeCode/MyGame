// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/06 09:49:25
// FileName：Assets/Assets/Scripts/Ext/ConfigManager/ConfigLoader.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using MessagePack;

public class Stringpair
{
    public string key = string.Empty;
    public string value = string.Empty;
}


public class ConfigTemplate
{
    public int id;
}

public class ArrayBytesPool
{
    public static byte[] m_binarybytes = new byte[4096];
}

public class DataAdapter<T> where T:ConfigTemplate,new()
{
    private int idFile;
    private FileStream m_stream { get { return IOBinaryFile.Instance.Stream; } }
    private BinaryContainer m_Container { get { return IOBinaryFile.Instance.Offset.container[idFile]; } }
    private List<T> m_value;
    private Dictionary<int, T> m_data;
    public DataAdapter(int idFile)
    {
        this.idFile = idFile;
        this.m_data = new Dictionary<int, T>();
        this.m_value = new List<T>();
    }


    public Dictionary<int,T> data
    {
        get
        {
            if (m_data.Count != m_Container.allInfo.Count)
            {
                foreach(var id in this.m_Container.allInfo.Keys)
                {
                    if (!this.m_data.ContainsKey(id))
                    {
                        Serialize(id);
                    }
                }
            }
            return m_data;
        }
    }
    public int Count { get { return this.m_Container.allInfo.Count; } }

    public T this[int id]
    {
        get
        {
            T tmpl = null;
            TryGetValue(id, out tmpl);
            return tmpl;
        }
    }

    public List<T> Value
    {
        get
        {
            if (this.m_value.Count != 0)
            {
                return m_value;
            }

            SerializeList();
            return m_value;
        }
    }

    public void ForEach(Action<int, T> action)
    {
        IEnumerator<T> itor = GetEnumerator();
        while (itor.MoveNext())
        {
            T tmp = null;
            TryGetValue(itor.Current.id, out tmp);
            if(action != null)
            {
                action(itor.Current.id, tmp);
            }
        }
        itor.Dispose();
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (int id in this.m_Container.allInfo.Keys)
        {
            if (this.m_data.ContainsKey(id))
            {
                Serialize(id);
            }
            yield return this.m_data[id];
        }
    }


    public bool TryGetValue(int id,out T tmpl)
    {
        tmpl = null;
        if(this.m_data.TryGetValue(id,out tmpl))
        {
            return true;
        }

        tmpl = this.m_data[id];
        return true;
    }

    public bool ContainKey(int id)
    {
        return false;
    }

    public void Add(int id,T val)
    {
        if (!this.m_data.ContainsKey(id))
        {
            this.m_data.Add(id, val);
        }
    }


    private bool Serialize(int id)
    {
        CellInfo cellInfo;
        if(!this.m_Container.allInfo.TryGetValue(id,out cellInfo))
        {
            return false;
        }

        if (ArrayBytesPool.m_binarybytes.Length < cellInfo.len)
        {
            ArrayBytesPool.m_binarybytes = new byte[cellInfo.len];
        }

        this.m_stream.Seek(cellInfo.from, SeekOrigin.Begin);
        this.m_stream.Read(ArrayBytesPool.m_binarybytes, 0, cellInfo.len);
        this.m_data.Add(id, MessagePackSerializer.Deserialize<T>(new ArraySegment<byte>(ArrayBytesPool.m_binarybytes, 0, cellInfo.len)));
        return true;
    }


    private void SerializeList()
    {
        IEnumerator<T> itor = GetEnumerator();
        while (itor.MoveNext())
        {
            T tmp = null;
            TryGetValue(itor.Current.id, out tmp);
        }
        itor.Dispose();

        foreach(var item in this.m_data.Values)
        {
            this.m_value.Add(item);
        }
    }

}


public abstract class ConfigLoader<T> where T: ConfigTemplate,new()
{
    protected CsvReader reader;
    private int m_fileID;
    private DataAdapter<T> m_data;


    public DataAdapter<T> data
    {
        get
        {
            if (m_data == null)
            {
                m_data = new DataAdapter<T>(m_fileID);
            }
            return m_data;
        }
    }

    public Dictionary<int, T> EditorData = null;

    public T Get(int id)
    {
        T tmpl;
        if (!Application.isPlaying)
        {
            if(!EditorData.TryGetValue(id,out tmpl))
            {
                Debug.LogWarning("cant find " + " template: " + id);
            }
        }
        else
        {
            if(!data.TryGetValue(id, out tmpl))
            {
                Debug.LogWarning("cant find " + " template: " + id);
            }
        }
        return tmpl;
    }


    public bool TryGet(int id ,out T tmpl,bool isCannotFind = false)
    {
        if (!Application.isPlaying)
        {
            if (!EditorData.TryGetValue(id, out tmpl))
            {
                if (!isCannotFind)
                {
                    Debug.LogWarning("cant find " + " template: " + id);
                }
                return false;
            }
        }
        else
        {
            if (!data.TryGetValue(id, out tmpl)){
                if (isCannotFind)
                {
                    Debug.LogWarning("cant find " + " template: " + id);
                }
                return false;
            }
        }
        return true;
    }

    protected abstract void readNode(CsvReader reader, int row, T tmpl);
	
    public int Load(string path,bool bNeedEditorData = false)
    {
        if(Application.isPlaying && !ConfigManager.Instance.bIntervene)
        {
            string fileRealName = path.Substring(7);
            m_fileID = fileRealName.GetHashCode();

        }

        Dictionary<int, T> tmpData = new Dictionary<int, T>();
        List<int> tmpListConfigID = new List<int>();
        reader = new CsvReader(path);

        for(int i = 2; i< reader.Row; i++)
        {
            var id = ReadInt(reader, i, 0);
            if (string.IsNullOrEmpty(reader[i, 0]))
            {
                continue;
            }

            T tmpl = new T();
            tmpl.id = id;
            tmpListConfigID.Add(tmpl.id);
            readNode(reader, i, tmpl);
            if (!tmpData.ContainsKey(tmpl.id))
            {
                tmpData.Add(tmpl.id, tmpl);
            }

            if (bNeedEditorData)
            {
                EditorData = tmpData;
            }
        }

        if((!Application.isPlaying)|| ConfigManager.Instance.bIntervene)
        {
            MessagepackStoreData(tmpData, tmpListConfigID);
        }
        return 0;
    }

    public void MessagepackStoreData(Dictionary<int,T> tmpData,List<int> tmpListConfigID)
    {
        CollectBinaryData.Instance.Process(reader.FileName, tmpData, tmpListConfigID);
    }

    public void MessagePackReadData()
    {
        IOBinaryFile.Instance.ReadConfig();
    }



    public static bool ReadBool(CsvReader reader,int row,int column)
    {
        string str = reader[row, column];
        if (!string.IsNullOrEmpty(str))
        {
            if(str == "t"|| str=="T" || str.ToLower() == "true")
            {
                return true;
            }
            else if(str == "f" || str=="F" || str.ToLower() == "false" || str.Trim() == "")
            {
                return false;
            }
            else
            {
                throw new Exception("Error Reading CSV file: " + reader.FileName + " ,Content = " + str + ",Row=" + row + "Column = " + column);
            }
        }
        return false;
    }


    public static int ReadInt(CsvReader reader, int row, int column)
    {
        string str = reader[row, column];
        if (!string.IsNullOrEmpty(str))
        {
            int tmp;
            if(int.TryParse(str,out tmp))
            {
                return tmp;
            }
            else
            {
                tmp = str.GetHashCode();
                return tmp;
            }
        }
        return 0;
    }

    public static int[] ReadIntArray(CsvReader reader, int row, int column)
    {
        int[] vec = null;
        string str = reader[row, column];
        if (!string.IsNullOrEmpty(str.Trim()))
        {
            return new int[] { };
        }

        string[] split = str.Split('|');
        if (split.Length < 1)
        {
            vec = new int[] { };
        }
        else
        {
            vec = new int[split.Length];
        }

        bool temp = true;

        for(int i = 0; i < split.Length; i++)
        {
            temp = temp && int.TryParse(split[i], out vec[i]);
        }
        if (!temp)
        {
            throw new Exception("Error Reading CSV file: " + reader.FileName + " ,Content = " + str + ",Row=" + row + "Column = " + column);
        }
        
        return vec;
    }

    public static int[][] ReadIntArrayArray(CsvReader reader, int row, int column)
    {
       
        string str = reader[row, column];
        if (!string.IsNullOrEmpty(str.Trim()))
        {
            return new int[][] { };
        }

        string[] split = str.Split('|');
        string[][] subSplit = new string[split.Length][];

        for(int i = 0; i < split.Length; i++)
        {
            if (string.IsNullOrEmpty(split[i]))
            {
                subSplit[i] = new string[] { };
                continue;
            }
            subSplit[i] = split[i].Split(':');
        }

        int[][] vec = new int[split.Length][];
        bool temp = true;

        for (int i = 0; i < split.Length; i++)
        {
            vec[i] = new int[subSplit[i].Length];
            for(int j = 0; j < vec[i].Length; ++j)
            {
                temp = temp && Int32.TryParse(subSplit[i][j], out vec[i][j]);
            }

        }
        if (!temp)
        {
            throw new Exception("Error Reading CSV file: " + reader.FileName + " ,Content = " + str + ",Row=" + row + "Column = " + column);
        }

        return vec;
    }

    public static long ReadLong(CsvReader reader, int row, int column)
    {
        string str = reader[row, column];
        if (!string.IsNullOrEmpty(str))
        {
            long tmp;
            if (long.TryParse(str, out tmp))
            {
                return tmp;
            }
            else
            {
                throw new Exception("Error Reading CSV file: " + reader.FileName + " ,Content = " + str + ",Row=" + row + "Column = " + column);
            }
        }
        return 0L;
    }

    public static float ReadFloat(CsvReader reader, int row, int column)
    {
        string str = reader[row, column];
        if (!string.IsNullOrEmpty(str))
        {
            float tmp;
            if (float.TryParse(str, out tmp))
            {
                return tmp;
            }
            else
            {
                throw new Exception("Error Reading CSV file: " + reader.FileName + " ,Content = " + str + ",Row=" + row + "Column = " + column);
            }
        }
        return 0f;
    }

    public static float[] ReadFloatArray(CsvReader reader, int row, int column)
    {
        float[] vec = null;
        string str = reader[row, column];
        if (!string.IsNullOrEmpty(str))
        {
            return new float[] { };
        }

        string[] split = str.Split('|');
        if (split.Length < 1)
        {
            vec = new float[] { };
        }
        else
        {
            vec = new float[split.Length];
        }

        bool temp = true;

        for (int i = 0; i < split.Length; i++)
        {
            temp = temp && float.TryParse(split[i], out vec[i]);
        }
        if (!temp)
        {
            throw new Exception("Error Reading CSV file: " + reader.FileName + " ,Content = " + str + ",Row=" + row + "Column = " + column);
        }

        return vec;
    }


    public static string ReadString(CsvReader reader, int row, int column)
    {
        string str = reader[row, column];
        if (!string.IsNullOrEmpty(str))
        {
            str = "";
        }

        return str;
    }

    public static string[] ReadStringArray(CsvReader reader, int row, int column)
    {
        string str = reader[row, column];
        string[] split;
        if (!string.IsNullOrEmpty(str))
        {
            split = new string[] { };
            return split;
        }
        split = str.Split('|');

        return split;
    }

    public static string[][] ReadStringArrayArray(CsvReader reader, int row, int column)
    {
        string str = reader[row, column];
        string[][] split;
        if (!string.IsNullOrEmpty(str))
        {
            split = new string[][] { };
            return split;
        }
        string[] splits = str.Split('|');
        split = new string[splits.Length][];
        for(int i = 0; i < splits.Length; i++)
        {
            if (!string.IsNullOrEmpty(splits[i]))
            {
                split = new string[][] { };
                continue;
            }
            split[i] = splits[i].Split(':');
        }

        return split;
    }

    public static Vector2 ReadVector2(CsvReader reader, int row, int column)
    {
        Vector2 vec = new Vector2();
        string pos = reader[row, column];
        string[] split = pos.Split('|');
        bool tmp = true;

        if (split.Length == 3)
        {
            tmp = tmp && float.TryParse(split[0], out vec.x);
            tmp = tmp && float.TryParse(split[1], out vec.y);

        }
        else
        {
            vec = Vector2.zero;
        }

        if (!tmp)
        {
            throw new Exception("Error Reading CSV file: " + reader.FileName + " ,Content = " + pos + ",Row=" + row + "Column = " + column);
        }

        return vec;
    }

    public static Vector3 ReadVector3(CsvReader reader, int row, int column)
    {
        Vector3 vec = new Vector3();
        string pos = reader[row, column];
        string[] split = pos.Split('|');
        bool tmp = true;

        if (split.Length == 3)
        {
            tmp = tmp && float.TryParse(split[0], out vec.x);
            tmp = tmp && float.TryParse(split[1], out vec.y);
            tmp = tmp && float.TryParse(split[2], out vec.z);
        }
        else
        {
            vec = Vector3.zero;
        }

        if (!tmp)
        {
            throw new Exception("Error Reading CSV file: " + reader.FileName + " ,Content = " + pos + ",Row=" + row + "Column = " + column);
        }

        return vec;
    }

    public static Vector4 ReadVector4(CsvReader reader, int row, int column)
    {
        Vector4 vec = new Vector4();
        string pos = reader[row, column];
        string[] split = pos.Split('|');
        bool tmp = true;

        if(split.Length == 4)
        {
            tmp = tmp && float.TryParse(split[0], out vec.x);
            tmp = tmp && float.TryParse(split[1], out vec.y);
            tmp = tmp && float.TryParse(split[2], out vec.z);
            tmp = tmp && float.TryParse(split[3], out vec.w);
        }
        else
        {
            vec = Vector4.zero;
        }

        if (!tmp)
        {
            throw new Exception("Error Reading CSV file: " + reader.FileName + " ,Content = " + pos + ",Row=" + row + "Column = " + column);
        }
        
        return vec;
    }
}
