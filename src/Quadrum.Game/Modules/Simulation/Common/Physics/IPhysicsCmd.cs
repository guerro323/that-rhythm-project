using DefaultEcs;
using revecs.Core;
using revecs.Extensions.Generator;

namespace Quadrum.Game.Modules.Simulation.Common.Physics;

public interface IPhysicsCmdRead : IRevolutionCommand
{
    public const string Variables = @"        public readonly ComponentType<global::Quadrum.Game.Modules.Simulation.Common.Physics.PhysicsCollider> PhysicsColliderType;
        private readonly global::Quadrum.Game.Modules.Simulation.Common.Physics.IPhysicsEngine PhysicsEngine;
        private readonly SwapDependency PhysicsColliderDependency;
        private readonly int PhysicsColliderDependency_WriteCount;";
    
    public const string Init = @"
            PhysicsColliderType = global::Quadrum.Game.Modules.Simulation.Common.Physics.PhysicsCollider.Type.GetOrCreate(World);
            PhysicsEngine = global::Quadrum.Game.Modules.Simulation.Common.Physics.RevolutionWorldExtension.GetPhysicsEngine(World);
            PhysicsColliderDependency = World.GetComponentDependency(PhysicsColliderType);
            PhysicsColliderDependency_WriteCount = 0;

            if (PhysicsEngine == null)
                throw new InvalidOperationException(""No IPhysicsEngine present."");
";
    
    public const string Body = @"
        public bool Overlaps<T1, T2>(in T1 a, in T2 b)
        {
            return PhysicsEngine.OnOverlaps(ref Unsafe.AsRef(in a), ref Unsafe.AsRef(in b));
        }

        public global::Quadrum.Game.Modules.Simulation.Common.Physics.DistanceOutput Distance<TOrigin, TAgainst>
                (global::Quadrum.Game.Modules.Simulation.Common.Physics.DistanceInput input, in TOrigin origin, in TAgainst against)
        {
            var handler = default(global::Quadrum.Game.Modules.Simulation.Common.Physics.NearestDistanceHandler);
            PhysicsEngine.OnDistance(input, ref handler, ref Unsafe.AsRef(in origin), ref Unsafe.AsRef(in against));
            return handler.Hit;
        }

        public bool Distance<THandler, TOrigin, TAgainst>
                (global::Quadrum.Game.Modules.Simulation.Common.Physics.DistanceInput input, ref THandler handler, in TOrigin origin, in TAgainst against)
            where THandler : global::Quadrum.Game.Modules.Simulation.Common.Physics.IDistanceHandler
        {
            return PhysicsEngine.OnDistance(input, ref handler, ref Unsafe.AsRef(in origin), ref Unsafe.AsRef(in against));
        }
";
    
    public const string Dependencies = 
        "PhysicsColliderDependency_WriteCount > 0 ? PhysicsColliderDependency.TrySwap(runner, request) : PhysicsColliderDependency.IsCompleted(runner, request)";

    bool Overlaps<T1, T2>(in T1 a, in T2 b);
    bool Distance<THandler, TOrigin, TAgainst>(DistanceInput input, ref THandler handler, in TOrigin origin, in TAgainst against)
        where THandler : IDistanceHandler;
    DistanceOutput Distance<TOrigin, TAgainst>(DistanceInput input, in TOrigin origin, in TAgainst against);
}

public interface IPhysicsCmdAdmin : IPhysicsCmdRead
{
    public const string Init = @"
            PhysicsColliderDependency_WriteCount += 1;
";

    public const bool WriteAccess = true;
    
    public const string Body = @"
        public void AssignCollider(in UEntityHandle handle, in global::DefaultEcs.Entity settings)
        {
            PhysicsEngine.AssignCollider(handle, settings);
        }
";

    void AssignCollider(in UEntityHandle handle, in Entity settings);
}