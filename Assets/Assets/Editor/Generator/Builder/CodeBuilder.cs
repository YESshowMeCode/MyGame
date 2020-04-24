using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public abstract class CodeBuilder {

	public abstract StringBuilder Build(GenClassInfo genClassInfo);

	protected string tabs(int count)
	{
		StringBuilder sb = new StringBuilder();
		sb.Append("\n");
		for(int i = 0;i < count;i++)
		{
			sb.Append("    ");
		}
		return sb.ToString();
	}
	
}
