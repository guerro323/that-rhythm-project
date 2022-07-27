using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Utilities;
using revecs;
using revghost;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial class ResetStateOnStoppedSystem : SimulationSystem
{
    public ResetStateOnStoppedSystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            p => p
                .SetGroup<RhythmEngineExecutionGroup>()
                .After(typeof(ApplyTagsSystem))
                .After(typeof(ProcessSystem))
        );
    }

    private EngineQuery _query;

    protected override void OnInit()
    {
        _query = new EngineQuery(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        foreach (var engine in _query)
        {
            engine.GameComboState = default;
            engine.RhythmEngineRecoveryState = default;
        }
    }

    private partial record struct EngineQuery : IQuery<(
        Write<GameComboState>,
        Write<RhythmEngineRecoveryState>,
        None<RhythmEngineIsPlaying>,
        None<RhythmEngineIsPaused>)>;
}