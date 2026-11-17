using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
	[GlobalClass] [Tool]
	public partial class AttackProfile : Resource
	{
		[ExportGroup("Damage Settings")]
		 [Export] public HitType HitType = HitType.MEDIUM;
        [Export] public int BaseDamage = 150;
        [Export] public int ChipDamage = 0;
        [Export] public bool ChipDeath = false;
        [Export] public bool KillingBlow = true;
        [Export] public bool GuardCrush = false;
        [Export] public bool MultiHitDamageScaling;
        [Export] public int GuardCrushDamage = 100;
		[Export] public AttackLevel AttackLevel;

        //Ground Hitstun settings
        [ExportGroup("Ground Hit Settings")]
        [Export] public HitstunType GroundHitstunType = HitstunType.BASIC;
        [Export] public int GroundHitReaction = 0;
        [Export] public int CrouchHitReaction = 2;
        [Export] public Vector2I GroundHitKnockback = new (-30000, 0);
        [Export] public Vector2I GroundBlockKnockback = new (-30000, 0);

        //Air Hitstun settings
        [ExportGroup("Air Hit Settings")]
        [Export] public HitstunType AirHitstunType = HitstunType.BASIC;
        [Export] public int AirHitReaction = 3;
        [Export] public Vector2I AirHitKnockback = new (-25000, 35000);
        [Export] public Vector2I AirBlockKnockback = new (-25000, 20000);
        
        //Throw settings
        [ExportGroup("Throw Hit Settings")]
        [Export] public bool GroundThrow = true;
        [Export] public bool AirThrow = false;
        [Export] public bool Immediate = false;
        [Export] public int ThrowHitReaction = -1;

        //Bounce settings
        [ExportGroup("Bounce Settings")]
        [Export] public int BounceTime = 0;
        [Export] public int BounceXIntensity = 100;
        [Export] public int BounceYIntensity = 100;

		//Overrides
		[ExportGroup("Override Values")]
        [Export] public ushort OverridePriority = 0;
		[Export] public int OverrideSelfHitStop = -1;
        [Export] public int OverrideOpponentHitStop = -1;
        [Export] public int OverrideBlockstop = -1;
		[Export] public int OverrideArmorDamage = -1;
		[Export] public int OverrideGroundHitstun = -1;
		[Export] public int OverrideAirHitstun = -1;
        [Export] public int OverrideGroundBlockstun = -1;
		[Export] public int OverrideAirBlockstun = -1;
        [Export] public int OverrideOpponentSuperGaugeGain = -1;
        [Export] public int OverrideSelfSuperGaugeGain = -1;
        [Export] public int OverrideDamageScalingSubtract = -1;
		[Export] public int OverrideHitVFX = -1;
        [Export] public int OverrideBlockVFX = -1;
        [Export] public int OverrideClashVFX = -1;
        [Export] public int OverrideArmorVFX = -1;
		[Export] public int OverrideHitKnockbackDuration = -1;
		[Export] public int OverrideBlockKnockbackDuration = -1;
		[Export] public int OverrideGroundKnockbackGravity = -1;
		[Export] public int OverrideAirKnockbackGravity = -1;
		[Export] public int OverrideCornerPushback = 0;

        // Select override values
        public ushort CurrentPriority { get { return OverridePriority > AttackLevel.Priority ? OverridePriority : AttackLevel.Priority; } }
        public uint CurrentSeftHitstop { get { return OverrideSelfHitStop < 0 ?  GlobalVariables.DefaultBaseHitstop : (uint)OverrideSelfHitStop; } }
        public uint CurrentOpponentHitstop { get { return OverrideOpponentHitStop < 0 ?  GlobalVariables.DefaultBaseHitstop : (uint)OverrideOpponentHitStop; } }
        public uint CurrentBlockstop { get { return OverrideBlockstop < 0 ?  GlobalVariables.DefaultBlockHitstop : (uint)OverrideBlockstop; } }
        public int CurrentArmorDamage { get { return OverrideArmorDamage < 0 ?  AttackLevel.ArmorDamage : OverrideArmorDamage; } }
        public uint CurrentGroundHitstun { get { return OverrideGroundHitstun < 0 ?  AttackLevel.Hitstun : (uint)OverrideGroundHitstun; } }
        public uint CurrentAirHitstun { get { return OverrideAirHitstun < 0 ?  AttackLevel.Hitstun : (uint)OverrideAirHitstun; } }
        public uint CurrentGroundBlockstun { get { return OverrideGroundBlockstun < 0 ?  AttackLevel.Blockstun : (uint)OverrideGroundBlockstun; } }
        public uint CurrentAirBlockstun { get { return OverrideAirBlockstun < 0 ?  AttackLevel.Blockstun : (uint)OverrideAirBlockstun; } }
        public int CurrentSuperGainSelf { get { return OverrideSelfSuperGaugeGain < 0 ? GlobalVariables.DefaultSelfSuperGaugeGain :  OverrideSelfSuperGaugeGain; } }
        public int CurrentSuperGainOpp { get { return OverrideOpponentSuperGaugeGain < 0 ? GlobalVariables.DefaultOpponentSuperGaugeGain :  OverrideOpponentSuperGaugeGain; } }
        public ushort CurrentDamageScaling { get { return OverrideDamageScalingSubtract < 0 ? AttackLevel.DamageScalingSubtract : (ushort)OverrideDamageScalingSubtract; } }
        public int CurrentHitKnockbackDuration { get { return OverrideHitKnockbackDuration < 0 ? GlobalVariables.DefaultKnockbackDuration : OverrideHitKnockbackDuration; } }
        public int CurrentBlockKnockbackDuration { get { return OverrideBlockKnockbackDuration < 0 ? GlobalVariables.DefaultKnockbackDuration : OverrideBlockKnockbackDuration; } }
        public int CurrentGroundKnockbackGravity { get { return OverrideGroundKnockbackGravity < 0 ? 0 : OverrideGroundKnockbackGravity; } }
        public int CurrentAirKnockbackGravity { get { return OverrideAirKnockbackGravity < 0 ? GlobalVariables.DefaultGravity : OverrideAirKnockbackGravity; } }
        public int CurrentCornerPushback { get { return OverrideCornerPushback == 0 ? AttackLevel.CornerPushback : OverrideCornerPushback; } }

        public int CurrentHitVFX { get { return OverrideHitVFX < 0 ? GlobalVariables.DefaultHitVFX : OverrideHitVFX; } }
        //public int CurrentBlockVFX { get { return OverrideBlockVFX < 0 ? GlobalVariables.DefaultBlockVFX : OverrideBlockVFX; } }
        //public int CurrentClashVFX { get { return OverrideClashVFX < 0 ? GlobalVariables.DefaultClashVFX : OverrideClashVFX; } }
        //public int CurrentArmorVFX { get { return OverrideArmorVFX < 0 ? GlobalVariables.DefaultArmorVFX : OverrideArmorVFX; } }
	}
}
