using System;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using Quadrum.Game.Utilities;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial struct ProcessSystem : IRevolutionSystem,
    RhythmEngineIsPlaying.Cmd.IAdmin
{
    public void Constraints(in SystemObject sys)
    {
        sys.SetGroup<RhythmEngineExecutionGroup>();
        {
            sys.DependOn<ApplyTagsSystem>();
        }
    }

    public void Body()
    {
        var time = RequiredResource<GameTime>();
        foreach (var engine in RequiredQuery(
                     Write<RhythmEngineState>("State"),
                     Read<RhythmEngineSettings>("Settings"),
                     Read<RhythmEngineController>("Controller"),
                     All<RhythmEngineIsPlaying>()))
        {
            if (engine.Controller.StartTime != engine.State.PreviousStartTime)
            {
                engine.State.PreviousStartTime = engine.Controller.StartTime;
                engine.State.Elapsed = time.Total - engine.Controller.StartTime;
            }

            engine.State.Elapsed += time.Delta;

            if (engine.State.Elapsed < TimeSpan.Zero)
            {
                Cmd.RemoveRhythmEngineIsPlaying(engine.Handle);
            }

            var nextCurrentBeats = RhythmUtility.GetActivationBeat(engine.State, engine.Settings);
            if (engine.State.CurrentBeat != nextCurrentBeats)
                engine.State.NewBeatTick = (uint) time.Frame;

            engine.State.CurrentBeat = nextCurrentBeats;
        }
    }
}