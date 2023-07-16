using PataNext.Game.Modules.Abilities.SystemBase;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Interaction.Damage;
using Quadrum.Game.Modules.Simulation.Interaction.HitBoxes;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using Quadrum.Modules.Abilities;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revghost;

namespace QuadrumPrototype.Code.Abilities;

public partial struct DefaultAttackAbility : ISparseComponent
{
    public class Provider : BaseRhythmAbilityProvider<DefaultAttackAbility>
    {
        public Provider(Scope scope) : base(scope)
        {
        }

        protected override void GetComboCommands<TList>(TList componentTypes)
        {
        }

        protected override RhythmCommandAction[] CommandActions { get; } = {
            RhythmCommandAction.With(0, (int) DefaultCommandKeys.Right),
            RhythmCommandAction.With(1, (int) DefaultCommandKeys.Right),
            RhythmCommandAction.With(2, (int) DefaultCommandKeys.Left),
            RhythmCommandAction.With(3, (int) DefaultCommandKeys.Right),
        };

        public override void SetEntityData(ref UEntityHandle handle, CreateAbility data)
        {
            base.SetEntityData(ref handle, data);
            
            Simulation.AddAttackAbilitySettings(handle, new AttackAbilitySettings
            {
                DelayBeforeAttack = TimeSpan.FromSeconds(0.1f),
                PauseAfterAttack = TimeSpan.FromSeconds(0.1f)
            });
            Simulation.AddAttackAbilityState(handle);
            Simulation.AddPositionComponent(handle);
            Simulation.AddDamageFrameData(handle);
            Simulation.AddHitBoxAgainstTeam(handle);
            Simulation.AddHitBoxHistory(handle, ReadOnlySpan<HitBoxHistory>.Empty);
        }
    }
}