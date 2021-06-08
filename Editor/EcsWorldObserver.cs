using Leopotam.Ecs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Bibyter.LeoecsEditor
{
    public sealed class EcsWorldList
    {
        public event System.Action<EcsWorldObserver> onAdd;
        public event System.Action<EcsWorldObserver> onDelete;

        List<EcsWorldObserver> _observers;

        public EcsWorldList()
        {
            _observers = new List<EcsWorldObserver>();
        }

        public void OnCreateWorld(EcsWorld world)
        {
            var observer = new EcsWorldObserver(world);
            observer.onWorldDestoy += OnDestroyWorld;
            world.AddDebugListener(observer);
            _observers.Add(observer);
            onAdd?.Invoke(observer);
        }

        public int Count
        {
            get { return _observers.Count; }
        }

        public EcsWorldObserver Get(int i)
        {
            return _observers[i];
        }

        void OnDestroyWorld(EcsWorldObserver observer)
        {
            _observers.Remove(observer);
            onDelete?.Invoke(observer);
        }
    }

    public sealed class EcsWorldObserver : IEcsWorldDebugListener
    {
        public struct Entity
        {
            public bool isActive;
            public string name;
            public EcsEntity ecsEntity;
        }

        static Type[] _componentTypesCache = new Type[32];

        public event Action<EcsWorldObserver> onWorldDestoy;

        Dictionary<int, int> _entityIds;
        Entity[] _entities;
        EcsWorld _world;

        int _entitiesCount;
        public int entitiesCount
        {
            get { return _entitiesCount; }
        }

        public EcsWorld ecsWorld
        {
            get { return _world; }
        }


        public EcsWorldObserver(EcsWorld world)
        {
            _entities = new Entity[32];
            _entityIds = new Dictionary<int, int>(128);
            _world = world;
        }

        public bool HasEntity(EcsEntity entity)
        {
            return _entityIds.ContainsKey(entity.GetInternalId());
        }

        public int GetLocalIdEntity(EcsEntity ecsEntity)
        {
            return _entityIds[ecsEntity.GetInternalId()];
        }

        public ref Entity GetEntityData(int id)
        {
            return ref _entities[id];
        }

        void IEcsWorldDebugListener.OnEntityCreated(EcsEntity entity)
        {
            if (!_entityIds.TryGetValue(entity.GetInternalId(), out var windowEntityId))
            {
                _entityIds.Add(entity.GetInternalId(), _entitiesCount);
                windowEntityId = _entitiesCount;
                _entitiesCount++;

                ValidateEntitiesArraySize();
            }

            _entities[windowEntityId].name = GetEntityName(entity, false);
            _entities[windowEntityId].isActive = true;
            _entities[windowEntityId].ecsEntity = entity;
        }

        void IEcsWorldDebugListener.OnEntityDestroyed(EcsEntity entity)
        {
            var windowEntityId = _entityIds[entity.GetInternalId()];
            _entities[windowEntityId].isActive = false;
            _entities[windowEntityId].name = GetEntityName(entity, false);
            _entities[windowEntityId].ecsEntity = EcsEntity.Null;
        }

        void IEcsWorldDebugListener.OnComponentListChanged(EcsEntity entity)
        {
            var windowEntityId = _entityIds[entity.GetInternalId()];
            _entities[windowEntityId].name = GetEntityName(entity, true);
        }

        void IEcsWorldDebugListener.OnFilterCreated(EcsFilter filter)
        { }

        void IEcsWorldDebugListener.OnWorldDestroyed(EcsWorld world)
        {
            _entitiesCount = 0;
            onWorldDestoy?.Invoke(this);
        }

        void ValidateEntitiesArraySize()
        {
            if (_entitiesCount >= _entities.Length)
            {
                Array.Resize(ref _entities, _entities.Length * 2);
            }
        }

        string GetEntityName(EcsEntity entity, bool requestComponents)
        {
            var entityId = entity.GetInternalId();
            var entityName = entityId.ToString("D8");

            if (!entity.IsAlive() || !requestComponents)
                return entityName;

            var stringBuilder = new StringBuilder(entityName, 128);
            var count = entity.GetComponentTypes(ref _componentTypesCache);
            for (var i = 0; i < count; i++)
            {
                stringBuilder.Append(":");
                stringBuilder.Append(_componentTypesCache[i].Name);
                _componentTypesCache[i] = null;
            }

            return stringBuilder.ToString();
        }
    }
}
