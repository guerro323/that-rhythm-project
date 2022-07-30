using System;
using revecs.Core;
using revecs.Core.Boards;

namespace Quadrum.Game.Modules.Simulation.Abilities.Boards;

public delegate void AbilitySetup(ReadOnlySpan<UEntityHandle> abilities);
public delegate void AbilityExecute(UEntityHandle owner, UEntityHandle self);

public class AbilitiesFunctionBoard : BoardBase
{
    private (byte b, AbilityExecute[] execute) column;

    public AbilitiesFunctionBoard(RevolutionWorld world) : base(world)
    {
        world.ComponentTypeBoard.CurrentSize.Subscribe((prev, next) =>
        {
            Array.Resize(ref column.execute, next);
        }, true);
    }

    public ReadOnlySpan<AbilityExecute> ExecuteFunction => column.execute;

    public void SetFunctions(ComponentType abilityType, AbilityExecute execute)
    {
        column.execute[abilityType.Handle] = execute;
    }

    public override void Dispose()
    {
        
    }

    internal static AbilitiesFunctionBoard GetOrCreate(RevolutionWorld world)
    {
        const string name = "AbilitiesFunctionBoard";
        var board = world.GetBoardOrDefault<AbilitiesFunctionBoard>(name);
        if (board == null)
            world.AddBoard(name, board = new AbilitiesFunctionBoard(world));

        return board;
    }
}