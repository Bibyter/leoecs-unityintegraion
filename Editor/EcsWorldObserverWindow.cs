using Leopotam.Ecs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Bibyter.LeoecsEditor
{
    public sealed class EcsWorldObserverWindow : ISubwindow
    {
        Vector2 _entitiesListScrollPosition;
        int _activeEntity;
        string _filter;
        EntityInspector _entityDrawer;
        GUIStyle _entityButtonStyle;
        EcsWorldObserver _worldObserver;

        public EcsWorldObserver worldObserver
        {
            get { return _worldObserver; }
        }


        public EcsWorldObserverWindow(EcsWorldObserver worldObserver)
        {
            _worldObserver = worldObserver;
            _entityDrawer = new EntityInspector();
            _activeEntity = -1;
            _entityButtonStyle = null;
        }

        public void OnEnable()
        {
            
        }

        public void OnDisable()
        {

        }

        public void OnGui(Rect position)
        {
            _entityButtonStyle = _entityButtonStyle ?? GetEntityButtonStyle();

            EditorGUILayout.Space();

            _filter = EditorGUILayout.DelayedTextField(_filter, EditorStyles.toolbarSearchField);


            EditorGUILayout.BeginHorizontal();

            _entitiesListScrollPosition = EditorGUILayout.BeginScrollView(_entitiesListScrollPosition, false, true, GUILayout.Height(position.height - 60f), GUILayout.MinWidth(position.width * 0.55f));
            EditorGUILayout.BeginVertical();
            DrawEntities();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            GUILayout.Box(string.Empty, GUILayout.MinHeight(2000f), GUILayout.Width(1f));

            EditorGUILayout.BeginVertical();
            DrawEntityComponents();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        public void SetSelectedEntity(EcsEntity entity)
        {
            if (_worldObserver.HasEntity(entity))
            {
                _activeEntity = _worldObserver.GetLocalIdEntity(entity);
                EditorGUIUtility.editingTextField = false;
            }
        }

        void DrawEntities()
        {
            for (int i = 0; i < _worldObserver.entitiesCount; i++)
            {
                ref var entityData = ref _worldObserver.GetEntityData(i);

                if (!string.IsNullOrEmpty(_filter) && !entityData.name.Contains(_filter))
                    continue;

                if (i == _activeEntity)
                    GUI.enabled = false;

                bool isClick = GUILayout.Button(entityData.name, _entityButtonStyle);

                if (i == _activeEntity)
                    GUI.enabled = true;

                if (isClick)
                {
                    _activeEntity = i;
                    EditorGUIUtility.editingTextField = false;
                }
            }
        }

        void DrawEntityComponents()
        {
            if (_activeEntity >= 0 && _activeEntity < _worldObserver.entitiesCount)
            {
                _entityDrawer.Draw(_worldObserver.GetEntityData(_activeEntity).ecsEntity);
            }
            else
            {
                EditorGUILayout.LabelField("Dont selected entity");
            }
        }

        GUIStyle GetEntityButtonStyle()
        {
            var style = new GUIStyle(GUI.skin.button);
            style.alignment = TextAnchor.MiddleLeft;

            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
            t.Apply();
            style.normal.background = t;

            var t2 = new Texture2D(1, 1);
            t2.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.08f));
            t2.Apply();
            style.hover.background = t2;

            style.fixedHeight = 17;

            return style;
        }
    }
}
