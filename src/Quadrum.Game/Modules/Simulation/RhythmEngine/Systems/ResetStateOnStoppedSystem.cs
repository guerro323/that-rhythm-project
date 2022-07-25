using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Utilities;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial struct ResetStateOnStoppedSystem : IRevolutionSystem
{
    public void Constraints(in SystemObject sys)
    {
        sys.SetGroup<RhythmEngineExecutionGroup>();
        {
            sys.DependOn<ApplyTagsSystem>();
            sys.DependOn<ProcessSystem>();
        }
    }

    public void Body()
    {
        foreach (var engine in RequiredQuery(
                     Write<GameComboState>(),
                     Write<RhythmEngineRecoveryState>(),
                     None<RhythmEngineIsPlaying>(),
                     None<RhythmEngineIsPaused>()))
        {
            engine.GameComboState = default;
            engine.RhythmEngineRecoveryState = default;
        }
    }
}