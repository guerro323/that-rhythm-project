using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Units;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.Abilities.Systems.Conditions;

/*public partial struct LivingOwnerAbilityConditionSystem : IRevolutionSystem,
    AbilityDiscardFromSelectionTag.Cmd.IAdmin
{
    public void Constraints(in SystemObject sys)
    {
        sys.AbilityConditionConstraint();
    }

    public void Body()
    {
        foreach (var ability in RequiredQuery(
                     Read<AbilityOwnerDescription.Relative>("owner"),
                     All<LivingOwnerAbilityCondition>(),
                     // don't include already discarded entities
                     None<AbilityDiscardFromSelectionTag>()
                 ))
        {
            if (false) // if (Cmd.HasLivableIsDead(owner))
            {
                Cmd.AddAbilityDiscardFromSelectionTag(ability.Handle);
            }
        }
    }
}*/