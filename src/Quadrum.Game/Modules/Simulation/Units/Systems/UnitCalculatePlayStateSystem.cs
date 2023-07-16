using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Utilities;
using revecs;
using revghost;

namespace Quadrum.Game.Modules.Simulation.Units.Systems;

public partial class UnitCalculatePlayStateSystem : SimulationSystem
{
    public UnitCalculatePlayStateSystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate
        );
    }

    private UnitQuery _unitQuery;

    protected override void OnInit()
    {
        _unitQuery = new UnitQuery(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        foreach (var ent in _unitQuery)
        {
            ref var state = ref ent.playState;
            ref readonly var original = ref ent.original;

            var multiplier = 0.0f;
            if (Simulation.HasRhythmEngineDescriptionRelative(ent.Handle))
            {
                var engine = Simulation.GetRhythmEngineDescriptionRelative(ent.Handle);
                var comboState = Simulation.GetGameComboState(engine);
                multiplier = ((int) (comboState.Score * 4)) * 0.25f;
                if (multiplier < 0.99f)
                    multiplier *= 0.5f;
            }

            state.MovementSpeed       = MathUtils.LerpNormalized(original.BaseWalkSpeed, original.FeverWalkSpeed, multiplier);
            state.Defense             = original.Defense;
            state.Attack              = original.Attack;
            state.AttackSpeed         = original.AttackSpeed;
            state.AttackSeekRange     = original.AttackSeekRange;
            state.MovementAttackSpeed = MathUtils.LerpNormalized(original.MovementAttackSpeed, original.MovementAttackSpeed * 1.8f, multiplier);
            state.MovementReturnSpeed = MathUtils.LerpNormalized(original.MovementAttackSpeed, original.MovementAttackSpeed * 1.7f, multiplier);
            state.Weight              = original.Weight;
            state.KnockbackPower      = original.KnockbackPower;

            state.ReceiveDamagePercentage = 1;
        }
    }

    private partial struct UnitQuery : IQuery<(Write<UnitPlayState> playState, Read<UnitStatistics> original)>
    {
        
    }
}