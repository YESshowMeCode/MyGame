// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/06 09:49:13
// FileName：Assets/Assets/Scripts/Ext/ConfigManager/ConfigManager.cs
// ========================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class ConfigManager : MonoSingleton<ConfigManager>
{

	private ConfigManager() { }

    private TestConfig m_TextConfig;


    public TestConfig testConfig { get { return m_TextConfig; } }


    private bool m_bGenIntervene;

    public bool bIntervene
    {
        get
        {
            return m_bGenIntervene;
        }
    }

    public void Initialize(bool intervene = false)
    {
        m_TextConfig = new TestConfig();
        m_TextConfig.Load("config/TestConfig");
    }
}
