using Leopotam.Ecs;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bibyter.LeoecsEditor
{
    public sealed class NotListenedWindow : ISubwindow
    {
        GUIStyle _labelStyle;

        public void OnDisable()
        {
        }

        public void OnEnable()
        {
        }

        public void OnGui(Rect position)
        {
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label);
                _labelStyle.alignment = TextAnchor.MiddleCenter;
                _labelStyle.fontSize = 16;
            }

            EditorGUILayout.LabelField("Not connected ecs world", _labelStyle, GUILayout.Height(position.height));
        }
    }
}
