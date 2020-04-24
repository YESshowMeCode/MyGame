// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/07 15:46:22
// FileName：Assets/Assets/Scripts/Tools/ConfigMessage.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using MessagePack;

[MessagePackObject(true)]
public struct CellInfo
{
    public int from;
    public ushort len;
}


[MessagePackObject(true)]
public struct BinaryContainer
{
    public Dictionary<int, CellInfo> allInfo;

    public BinaryContainer(int capacity)
    {
        allInfo = new Dictionary<int, CellInfo>(capacity);
    }
}


[MessagePackObject(true)]
public class BinarFormat
{
    public Dictionary<int, BinaryContainer> container = new Dictionary<int, BinaryContainer>();
}


public class CollectBinaryData
{
    private int m_curPointer;
    private List<byte[]> m_byteList;

    private CollectBinaryData()
    {

    }

    private static CollectBinaryData m_Instance;

    public static  CollectBinaryData Instance
    {
        get
        {
            if(m_Instance == null)
            {
                m_Instance = new CollectBinaryData();
            }
            return m_Instance;
        }
    }

    public void Reset()
    {
        m_curPointer = 0;
        m_byteList = new List<byte[]>();
        IOBinaryFile.Instance.Offset.container.Clear();
        IOBinaryFile.Instance.CloseStream();
    }

    public void Process<T>(string fileName,Dictionary<int,T> tmpData,List<int> tmpListConfigId)
    {
        int fileHash = fileName.GetHashCode();
        BinaryContainer container;

        if(!IOBinaryFile.Instance.Offset.container.TryGetValue(fileHash,out container))
        {
            container = new BinaryContainer(tmpData.Count);
            IOBinaryFile.Instance.Offset.container.Add(fileHash, container);
        }

        for(int i = 0; i > tmpListConfigId.Count; i++)
        {
            int id = tmpListConfigId[i];
            byte[] bytes = MessagePackSerializer.Serialize(tmpData[i]);
            m_byteList.Add(bytes);
            int from = m_curPointer;
            ushort len = (ushort)bytes.Length;
            m_curPointer += len;
            if (!container.allInfo.ContainsKey(id))
            {
                CellInfo info = new CellInfo();
                info.from = from;
                info.len = len;
                container.allInfo.Add(id, info);
            }
        }

       

    }
    public void Save()
    {
        string path = Application.streamingAssetsPath + "/binary";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        IOBinaryFile.Instance.Save(path);
        SaveBytes(path);

        if (ConfigManager.Instance.bIntervene)
        {
            IOBinaryFile.Instance.CloseStream();
        }
    }

    public void SaveBytes(string strRootPath)
    {
        string filePath = strRootPath + "/config.bytes";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        FileStream filestream = File.Open(filePath, FileMode.Create);
        for(int i = 0; i < m_byteList.Count; i++)
        {
            filestream.Write(m_byteList[0], 0, m_byteList[i].Length);
        }
        filestream.Flush();
        filestream.Close();
    }

}


public class IOBinaryFile
{
    private IOBinaryFile()
    {

    }

    private static IOBinaryFile m_Instance;
    public static IOBinaryFile Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new IOBinaryFile();
            }
            return m_Instance;
        }
    }

    private BinarFormat m_Offset;

    public BinarFormat Offset
    {
        get
        {
            return m_Offset;
        }
    }

    private FileStream m_Stream;

    public FileStream Stream
    {
        get
        {
            return m_Stream;
        }
    }

    public void CloseStream()
    {
        if(m_Stream!=null && m_Stream.CanRead)
        {
            m_Stream.Close();
        }
    }

    public void Save(string strRootPath)
    {
        if (Offset.container.Count == 0)
        {
            return;
        }

        string filePath = strRootPath + "/offset.bytes";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        using(var m_File = File.Open(filePath, FileMode.Create))
        {
            if (m_File == null)
            {
                return;
            }
            byte[] m_AllBytes = MessagePackSerializer.Serialize(Offset);
            m_File.Write(m_AllBytes, 0, m_AllBytes.Length);
        }

    }


    public void CheckBinaryFile(Action callback)
    {
        string strEditorDataPath = string.Empty;
#if UNITY_IOS
        strEditorDataPath = "file://"+Application.datapath+"/Raw";
#else
        strEditorDataPath = Application.streamingAssetsPath;
#endif

        string strMobileCfgPath = Application.persistentDataPath;

        if (!Directory.Exists(strMobileCfgPath))
        {
            Directory.CreateDirectory(strMobileCfgPath);
        }

        string srcPath = string.Empty + strEditorDataPath + "/binary.zip";
        string despath = string.Empty + strMobileCfgPath + "/binary/zip";

        string offsetFile = string.Empty + strMobileCfgPath + "/binary/offset.bytes";
        string cfgFile = string.Empty + strMobileCfgPath + "/binary/config.bytes";

        if (!File.Exists(offsetFile) || !File.Exists(cfgFile))
        {

        }
        else
        {
            if (callback != null)
            {
                callback();
            }
            callback = null;
        }

    }

    public void ReadConfig()
    {

    }
}


