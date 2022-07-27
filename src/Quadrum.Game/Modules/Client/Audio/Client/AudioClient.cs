using DefaultEcs;
using DefaultEcs.Resource;
using revghost;
using revghost.Ecs;
using revghost.Injection;
using revghost.Injection.Dependencies;
using revghost.IO.Storage;

namespace Quadrum.Game.Modules.Client.Audio.Client;

public class AudioClient : AppSystem
{
    private World _world;

    public AudioClient(Scope scope) : base(scope)
    {
        scope.Context.Register(this);
        
        Dependencies.Add(() => ref _world);
    }

    protected override void OnInit()
    {
    }

    public AudioPlayerEntity CreatePlayer()
    {
        return new AudioPlayerEntity(_world.CreateEntity());
    }

    public Entity CreateAudio(IFile file)
    {
        var resource = _world.CreateEntity();
        resource.Set(ManagedResource<AudioResource>.Create(file));

        return resource;
    }
}