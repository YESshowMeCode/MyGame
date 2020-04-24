using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenClassInfo  {

	public List<string> notes = new List<string>();
	public 	List<string> attrs = new List<string>();
	public List<string> usings =new List<string>();

	public string namespaceStr;

	public string parent;
	public string name;
	public 	GenBindingFlags flags;


	public List<GenFieldInfo> fields = new List<GenFieldInfo>();
	public List<GenPropertyInfo> propertys = new List<GenPropertyInfo>();

	public Dictionary<string, GenMethodInfo> methods = new Dictionary<string, GenMethodInfo>();
}
