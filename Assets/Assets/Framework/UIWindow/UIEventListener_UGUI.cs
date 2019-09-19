using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class UIEventListener_UGUI : MonoBehaviour
 {

	 public UIDele.Dele<GameObject> onClick;

	 public static UIEventListener_UGUI Get(GameObject go , params object[] param)
	 {
		 UIEventListener_UGUI listener = go.GetComponent<UIEventListener_UGUI>();
		 if(listener == null)
		 {
			 listener = go.AddComponent<UIEventListener_UGUI>();
			 
		 }
		return listener;
	 }

	
}
