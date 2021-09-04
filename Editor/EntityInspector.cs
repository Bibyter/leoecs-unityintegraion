using Leopotam.Ecs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Bibyter.LeoecsEditor
{
    public sealed class EntityInspector
    {
        static object[] _componentsCache = new object[32];

        public event System.Action<EcsEntity> onEntityClick;

        FoldoutDict _foldoutDict;
        bool _needInit;
        GUIStyle _entityButtonStyle;

        public EntityInspector(FoldoutDict foldoutDict)
        {
            _foldoutDict = foldoutDict;
            _needInit = true;
        }

        public void Draw(EcsEntity entity)
        {
            if (_needInit)
            {
                Init();
                _needInit = false;
            }

            var count = entity.IsAlive() ? entity.GetComponentValues(ref _componentsCache) : 0;

            if (count == 0)
            {
                EditorGUILayout.LabelField("Empty component list");
                return;
            }

            for (var i = 0; i < count; i++)
            {
                var component = _componentsCache[i];
                _componentsCache[i] = null;
                var type = component.GetType();
                GUILayout.BeginVertical(GUI.skin.box);
                var typeName = type.Name;

                EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    DrawField(component, field);
                }
                EditorGUI.indentLevel--;

                GUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        void Init()
        {
            _entityButtonStyle = new GUIStyle(GUI.skin.button);
            _entityButtonStyle.alignment = TextAnchor.MiddleLeft;
        }

        void DrawField(object instance, FieldInfo field)
        {
            var fieldValue = field.GetValue(instance);
            var fieldType = field.FieldType;


            if (fieldType == typeof(EcsEntity))
            {
                EntityField(fieldValue, field.Name);
                return;
            }

            if (Attribute.IsDefined(fieldType, typeof(FullDrawInEcsWindowAttribute)) && fieldType.IsValueType)
            {
                NestedStructField(fieldValue, field);
                return;
            }

            if (fieldValue is IList)
            {
                ListField(fieldValue, field.Name);
                return;
            }

            if (fieldType == typeof(UnityEngine.Object) || fieldType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                UnityObjectField(fieldValue, field);
                return;
            }

            SelectableLabel(field.Name, fieldValue == null ? "null" : fieldValue.ToString());
        }

        void UnityObjectField(object fieldValue, FieldInfo fieldInfo)
        {
            EditorGUILayout.BeginHorizontal();
            // don't label inactive
            EditorGUILayout.LabelField(fieldInfo.Name, GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 16));
            GUI.enabled = false;
            EditorGUILayout.ObjectField(fieldValue as UnityEngine.Object, fieldInfo.FieldType, false);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        void NestedStructField(object fieldValue, FieldInfo fieldInfo)
        {
            var fieldType = fieldInfo.FieldType;
            var name = fieldInfo.Name;

            EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            foreach (var structField in fieldType.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                DrawField(fieldValue, structField);
            }
            EditorGUI.indentLevel--;
        }

        void ListField(object fieldValue, string name)
        {
            var foldoutValue = _foldoutDict.Get(name);
            var newFoldoutValue = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutValue, name);

            if (newFoldoutValue != foldoutValue)
            {
                _foldoutDict.Set(name, newFoldoutValue);
            }


            if (newFoldoutValue)
            {
                EditorGUI.indentLevel++;

                var enumerable = fieldValue as IEnumerable;
                int index = 0;

                if (fieldValue is IList<EcsEntity>)
                {
                    foreach (var item in enumerable)
                    {
                        EntityField(item, index.ToString());
                        index++;
                    }
                }
                else
                {
                    foreach (var item in enumerable)
                    {
                        SelectableLabel(index.ToString(), item.ToString());
                        index++;
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void EntityField(object fieldValue, string name)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 16));
            var isClick = GUILayout.Button(fieldValue.ToString(), _entityButtonStyle);
            EditorGUILayout.EndHorizontal();

            if (isClick)
            {
                onEntityClick?.Invoke((EcsEntity)fieldValue);
            }
        }

        void SelectableLabel(string label1, string label2)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label1, GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 16));
            EditorGUILayout.SelectableLabel(label2, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();
        }
    }

    public sealed class FoldoutDict
    {
        Dictionary<string, bool> _dict;

        public FoldoutDict()
        {
            _dict = new Dictionary<string, bool>(32);
        }

        public bool Get(string fieldName)
        {
            if (_dict.TryGetValue(fieldName, out bool value))
            {
                return value;
            }

            return false;
        }

        public void Set(string fieldName, bool value)
        {
            _dict[fieldName] = value;
        }
    }
}
