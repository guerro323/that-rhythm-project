using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using Quadrum.Game.Utilities;
using revecs;
using revghost;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial class ResizeCommandBufferSystem : SimulationSystem
{
    public ResizeCommandBufferSystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            p => p
                .SetGroup<RhythmEngineExecutionGroup>()
                .After(typeof(GetNextCommandEngineSystem))
        );
    }

    private EngineQuery _query;

    protected override void OnInit()
    {
        _query = new EngineQuery(Simulation);
    }
    
    private void OnUpdate(Entity obj)
    {
        _query.QueueAndComplete(Runner, static (_, enumerator) =>
        {
            foreach (var engine in enumerator)
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
        });
    }

    private partial record struct EngineQuery : IQuery<(
        Write<RhythmEngineCommandProgress> Buffer,
        Read<RhythmEngineState> State,
        Read<RhythmEngineSettings> Settings,
        Read<RhythmEngineRecoveryState> Recovery)>;
}