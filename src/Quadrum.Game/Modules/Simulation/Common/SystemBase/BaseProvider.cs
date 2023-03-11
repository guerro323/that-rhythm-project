using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using revecs.Core;
using revghost;

namespace Quadrum.Game.Modules.Simulation.Common.SystemBase;

public abstract class BaseProvider : SimulationSystem
{
    protected BaseProvider(Scope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
    }

    public UEntityHandle SpawnEntity(Entity data)
    {
        var handle = Simulation.CreateEntity();
        SetEntityData(ref handle, data);
        return handle;
    }
    
    public abstract void SetEntityData(ref UEntityHandle handle, Entity data);
}

public abstract class BaseProvider<TCreateData> : BaseProvider
    where TCreateData : struct
{
    protected BaseProvider(Scope scope) : base(scope)
    {
    }

    public abstract void SetEntityData(ref UEntityHandle handle, TCreateData data);

    public override void SetEntityData(ref UEntityHandle handle, Entity data)
    {
        SetEntityData(ref handle, data.Get<TCreateData>());
    }

    public UEntityHandle SpawnEntity(TCreateData data)
    {
        var handle = Simulation.CreateEntity();
        SetEntityData(ref handle, data);
        return handle;
    }
}