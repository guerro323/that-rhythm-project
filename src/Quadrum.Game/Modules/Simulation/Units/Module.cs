using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Units.Systems;
using revghost;
using revghost.Module;

namespace Quadrum.Game.Modules.Simulation.Units;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        TrackDomain((SimulationDomain domain) =>
        {
            _ = new UnitPhysicsSystem(domain.Scope);
        });
    }
}