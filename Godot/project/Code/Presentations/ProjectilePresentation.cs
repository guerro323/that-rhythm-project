using Collections.Pooled;
using Godot;
using PataNext.Export.Godot.Presentation;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Modules.Abilities;
using revecs.Core;
using revghost;
using revtask.Core;

namespace QuadrumPrototype.Code.Presentations;

public class ProjectilePresentation : PresentationGodotBaseSystem
{
    private const string Path = "res://scenes/projectile.tscn";
    
    public ProjectilePresentation(Scope scope) : base(scope)
    {
        ResourceLoader.LoadThreadedRequest(Path);
    }

    protected override void GetMatchedComponents(PooledList<ComponentType> all, PooledList<ComponentType> or, PooledList<ComponentType> none)
    {
        all.Add(ProjectileSettings.ToComponentType(GameWorld));
    }

    protected override bool EntityMatch(in UEntityHandle entity)
    {
        return true;
    }

    private PackedScene _scene;

    protected override bool OnSetPresentation(in UEntitySafe entity, out JobRequest job)
    {
        _scene ??= (PackedScene) ResourceLoader.LoadThreadedGet(Path);
        job = NewInstantiateJob(entity, _scene);
        
        return true;
    }

    protected override bool OnRemovePresentation(in UEntitySafe entity, in Node node)
    {
        return true;
    }

    protected override void OnPresentationLoop()
    {
        base.OnPresentationLoop();
        
        var posAccessor = GameWorld.AccessSparseSet(PositionComponent.Type.GetOrCreate(GameWorld));
        foreach (var entity in QueryWithPresentation)
        {
            if (!TryGetNode(entity, out var node))
                continue;

            var pos = posAccessor[entity].Value;
            ((Node3D) node).Position = new Vector3(pos.X, pos.Y, 0);
        }
    }
}