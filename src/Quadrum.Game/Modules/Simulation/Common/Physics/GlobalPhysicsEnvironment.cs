using revecs;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Common.Physics;

public partial struct GlobalPhysicsEnvironment : ITagComponent
{

}

public partial struct GlobalEnvironmentQuery : IQuery<(
    All<ColliderRootDescription>,
    All<GlobalPhysicsEnvironment>)>
{
    
}