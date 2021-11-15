using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using revecs;
using revecs.Systems;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial struct ApplyTagsSystem : ISystem
{
    [RevolutionSystem]
    [DependOn(typeof(RhythmEngineExecutionGroup.Begin)), AddForeignDependency(typeof(RhythmEngineExecutionGroup.End))]
    private static void Method(
        [Query] eq<
            Read<RhythmEngineController>
        > query,
        [Cmd] ec<
            RhythmEngineIsPlaying.Cmd.IAdmin,
            RhythmEngineIsPaused.Cmd.IAdmin
        > cmd)
    {
        foreach (var (handle, controller) in query)
        {
            switch (controller.State)
            {
                case RhythmEngineController.EState.Playing:
                    cmd.AddRhythmEngineIsPlaying(handle);
                    cmd.RemoveRhythmEngineIsPaused(handle);
                    break;
                case RhythmEngineController.EState.Paused:
                    cmd.RemoveRhythmEngineIsPlaying(handle);
                    cmd.AddRhythmEngineIsPaused(handle);
                    break;
                case RhythmEngineController.EState.Stopped:
                    cmd.RemoveRhythmEngineIsPlaying(handle);
                    cmd.RemoveRhythmEngineIsPaused(handle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}