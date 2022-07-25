using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Cursors;

public partial struct CursorOffset : ISparseComponent
{
    public float Value;

    public CursorOffset(float value)
    {
        Value = value;
    }
}