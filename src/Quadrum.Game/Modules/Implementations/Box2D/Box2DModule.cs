using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Physics;
using revghost;
using revghost.Module;

namespace Quadrum.Game.Modules.Implementations.ImplBox2D;

public class Box2DModule : HostModule
{
    public Box2DModule(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        TrackDomain((SimulationDomain domain) =>
        {
            var physicsEngine = new Box2DPhysicsEngine(domain.Scope);
            Disposables.Add(physicsEngine);

            domain.GameWorld.SetPhysicsEngine(physicsEngine);
        });
    }
}