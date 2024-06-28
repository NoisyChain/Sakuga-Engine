using Godot;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class CombatTracker : Node
    {
        public int HitCombo;
        public int LastDamage;
        public int CurrentCombo;
        public int HighestCombo;
        public int TotalFrames;
        public int FrameData;
        public int FrameAdvantage;
        public int HitFrame;
        public int LastHitType;
        public bool invalidHit;

        public void Initialize() 
        {
            HitCombo = 0;
            LastDamage = 0;
            CurrentCombo = 0;
            HighestCombo = 0;
            TotalFrames = 0;
            FrameData = 0;
            FrameAdvantage = 0;
            HitFrame = 0;
            LastHitType = 0;
            invalidHit = false;
        }
        public void UpdateTrackers(int damage, int hitFrame, int hitType, bool isInvalid) 
        {
            if (isInvalid)
                invalidHit = true;

            HitCombo++;
            LastDamage = damage;
            CurrentCombo += damage;
            if (CurrentCombo > HighestCombo)
                HighestCombo = CurrentCombo;
            HitFrame = hitFrame;
            LastHitType = hitType;
        }

        public void UpdateFrameData(FighterBody owner)
        {
            FrameData = owner.Animator.StateType() == 1 ? 0 : owner.Animator.GetCurrentState().Duration - owner.Animator.Frame;
            FrameAdvantage = (owner.Animator.StateType() == 3 || owner.GetOpponent().Animator.StateType() == 3) ? 
                        HitFrame - owner.GetOpponent().Tracker.HitFrame : 0;

            FrameData = Mathf.Clamp(FrameData, 0, owner.Animator.GetCurrentState().Duration);
        }

        public void Reset()
        {
            HitCombo = 0;
            LastDamage = 0;
            CurrentCombo = 0;
            invalidHit = false;
        }

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(HitCombo);
            bw.Write(LastDamage);
            bw.Write(CurrentCombo);
            bw.Write(HighestCombo);
            bw.Write(TotalFrames);
            bw.Write(FrameData);
            bw.Write(FrameAdvantage);
			bw.Write(HitFrame);
			bw.Write(LastHitType);

			bw.Write(invalidHit);
        }

        public void Deserialize(BinaryReader br)
        {
            HitCombo = br.ReadInt32();
            LastDamage = br.ReadInt32();
            CurrentCombo = br.ReadInt32();
            HighestCombo = br.ReadInt32();
			TotalFrames = br.ReadInt32();
			FrameData = br.ReadInt32();
            FrameAdvantage = br.ReadInt32();
            HitFrame = br.ReadInt32();
            LastHitType = br.ReadInt32();

			invalidHit = br.ReadBoolean();
        }
    }
}
