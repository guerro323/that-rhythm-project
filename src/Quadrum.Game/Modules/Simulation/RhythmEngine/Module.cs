using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using revecs.Systems.Generator;
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
            domain.SystemGroup.Add(new Systems.OnRhythmInputSystem());
            domain.SystemGroup.Add(new Systems.GetNextCommandEngineSystem());
            domain.SystemGroup.Add(new Systems.ApplyCommandEngineSystem());
            domain.SystemGroup.Add(new Systems.RhythmEngineExecutionGroup.Begin());
            domain.SystemGroup.Add(new Systems.RhythmEngineExecutionGroup.End());
        });
    }
}