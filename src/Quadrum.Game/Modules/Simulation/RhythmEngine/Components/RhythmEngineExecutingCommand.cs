using System;
using revecs.Core;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Components;

public partial struct RhythmEngineExecutingCommand : ISparseComponent
{
    public UEntitySafe Previous;
    public UEntitySafe CommandTarget;

    /// <summary>
    ///     At which 'activation' beat will the command start?
    /// </summary>
    public int ActivationBeatStart;

    /// <summary>
    ///     At which 'activation' beat will the command end?
    /// </summary>
    public int ActivationBeatEnd;

    /// <summary>
    ///     Power is associated with beat score, this is a value between 0 and 100.
    /// </summary>
    /// <remarks>
    ///     This is not associated at all with fever state, the command will check if there is fever or not on the engine.
    ///     The game will check if it can enable hero mode if power is 100.
    /// </remarks>
    public int PowerInteger;

    public bool WaitingForApply;

    /// <summary>
    ///     Return a power between a range of [0..1]
    /// </summary>
    public double Power
    {
        get => PowerInteger * 0.01;
        set => PowerInteger = (int) Math.Clamp(value * 100, 0, 100);
    }

    public bool IsPerfect => PowerInteger >= 99;

    public override string ToString()
    {
        return $"Target={CommandTarget}, ActiveAt={ActivationBeatStart}, Power={Power:0.00%}";
    }
}