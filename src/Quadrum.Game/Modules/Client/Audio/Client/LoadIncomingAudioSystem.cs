using System.Threading.Tasks;
using Collections.Pooled;
using DefaultEcs;
using DefaultEcs.Resource;
using revghost;
using revghost.Ecs;
using revghost.Injection.Dependencies;
using revghost.IO.Storage;
using revghost.Shared.Collections;
using revghost.Utility;

namespace Quadrum.Game.Modules.Client.Audio.Client;

public class LoadIncomingAudioSystem : AppSystem
{
    private World _world;
    
    public LoadIncomingAudioSystem(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _world);
    }

    private ResourceManager _resourceManager;

    protected override void OnInit()
    {
        _resourceManager = new ResourceManager();
        _resourceManager.Manage(_world);
    }

    public class ResourceManager : AResourceManager<IFile, AudioResource>
    {
        protected override AudioResource Load(IFile info)
        {
            return new AudioResource(
                info.FullName,
                Task.Run(async () =>
                {
                    using var list = new PooledList<byte>();
                    await info.GetContentAsync(list);
                    
                    return list.Span.ToArray();
                })
            );
        }

        protected override void OnResourceLoaded(in Entity entity, IFile info, AudioResource resource)
        {
            HostLogger.Output.Info($"Loaded for {resource.Key}");
            entity.Set(resource);
        }
    }
}