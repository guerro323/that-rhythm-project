using Quadrum.Game.Modules.Simulation.Application;
using revecs;

namespace Quadrum.Game.Modules.Simulation;

public partial struct GameTimeSingleton :
    IQuery<Read<GameTime>>,
    Singleton
{

}