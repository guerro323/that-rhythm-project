using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Cursors;

public partial struct CursorOffset : ISparseComponent
{
    // offset when in an idle stance (walking, retreating, etc...)
    public float Idle;
    // offset when in attack stance (must be an offset relative to the army)
    public float Attack;
    
    public static float CenterComputeV1(int i, int size, float space)
    {
        if (size == 1 && i == 0)
            return 0;
			
        return (i - (size/* - 1*/) / 2) * space + space / 2f;
    }
    
    
    // A clear example of how it would work:
    // 
    // <B   B   B>   <H>   <Y   Y   Y>   <T> (b=yumipon, h=hatapon, y=yaripon, t=tatepon)
    // Idle
    //  -3  -2  -1    +0    +1  +2  +3    +4
    //
    // Attack
    //  -1  +0  +1    +0    -1  +0  +1    +0
}