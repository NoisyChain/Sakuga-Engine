using Godot;
using SakugaEngine.GameState;
using SakugaEngine.Global;
using MessagePack;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class SakugaVFX : SakugaNode
    {
        private SakugaActor _master;
        [ExportCategory("Settings")]
        [Export] private int Duration;
        [Export] private AnimationPlayer Player;
        [Export] private Node3D Graphics;
        [Export] private string AnimationName;
        [Export] private SoundQueue Sound;
        public bool AttachedToOwner;
        public Vector2I FixedPosition;
        public int Frame;
        public int Side;

        SakugaVFX actor;
        private SakugaVFXState State;

        public void SetMaster(SakugaActor newMaster) => _master = newMaster;
        public SakugaActor GetMaster() => _master;

        public override void Render()
        {
            Graphics.Visible = IsActive;
            Vector2I pos = FixedPosition;
            if (AttachedToOwner) pos = _master.Body.FixedPosition + new Vector2I(FixedPosition.X, FixedPosition.Y);
            Graphics.GlobalPosition = GlobalFunctions.ToScaledVector3(pos);
            Graphics.GlobalRotation = Vector3.Zero;
            Graphics.Scale = new Vector3(Side, 1, 1);
            
            Player.Play(AnimationName);
            Player.Seek(Frame / (float)GlobalVariables.TicksPerSecond, true);
        }

        public override void Initialize()
        {
            base.Initialize();
            actor = this;
            FixedPosition = Vector2I.Zero;
            Side = 0;
            Frame = -1;
        }
        public void Spawn(Vector2I origin, int side, bool attached)
        {
            AttachedToOwner = attached;
            FixedPosition = origin;
            Side = side;
            Frame = Duration;
            if (AttachedToOwner)
            {
                FixedPosition -= new Vector2I(_master.Body.FixedPosition.X, _master.Body.FixedPosition.Y);
            } 
            if (Sound != null)
            {
                Sound.SimpleQueueSound();
            }
            IsActive = true;
        }

        public override void LateTick()
        {
            if (!IsActive) return;

            Frame--;
            if (Frame < 0) { IsActive = false;  return; }
        }

        public override byte[] GetStateData()
        {
            State.GetStateData(ref actor);
            return MessagePackSerializer.Serialize(State);
        }
		public override void SetStateData(byte[] stateBuffer)
        {
            State = MessagePackSerializer.Deserialize<SakugaVFXState>(stateBuffer);
            State.SetStateData(ref actor);
        }
    }
}
