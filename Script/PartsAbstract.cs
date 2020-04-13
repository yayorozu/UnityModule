using UnityEngine;

namespace Yorozu
{
	/// <summary>
	/// パーツのコンポーネント
	/// </summary>
	public abstract class PartsAbstract : MonoBehaviour
	{
		private PartsControlAbstract _owner;

		protected T GetController<T>() where T : PartsControlAbstract
		{
			return _owner as T;
		}

		public void SetUp(PartsControlAbstract owner)
		{
			_owner = owner;
			OnSetUp();
		}

		protected virtual void OnSetUp() { }

		public void UpdateFromOwner()
		{
			if (_owner == null)
				return;

			OnUpdate();
		}

		protected virtual void OnUpdate() { }

#if UNITY_EDITOR

		/// <summary>
		/// 必要なら override する
		/// </summary>
		public virtual void DrawEditor(PartsControlAbstract parts, UnityEditor.SerializedProperty property)
		{
		}

		private void OnValidate()
		{
			hideFlags = HideFlags.HideInInspector;
			OnValidated();
		}

		protected virtual void OnValidated() { }

#endif
	}
}