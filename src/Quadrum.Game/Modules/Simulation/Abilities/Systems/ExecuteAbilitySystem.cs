using System;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Abilities.Boards;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Utilities;
using revecs;
using revecs.Core;
using revecs.Querying;
using revecs.Systems;
using revghost;
using revghost.Shared.Threading.Schedulers;
using revghost.Threading;

namespace Quadrum.Game.Modules.Simulation.Abilities.Systems;

public class ExecuteAbilitySystem : SimulationSystem
{
    public readonly IScheduler Post;

    public ExecuteAbilitySystem(Scope scope) : base(scope)
    {
        Post = new ConcurrentScheduler();
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            b => b.SetGroup<AbilityExecutionSystemGroup>()
        );
    }
    
    private ArchetypeQuery _executorQuery;
    private AbilitiesFunctionBoard _functionBoard;

    protected override void OnInit()
    {
        OwnerActiveAbility.Type.GetOrCreate(Simulation);
        AbilityType.Type.GetOrCreate(Simulation);
        
        _executorQuery = new ArchetypeQuery(Simulation, new[]
        {
            OwnerActiveAbility.ToComponentType(Simulation)
        });
        _functionBoard = AbilitiesFunctionBoard.GetOrCreate(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        Parallel
        (
            _executorQuery,
            static (in ReadOnlySpan<UEntityHandle> entities, in SystemState<AbilitiesFunctionBoard> state) =>
            {
                var world = state.World;
                var board = state.Data;

                var ownerAbilityAccessor = world.AccessSparseSet(OwnerActiveAbility.Type.GetOrCreate(world));
                var abilityTypeAccessor = world.AccessSparseSet(AbilityType.Type.GetOrCreate(world));
                foreach (var entity in entities)
                {
                    ref readonly var ownerActiveAbility = ref ownerAbilityAccessor[entity];

                    // Don't execute duplicate abilities
                    if (ownerActiveAbility.PreviousActive.Id != ownerActiveAbility.Active.Id)
                    {
                        TryInvoke(entity, ownerActiveAbility.PreviousActive, in abilityTypeAccessor);
                        TryInvoke(entity, ownerActiveAbility.Active, in abilityTypeAccessor);
                        if (ownerActiveAbility.PreviousActive.Id != ownerActiveAbility.Incoming.Id
                            && ownerActiveAbility.Active.Id != ownerActiveAbility.Incoming.Id)
                            TryInvoke(entity, ownerActiveAbility.Incoming, in abilityTypeAccessor);
                    }
                    else
                    {
                        TryInvoke(entity, ownerActiveAbility.Active, in abilityTypeAccessor);
                        if (ownerActiveAbility.Active.Id != ownerActiveAbility.Incoming.Id)
                            TryInvoke(entity, ownerActiveAbility.Incoming, in abilityTypeAccessor);
                    }
                }

                void TryInvoke(UEntityHandle owner, UEntityHandle ability, in SparseSetAccessor<AbilityType> accessor)
                {
                    if (!accessor.Contains(ability))
                        return;
                    
                    var typeHandle = accessor[ability].Target.Handle;
                    board.ExecuteFunction[typeHandle]?.Invoke(owner, ability);
                }
            },
            _functionBoard
        );
    }
}