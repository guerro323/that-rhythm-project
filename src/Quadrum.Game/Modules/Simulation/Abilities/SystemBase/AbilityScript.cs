using System;
using System.Runtime.CompilerServices;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Abilities.Boards;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Abilities.Systems;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Utilities;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revecs.Querying;
using revecs.Systems;
using revghost;
using revghost.Threading;

namespace Quadrum.Game.Modules.Simulation.Abilities.SystemBase;

public abstract class AbilityScript<T> : SimulationSystem
    where T : IRevolutionComponent
{
    protected virtual bool MultiThreadSetup => true;
    
    private AbilitySetup _setupDelegate;
    private AbilityExecute _executeDelegate;

    public ConcurrentScheduler PostScheduler;

    protected AbilityScript(Scope scope) : base(scope)
    {
        _setupDelegate = OnSetup;
        _executeDelegate = OnExecute;

        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            b => b.SetGroup<AbilityExecutionSystemGroup>()
                .Before(typeof(ExecuteAbilitySystem))
        );
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnPostUpdate,
            b => b.SetGroup<AbilityExecutionSystemGroup>()
                .After(typeof(ExecuteAbilitySystem))
        );

        PostScheduler = new ConcurrentScheduler();
    }

    private AbilitiesFunctionBoard _functionBoard;
    private ComponentType _componentType;

    private ComponentType _beforeActivationType, _activeType, _chainingType;
    
    private ArchetypeQuery _abilitySetupQuery;

    protected override void OnInit()
    {
        _componentType = T.ToComponentType(Simulation);

        _functionBoard = AbilitiesFunctionBoard.GetOrCreate(Simulation);
        _functionBoard.SetFunctions(_componentType, _executeDelegate);

        _beforeActivationType = AbilityStateBeforeActivationTag.ToComponentType(Simulation);
        _activeType = AbilityStateActiveTag.ToComponentType(Simulation);
        _chainingType = AbilityStateChainingTag.ToComponentType(Simulation);

        _abilitySetupQuery = new ArchetypeQuery(Simulation, new[]
        {
            AbilityLayout.ToComponentType(Simulation),
            _componentType
        });
    }

    private void OnUpdate(Entity _)
    {
        if (MultiThreadSetup)
        {
            Parallel
            (
                _abilitySetupQuery,
                (in ReadOnlySpan<UEntityHandle> entities, in SystemState<AbilitySetup> state) =>
                {
                    state.Data(entities);
                },
                _setupDelegate
            );
        }
        else
        {
            var board = Simulation.ArchetypeBoard;
            foreach (var archetype in _abilitySetupQuery.GetMatchedArchetypes())
            {
                OnSetup(board.GetEntities(archetype));
            }
        }
    }

    private void OnPostUpdate(Entity _)
    {
        PostScheduler.Run();
    }

    protected override void OnDispose()
    {
        if (_functionBoard != null)
        {
            if (_functionBoard.ExecuteFunction[_componentType.Handle] == _executeDelegate)
                _functionBoard.SetFunctions(_componentType, null);
        }

        base.OnDispose();
    }

    protected abstract void OnSetup(ReadOnlySpan<UEntityHandle> abilities);
    protected abstract void OnExecute(UEntityHandle owner, UEntityHandle self);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool HasBeforeActivationState(UEntityHandle ability)
    {
        return Simulation.HasComponent(ability, _beforeActivationType);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool HasActiveState(UEntityHandle ability)
    {
        return Simulation.HasComponent(ability, _activeType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool HasChainingState(UEntityHandle ability)
    {
        return Simulation.HasComponent(ability, _chainingType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool HasActiveOrChainingState(UEntityHandle ability)
    {
        return HasActiveState(ability) || HasChainingState(ability);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool HasAnyState(UEntityHandle ability)
    {
        return HasBeforeActivationState(ability) || HasActiveOrChainingState(ability);
    }
}