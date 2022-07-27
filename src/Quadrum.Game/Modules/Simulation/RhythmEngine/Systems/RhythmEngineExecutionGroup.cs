using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Utilities;
using revecs;
using revecs.Systems;
using revecs.Systems.Generator;
using revghost;
using revghost.Ecs;
using revghost.Injection;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public class RhythmEngineExecutionGroup : BaseSystemGroup<RhythmEngineExecutionGroup>
{
    public RhythmEngineExecutionGroup(Scope scope) : base(scope)
    {
    }
}