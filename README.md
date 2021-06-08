# leoecs-unityintegraion with EditorWindow

# How open window?
Window/Leoecs Editor

# How to connect The EcsWorld?
```csharp
void Start () {        
    _world = new EcsWorld ();
    _systems = new EcsSystems (_world);
#if UNITY_EDITOR
    Bibyter.LeoecsEditor.EcsEditorRouter.Create(_world);
#endif
    _systems
        .Add (new TestSystem1 ())
        .Init ();
}
```
