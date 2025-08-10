using Godot;
using SakugaEngine.Collision;
using System.IO;

namespace SakugaEngine
{
    public partial class SakugaVFX : SakugaNode
    {
        [ExportCategory("Settings")]
        [Export] private int Duration;
        [Export] private AnimationPlayer Player;
        [Export] private Node3D Graphics;
        [Export] private string AnimationName;
        [Export] private SoundQueue Sound;
        public bool IsActive;
        public PhysicsBody ParentBody;
        public Vector2I PreviousPosition;
        public Vector2I FixedPosition;
        public int Frame;
        public int Side;

        public override void Render()
        {
            GlobalPosition = Global.ToScaledVector3(FixedPosition);
            Graphics.Scale = new Vector3(Side, 1, 1);
            Graphics.Visible = IsActive;
            Player.Play(AnimationName);
            Player.Seek(Frame / (float)Global.TicksPerSecond, true);
        }

        public void Initialize()
        {
            FixedPosition = Vector2I.Zero;
            Side = 0;
            Frame = -1;
            IsActive = false;
        }
        public void Spawn(Vector2I origin, int side, PhysicsBody setParent)
        {
            ParentBody = setParent;
            if (setParent != null)
            {
                FixedPosition = origin;
                PreviousPosition = ParentBody.FixedPosition;
            }
            else
            {
                FixedPosition = origin;
            }
            Side = side;
            Frame = Duration;
            //Sound.Stop();
            Sound.SimpleQueueSound();
            IsActive = true;
        }

        public override void LateTick()
        {
            if (!IsActive) return;

            Frame--;
            if (Frame < 0) { IsActive = false;  return; }
            if (ParentBody != null)
            {
                Vector2I additional = ParentBody.FixedPosition - PreviousPosition;
                FixedPosition += additional;
                PreviousPosition = ParentBody.FixedPosition;
            }
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write(FixedPosition.X);
            bw.Write(FixedPosition.Y);
            bw.Write(PreviousPosition.X);
            bw.Write(PreviousPosition.Y);
            bw.Write(IsActive);
            bw.Write(Frame);
            bw.Write(Side);
        }

        public override void Deserialize(BinaryReader br)
        {
            FixedPosition.X = br.ReadInt32();
            FixedPosition.Y = br.ReadInt32();
            PreviousPosition.X = br.ReadInt32();
            PreviousPosition.Y = br.ReadInt32();
            IsActive = br.ReadBoolean();
            Frame = br.ReadInt32();
            Side = br.ReadInt32();
        }
    }
}
