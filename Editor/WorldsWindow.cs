using Leopotam.Ecs;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bibyter.LeoecsEditor
{
    public sealed class WorldsWindow : ISubwindow
    {
        struct WorldObserwerWindow
        {
            public EcsWorldObserverWindow window;
            public string name;
        }

        public event System.Action onAllWorldDestroy;

        List<WorldObserwerWindow> _worldObserverWindows;
        EcsWorldObserverWindow _currentWindow;
        EcsWorldList _worldList;
        SerializeContainer _serializeContainer;


        public WorldsWindow(EcsWorldList worldList, SerializeContainer serializeContainer)
        {
            _serializeContainer = serializeContainer;
            _worldList = worldList;
            _worldObserverWindows = new List<WorldObserwerWindow>();
        }

        public void OnEnable()
        {
            _worldObserverWindows.Clear();

            for (int i = 0; i < _worldList.Count; i++)
            {
                AddWindow(_worldList.Get(i));
            }

            _worldList.onAdd += AddWindow;
            _worldList.onDelete += DeleleWindow;
            EcsEditorRouter.onSelectEntity += OnSelectedEntity;
        }

        public void OnDisable()
        {
            _worldList.onAdd -= AddWindow;
            _worldList.onDelete -= DeleleWindow;
            EcsEditorRouter.onSelectEntity -= OnSelectedEntity;
        }

        public void OnGui(Rect position)
        {
            DrawWorldPanel();

            _currentWindow.OnGui(position);
        }

        void AddWindow(EcsWorldObserver observer)
        {
            var window = new EcsWorldObserverWindow(observer, _serializeContainer.data1[_worldObserverWindows.Count]);

            _worldObserverWindows.Add(new WorldObserwerWindow 
            { 
                name = $"world {_worldObserverWindows.Count + 1}", 
                window = window 
            });

            if (_currentWindow == null)
            {
                _currentWindow = window;
                _currentWindow.OnEnable();
            }
        }

        void DeleleWindow(EcsWorldObserver observer)
        {
            var index = _worldObserverWindows.FindIndex((item) => item.window.worldObserver == observer);
            _worldObserverWindows[index].window.OnDisable();

            _worldObserverWindows.RemoveAt(index);

            if (_worldObserverWindows.Count == 0)
            {
                onAllWorldDestroy?.Invoke();
            }
        }

        public void SetSubwindow(EcsWorldObserverWindow subwindow)
        {
            _currentWindow?.OnDisable();
            _currentWindow = subwindow;
            _currentWindow.OnEnable();
        }

        void DrawWorldPanel()
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < _worldObserverWindows.Count; i++)
            {
                if (_worldObserverWindows[i].window == _currentWindow)
                    GUI.enabled = false;

                bool isClick = GUILayout.Button(_worldObserverWindows[i].name);

                if (_worldObserverWindows[i].window == _currentWindow)
                    GUI.enabled = true;

                if (isClick)
                {
                    _currentWindow.OnDisable();
                    _currentWindow = _worldObserverWindows[i].window;
                    _currentWindow.OnEnable();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void OnSelectedEntity(EcsWorld world, EcsEntity entity)
        {
            if (_worldObserverWindows.Count == 0)
            {
                Debug.LogError("Not connected ecs world");
                return;
            }

            if (world == null)
            {
                SetSubwindow(_worldObserverWindows[0].window);
                _worldObserverWindows[0].window.SetSelectedEntity(entity);
            }
            else
            {
                var index = _worldObserverWindows.FindIndex((item) => item.window.worldObserver.ecsWorld == world);

                if (index == -1)
                {
                    Debug.LogError("Not find ecs world");
                }
                else
                {
                    SetSubwindow(_worldObserverWindows[index].window);
                    _worldObserverWindows[index].window.SetSelectedEntity(entity);
                }
            }

        }
    }
}
