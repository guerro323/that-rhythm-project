using GameHost.Native;
using GameHost.Native.Fixed;
using revecs.Core;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Abilities.Components;

public partial struct OwnerActiveAbility : ISparseComponent
{
    public int LastCommandActiveTime;
    public int LastActivationTime;

    public UEntityHandle PreviousActive;
    public UEntityHandle Active;
    public UEntityHandle Incoming;

    /// <summary>
    /// Current command combo of the owner
    /// </summary>
    public FixedBuffer32<UEntityHandle> Combo;
    
    public void AddCombo(UEntityHandle ent)
    {
        while (Combo.GetLength() >= Combo.GetCapacity())
            Combo.RemoveAt(0);
        Combo.Add(ent);
    }

    public bool RemoveCombo(UEntityHandle ent)
    {
        var index = Combo.IndexOf(ent);
        if (index < 0)
            return false;
        Combo.RemoveAt(index);
        return true;
    }
}