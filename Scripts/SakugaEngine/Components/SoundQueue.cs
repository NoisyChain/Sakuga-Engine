using Godot;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class SoundQueue : AudioStreamPlayer3D
    {
        [Export] private ushort QueueDelay = 12;
        private bool Queued;
        public ushort QueueTimer = 0;
        private AudioStream bufferedSound;

        public override void _PhysicsProcess(double delta)
        {
            TickQueueTimer();
            PlayQueue();
        }

        public void SimpleQueueSound()
        {
            bufferedSound = Stream;
            Queued = true;
        }

        public void QueueSound(AudioStream sound)
        {
            if (Queued && Stream == sound) return;

            bufferedSound = sound;
            QueueTimer = 0;
            Queued = true;
            GD.Print("Sound queued");
        }

        public void PlayQueue()
        {
            if (!Queued) return;
            if (bufferedSound == null) return;
            if (QueueTimer > 0) return;

            Stream = bufferedSound;
            
            Play();
            QueueTimer = QueueDelay;
            Queued = false;
            bufferedSound = null;
        }

        private void TickQueueTimer()
        {
            if (QueueTimer == 0) return;

            QueueTimer--;
        }
    }
}
