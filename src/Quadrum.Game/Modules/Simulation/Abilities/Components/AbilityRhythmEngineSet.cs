using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using revecs.Core;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Abilities.Components;

/// <summary>
/// Data about the state of the rhythm engine when this ability was updated.
/// </summary>
/// <remarks>
/// Prefer this instead of getting engine component data since it's already computed.
/// This is also useful for custom AI bots.
/// </remarks>
public partial struct AbilityRhythmEngineSet : ISparseComponent
{
    public UEntityHandle Engine;

    public RhythmEngineState State;
    public RhythmEngineSettings Settings;
    public RhythmEngineExecutingCommand Executing;
    public GameComboState ComboState;
    public GameComboSettings ComboSettings;
    public GameCommandState CommandState;

    public UEntityHandle Command, PreviousCommand;
}