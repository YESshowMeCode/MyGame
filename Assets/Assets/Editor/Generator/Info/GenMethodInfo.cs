using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenMethodInfo  {

	public GenBindingFlags flags;
	public string name;

	public string returnType;
	public List<GenArgumentInfo> args = new List<GenArgumentInfo>();
	public List<string> codeLines = new List<string>();
}
