using System;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Components;

public partial struct PowerGaugeState : ISparseComponent
{
    public int Level;
    public int MaxLevel;

    public int Tick;
    public int MaxTick;

    public void Increase(int tick)
    {
        Tick += tick;
        while (Tick >= MaxTick)
        {
            Level += 1;
            Tick -= MaxTick;
        }

        var reachedMaxLevel = Level >= MaxLevel;
        Tick = reachedMaxLevel ? MaxTick : Math.Clamp(Tick, 0, MaxTick);
        
        Level = Math.Clamp(Level, 0, MaxLevel);
    }
}