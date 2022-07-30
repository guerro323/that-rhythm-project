using System;
using System.Runtime.CompilerServices;
using Collections.Pooled;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using revecs.Core;
using revecs.Utility;
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

    public abstract UEntityHandle SpawnEntity(Entity data);
}

public abstract class BaseProvider<TCreateData> : BaseProvider
    where TCreateData : struct
{
    protected BaseProvider(Scope scope) : base(scope)
    {
    }

    public override UEntityHandle SpawnEntity(Entity data)
    {
        return SpawnEntity(data.Get<TCreateData>());
    }

    public abstract UEntityHandle SpawnEntity(TCreateData data);
}