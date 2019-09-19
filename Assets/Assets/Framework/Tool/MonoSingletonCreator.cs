using System.Reflection;
using UnityEngine;


namespace  GameTool
{
	public sealed class MonoSingletonCreator{

		private static bool m_IsUnitTestMode;

		public static bool IsUnitTestMode{
			get{return m_IsUnitTestMode;}
			set{m_IsUnitTestMode = value;}
		}

		public static T CreateMonoSingleton<T>(bool isDontDestory) where T: MonoBehaviour,ISingleton
		{
			
			T instance = null;
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType(typeof(T)) as T;
			}
			if(instance != null)
				return instance;

			MemberInfo info = typeof(T);
			var attributes = info.GetCustomAttributes(true);
			foreach (var attr in attributes)
			{
				var defineAttr = attr as MonoSingletonPath;
				if(defineAttr == null)
				{
					continue;
				}
				//instance = CreateC
			}
			return instance;
		}


		private static T CreateComponentOnGameObject<T>(string path,bool dontDestory) where T:MonoBehaviour
		{
			var obj = FindGameObject(null, path, true, dontDestory);
			if(obj == null)
			{
				obj = new GameObject("(Singleton)" + typeof(T).Name);
				if(dontDestory && !m_IsUnitTestMode)
				{
					Object.DontDestroyOnLoad(obj);
				}
			}

			return obj.AddComponent<T>();
		}
	

		static GameObject FindGameObject(GameObject root, string path, bool build ,bool dontDestory)
		{
			if(path == null || path.Length == 0)
			{
				return null;
			}

			string[] subPath = path.Split('/');
			if(subPath == null || subPath.Length == 0)
				return null;

			return FindGameObject(null,subPath,0,build,dontDestory);
		}

		static GameObject FindGameObject(GameObject root,string[] subPath, int index, bool build, bool dontDestory)
		{
			GameObject client = null;
			if(root == null)
			{
				client = GameObject.Find(subPath[index]);
			}
			else
			{
				var child = root.transform.Find(subPath[index]);
				if(child != null)
				{
					client = child.gameObject;
				}
			}

			if(client == null)
			{
				if(build)
				{
					client = new GameObject(subPath[index]);
					if(root != null)
					{
						client.transform.SetParent(root.transform);
					}

					if(dontDestory && index == 0 && !m_IsUnitTestMode)
					{
						GameObject.DontDestroyOnLoad(client);
					}
				}

			}

			return ++index == subPath.Length ? client :FindGameObject(client,subPath,index,build,dontDestory);
		}
	}
}