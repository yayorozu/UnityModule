using System;

namespace Yorozu
{
	/// <summary>
	/// パーツのコンポーネント
	/// </summary>
	[Serializable]
	public abstract class ModuleAbstract
	{
		private ModuleControlAbstract _owner;

		protected T GetController<T>() where T : ModuleControlAbstract
		{
			return _owner as T;
		}

		public void SetUp(ModuleControlAbstract owner)
		{
			_owner = owner;
			Prepare();
		}

		public void UpdateFromOwner()
		{
			if (_owner == null)
				return;

			Update();
		}

		protected virtual void Prepare() { }

		protected virtual void Update() { }

#if UNITY_EDITOR

		/// <summary>
		/// 必要なら override する
		/// </summary>
		public virtual void DrawEditor(ModuleControlAbstract module, UnityEditor.SerializedProperty property)
		{
		}

#endif
	}
}
