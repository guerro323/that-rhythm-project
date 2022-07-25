using revecs.Core;
using revecs.Extensions.EntityLayout;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Components;

public partial struct RhythmEngineLayout : IEntityLayoutComponent
{
    public void GetComponentTypes(RevolutionWorld world, List<ComponentType> componentTypes)
    {
        componentTypes.AddRange(new ComponentType[]
        {
            RhythmEngineDescription.Type.GetOrCreate(world),
            
            RhythmEngineController.Type.GetOrCreate(world),
            RhythmEngineState.Type.GetOrCreate(world),
            RhythmEngineSettings.Type.GetOrCreate(world),
            RhythmEngineRecoveryState.Type.GetOrCreate(world),

            GameComboState.Type.GetOrCreate(world),
            GameComboSettings.Type.GetOrCreate(world),
            
            GameCommandState.Type.GetOrCreate(world),
            RhythmEngineExecutingCommand.Type.GetOrCreate(world),
            RhythmEngineCommandProgress.Type.GetOrCreate(world),
            RhythmEnginePredictedCommands.Type.GetOrCreate(world)
        });
    }
}