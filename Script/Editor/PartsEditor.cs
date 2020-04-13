using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Yorozu
{
	[CustomEditor(typeof(PartsControlAbstract), true)]
	public class PartsEditor : Editor
	{
		private PartsControlAbstract _parts;
		private Type[] _types;
		private SerializedProperty _partsProperty;
		private Dictionary<Type, TypeInfo> _componentInfos;

		private class TypeInfo
		{
			public PartsAbstract Component;
			public int Index;
			private Editor _editor;
			private float _height;
			public bool HasComponent => Component != null;
			public readonly bool IsOverrideMethod;

			public TypeInfo(Type type, PartsAbstract component)
			{
				Component = component;
				var method = type.GetMethod("DrawEditor", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
				if (method == null)
					IsOverrideMethod = false;
				else
					IsOverrideMethod = method == method.GetBaseDefinition();
			}

			public void SetComponent(PartsAbstract component)
			{
				if (component == null)
				{
					DestroyImmediate(Component, true);
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
			_parts = target as PartsControlAbstract;

			// 必要タイプを取得
			_types = Assembly.Load(Const.ASSEMBLY_PATH)
				.GetTypes()
				.Where(t => t.IsSubclassOf(_parts.PartsType()))
				.ToArray();

			_partsProperty = serializedObject.FindProperty("_parts");

			_componentInfos = new Dictionary<Type, TypeInfo>();
			foreach (var type in _types)
			{
				if (_parts == null)
					continue;

				var component = (PartsAbstract) _parts.gameObject.GetComponent(type);
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

			if (_partsProperty == null)
				return;

			serializedObject.UpdateIfRequiredOrScript();
			var iterator = serializedObject.GetIterator();
			for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
			{
				if (iterator.displayName == "Parts")
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
			EditorGUILayout.LabelField("Parts");

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
								.SetComponent(enable ? (PartsAbstract) _parts.gameObject.AddComponent(type) : null);

							_partsProperty.ClearArray();
							foreach (var pair in _componentInfos)
							{
								if (!pair.Value.HasComponent)
									continue;

								_partsProperty.InsertArrayElementAtIndex(_partsProperty.arraySize);
								var element = _partsProperty.GetArrayElementAtIndex(_partsProperty.arraySize - 1);
								element.objectReferenceValue = pair.Value.Component;
							}

							BuildIndexGroup();
							serializedObject.ApplyModifiedProperties();
							GUIUtility.ExitGUI();
						}
					}

					if (_componentInfos[type].IsOverrideMethod && _componentInfos[type].Index >= 0)
						_componentInfos[type].Component.DrawEditor(_parts,
							_partsProperty.GetArrayElementAtIndex(_componentInfos[type].Index));
					else
						_componentInfos[type].Draw();
				}
		}

		private void BuildIndexGroup()
		{
			foreach (var pair in _componentInfos)
			{
				pair.Value.Index = -1;
				for (var i = 0; i < _partsProperty.arraySize; i++)
				{
					var element = _partsProperty.GetArrayElementAtIndex(i).objectReferenceValue;

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