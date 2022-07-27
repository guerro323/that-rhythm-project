using System;
using Quadrum.Game.Modules.Simulation.Abilities.Systems;
using Quadrum.Game.Modules.Simulation.Application;
using revecs.Systems.Generator;
using revghost;
using revghost.Module;
using revghost.Utility;

namespace Quadrum.Game.Modules.Simulation.Abilities;

public class Module : HostModule
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
                new AbilitySystemGroup(domain.Scope),
                new AbilityExecutionSystemGroup(domain.Scope),
                new AbilityConditionSystemGroup(domain.Scope),

                new UpdateActiveAbilitySystem(domain.Scope),
            });
        });
    }
}