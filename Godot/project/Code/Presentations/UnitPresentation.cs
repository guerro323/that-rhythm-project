using Collections.Pooled;
using Godot;
using PataNext.Export.Godot.Presentation;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Interaction.HitBoxes;
using Quadrum.Game.Modules.Simulation.Units;
using revecs;
using revecs.Core;
using revghost;
using revtask.Core;

namespace QuadrumPrototype.Code.Presentations;

public partial class UnitPresentation : PresentationGodotBaseSystem
{
    private const string Path = "res://scenes/{0}.tscn";

    public UnitPresentation(Scope scope) : base(scope)
    {
        //ResourceLoader.LoadThreadedRequest(Path);
    }

    private HitEvent _hitEventQuery;
    
    protected override void GetMatchedComponents(PooledList<ComponentType> all, PooledList<ComponentType> or, PooledList<ComponentType> none)
    {
        _hitEventQuery = new HitEvent(GameWorld);
        
        all.Add(UnitDescription.ToComponentType(GameWorld));
        all.Add(PositionComponent.ToComponentType(GameWorld));
    }

    protected override bool EntityMatch(in UEntityHandle entity)
    {
        return true;
    }


    protected override bool OnSetPresentation(in UEntitySafe entity, out JobRequest job)
    {
        var type = "unit";
        if (GameWorld.HasUnitIdentifier(entity.Handle))
        {
            type = GameWorld.GetUnitIdentifier(entity.Handle).Value;
        }
        
        job = NewInstantiateJob(entity, (PackedScene) ResourceLoader.Load(string.Format(Path, type)));
        
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

            foreach (var ev in _hitEventQuery)
            {
                if (!ev.HitBoxEvent.Victim.Handle.Equals(entity))
                    continue;

                node.Call("on_hit");
            }
        }
    }

    private partial struct HitEvent : IQuery<Read<HitBoxEvent>> {}
}