using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Interaction.Health.EventLoops;
using Quadrum.Game.Modules.Simulation.Interaction.Health.Implementations;
using Quadrum.Game.Modules.Simulation.Interaction.Health.Systems;
using revghost;
using revghost.Module;

namespace Quadrum.Game.Modules.Simulation.Interaction.Health;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        TrackDomain((SimulationDomain domain) =>
        {
            domain.Scope.Context.Register<IHealthEventLoopSubscriber>(new HealthEventLoop(domain.World));

            _ = new UpdateAllHealthSystem(domain.Scope);
            // Implementations
            {
                _ = new SimpleHealthImplSystem(domain.Scope);
            }
        });
    }
}