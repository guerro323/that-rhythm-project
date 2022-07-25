using System;
using DefaultEcs;
using revghost;
using revghost.Domains;
using revghost.Domains.Time;
using revghost.Loop;
using revghost.Loop.EventSubscriber;
using revghost.Threading.V2;
using revghost.Threading.V2.Apps;

namespace Quadrum.Game.Modules.Client.Audio.Server;

public class AudioDomain : CommonDomainThreadListener
{
    public readonly AudioScope Scope;
    
    private readonly DomainWorker worker;
    
    public readonly IDomainUpdateLoopSubscriber UpdateLoop;
    private readonly DefaultDomainUpdateLoopSubscriber _updateLoop;

    public readonly IManagedWorldTime WorldTime;
    private readonly ManagedWorldTime _worldTime;
    
    public AudioDomain(Scope scope, Entity domainEntity) : base(scope, domainEntity)
    {
        worker = new DomainWorker("audio-server");

        Scope = new AudioScope(scope);
        {
            Scope.Context.Register(UpdateLoop = _updateLoop = new DefaultDomainUpdateLoopSubscriber(Scope.World));
            Scope.Context.Register(WorldTime = _worldTime = new ManagedWorldTime());
        }
    }

    private const int TargetFrameRate = 2;

    protected override ListenerUpdate OnUpdate()
    {
        using (worker.StartMonitoring(TimeSpan.FromMilliseconds(TargetFrameRate)))
        {
            base.OnUpdate();
        }

        return new ListenerUpdate
        {
            // low latency for audio
            TimeToSleep = TimeSpan.FromMilliseconds(TargetFrameRate)
        };
    }

    protected override void DomainUpdate()
    {
        _worldTime.Delta = worker.Delta;
        _worldTime.Total = worker.Elapsed;

        _updateLoop.Invoke(worker.Elapsed, worker.Delta);
    }
}

public class AudioScope : Scope
{
    public World World;

    public AudioScope(Scope parent) : base(new ChildScopeContext(parent.Context))
    {
        Context.Register(World = new World());
    }

    public override void Dispose()
    {
        base.Dispose();
        
        World.Dispose();
    }
}