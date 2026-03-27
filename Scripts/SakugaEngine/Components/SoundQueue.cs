using Godot;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class SoundQueue : AudioStreamPlayer3D
    {
        [Export] private ushort QueueDelay = 12;
        [Export] private AudioStream DefaultSound;
        private bool Queued;
        public ushort QueueTimer = 0;
        private AudioStream BufferedSound;

        public override void _PhysicsProcess(double delta)
        {
            TickQueueTimer();
            PlayQueue();
        }

        public void SimpleQueueSound()
        {
            BufferedSound = DefaultSound;
            Queued = true;
        }

        public void QueueSound(AudioStream sound)
        {
            if (Queued && Stream == sound) return;

            BufferedSound = sound;
            QueueTimer = 0;
            Queued = true;
        }

        public void PlayQueue()
        {
            if (!Queued) return;
            if (BufferedSound == null) return;
            if (QueueTimer > 0) return;

            Stream = BufferedSound;
            
            Play();
            QueueTimer = QueueDelay;
            Queued = false;
            BufferedSound = null;
        }

        private void TickQueueTimer()
        {
            if (QueueTimer == 0) return;

            QueueTimer--;
        }
    }
}
