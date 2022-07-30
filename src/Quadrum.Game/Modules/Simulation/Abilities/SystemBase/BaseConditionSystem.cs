using System;
using System.Threading;
using Collections.Pooled;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Utilities;
using revecs.Core;
using revecs.Querying;
using revecs.Systems;
using revghost;

namespace Quadrum.Game.Modules.Simulation.Abilities.SystemBase;

public abstract partial class BaseConditionSystem : SimulationSystem
{
    protected BaseConditionSystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            b => b.SetGroup<AbilityConditionSystemGroup>()
        );
    }

    private ArchetypeQuery _query;
    private Commands _cmd;

    protected override void OnInit()
    {
        using var list = new PooledList<ComponentType>();
        list.Add(AbilityLayout.ToComponentType(Simulation));
        GetComponentTypes(list);

        _query = new ArchetypeQuery(Simulation, list.Span);
        _cmd = new Commands(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        Parallel(_query, (in ReadOnlySpan<UEntityHandle> entities, in SystemState<Commands> state) =>
        {
            foreach (var entity in entities)
            {
                var flag = CanExecuteAbility(entity);
                if (!flag)
                {
                    _cmd.AddAbilityDiscardFromSelectionTag(entity);
                }
                else
                {
                    _cmd.RemoveAbilityDiscardFromSelectionTag(entity);
                }
            }
        }, _cmd);
    }

    protected abstract void GetComponentTypes<TList>(TList componentTypes) where TList : IList<ComponentType>;
    protected abstract bool CanExecuteAbility(UEntityHandle ability);

    private partial record struct Commands : AbilityDiscardFromSelectionTag.Cmd.IAdmin;
}