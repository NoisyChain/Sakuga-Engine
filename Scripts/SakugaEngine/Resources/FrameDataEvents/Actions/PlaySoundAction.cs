using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class PlaySoundAction : FrameDataAction
    {
        [Export] public SoundType SoundType;
        [Export] public int Source;
        [Export] public int Index;
        [Export] public bool IsRandom;
        [Export] public int Range;
        [Export] public int FromExtraVariable = -1;
        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Parameters.SoundSources == null || Actor.Parameters.SoundSources.Length == 0) return;
            if (Actor.SFXList == null) return;
            if (Actor.VoiceLines == null) return;
            
            int ind = IsRandom ? RNG.Next(Index, Range) : Index;
            if (FromExtraVariable >= 0)
            {
                ind = Actor.Parameters.Variables[FromExtraVariable].CurrentValue;
                Actor.Parameters.Variables[FromExtraVariable].ChangeBehavior(CustomVariableBehaviorTarget.ON_USE);
            }
            AudioStream selectedSound = null;
            switch (SoundType)
            {
                case SoundType.SFX:
                    selectedSound = Actor.SFXList.Sounds[ind];
                    break;
                case SoundType.VOICE:
                    selectedSound = Actor.VoiceLines.Sounds[ind];
                    break;
            }
            Actor.Parameters.SoundSources[Source].QueueSound(selectedSound);                
        }
    }
}
