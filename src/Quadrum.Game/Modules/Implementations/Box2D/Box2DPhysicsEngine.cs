using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Box2D.NetStandard.Collision;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Common;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.Fixtures;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Common.Physics;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revghost;
using revghost.Ecs;
using revghost.Injection.Dependencies;
using revghost.Utility;
using DistanceInput = Quadrum.Game.Modules.Simulation.Common.Physics.DistanceInput;
using DistanceOutput = Quadrum.Game.Modules.Simulation.Common.Physics.DistanceOutput;

namespace Quadrum.Game.Modules.Implementations.ImplBox2D;

public partial class Box2DPhysicsEngine : AppSystem, IPhysicsEngine
{
    private static readonly HostLogger Logger = new(nameof(Box2DPhysicsEngine));

    private readonly ThreadLocal<WorldManifold> cachedWorldManifold = new(() => new WorldManifold());

    private ComponentType<UEntityHandle> _colliderRootType;
    private ComponentType<PhysicsCollider> _colliderType;
    private ComponentType<Box2DColliderMeta> _metadataType;

    private RevolutionWorld _world;

    public Box2DPhysicsEngine(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _world);

        Disposables.Add(cachedWorldManifold);
    }

    public void AssignCollider(UEntityHandle entity, Entity settings)
    {
        Shape[] shapes;
        if (settings.TryGet(out shapes))
        {
            ;
        }
        else if (settings.TryGet(out Shape shape))
        {
            shapes = new Shape[1] {shape};
        }
        else
        {
            Logger.Error($"settings '{entity}' for '{entity}' had no {nameof(Shape)} component.");
            return;
        }

        _world.AddComponent(entity, _colliderType, default);
        _world.AddComponent(entity, _metadataType, new Box2DColliderMeta
        {
            Shapes = shapes
        });
    }

    public bool OnOverlaps<T1, T2>(ref T1 a, ref T2 b)
    {
        var handler = new NearestDistanceHandler();
        return OnDistance(
            new DistanceInput(
                Vector2.Zero,
                0,
                new DistanceInput.EntityOverrides {EarlyHitReturn = true},
                new DistanceInput.EntityOverrides {EarlyHitReturn = true}),
            ref handler,
            ref a,
            ref b
        );
    }

    public bool OnDistance<THandler, TOrigin, TAgainst>(DistanceInput input, ref THandler handler, ref TOrigin origin,
        ref TAgainst against) where THandler : IDistanceHandler
    {
        ref var entityA = ref AsEntity(ref origin);
        ref var entityB = ref AsEntity(ref against);

        // TODO: Entity Collection

        // ========================================== //
        //  Real work
        // ========================================== //
        var (posA, rotA) = input.GetOriginValues(ref origin, _world);
        var (posB, rotB) = input.GetAgainstValues(ref against, _world);
        if (!handler.IsValid(this, ref origin, ref against, input, posB, rotB))
            return false;

        var shapesA = GetShapes(ref origin);
        var shapesB = GetShapes(ref against);

        var didHit = false;
        foreach (var shapeA in shapesA)
        {
            var fixtureA = new Fixture();
            fixtureA.Create(shapeA);

            var transformA = new Transform();
            transformA.Set(posA, rotA);

            foreach (var shapeB in shapesB)
            {
                var fixtureB = new Fixture();
                fixtureB.Create(shapeB);

                var transformB = new Transform();
                transformB.Set(posB, rotB);

                var contact = Contact.Create(fixtureA, 0, fixtureB, 1);
                Debug.Assert(contact != null, "contact != null");

                contact.Evaluate(out var manifold, transformA, transformB);

                var worldManifold = cachedWorldManifold.Value!;
                worldManifold.normal = default;
                worldManifold.points.AsSpan().Clear();
                worldManifold.separations.AsSpan().Clear();
                worldManifold.Initialize(manifold, transformA, shapeA.m_radius, transformB, shapeB.m_radius);

                // don't modify the result if either no points were touched, or the normal is zero.
                if (manifold.pointCount == 0 || worldManifold.normal == Vector2.Zero)
                    continue;

                DistanceOutput result;
                result.Distance = new ArraySegment<float>(worldManifold.separations).Average();
                result.Normal = worldManifold.normal;
                result.Position = worldManifold.points[0];

                for (var i = 1; i < manifold.pointCount; i++)
                    result.Position = Vector2.Lerp(result.Position, worldManifold.points[i], 0.5f);

                handler.AddHit(this, ref against, result, posB, rotB);
                didHit = true;

                if (input.AgainstOverrides.EarlyHitReturn)
                    return true;
            }

            if (didHit && input.OriginOverrides.EarlyHitReturn)
                return true;
        }

        return didHit;
    }

    protected override void OnInit()
    {
        _colliderType = PhysicsCollider.Type.GetOrCreate(_world);
        _colliderRootType = ColliderRootDescription.Type.GetOrCreate(_world);
        _metadataType = Box2DColliderMeta.Type.GetOrCreate(_world);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsEntity<T>()
    {
        return typeof(T) == typeof(UEntityHandle) || typeof(T) == typeof(UEntitySafe);
    }

    private static ref UEntityHandle AsEntity<T>(ref T t)
    {
        if (IsEntity<T>())
            // UEntitySafe can safely be converted to UEntityHandle like that
            // (the row is the first field)
            return ref Unsafe.As<T, UEntityHandle>(ref t);

        return ref Unsafe.NullRef<UEntityHandle>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEntityCollection(UEntityHandle handle)
    {
        return _world.HasComponent(handle, _colliderRootType);
    }

    private Span<Shape> GetShapes<T>(ref T t)
    {
        switch (t)
        {
            case Shape[] shapes:
                return shapes.AsSpan();
            case Shape:
                return MemoryMarshal.CreateSpan(ref Unsafe.As<T, Shape>(ref t), 1);
        }

        if (!IsEntity<T>())
            throw new InvalidOperationException("Invalid Type: " + typeof(T));

        var data = _world.GetComponentData(AsEntity(ref t), _metadataType);
        return data.Shapes.AsSpan();
    }

    public partial struct Box2DColliderMeta : ISparseComponent
    {
        public Shape[] Shapes;
    }
}