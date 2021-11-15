using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs;
using revecs.Systems;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial struct ProcessSystem : ISystem
{
    // Without prediction system for now

    [RevolutionSystem]
    [DependOn(typeof(RhythmEngineExecutionGroup.Begin)), AddForeignDependency(typeof(RhythmEngineExecutionGroup.End))]
    [DependOn(typeof(ApplyTagsSystem))]
    private static void Method(
        [Singleton] GameTimeSingleton time,
        [Cmd] c<RhythmEngineIsPlaying.Cmd.IAdmin> cmd,
        [Query, revecs.Optional] qEngines<
            Read<RhythmEngineController>,
            Read<RhythmEngineSettings>,
            Write<RhythmEngineState>,
            // Require rhythm engine to be in playing state.
            // The component will get removed if state.Elapsed is less than 0.
            With<RhythmEngineIsPlaying>
        > engines
    )
    {
        foreach (var (entity, controller, settings, state) in engines)
        {
            if (controller.StartTime != state.PreviousStartTime)
            {
                state.PreviousStartTime = controller.StartTime;
                // TODO: add events and later prediction will need to recalculate 'state.Elapsed'

                state.Elapsed = time.Total - controller.StartTime;
            }

            state.Elapsed += time.Delta;

            if (state.Elapsed < TimeSpan.Zero)
            {
                cmd.RemoveRhythmEngineIsPlaying(entity);
            }

            var nextCurrentBeats = RhythmEngineUtility.GetActivationBeat(state.__ref, settings.__ref);
            if (state.CurrentBeat != nextCurrentBeats)
                state.NewBeatTick = (uint) time.Frame;

            state.CurrentBeat = nextCurrentBeats;
        }
    }
}