using Quadrum.Game.Modules.Client.Audio.Server;
using revghost;
using revghost.Module;
using revghost.Threading.Components;
using revghost.Threading.V2;

namespace Quadrum.Game.Modules.Client.Audio;

public class Module : HostModule
{
    private readonly HostRunnerScope _hostScope;
    public Module(HostRunnerScope scope) : base(scope)
    {
        _hostScope = scope;
    }

    protected override void OnInit()
    {
        var listenerCollection = _hostScope.World.CreateEntity();
        listenerCollection.Set<ListenerCollectionBase>(new ListenerCollection());

        var domainEntity = _hostScope.World.CreateEntity();
        var serverDomain = new AudioDomain(_hostScope, domainEntity);
        domainEntity.Set<IListener>(serverDomain);
        domainEntity.Set(new PushToListenerCollection(listenerCollection));

        //
        _hostScope.Context.Register(serverDomain);
    }
}