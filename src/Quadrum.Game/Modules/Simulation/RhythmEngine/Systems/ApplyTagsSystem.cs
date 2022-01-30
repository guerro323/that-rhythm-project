using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial struct ApplyTagsSystem : IRevolutionSystem,
    RhythmEngineIsPlaying.Cmd.IAdmin,
    RhythmEngineIsPaused.Cmd.IAdmin
{
    public void Constraints(in SystemObject sys)
    {
        sys.DependOn<RhythmEngineExecutionGroup.Begin>();
        sys.AddForeignDependency<RhythmEngineExecutionGroup.End>();
    }

    public void Body()
    {
        foreach (var engine in RequiredQuery(Read<RhythmEngineController>("Controller")))
        {
            switch (engine.Controller.State)
            {
                case RhythmEngineController.EState.Playing:
                    Cmd.AddRhythmEngineIsPlaying(engine.Handle);
                    Cmd.RemoveRhythmEngineIsPaused(engine.Handle);
                    break;
                case RhythmEngineController.EState.Paused:
                    Cmd.RemoveRhythmEngineIsPlaying(engine.Handle);
                    Cmd.AddRhythmEngineIsPaused(engine.Handle);
                    break;
                case RhythmEngineController.EState.Stopped:
                    Cmd.RemoveRhythmEngineIsPlaying(engine.Handle);
                    Cmd.RemoveRhythmEngineIsPaused(engine.Handle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}