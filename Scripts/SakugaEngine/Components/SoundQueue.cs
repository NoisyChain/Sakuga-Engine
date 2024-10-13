using Godot;
using System;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class SoundQueue : AudioStreamPlayer3D
    {
        private bool Queued;

        public override void _Process(double delta)
        {
            PlayQueue();
        }

        public void SimpleQueueSound()
        {
            Queued = true;
        }

        public void QueueSound(AudioStream sound)
        {
            if (Queued && Stream == sound) return;

            Stream = sound;
            Queued = true;
        }

        public void PlayQueue()
        {
            if (!Queued) return;
            if (Stream == null) return;
            if (Playing) return;

            Play();
            Queued = false;
        }
    }
}
