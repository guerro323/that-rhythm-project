using System.Threading.Tasks;
using Collections.Pooled;
using DefaultEcs;
using DefaultEcs.Resource;
using revghost;
using revghost.Ecs;
using revghost.IO.Storage;
using revghost.Shared.Collections;
using revghost.Utility;

namespace Quadrum.Game.Modules.Client.Audio.Client;

public class LoadIncomingAudioSystem : AppSystem
{
    public LoadIncomingAudioSystem(Scope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
    }

    public class Ok : AResourceManager<IFile, AudioResource>
    {
        protected override AudioResource Load(IFile info)
        {
            return new AudioResource(
                info.FullName,
                Task.Run(async () =>
                {
                    using var list = new ValueList<byte>();
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