using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Utilities;
using revghost;
using revghost.Ecs;

namespace Quadrum.Game.Modules.Client.Audio;

public class AudioSystemGroup : BaseSystemGroup<AudioSystemGroup>
{
    public AudioSystemGroup(Scope scope) : base(scope)
    {
    }
}