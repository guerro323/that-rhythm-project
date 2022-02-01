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

    private readonly HostLogger _logger = new HostLogger(nameof(SoloudServerSystem));

    private World _world;
    private IDomainUpdateLoopSubscriber _updateLoop;
    private IManagedWorldTime _worldTime;

    public SoloudServerSystem(Soloud soloudObj, Scope scope) : base(scope)
    {
        SoloudObj = soloudObj;

        Dependencies.AddRef(() => ref _world);
        Dependencies.AddRef(() => ref _updateLoop);
        Dependencies.AddRef(() => ref _worldTime);
    }

    private EntitySet _resourceSet;
    private Dictionary<string, Wav> _resourceWavMap = new();

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

            (
                _resourceSet = _world.GetEntities()
                    .With<AudioResource>()
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

    private unsafe void OnUpdate(WorldTime worldTime)
    {
        foreach (ref readonly var entity in _resourceSet.GetEntities())
        {
            var res = entity.Get<AudioResource>();
            if (_resourceWavMap.TryGetValue(res.Key, out var wav))
                continue;

            wav = new Wav();
            _resourceWavMap[res.Key] = wav;

            fixed (byte* dataPtr = res.Bytes)
            {
                wav.loadMem((IntPtr) dataPtr, (uint) res.Bytes.Length, 1);
            }

            _logger.Info($"Loaded wav for {res.Key} (bytes={res.Bytes.Length})");
        }

        using var list = new ValueList<Entity>(0);
        foreach (ref readonly var entity in _controllerSet.GetEntities())
            list.Add(entity);

        foreach (ref readonly var entity in list)
            UpdateEntity(worldTime, entity);
    }

    private void UpdateEntity(in WorldTime worldTime, in Entity entity)
    {
        if (!entity.TryGet(out AudioResource resource))
            return;

        if (!_resourceWavMap.TryGetValue(resource.Key, out var wav))
            return;

        if (entity.Has<PlayAudioRequest>())
            //if (!entity.TryGet(out AudioDelayComponent delay) || worldTime.Total >= delay.Delay)
        {
            _logger.Info("Playing for " + entity, "Event");

            if (entity.TryGet(out uint currSoloudId))
            {
                if (entity.TryGet(out AudioDelayComponent delay))
                {
                    SoloudObj.scheduleStop(currSoloudId, delay.Delay.TotalSeconds);
                }
                else
                {
                    SoloudObj.stop(currSoloudId);
                }
            }

            uint play;
            {
                if (entity.TryGet(out AudioDelayComponent delay))
                {
                    play = SoloudObj.play(wav, aPaused: 1);

                    var rate = SoloudObj.getSamplerate(play);
                    // TODO: investigate why it doesn't work as expect (this is the reason we force it back to 44150hz)
                    rate = 44100;
                    
                    SoloudObj.setDelaySamples(play, (uint) (rate * delay.Delay.TotalSeconds));
                    SoloudObj.setPause(play, 0);
                }
                else
                {
                    play = SoloudObj.play(wav);
                }
            }

            entity.Set(play);

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