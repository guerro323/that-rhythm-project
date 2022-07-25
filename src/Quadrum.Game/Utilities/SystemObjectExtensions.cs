using revecs.Systems.Generator;

namespace Quadrum.Game.Utilities;

public static class SystemObjectExtensions
{
    public static void SetGroup<T>(this SystemObject sys)
        where T : ISystemGroup
    {
        T.AddToGroup(sys);
    }
}

public interface ISystemGroup
{
    static abstract void AddToGroup(SystemObject systemObject);
}