using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Leopotam.Ecs;
using System;
using System.Text;

namespace Bibyter.LeoecsEditor
{
    public interface ISubwindow
    {
        void OnEnable();
        void OnDisable();
        void OnGui(Rect position);
    }

    public sealed class LeoecsWindow : EditorWindow
    {
        static EcsWorldList _worldList;
        bool _isListened;
        ISubwindow _subwindow;
        

        public void OnEnable()
        {
            if (_worldList.Count > 0)
            {
                SetListenedWindow();
                _isListened = true;
            }
            else
            {
                SetSubwindow(new NotListenedWindow());
                _isListened = false;
            }

            EcsEditorRouter.onCreate += OnCreateWorld;
        }

        public void OnDisable()
        {
            EcsEditorRouter.onCreate -= OnCreateWorld;
        }

        public void OnGUI()
        {
            _subwindow.OnGui(position);
        }
       

        public void Update()
        {
            Repaint();
        }

        void OnAllWorldsDestroy()
        {
            if (_isListened)
            {
                SetSubwindow(new NotListenedWindow());
                _isListened = false;
            }
        }

        void SetListenedWindow()
        {
            if (!_isListened)
            {
                var window = new WorldsWindow(_worldList);
                window.onAllWorldDestroy += OnAllWorldsDestroy;

                SetSubwindow(window);

                _isListened = true;
            }
        }

        public void SetSubwindow(ISubwindow subwindow)
        {
            _subwindow?.OnDisable();
            _subwindow = subwindow;
            _subwindow.OnEnable();
        }

        void OnCreateWorld(EcsWorld world)
        {
            SetListenedWindow();
        }

        [MenuItem("Window/Leoecs Editor")]
        static void Init()
        {
            var window = EditorWindow.GetWindow<LeoecsWindow>();
            window.Show();
        }

        [InitializeOnLoadMethod]
        static void StartupMethod()
        {
            if (_worldList == null)
                _worldList = new EcsWorldList();

            EcsEditorRouter.onCreate += (world) =>
            {
                _worldList.OnCreateWorld(world);
            };
        }
    }
}
