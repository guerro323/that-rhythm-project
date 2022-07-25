using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using Quadrum.Game.Utilities;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial struct ResizeCommandBufferSystem : IRevolutionSystem
{
    public void Constraints(in SystemObject sys)
    {
        sys.SetGroup<RhythmEngineExecutionGroup>();
        {
            sys.DependOn<GetNextCommandEngineSystem>();
        }
    }

    public void Body()
    {
        foreach (var engine in RequiredQuery(
                     Write<RhythmEngineCommandProgress>("Buffer"),
                     Read<RhythmEngineState>("State"),
                     Read<RhythmEngineSettings>("Settings"),
                     Read<RhythmEngineRecoveryState>("Recovery")))
        {
            var progress = engine.Buffer.Reinterpret<FlowPressure>();

            var flowBeat = RhythmUtility.GetFlowBeat(engine.State, engine.Settings);
            var mercy = 0; // todo: when on authoritative server, increment it by one
            for (var i = 0; i != progress.Count; i++)
            {
                var currCommand = progress[i];
                if (flowBeat >= currCommand.FlowBeat + mercy + engine.Settings.MaxBeats
                    || engine.Recovery.IsRecovery(flowBeat))
                {
                    progress.RemoveAt(i--);
                }
            }
        }
    }
}