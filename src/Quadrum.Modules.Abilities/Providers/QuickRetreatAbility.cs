using PataNext.Game.Modules.Abilities.SystemBase;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs.Extensions.Generator.Components;
using revghost;

namespace Quadrum.Modules.Abilities.Providers;

public partial struct QuickRetreatAbility : ISparseComponent
{
    public const float StopTime      = 0.3f;
    public const float MaxActiveTime = StopTime + 0.5f;

    public int LastActiveId;

    public float AccelerationFactor;
    public float StartPosition;
    public float BackVelocity;
    public bool  IsRetreating;
    public float ActiveTime;
    
    public class Provider : BaseRhythmAbilityProvider<QuickRetreatAbility>
    {
        public Provider(Scope scope) : base(scope)
        {
        }

        protected override void GetComboCommands<TList>(TList componentTypes)
        {
        }

        protected override RhythmCommandAction[] CommandActions { get; } =
        {
            RhythmCommandAction.WithSlider(0, 2, (int) DefaultCommandKeys.Right),
            RhythmCommandAction.With(1, (int) DefaultCommandKeys.Left),
        };

        protected override int CommandDuration => 3;
    }
}