using System;
using System.Linq;
using DefaultEcs;
using GameHost.Audio.Players;
using Quadrum.Game.Modules.Client.Audio.Client;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using revghost;
using revghost.Domains;
using revghost.Domains.Time;
using revghost.Ecs;
using revghost.Injection;
using revghost.Injection.Dependencies;
using revghost.IO.Storage;
using revghost.Loop.EventSubscriber;

namespace Quadrum.Export.Godot;

public class TestSoundSystem : SimulationSystem
{
    private AudioClient _audioClient;
    private World _world;

    private IStorage _storage;

    public TestSoundSystem(IStorage storage, Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _audioClient);
        Dependencies.Add(() => ref _world);

        _storage = storage;
        
        SubscribeTo<IDomainUpdateLoopSubscriber>(e =>
        {
            
        });
    }

    protected override void OnInit()
    {
        Entity audio;
        {
            using var files = _storage.GetPooledFiles("fever_entrance_0.ogg");

            audio = _audioClient.CreateAudio(files.First());

            Console.WriteLine(files.First().FullName);
        }

        (Dependencies as DependencyCollection).Context.TryGet(out WorldTime worldTime);
        for (var i = 0; i < 16; i++)
        {
            var player = _audioClient.CreatePlayer();
            player.SetAudio(audio);
            player.PlayDelayed(worldTime.Total
                .Add(TimeSpan.FromSeconds(1))
                .Add(TimeSpan.FromMilliseconds(4000 * i)));
        }
    }
}