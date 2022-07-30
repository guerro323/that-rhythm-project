using System;
using DefaultEcs;
using revecs.Core;
using revecs.Systems;
using revghost;
using revtask.Core;
using revtask.OpportunistJobRunner;

namespace Quadrum.Game.Modules.Simulation.Application;

public class SimulationScope : Scope
{
    public readonly World World;
    public readonly IJobRunner JobRunner;
    public readonly RevolutionWorld GameWorld;

    public SimulationScope(Scope parent) : base(new ChildScopeContext(parent.Context))
    {
        Context.Register(World = new World());
        Context.Register(JobRunner = new OpportunistJobRunner(1f));
        Context.Register(GameWorld = new RevolutionWorld());
    }

    public override void Dispose()
    {
        base.Dispose();

        World.Dispose();
        (JobRunner as IDisposable)!.Dispose();
        GameWorld.Dispose();
    }
}