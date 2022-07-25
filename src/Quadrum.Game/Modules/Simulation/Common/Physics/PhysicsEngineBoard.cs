using revecs.Core;
using revecs.Core.Boards;

namespace Quadrum.Game.Modules.Simulation.Common.Physics;

public class PhysicsEngineBoard : BoardBase
{
    public const string BoardName = "PhysicsEngine";
    
    public IPhysicsEngine? Current { get; set; }

    public PhysicsEngineBoard(RevolutionWorld world) : base(world)
    {
    }

    public override void Dispose()
    {
        
    }
}