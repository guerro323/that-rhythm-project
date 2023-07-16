using PataNext.Game.Modules.Abilities.SystemBase;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs.Extensions.Generator.Components;
using revghost;

namespace Quadrum.Modules.Abilities.Providers;

public partial struct DefaultJumpAbility : ISparseComponent
{
    public int LastActiveId;

    public bool  IsJumping;
    public float ActiveTime;
    
    public class Provider : BaseRhythmAbilityProvider<DefaultJumpAbility>
    {
        public Provider(Scope scope) : base(scope)
        {
        }

        protected override void GetComboCommands<TList>(TList componentTypes)
        {
        }

        protected override RhythmCommandAction[] CommandActions { get; } =
        {
            RhythmCommandAction.With(0, (int) DefaultCommandKeys.Down),
            RhythmCommandAction.With(1, (int) DefaultCommandKeys.Down),
            RhythmCommandAction.With(2, (int) DefaultCommandKeys.Up),
            RhythmCommandAction.With(3, (int) DefaultCommandKeys.Up),
        };
    }
}