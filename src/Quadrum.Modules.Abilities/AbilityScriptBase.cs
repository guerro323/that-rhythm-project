using System.Numerics;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation;
using Quadrum.Game.Modules.Simulation.Abilities.SystemBase;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Cursors;
using Quadrum.Game.Modules.Simulation.Units;
using Quadrum.Game.Utilities;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revecs.Extensions.RelativeEntity;
using revghost;
using revghost.Injection.Dependencies;

namespace Quadrum.Modules.Abilities;

public enum ENearestOrder
{
    SelfThenRelative,
    RelativeThenSelf,
    SelfOnly,
    RelativeOnly
}

public abstract class AbilityScriptBase<T> : AbilityScript<T> where T : IRevolutionComponent
{
    public (UEntitySafe enemy, float dist, bool isSelf) GetNearestEnemyComplete(UEntityHandle entity,
        float? maxSelfDistance, float? maxRelativeDistance,
        ENearestOrder nearestOrder = ENearestOrder.SelfThenRelative)
    {
        if (!Simulation.Exists(entity))
            return default;

        var defaultSeekingRange = Simulation.GetUnitPlayState(entity).AttackSeekRange;
        maxSelfDistance ??= defaultSeekingRange;
        maxRelativeDistance ??= defaultSeekingRange;

        UEntityHandle relative;
        UnitEnemySeekingState seekingState = default;

        bool tryGetSeekingState(UEntityHandle target)
        {
            if (Simulation.HasUnitEnemySeekingState(target))
            {
                seekingState = Simulation.GetUnitEnemySeekingState(target);
                return true;
            }

            return false;
        }

        var descTypeRelative = new DescriptionType(
            CursorDescription.ToComponentType(Simulation),
            CursorDescription.Relative.ToComponentType(Simulation)
        );

        var result = (enemy: default(UEntitySafe), dist: 0f);
        switch (nearestOrder)
        {
            case ENearestOrder.SelfThenRelative:
                if (tryGetSeekingState(entity))
                {
                    result = (seekingState.Enemy, seekingState.SelfDistance);
                    if (result.dist <= maxSelfDistance)
                        return (result.enemy, result.dist, true);

                    result = (seekingState.Enemy, seekingState.RelativeDistance);
                    if (result.dist <= maxRelativeDistance)
                        return (result.enemy, result.dist, false);
                }

                if (Simulation.TryGetRelative(descTypeRelative, entity, out relative)
                    && tryGetSeekingState(relative))
                {
                    result = (seekingState.Enemy, seekingState.SelfDistance);
                    if (result.dist <= maxRelativeDistance)
                        return (result.enemy, result.dist, false);
                }

                return default;
            case ENearestOrder.RelativeThenSelf:
                if (tryGetSeekingState(entity))
                {
                    result = (seekingState.Enemy, seekingState.RelativeDistance);
                    if (result.dist <= maxRelativeDistance)
                        return (result.enemy, result.dist, false);

                    result = (seekingState.Enemy, seekingState.SelfDistance);
                    if (result.dist <= maxSelfDistance)
                        return (result.enemy, result.dist, true);
                }

                if (Simulation.TryGetRelative(descTypeRelative, entity, out relative)
                    && tryGetSeekingState(relative))
                {
                    result = (seekingState.Enemy, seekingState.SelfDistance);
                    if (result.dist <= maxRelativeDistance)
                        return (result.enemy, result.dist, false);
                }

                return default;
            case ENearestOrder.SelfOnly:
                if (tryGetSeekingState(entity))
                    return (seekingState.Enemy, seekingState.SelfDistance, false);

                break;
            case ENearestOrder.RelativeOnly:
                if (Simulation.TryGetRelative(descTypeRelative, entity, out relative)
                    && tryGetSeekingState(entity))
                    return (seekingState.Enemy, seekingState.RelativeDistance, false);

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(nearestOrder), nearestOrder, null);
        }

        return default;
    }

    public (UEntitySafe enemy, float dist) GetNearestEnemy(UEntityHandle entity,
        float? maxSelfDistance, float? maxRelativeDistance,
        ENearestOrder nearestOrder = ENearestOrder.SelfThenRelative)
    {
        var (enemy, dist, _) = GetNearestEnemyComplete(entity,
            maxSelfDistance, maxRelativeDistance,
            nearestOrder);
        return (enemy, dist);
    }

    public RoutineGetNearOfEnemyResult RoutineGetNearOfEnemy(UEntitySafe entity,
        Vector2 deltaPosition,
        float? attackNearDistance = null,
        float? attackFarDistance = null,
        float? selfDistance = null, float? targetDistance = null,
        ENearestOrder nearestOrder = ENearestOrder.SelfThenRelative,
        bool addEnemyWeakPoint = true)
    {
        var hasRelative = Simulation.HasCursorDescription(entity.Handle);
        // If the unit doesn't have a playstate (can happen if it's fixed to a position)
        // Then modify the distance based on the SeekRange and force nearestOrder to be SelfOnly
        if (!hasRelative && Simulation.HasUnitPlayState(entity.Handle))
        {
            ref readonly var playState = ref Simulation.GetUnitPlayState(entity.Handle);
            
            targetDistance = playState.AttackSeekRange;
            selfDistance = playState.AttackSeekRange;

            nearestOrder = ENearestOrder.SelfOnly;
        }

        var (enemy, enemyDist, isFromSelf) =
            GetNearestEnemyComplete(entity.Handle, selfDistance, targetDistance, nearestOrder);
        if (enemy.Handle.Id != default)
        {
            RoutineGetNearOfEnemyResult result;

            var position = Simulation.GetPositionComponent(entity.Handle).Value;
            var targetPosition = Simulation.GetPositionComponent(enemy.Handle).Value;

            result.EnemyPosition = targetPosition;
            result.Enemy = enemy;

            // Search for any weakpoint the enemy has, and if it does, add it to the deltaPosition var
            if (addEnemyWeakPoint && Simulation.HasUnitWeakPoint(enemy.Handle))
            {
                var weakPoints = Simulation.GetUnitWeakPoint(enemy.Handle);
                if (weakPoints.GetNearest(targetPosition - position) is var (weakPoint, dist) && dist >= 0)
                    deltaPosition += weakPoint;
            }

            targetPosition.X -= deltaPosition.X;

            result.Target = targetPosition;
            result.Distance = enemyDist;

            // If we're near enough of where we should throw the spear, throw it.
            attackNearDistance ??= selfDistance ?? 2;
            attackFarDistance ??= selfDistance ?? 2;

            // (T = target, Y = Your unit, E = Enemy, . = 1m)
            //
            //	Near=2 Far=2
            //
            //  . . . . . . .
            //    T     Y E
            //    2 |   5 6
            //
            //  [Will not work since we're way too near of the enemy]
            //

            var finalDist = targetPosition.X - position.X;
            if (Simulation.GetUnitDirection(entity.Handle).Value > 0)
                finalDist = -finalDist;

            result.CanTriggerAttack =
                attackNearDistance >= finalDist && finalDist >= -attackFarDistance; // the last minus is important

            return result;
        }

        return default;
    }

    public struct RoutineGetNearOfEnemyResult
    {
        public bool CanTriggerAttack;
        public float Distance;
        public Vector2 Target;
        public UEntitySafe Enemy;
        public Vector2 EnemyPosition;
    }

    private World _world;
    public World World => _world;
    
    public GameTime GameTime { get; private set; }

    protected AbilityScriptBase(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _world);
    }

    private GameTimeQuery _gameTimeQuery;

    protected override void OnInit()
    {
        base.OnInit();

        _gameTimeQuery = new GameTimeQuery(Simulation);
    }

    protected override void OnSetup(ReadOnlySpan<UEntityHandle> abilities)
    {
        GameTime = _gameTimeQuery.First().GameTime;
    }
}