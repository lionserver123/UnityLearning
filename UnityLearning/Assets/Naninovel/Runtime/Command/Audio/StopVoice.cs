﻿// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading;
using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Stops playback of the currently played voice clip.
    /// </summary>
    public class StopVoice : Command
    {
        public override Task ExecuteAsync (CancellationToken cancellationToken = default)
        {
            Engine.GetService<AudioManager>()?.StopVoice();
            return Task.CompletedTask;
        }
    } 
}
