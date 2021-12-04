using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using revghost;
using revghost.Module;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine;

public partial class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        TrackDomain((SimulationDomain domain) =>
        {
            domain.SystemGroup.Add(new Systems.ApplyTagsSystem());
            domain.SystemGroup.Add(new Systems.ResetStateOnStoppedSystem());
            domain.SystemGroup.Add(new Systems.ProcessSystem());
            domain.SystemGroup.Add(new Systems.ResizeCommandBufferSystem());
            domain.SystemGroup.Add(new Systems.GetNextCommandEngineSystem());
            domain.SystemGroup.Add(new Systems.ApplyCommandEngineSystem());
            domain.SystemGroup.Add(new Systems.RhythmEngineExecutionGroup.Begin());
            domain.SystemGroup.Add(new Systems.RhythmEngineExecutionGroup.End());

            var entity = domain.GameWorld.CreateEntity();
            domain.GameWorld.AddComponent(entity, RhythmEngineLayout.Type.GetOrCreate(domain.GameWorld), default);

            ref var controller = ref domain.GameWorld.GetComponentData(
                entity,
                RhythmEngineController.Type.GetOrCreate(domain.GameWorld)
            );
            
            controller = controller with
            {
                State = RhythmEngineController.EState.Playing,
                StartTime = domain.WorldTime.Total.Add(TimeSpan.FromSeconds(2))
            };

            ref var settings = ref domain.GameWorld.GetComponentData(
                entity,
                RhythmEngineSettings.Type.GetOrCreate(domain.GameWorld)
            );

            settings = settings with
            {
                MaxBeats = 4,
                BeatInterval = TimeSpan.FromMilliseconds(500)
            };
        });
    }
}