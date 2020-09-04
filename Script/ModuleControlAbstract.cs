using System;
using System.Linq;
using UnityEngine;

namespace Yorozu
{
	public abstract class ModuleControlAbstract : MonoBehaviour
	{
		[SerializeReference]
		private ModuleAbstract[] _modules = new ModuleAbstract[0];

#if UNITY_EDITOR

		internal ModuleAbstract[] Modules
		{
			get => _modules;
			set => _modules = value;
		}
#endif

		public virtual Type ModuleType()
		{
			return null;
		}

		/// <summary>
		/// Initialize
		/// </summary>
		public void SetUp()
		{
			foreach (var module in _modules)
				module.SetUp(this);
		}

		protected virtual void Update()
		{
			foreach (var module in _modules)
				module.UpdateFromOwner();
		}

		public bool TryGetModule<T>(out T findPart) where T : ModuleAbstract
		{
			var data = _modules.First(p => p.GetType() == typeof(T));
			if (data == null)
			{
				findPart = null;
				return false;
			}

			findPart = data as T;

			return true;
		}
	}
}
