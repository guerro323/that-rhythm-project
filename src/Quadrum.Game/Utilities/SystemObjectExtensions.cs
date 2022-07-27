using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using revecs.Systems.Generator;
using revghost;
using revghost.Ecs;

namespace Quadrum.Game.Utilities;

public static class SystemObjectExtensions
{
    public static OrderBuilder SetGroup<T>(this OrderBuilder builder)
        where T : ISystemGroup
    {
        T.AddToGroup(builder);
        return builder;
    }
    
    public static OrderBuilder AfterGroup<T>(this OrderBuilder builder)
        where T : ISystemGroup
    {
        T.AddAfterGroup(builder);
        return builder;
    }
    
    public static OrderBuilder BeforeGroup<T>(this OrderBuilder builder)
        where T : ISystemGroup
    {
        T.AddBeforeGroup(builder);
        return builder;
    }
}

public interface ISystemGroup
{
    static abstract void AddToGroup(OrderBuilder builder);
    static abstract void AddAfterGroup(OrderBuilder builder);
    static abstract void AddBeforeGroup(OrderBuilder builder);
}

public class BaseSystemGroup<T> : SimulationSystem, ISystemGroup
    // This is done to generate different versions of 'Begin' and 'End' per type
    where T : BaseSystemGroup<T>
{
    public static void AddToGroup(OrderBuilder builder)
    {
        builder.After(typeof(Begin));
        builder.Before(typeof(End));
    }

    public static void AddAfterGroup(OrderBuilder builder)
    {
        builder.After(typeof(End));
    }

    public static void AddBeforeGroup(OrderBuilder builder)
    {
        builder.Before(typeof(Begin));
    }


    protected virtual void SetBegin(OrderBuilder builder)
    {
    }

    protected virtual void SetEnd(OrderBuilder builder)
    {
        builder.After(typeof(Begin));
    }
    
    public BaseSystemGroup(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            _ => { },
            SetBegin,
            typeof(Begin),
            disableArchetypeSynchronization: true
        );
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            _ => { },
            SetEnd,
            typeof(End),
            disableArchetypeSynchronization: true
        );
    }

    protected override void OnInit()
    {
        
    }

    public static class Begin
    {
    }
    
    public static class End
    {
    }
}