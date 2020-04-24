using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestWindow : MonoBehaviour {

	public Button Btn_Text;
	// Use this for initialization
	void Start () {
		Btn_Text.onClick.AddListener(()=>{
			UIWindowManager.Instance.ShowUIWindow(UIWindowEnum.eNone);
		});
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
