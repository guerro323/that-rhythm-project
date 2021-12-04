using Quadrum.Game.Modules.Client.Audio;

namespace GameHost.Audio.Players;

/// <summary>
///     Manage the delay of an incoming <see cref="AudioPlayerEntity"/> request
/// </summary>
public struct AudioDelayComponent
{
    public TimeSpan Delay;

    public AudioDelayComponent(TimeSpan delay)
    {
        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;
        Delay = delay;
    }
}