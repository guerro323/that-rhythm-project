using Quadrum.Game.Modules.Simulation.Abilities.SystemBase;
using Quadrum.Game.Modules.Simulation.Application;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revghost;

namespace Quadrum.Modules.Abilities;

public abstract class AttackAbilityScriptBase<T> : AbilityScriptBase<T> where T : IRevolutionComponent
{
    public AttackAbilityScriptBase(Scope scope) : base(scope)
    {
    }

    protected override void OnExecute(UEntityHandle owner, UEntityHandle self)
    {
        OnExecute(owner, self, ref Simulation.GetAttackAbilityState(self), in Simulation.GetAttackAbilitySettings(self));
    }

    protected abstract void OnExecute(UEntityHandle owner, UEntityHandle self, ref AttackAbilityState attackState,
        in AttackAbilitySettings attackSettings);

    public void StopAttack(ref AttackAbilityState state)
    {
        state.AttackStart = default;
        state.DidAttack = false;
    }

    public bool TriggerAttack(ref AttackAbilityState state, in GameTime worldTime)
    {
        if (state.AttackStart == TimeSpan.Zero && state.Cooldown <= TimeSpan.Zero)
        {
            state.AttackStart = worldTime.Total;
            state.DidAttack = false;
            return true;
        }

        return false;
    }

    public bool CanAttackThisFrame(ref AttackAbilityState state, in AttackAbilitySettings settings,
        in TimeSpan currentTime, TimeSpan cooldown)
    {
        if (currentTime > state.AttackStart.Add(settings.DelayBeforeAttack) && !state.DidAttack)
        {
            state.Cooldown = cooldown;
            state.DidAttack = true;
            return true;
        }

        return false;
    }

    public bool IsAttackingAndUpdate(ref AttackAbilityState state, in AttackAbilitySettings settings,
        in TimeSpan currentTime)
    {
        if (state.AttackStart != default)
        {
            if (state.AttackStart.Add(settings.DelayBeforeAttack + settings.PauseAfterAttack) <= currentTime)
            {
                state.AttackStart = default;
            }

            return true;
        }

        return false;
    }
}