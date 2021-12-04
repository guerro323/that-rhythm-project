using DefaultEcs;
using GameHost.Audio.Players;
using Quadrum.Game.Modules.Client.Audio;
using revghost;
using revghost.Domains.Time;
using revghost.Ecs;
using revghost.Injection.Dependencies;
using revghost.Loop.EventSubscriber;
using revghost.Shared;
using revghost.Shared.Collections;
using revghost.Utility;

namespace GameHost.Audio;

public class SoloudServerSystem : AppSystem
{
    public readonly Soloud SoloudObj;

    private HostLogger _hostLogger;
    
    private World _world;
    private IDomainUpdateLoopSubscriber _updateLoop;
    private IManagedWorldTime _worldTime;

    public SoloudServerSystem(Soloud soloudObj, Scope scope) : base(scope)
    {
        SoloudObj = soloudObj;

        Dependencies.AddRef(() => ref _hostLogger);
        Dependencies.AddRef(() => ref _world);
        Dependencies.AddRef(() => ref _updateLoop);
        Dependencies.AddRef(() => ref _worldTime);
    }

    private EntitySet _controllerSet;

    protected override void OnInit()
    {
        Disposables.AddRange(new[]
        {
            _updateLoop.Subscribe(OnUpdate)
                .IntendedBox(),

            (
                _controllerSet = _world.GetEntities()
                    .With<AudioPlayerComponent>()
                    .AsSet()
            ).IntendedBox(),

            _world.SubscribeComponentAdded((in Entity entity, in PlayAudioRequest _) =>
            {
                UpdateEntity(_worldTime.ToStruct(), entity);
            }).IntendedBox(),

            _world.SubscribeComponentAdded((in Entity entity, in StopAudioRequest _) =>
            {
                UpdateEntity(_worldTime.ToStruct(), entity);
            }).IntendedBox()
        });
    }

    private void OnUpdate(WorldTime worldTime)
    {
        using var list = new ValueList<Entity>();
        foreach (ref readonly var entity in _controllerSet.GetEntities())
            list.Add(entity);

        foreach (ref readonly var entity in list)
            UpdateEntity(worldTime, entity);
    }

    private unsafe void UpdateEntity(in WorldTime worldTime, in Entity entity)
    {
        _hostLogger.Info("Playing for " + entity);
        
        if (!entity.Has<Wav>())
        {
            if (!entity.TryGet(out AudioResource res))
            {
                _hostLogger.Error($"No AudioResource found on {entity}");
                return;
            }

            var wav = new Wav();
            fixed (byte* dataPtr = res.Bytes)
            {
                wav.loadMem((IntPtr) dataPtr, (uint) res.Bytes.Length, 1);
            }

            entity.Set(wav);
        }

        if (entity.Has<PlayAudioRequest>())
            if (!entity.TryGet(out AudioDelayComponent delay) || worldTime.Total >= delay.Delay)
            {
                if (entity.TryGet(out uint currSoloudId))
                    SoloudObj.stop(currSoloudId);

                entity.Set(SoloudObj.play(entity.Get<Wav>()));

                entity.Remove<PlayAudioRequest>();
                entity.Remove<AudioDelayComponent>();
            }

        if (entity.Has<StopAudioRequest>())
            if (!entity.TryGet(out AudioDelayComponent delay) || worldTime.Total >= delay.Delay)
            {
                if (entity.TryGet(out uint currSoloudId))
                    SoloudObj.stop(currSoloudId);

                entity.Remove<StopAudioRequest>();
                entity.Remove<AudioDelayComponent>();
            }
    }
}