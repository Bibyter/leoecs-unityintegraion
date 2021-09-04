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
        Vector2 _entityComponentsScrollPosition;
        string _filter;
        EntityInspector _entityDrawer;
        GUIStyle _entityButtonStyle;
        EcsWorldObserver _worldObserver;
        EcsWorldObserverWindowData _data;

        public EcsWorldObserver worldObserver
        {
            get { return _worldObserver; }
        }


        public EcsWorldObserverWindow(EcsWorldObserver worldObserver, EcsWorldObserverWindowData data, EntityInspector entityInspector)
        {
            _data = data;
            _worldObserver = worldObserver;
            _entityDrawer = entityInspector;
            _data.activeEntity = -1;
            _entityButtonStyle = null;
        }

        public void OnEnable()
        {
            _entityDrawer.onEntityClick += SetSelectedEntity;
        }

        public void OnDisable()
        {
            _entityDrawer.onEntityClick -= SetSelectedEntity;
        }

        public void OnGui(Rect position)
        {
            _entityButtonStyle = _entityButtonStyle ?? GetEntityButtonStyle();

            EditorGUILayout.Space();

            _filter = EditorGUILayout.DelayedTextField(_filter, EditorStyles.toolbarSearchField);


            EditorGUILayout.BeginHorizontal();

            _entitiesListScrollPosition = EditorGUILayout.BeginScrollView(_entitiesListScrollPosition, false, true, GUILayout.Height(position.height - 60f), GUILayout.Width(position.width * 0.5f));
            EditorGUILayout.BeginVertical();
            DrawEntities();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            GUILayout.Box(string.Empty, GUILayout.MinHeight(2000f), GUILayout.Width(1f));

            _entityComponentsScrollPosition = EditorGUILayout.BeginScrollView(_entityComponentsScrollPosition, false, true, GUILayout.Height(position.height - 60f), GUILayout.Width(position.width * 0.5f));
            EditorGUILayout.BeginVertical();
            DrawEntityComponents();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndHorizontal();
        }

        public void SetSelectedEntity(EcsEntity entity)
        {
            if (_worldObserver.HasEntity(entity))
            {
                _data.Record();
                _data.activeEntity = _worldObserver.GetLocalIdEntity(entity);
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

                if (i == _data.activeEntity)
                    GUI.enabled = false;

                bool isClick = GUILayout.Button(entityData.name, _entityButtonStyle);

                if (i == _data.activeEntity)
                    GUI.enabled = true;

                if (isClick)
                {
                    _data.Record();
                    _data.activeEntity = i;
                    EditorGUIUtility.editingTextField = false;
                }
            }
        }

        void DrawEntityComponents()
        {
            if (_data.activeEntity >= 0 && _data.activeEntity < _worldObserver.entitiesCount)
            {
                _entityDrawer.Draw(_worldObserver.GetEntityData(_data.activeEntity).ecsEntity);
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
