namespace  GameTool
{
	using UnityEngine;

	public abstract class MonoSingleton<T> : MonoBehaviour,ISingleton where T: MonoSingleton<T>{

		protected static T m_Instance = null;
		static bool isDistoryed = false;

		public static bool Isinitialized{get { return m_Instance != null;}}

		public static T Instance
		{
			get{
				if(m_Instance == null)
				{
					var tmp = MonoSingletonCreator.CreateMonoSingleton<T>(true);
					m_Instance = tmp;
					if(m_Instance != null)
					{
						m_Instance.OnSingletonInit();
					}
				}
				return m_Instance;
			}

		}

		public static void Initialize(bool isDontDestory = true)
		{
			m_Instance = MonoSingletonCreator.CreateMonoSingleton<T>(isDontDestory);
			m_Instance.OnSingletonInit();
		}


		public virtual void OnSingletonDestory()
		{

		}

		public virtual void OnSingletonInit(){

		}

		public virtual void Dispose()
		{
			if(MonoSingletonCreator.IsUnitTestMode)
			{
				Transform curTrans = transform;
				do{
					var parent = curTrans.parent;
					DestroyImmediate(curTrans.gameObject);
					curTrans = parent;
				}while(curTrans != null);

				m_Instance = null;
			}
			else
			{
				Destroy(gameObject);
			}
		}

		protected virtual void OnDestory()
		{
			if(m_Instance && m_Instance != this)
			{
				return;
			}

			OnSingletonDestory();
			m_Instance = null;
			isDistoryed = true;
		}

	}
	
}