using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {

        UIWindowManager.Instance.Init();
        //UIWindowManager.Instance.ShowUIWindow(UIWindowEnum.eMainStart);
		//ConfigManager.Instance.Initialize();
    }
	
	// Update is called once per frame
	void Update () {
		//var tmp = ConfigManager.Instance.testConfig.Get(1).name;
		//Debug.LogError(tmp);
	}
}
