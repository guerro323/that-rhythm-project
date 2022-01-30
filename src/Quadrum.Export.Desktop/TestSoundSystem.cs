using System;
using System.Linq;
using DefaultEcs;
using Quadrum.Game.Modules.Client.Audio.Client;
using revghost;
using revghost.Domains;
using revghost.Ecs;
using revghost.Injection;
using revghost.Injection.Dependencies;
using revghost.IO.Storage;
using revghost.Loop.EventSubscriber;

namespace Quadrum.Export.Godot;

public class TestSoundSystem : AppSystem
{
    private AudioClient _audioClient;
    private World _world;

    private IStorage _storage;

    public TestSoundSystem(IStorage storage, Scope scope) : base(scope)
    {
        Dependencies.AddRef(() => ref _audioClient);
        Dependencies.AddRef(() => ref _world);

        _storage = storage;
    }

    protected override void OnInit()
    {
        Entity audio;
        {
            using var files = _storage.GetPooledFiles("fever_entrance_0.ogg");
            
            audio = _audioClient.CreateAudio(files.First());

            Console.WriteLine(files.First().FullName);
        }
        var player = _audioClient.CreatePlayer();
        player.SetAudio(audio);
        player.Play();
    }
}