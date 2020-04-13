using System;
using System.Linq;
using UnityEngine;

namespace Yorozu
{
	public abstract class PartsControlAbstract : MonoBehaviour
	{
		[SerializeField]
		protected PartsAbstract[] _parts = new PartsAbstract[0];

		/// <summary>
		/// 制御できる class
		/// </summary>
		public abstract Type PartsType();

		/// <summary>
		/// Initialize
		/// </summary>
		public void SetUp()
		{
			foreach (var part in _parts)
				part.SetUp(this);
		}

		public bool TryGetPart<T>(out T findPart) where T : PartsAbstract
		{
			var data = _parts.First(p => p.GetType() == typeof(T));
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
			foreach (var part in _parts)
				part.UpdateFromOwner();
		}

		protected virtual void OnUpdate()
		{
		}
	}
}
