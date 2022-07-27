using System;
using Quadrum.Game.Modules.Simulation.Application;
using revghost;
using revghost.Module;
using revghost.Utility;

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
            Disposables.AddRange(new IDisposable[]
            {
                new Systems.RhythmEngineExecutionGroup(domain.Scope),
                new Systems.ApplyTagsSystem(domain.Scope),
                new Systems.ProcessSystem(domain.Scope),
                new Systems.ResetStateOnStoppedSystem(domain.Scope),
                new Systems.ResizeCommandBufferSystem(domain.Scope),
                new Systems.OnRhythmInputSystem(domain.Scope),
                new Systems.GetNextCommandEngineSystem(domain.Scope),
                new Systems.ApplyCommandEngineSystem(domain.Scope),
            });
        });
    }
}