namespace  GameTool
{
	using System;

	[AttributeUsage(AttributeTargets.Class)]

	public class MonoSingletonPath : Attribute
	{
		private string m_PathInHierachy;

		public MonoSingletonPath(string pathInHierarchy)
		{
			m_PathInHierachy = pathInHierarchy;
		}

		public string PathInHierachy
		{
			get{return m_PathInHierachy;}
		}

		[Obsolete("kanbudong")]
		[AttributeUsage(AttributeTargets.Class)]
		public class MonoSingletonAttribute : MonoSingletonPath
		{
			public MonoSingletonAttribute(string pathInHierarchy) : base(pathInHierarchy)
			{
					
			}
		}
	}
	
}