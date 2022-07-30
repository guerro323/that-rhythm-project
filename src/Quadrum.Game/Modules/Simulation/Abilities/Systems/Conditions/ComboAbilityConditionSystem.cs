using System;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Abilities.Components.Conditions;
using Quadrum.Game.Modules.Simulation.Abilities.SystemBase;
using revecs.Core;
using revecs.Utility;
using revghost;

namespace Quadrum.Game.Modules.Simulation.Abilities.Systems.Conditions;

public partial class ComboAbilityConditionSystem : BaseConditionSystem
{
    public ComboAbilityConditionSystem(Scope scope) : base(scope)
    {
    }

    private Commands _cmd;

    protected override void OnInit()
    {
        base.OnInit();
        _cmd = new Commands(Simulation);
    }

    protected override void GetComponentTypes<TList>(TList componentTypes)
    {
        componentTypes.Add(ComboAbilityCondition.ToComponentType(Simulation));
    }

    protected override bool CanExecuteAbility(UEntityHandle ability)
    {
        var owner = _cmd.ReadAbilityOwnerDescriptionRelative(ability);
        
        var active = _cmd.ReadOwnerActiveAbility(owner);
        var condition = _cmd.ReadComboAbilityCondition(ability);

        return IsComboIdentical(
            condition.Span.UnsafeCast<ComboAbilityCondition, ComponentType>(),
            active.Combo.Span
        );
    }
    
    private bool IsComboIdentical(ReadOnlySpan<ComponentType> abilityCombo, ReadOnlySpan<UEntityHandle> unitCombo)
    {
        var start = unitCombo.Length - abilityCombo.Length;
        var end   = unitCombo.Length;

        if (end - start < abilityCombo.Length || start < 0)
            return false;

        for (var i = start; i != end; i++)
        {
            if (!Simulation.HasComponent(unitCombo[i], abilityCombo[i - start]))
                return false;
        }

        return true;
    }

    private partial record struct Commands :
        ComboAbilityCondition.Cmd.IRead,
        AbilityOwnerDescription.Relative.Cmd.IRead,
        OwnerActiveAbility.Cmd.IRead;
}