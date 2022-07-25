using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using revecs.Core;

namespace Quadrum.Game.Modules.Simulation.Common.Physics;

public interface IPhysicsEngine
{
    void AssignCollider(UEntityHandle entity, Entity settings);

    bool OnOverlaps<T1, T2>(ref T1 a, ref T2 b);
    
    bool OnDistance<THandler, TOrigin, TAgainst>(DistanceInput input, ref THandler handler, ref TOrigin origin, ref TAgainst against)
        where THandler : IDistanceHandler;
}

public struct DistanceInput
{
    public struct EntityOverrides
    {
        public Vector2? Position;
        public float? Rotation;

        public bool EarlyHitReturn;
        
        public (Vector2 pos, float rot) GetOrDefaultCore<T>(ref T entity, RevolutionWorld world)
        {
            if (typeof(T) != typeof(UEntityHandle) && typeof(T) != typeof(UEntitySafe))
                return (Position.GetValueOrDefault(), Rotation.GetValueOrDefault(0f));

            var handle = typeof(T) == typeof(UEntityHandle)
                ? Unsafe.As<T, UEntityHandle>(ref entity)
                : Unsafe.As<T, UEntitySafe>(ref entity).Handle;
            
            var pos = Vector2.Zero;
            var rot = 0f;

            if (Position is null)
            {
                var positionType = PositionComponent.Type.GetOrCreate(world);
                if (world.HasComponent(handle, positionType))
                {
                    pos = world.GetComponentData(handle, positionType).Value;
                }
            }
            else
            {
                pos = Position.Value;
            }

            if (Rotation is null)
            {
                var rotationType = RotationComponent.Type.GetOrCreate(world);
                if (world.HasComponent(handle, rotationType))
                {
                    rot = world.GetComponentData(handle, rotationType).Value;
                }
            }
            else
            {
                rot = Rotation.Value;
            }

            return (pos, rot);
        }
    }

    public DistanceInput(Vector2 velocity,
        EntityOverrides origin = default, EntityOverrides against = default)
        : this(velocity, velocity.Length(), origin, against)
    {

    }

    public DistanceInput(Vector2 directionNormal, float maxDistance,
        EntityOverrides origin = default, EntityOverrides against = default)
    {
        Direction = directionNormal;
        MaxDistance = maxDistance;

        OriginOverrides = origin;
        AgainstOverrides = against;
    }

    public Vector2 Direction;
    public float MaxDistance;

    public EntityOverrides OriginOverrides;
    public EntityOverrides AgainstOverrides;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (Vector2 pos, float rot) GetOriginValues<T>(ref T entity, RevolutionWorld world)
    {
        return OriginOverrides.GetOrDefaultCore(ref entity, world);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (Vector2 pos, float rot) GetAgainstValues<T>(ref T entity, RevolutionWorld world)
    {
        return AgainstOverrides.GetOrDefaultCore(ref entity, world);
    }
}

public struct DistanceOutput
{
    public float Distance;
    public Vector2 Normal;
    public Vector2 Position;
    public bool Collided => Normal != Vector2.Zero && Math.Abs(Distance) > float.Epsilon;
}

public interface IDistanceHandler
{
    bool IsValid<T1, T2>(in IPhysicsEngine engine, ref T1 origin, ref T2 against, in DistanceInput input,
        Vector2 position, float rotation);

    void AddHit<T>(in IPhysicsEngine engine, ref T obj, in DistanceOutput output, Vector2 position,
        float rotation);
}

public struct NearestDistanceHandler : IDistanceHandler
{
    public DistanceOutput Hit;

    public bool IsValid<T1, T2>(in IPhysicsEngine engine, ref T1 origin, ref T2 against, in DistanceInput input,
        Vector2 position,
        float rotation)
    {
        return true;
    }

    public void AddHit<T>(in IPhysicsEngine engine, ref T obj, in DistanceOutput output, Vector2 position,
        float rotation)
    {
        if (Hit.Normal == default || Hit.Distance > output.Distance)
        {
            Hit = output;
        }
    }
}