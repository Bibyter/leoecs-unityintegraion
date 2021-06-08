using Leopotam.Ecs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bibyter.LeoecsEditor
{
#if UNITY_EDITOR
    public sealed class FullDrawInEcsWindowAttribute : System.Attribute
    { }

    public static class EcsEditorRouter

    {
        public static event System.Action<EcsWorld> onCreate;
        public static event System.Action<EcsWorld, EcsEntity> onSelectEntity;

        public static void Create(EcsWorld world)
        {
            onCreate?.Invoke(world);
        }


        public static void SelectEntity(EcsEntity entity)
        {
            onSelectEntity?.Invoke(null, entity);
        }

        public static void SelectEntity(EcsEntity entity, EcsWorld world)
        {
            onSelectEntity?.Invoke(world, entity);
        }
    }
#endif
}
