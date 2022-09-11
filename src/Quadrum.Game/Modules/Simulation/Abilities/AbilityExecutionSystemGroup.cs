using Quadrum.Game.Utilities;
using revghost;
using revghost.Ecs;

namespace Quadrum.Game.Modules.Simulation.Abilities;

// Update in AbilitySystemGroup
public partial class AbilityExecutionSystemGroup : BaseSystemGroup<AbilityExecutionSystemGroup>
{
    public AbilityExecutionSystemGroup(Scope scope) : base(scope)
    {
    }

    protected override void SetBegin(OrderBuilder builder)
    {
        base.SetBegin(builder);
        builder.SetGroup<AbilitySystemGroup>();
        builder.AfterGroup<AbilityConditionSystemGroup>();
    }

    protected override void SetEnd(OrderBuilder builder)
    {
        base.SetEnd(builder);
        builder.SetGroup<AbilitySystemGroup>();
    }
}