using System;
using System.Linq;
using UnityEngine;

namespace Yorozu
{
	public abstract class ModuleControlAbstract : MonoBehaviour
	{
		[SerializeField]
		protected ModuleAbstract[] _modules = new ModuleAbstract[0];

		/// <summary>
		/// 制御できる class
		/// </summary>
		public virtual Type PartsType()
		{
			return null;
		}

		/// <summary>
		/// Initialize
		/// </summary>
		public void SetUp()
		{
			foreach (var part in _modules)
				part.SetUp(this);
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

		private void Update()
		{
			OnUpdate();
			foreach (var part in _modules)
				part.UpdateFromOwner();
		}

		protected virtual void OnUpdate()
		{
		}
	}
}
