using Quadrum.Game.Modules.Simulation.Application;
using revecs.Systems.Generator;
using revghost;
using revghost.Module;

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
            domain.SystemGroup.Add<AbilitySystemGroup.Begin>();
            {
                domain.SystemGroup.Add<AbilityExecutionSystemGroup.Begin>();
                domain.SystemGroup.Add<AbilityExecutionSystemGroup.End>();
            }
            domain.SystemGroup.Add<AbilitySystemGroup.End>();
        });
    }
}