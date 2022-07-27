using Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;
using Quadrum.Game.Utilities;
using revecs.Systems.Generator;
using revghost;
using revghost.Ecs;

namespace Quadrum.Game.Modules.Simulation.Abilities;

public partial class AbilitySystemGroup : BaseSystemGroup<AbilitySystemGroup>
{
    public AbilitySystemGroup(Scope scope) : base(scope)
    {
    }

    protected override void SetBegin(OrderBuilder builder)
    {
        base.SetBegin(builder);
        builder.AfterGroup<RhythmEngineExecutionGroup>();
    }
}