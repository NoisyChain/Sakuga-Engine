using Godot;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class SoundQueue : AudioStreamPlayer3D
    {
        [Export] private ushort QueueDelay = 12;
        private bool Queued;
        public ushort QueueTimer = 0;

        public override void _PhysicsProcess(double delta)
        {
            TickQueueTimer();
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
            QueueTimer = 0;
            Queued = true;
        }

        public void PlayQueue()
        {
            if (!Queued) return;
            if (Stream == null) return;
            if (QueueTimer > 0) return;

            Play();
            QueueTimer = QueueDelay;
            Queued = false;
        }

        private void TickQueueTimer()
        {
            if (QueueTimer == 0) return;

            QueueTimer--;
        }
    }
}
