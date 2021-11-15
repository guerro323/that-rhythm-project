using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs;
using revecs.Systems;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial struct ResizeCommandBufferSystem : ISystem
{
    private partial struct EngineQuery : IQuery<
        Write<RhythmEngineCommandProgress>,
        Read<RhythmEngineState>,
        Read<RhythmEngineSettings>,
        Read<RhythmEngineRecoveryState>
    >
    {
    }

    [RevolutionSystem]
    [DependOn(typeof(RhythmEngineExecutionGroup.Begin)), AddForeignDependency(typeof(RhythmEngineExecutionGroup.End))]
    [DependOn(typeof(GetNextCommandEngineSystem))]
    private static void Method([Query] EngineQuery query)
    {
        foreach (var (buffer, state, settings, recovery) in query)
        {
            var progress = buffer.Reinterpret<FlowPressure>();

            var flowBeat = RhythmEngineUtility.GetFlowBeat(state.__ref, settings.__ref);
            var mercy = 0; // todo: when on authoritative server, increment it by one
            for (var i = 0; i != progress.Count; i++)
            {
                var currCommand = progress[i];
                if (flowBeat >= currCommand.FlowBeat + mercy + settings.MaxBeats
                    || recovery.__ref.IsRecovery(flowBeat))
                {
                    progress.RemoveAt(i--);
                }
            }
        }
    }
}