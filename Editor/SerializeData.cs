using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bibyter.LeoecsEditor
{
    [System.Serializable]
    public sealed class SerializeContainer
    {
        public EcsWorldObserverWindowData[] data1;

        public SerializeContainer(LeoecsWindow window)
        {
            data1 = new EcsWorldObserverWindowData[16];
            for (int i = 0; i < data1.Length; i++)
            {
                data1[i] = new EcsWorldObserverWindowData();
                data1[i].SetWindow(window);
            }
        }
    }

    [System.Serializable]
    public class SerializeData
    {
        LeoecsWindow _window;

        public void SetWindow(LeoecsWindow window)
        {
            _window = window;
        }

        public void Record()
        {
            Undo.RecordObject(_window, "Leoecs Editor");
        }
    }

    [System.Serializable]
    public sealed class EcsWorldObserverWindowData : SerializeData
    {
        public int activeEntity;
    }
}
