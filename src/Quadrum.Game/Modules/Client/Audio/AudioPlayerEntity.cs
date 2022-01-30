using DefaultEcs;
using DefaultEcs.Resource;
using GameHost.Audio.Players;
using revghost.IO.Storage;
using revghost.Utility;

namespace Quadrum.Game.Modules.Client.Audio;

public struct AudioPlayerEntity : IDisposable
{
    public readonly Entity Original;
    
    public AudioPlayerEntity(Entity original)
    {
        Original = original;
        Original.Set<AudioPlayerComponent>();
    }
    
    public void Dispose()
    {
        Refresh();
        
        Original.Dispose();
    }

    /// <summary>
    /// Force an immediate refresh by sending/receiving data immediately
    /// </summary>
    public void Refresh()
    {
        
    }

    public void SetAudio(Entity audioEntity)
    {
        Original.Set(audioEntity.Get<AudioResource>());
    }

    public void SetAudio(IFile file)
    {
        Original.Set(ManagedResource<AudioResource>.Create(file));
    }

    public void Play()
    {
        Original.Remove<StopAudioRequest>();
        Original.Remove<PauseAudioRequest>();
        Original.Remove<AudioDelayComponent>();

        Original.Set(new PlayAudioRequest());
    }

    public void Stop()
    {
        Original.Remove<PlayAudioRequest>();
        Original.Remove<PauseAudioRequest>();
        Original.Remove<AudioDelayComponent>();

        Original.Set(new StopAudioRequest());
    }

    public void Pause()
    {
        Original.Remove<StopAudioRequest>();
        Original.Remove<PlayAudioRequest>();

        Original.Set(new PauseAudioRequest());
    }

    public void PlayDelayed(TimeSpan delay)
    {
        Original.Remove<StopAudioRequest>();
        Original.Remove<PauseAudioRequest>();

        Original.Set(new AudioDelayComponent(delay));
        Original.Set(new PlayAudioRequest());
    }

    public TimeSpan GetPlayTime()
    {
        if (!Original.TryGet(out AudioCurrentPlayTime currentPlayTime))
            return TimeSpan.Zero;
        return currentPlayTime.Value;
    }
}

public struct AudioPlayerComponent
{
    
}

public struct PlayAudioRequest
{
}

public struct PauseAudioRequest
{
}

public struct StopAudioRequest
{
}