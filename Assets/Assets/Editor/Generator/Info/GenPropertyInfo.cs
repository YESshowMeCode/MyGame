using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenPropertyInfo  {

	public enum Type{
		private_set_public_get,
	}

	public bool isStatic;
	public string name;
	public string type;

	public Type getSetType = Type.private_set_public_get;
}
