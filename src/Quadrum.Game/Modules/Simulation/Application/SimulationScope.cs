using System;
using DefaultEcs;
using revecs.Core;
using revghost;
using revtask.Core;
using revtask.ExecutiveJobRunner;
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
        Context.Register(JobRunner = new CustomExecutiveJobRunner());
        Context.Register(GameWorld = new RevolutionWorld());
    }

    public override void Dispose()
    {
        base.Dispose();

        World.Dispose();
        (JobRunner as IDisposable)!.Dispose();
        GameWorld.Dispose();
    }
    
    public class CustomExecutiveJobRunner : IJobRunner
    {
        public bool IsCancelled() => false;

        public bool IsWarmed() => true;

        public bool IsCompleted(JobRequest request) => true;

        public JobRequest Queue<T>(T batch) where T : IJob
        {
            var max = batch.SetupJob(new JobSetupInfo(1));
            if (max > 1)
                throw new InvalidOperationException();

            if (max == 0)
                return default;
            
            batch.Execute(this, new JobExecuteInfo {TaskCount = 1});
            return new JobRequest();
        }

        public void TryDivergeRequest(JobRequest request)
        {
        }

        public void TryDiverge()
        {
        }
    }
}