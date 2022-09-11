using Quadrum.Game.Utilities;
using revghost;
using revghost.Ecs;

namespace Quadrum.Game.Modules.Simulation.Abilities;

// Update in AbilitySystemGroup
/// <summary>
/// Represent a group of ability systems that will update their condition
/// </summary>
public partial class AbilityConditionSystemGroup : BaseSystemGroup<AbilityConditionSystemGroup>
{
    protected override void SetBegin(OrderBuilder builder)
    {
        base.SetBegin(builder);
        builder.SetGroup<AbilitySystemGroup>();
    }

    protected override void SetEnd(OrderBuilder builder)
    {
        base.SetEnd(builder);
        builder.SetGroup<AbilitySystemGroup>();
    }
    
    public AbilityConditionSystemGroup(Scope scope) : base(scope)
    {
        
    }
}