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
		private ModuleControlAbstract _module;
		private Type[] _types;
		private SerializedProperty _moduleProperty;
		private Dictionary<Type, TypeInfo> _componentInfos;

		private class TypeInfo
		{
			public ModuleAbstract Component;
			public int Index;

			private Type _type;
			private Editor _editor;
			private float _height;
			public bool HasComponent => Component != null;
			public readonly bool IsOverrideMethod;

			public TypeInfo(Type type, ModuleAbstract component)
			{
				_type = type;
				Component = component;
				var method = type.GetMethod("DrawEditor", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
				if (method == null)
					IsOverrideMethod = false;
				else
					IsOverrideMethod = method == method.GetBaseDefinition();
			}

			public void SetComponent(ModuleAbstract component)
			{
				// Addした際にコンポーネントある場合もあるのでその対応
				DestroyImmediate(Component, true);
				if (component == null)
				{
					Index = -1;
					DestroyImmediate(_editor);
				}

				Component = component;
			}

			private void SetEditor()
			{
				_editor = CreateEditor(Component);

				// 描画するものがあるか
				_height = 0f;
				var iterator = _editor.serializedObject.GetIterator();
				for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
					if ("m_Script" != iterator.propertyPath)
						_height += EditorGUI.GetPropertyHeight(iterator);
			}

			public void Draw()
			{
				if (!HasComponent)
					return;

				if (_editor == null)
					SetEditor();

				if (_height <= 0f)
					return;

				_editor.serializedObject.UpdateIfRequiredOrScript();
				var iterator = _editor.serializedObject.GetIterator();
				for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
					if ("m_Script" != iterator.propertyPath)
						EditorGUILayout.PropertyField(iterator, true);
				_editor.serializedObject.ApplyModifiedProperties();
			}
		}

		protected void OnEnable()
		{
			_module = target as ModuleControlAbstract;

			var partsType = _module.PartsType();
			if (partsType == null)
				partsType = typeof(T);

			// 必要タイプを取得
			_types = Assembly.Load(Const.ASSEMBLY_PATH)
				.GetTypes()
				.Where(t => !t.IsAbstract && t.IsSubclassOf(partsType))
				.ToArray();

			_moduleProperty = serializedObject.FindProperty("_modules");

			_componentInfos = new Dictionary<Type, TypeInfo>();
			foreach (var type in _types)
			{
				if (_module == null)
					continue;

				var component = (ModuleAbstract) _module.gameObject.GetComponent(type);
				var info = new TypeInfo(type, component);
				_componentInfos.Add(type, info);
			}

			BuildIndexGroup();
		}

		/// <summary>
		/// 多分今ヒエラルキーに表示しているものが消されたら全部呼ばれる
		/// </summary>
		private void OnDestroy()
		{
			if (target != null)
				return;

			// 自身が消されたためパーツを削除
			foreach (var pair in _componentInfos)
				pair.Value.SetComponent(null);
			_componentInfos.Clear();
		}

		public override void OnInspectorGUI()
		{
			if (_types.IsNullOrEmpty())
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

			DrawParts();
		}

		private void DrawParts()
		{
			EditorGUILayout.LabelField("Module");

			foreach (var type in _types)
				using (new GUILayout.VerticalScope("box"))
				{
					using (var check = new EditorGUI.ChangeCheckScope())
					{
						var enable = ToggleFoldout(GetDisplayName(type), _componentInfos[type].HasComponent);
						if (check.changed)
						{
							serializedObject.Update();
							_componentInfos[type]
								.SetComponent(enable ? (ModuleAbstract) _module.gameObject.AddComponent(type) : null);

							_moduleProperty.ClearArray();
							foreach (var pair in _componentInfos)
							{
								if (!pair.Value.HasComponent)
									continue;

								_moduleProperty.InsertArrayElementAtIndex(_moduleProperty.arraySize);
								var element = _moduleProperty.GetArrayElementAtIndex(_moduleProperty.arraySize - 1);
								element.objectReferenceValue = pair.Value.Component;
							}

							BuildIndexGroup();
							serializedObject.ApplyModifiedProperties();
							GUIUtility.ExitGUI();
						}
					}

					if (_componentInfos[type].IsOverrideMethod && _componentInfos[type].Index >= 0)
						_componentInfos[type].Component.DrawEditor(_module,
							_moduleProperty.GetArrayElementAtIndex(_componentInfos[type].Index));
					else
						_componentInfos[type].Draw();
				}
		}

		private void BuildIndexGroup()
		{
			foreach (var pair in _componentInfos)
			{
				pair.Value.Index = -1;
				for (var i = 0; i < _moduleProperty.arraySize; i++)
				{
					var element = _moduleProperty.GetArrayElementAtIndex(i).objectReferenceValue;

					if (element == null)
						continue;

					if (element == pair.Value.Component)
						pair.Value.Index = i;
				}
			}
		}
		
		private static bool ToggleFoldout(string label, bool foldout)
		{
			var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 18);
			var enable = GUI.Toggle(rect, foldout, new GUIContent(label), new GUIStyle("ShurikenModuleTitle"));
			rect.y -= 1;
			rect.x += 1;
			GUI.Toggle(rect, foldout, GUIContent.none, EditorStyles.toggle);

			return enable;
		}

		protected virtual string GetDisplayName(Type type)
		{
			return type.Name;
		}
	}
}