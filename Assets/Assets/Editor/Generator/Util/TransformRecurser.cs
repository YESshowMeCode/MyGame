using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


public class TransformRecurser {

	public Transform startTR;

	public TransformRecurser(Transform transform)
	{
		startTR = transform;
	}

	public void Recurse(Func<Transform,string,bool> func)
	{
		recurse(startTR,func,"");
	}

	private void recurse(Transform transform , Func<Transform, string,bool> func,string preOrder)
	{
		for(int i=0;i<transform.childCount;i++)
		{
			var child = transform.GetChild(i);
			if(func(child,preOrder+","+i))
			{
				recurse(child,func,preOrder+","+i);
			}
		}
	}

	public static List<int> GetOrder(string str)
	{
		var list = str.Split(',');
		List<int> intList = new List<int>();
		int tmp;
		for(int i=0;i<list.Count();i++)
		{
			
			if(int.TryParse(list[i] , out tmp))
			{
				intList.Add(tmp);
			}
		}
		return intList;

		// return str.Split(',').Where((s)=>s.isInt())
	}
}
