using PataNext.Game.Modules.Abilities;
using Quadrum.Game.Modules.Simulation.Application;
using revghost;
using revghost.Module;

namespace Quadrum.Game.Super.Modules.Abilities;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        TrackDomain((SimulationDomain domain) =>
        {
            domain.Scope.Context.Register(new AbilitySpawner(domain.Scope));
        });
    }
}