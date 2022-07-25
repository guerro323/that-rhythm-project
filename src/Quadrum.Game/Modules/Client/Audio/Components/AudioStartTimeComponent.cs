using System;
using Quadrum.Game.Modules.Client.Audio;

namespace GameHost.Audio.Players;

/// <summary>
///     Manage the delay of an incoming <see cref="AudioPlayerEntity"/> request
/// </summary>
public struct AudioStartTimeComponent
{
    public TimeSpan StartTime;

    public AudioStartTimeComponent(TimeSpan startTime)
    {
        if (startTime < TimeSpan.Zero)
            startTime = TimeSpan.Zero;
        StartTime = startTime;
    }
}