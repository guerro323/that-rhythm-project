using revecs.Core;

namespace Quadrum.Game.Modules.Simulation.Common.Physics;

public static class RevolutionWorldExtension
{
    public static void SetPhysicsEngine(this RevolutionWorld world, IPhysicsEngine engine)
    {
        var board = world.GetBoardOrDefault<PhysicsEngineBoard>(PhysicsEngineBoard.BoardName);
        if (board == null)
        {
            board = new PhysicsEngineBoard(world);
            world.AddBoard(PhysicsEngineBoard.BoardName, board);
        }

        board.Current = engine;
    }

    public static IPhysicsEngine? GetPhysicsEngine(this RevolutionWorld world)
    {
        var board = world.GetBoardOrDefault<PhysicsEngineBoard>(PhysicsEngineBoard.BoardName);
        if (board == null)
        {
            board = new PhysicsEngineBoard(world);
            world.AddBoard(PhysicsEngineBoard.BoardName, board);
        }

        return board.Current;
    }
}