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
        bool _foldoutListValue;

        public void Draw(EcsEntity entity)
        {
            var count = entity.IsAlive() ? entity.GetComponentValues(ref _componentsCache) : 0;

            if (count == 0)
            {
                EditorGUILayout.LabelField("Empty component list");
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

        void DrawField(object instance, FieldInfo field)
        {
            var fieldValue = field.GetValue(instance);
            var fieldType = field.FieldType;

            if (Attribute.IsDefined(fieldType, typeof(FullDrawInEcsWindowAttribute)) && fieldType.IsValueType)
            {
                EditorGUILayout.LabelField(field.Name, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                foreach (var structField in fieldType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    DrawField(fieldValue, structField);
                }
                EditorGUI.indentLevel--;
                return;
            }

            if (fieldValue is IList)
            {
                EditorGUILayout.LabelField(field.Name);

                _foldoutListValue = EditorGUILayout.BeginFoldoutHeaderGroup(_foldoutListValue, field.Name);

                if (_foldoutListValue)
                {
                    EditorGUI.indentLevel++;
                    var enumerable = fieldValue as IEnumerable;
                    int index = 0;
                    foreach (var item in enumerable)
                    {
                        SlectableLabel(index.ToString(), item.ToString());
                        index++;
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();

                return;
            }

            if (fieldType == typeof(UnityEngine.Object) || fieldType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField(field.Name, fieldValue as UnityEngine.Object, fieldType, false);
                GUI.enabled = true;
                return;
            }

            SlectableLabel(field.Name, fieldValue.ToString());
        }

        void SlectableLabel(string label1, string label2)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label1, GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 16));
            EditorGUILayout.SelectableLabel(label2, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();
        }
    }
}
