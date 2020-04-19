using UnityEngine;

namespace Yorozu
{
	/// <summary>
	/// パーツのコンポーネント
	/// </summary>
	public abstract class ModuleAbstract : MonoBehaviour
	{
		private ModuleControlAbstract _owner;

		protected T GetController<T>() where T : ModuleControlAbstract
		{
			return _owner as T;
		}

		public void SetUp(ModuleControlAbstract owner)
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
		public virtual void DrawEditor(ModuleControlAbstract module, UnityEditor.SerializedProperty property)
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