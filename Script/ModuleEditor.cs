#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Yorozu
{
	[CustomEditor(typeof(ModuleControlAbstract), true)]
	public class ModuleEditor : ModuleEditorAbstract<ModuleAbstract>
	{
	}

	public abstract class ModuleEditorAbstract<T> : Editor where T : ModuleAbstract
	{
		private ModuleControlAbstract _base;
		private Type[] _types;
		private SerializedProperty _moduleProperty;
		private Dictionary<Type, TypeInfo> _componentInfos;

		private class TypeInfo
		{
			public int Index => _index;
			public bool HasComponent => _index >= 0;
			public ModuleAbstract Instance => _owner._base.Modules[_index];

			private ModuleEditorAbstract<T> _owner;
			private Type _type;
			private int _index;
			public readonly bool IsOverrideMethod;

			public TypeInfo(Type type, ModuleEditorAbstract<T> owner)
			{
				_type = type;
				_owner = owner;

				UpdateIndex();

				var method = type.GetMethod("DrawEditor", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
				if (method == null)
					IsOverrideMethod = false;
				else
					IsOverrideMethod = method == method.GetBaseDefinition();
			}

			public void UpdateIndex()
			{
				_index = -1;
				for (var i = 0; i < _owner._base.Modules.Length; i++)
				{
					if (_owner._base.Modules[i].GetType() != _type)
						continue;

					_index = i;
				}
			}

			public void Add()
			{
				var m = _owner._base.Modules;
				ArrayUtility.Add(ref m, (T) Activator.CreateInstance(_type));
				_owner._base.Modules = m;
				_index = _owner._base.Modules.Length - 1;
			}

			public void Remove()
			{
				var m = _owner._base.Modules;
				ArrayUtility.RemoveAt(ref m, _index);
				_owner._base.Modules = m;
				_index = -1;
			}

			public void Draw()
			{
				if (!HasComponent)
					return;

				_owner._moduleProperty.serializedObject.UpdateIfRequiredOrScript();
				DrawProperty(_owner._moduleProperty.GetArrayElementAtIndex(_index));
				_owner._moduleProperty.serializedObject.ApplyModifiedProperties();
			}

			private void DrawProperty(SerializedProperty property)
			{
				if (!property.hasVisibleChildren)
					return;

				var depth = property.depth;
				var iterator = property.Copy();
				for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
				{
					if (iterator.depth < depth)
						return;

					depth = iterator.depth;

					using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
						EditorGUILayout.PropertyField(iterator, true);
				}
			}
		}

		protected void OnEnable()
		{
			_base = target as ModuleControlAbstract;

			var moduleType = _base.ModuleType() ?? typeof(T);

			// 必要タイプを取得
			_types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => !t.IsAbstract && t.IsSubclassOf(moduleType))
				.ToArray();

			_moduleProperty = serializedObject.FindProperty("_modules");

			_componentInfos = new Dictionary<Type, TypeInfo>();
			foreach (var type in _types)
			{
				var info = new TypeInfo(type, this);
				_componentInfos.Add(type, info);
			}
		}

		private void OnDestroy()
		{
			if (target != null)
				return;

			_componentInfos.Clear();
		}

		public override void OnInspectorGUI()
		{
			if (_types == null ||  _types.Length <= 0)
				return;

			if (_moduleProperty == null)
				return;

			serializedObject.UpdateIfRequiredOrScript();

			var iterator = serializedObject.GetIterator();
			for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
			{
				if (iterator.displayName == "Modules")
					continue;

				using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
				{
					EditorGUILayout.PropertyField(iterator, true);
				}
			}

			serializedObject.ApplyModifiedProperties();

			GUILayout.Space(10);

			DrawModules();
		}

		private void DrawModules()
		{
			EditorGUILayout.HelpBox("Modules", MessageType.None);

			foreach (var type in _types)
				using (new GUILayout.VerticalScope("box"))
				{
					using (var check = new EditorGUI.ChangeCheckScope())
					{
						ToggleFoldout(GetDisplayName(type), _componentInfos[type].HasComponent);
						if (check.changed)
						{
							serializedObject.Update();
							var m = _base.Modules;
							if (_componentInfos[type].HasComponent)
								ArrayUtility.RemoveAt(ref m, _componentInfos[type].Index);
							else
								ArrayUtility.Add(ref m, (T) Activator.CreateInstance(type));

							_base.Modules = m;

							foreach (var pair in _componentInfos)
								pair.Value.UpdateIndex();

							serializedObject.ApplyModifiedProperties();
							GUIUtility.ExitGUI();
						}
					}

					if (_componentInfos[type].IsOverrideMethod && _componentInfos[type].Index >= 0)
						_componentInfos[type].Instance.DrawEditor(_base, _moduleProperty.GetArrayElementAtIndex(_componentInfos[type].Index));
					else
						_componentInfos[type].Draw();
				}
		}

		private static void ToggleFoldout(string label, bool foldout)
		{
			var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 18);
			GUI.Toggle(rect, foldout, new GUIContent(label), new GUIStyle("ShurikenModuleTitle"));
			rect.y -= 1;
			rect.x += 1;
			GUI.Toggle(rect, foldout, GUIContent.none, EditorStyles.toggle);
		}

		protected virtual string GetDisplayName(Type type)
		{
			return type.Name;
		}
	}
}

#endif
