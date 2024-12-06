using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass] [Tool]
    public partial class HitboxElement : Resource
    {
        // Hitbox settings
        [ExportGroup("Box Settings")]
        [Export] public Vector2I Center = Vector2I.Zero;
        [Export] public Vector2I Size = Vector2I.Zero;
        [Export] public Global.HitboxType HitboxType;
        [Export] public Global.HitType HitType;

        // Damage settings
        [ExportGroup("Damage Settings")]
        [Export] public int BaseDamage = 150;
        [Export] public int ChipDamage;
        [Export] public bool ChipDeath;
        [Export] public bool KillingBlow = true;
        [Export] public bool GuardCrush;
        [Export] public int SelfMeterGain = 25;
        [Export] public int OpponentMeterGain = 10;
        [Export] public int ArmorDamage = 1;
        [Export] public int Priority = 0;
        [Export] public int HitStopDuration = 12;
        [Export] public int BlockStopDuration = 12;
        [Export] public int CounterHitStopDuration = 20;
        [Export] public int ClashHitStopDuration = 20;
        [Export] public int DamageScalingSubtract = 15;
        
        //VFX settings
        [ExportGroup("VFX Settings")]
        [Export] public int HitEffectIndex = 0;
        [Export] public int BlockEffectIndex = 1;
        [Export] public int ClashEffectIndex = 2;
        [Export] public int ArmorHitEffectIndex = 3;

        //Ground Hitstun settings
        [ExportGroup("Ground Hit Settings")]
        [Export] public Global.HitstunType GroundHitstunType;
        [Export] public int GroundHitReaction;
        [Export] public int CrouchHitReaction;
        [Export] public Vector2I GroundHitKnockback;
         [Export] public int GroundHitKnockbackGravity = 0;
        [Export] public int GroundHitKnockbackTime = 8;
        [Export] public int GroundHitstun = 8;
        [Export] public Vector2I GroundBlockKnockback;
        [Export] public int GroundBlockKnockbackGravity = 0;
        [Export] public int GroundBlockKnockbackTime = 8;
        [Export] public int GroundBlockstun = 8;

        //Air Hitstun settings
        [ExportGroup("Air Hit Settings")]
        [Export] public Global.HitstunType AirHitstunType;
        [Export] public int AirHitReaction;
        [Export] public Vector2I AirHitKnockback;
        [Export] public int AirHitKnockbackGravity = 300000;
        [Export] public int AirHitKnockbackTime = 8;
        [Export] public int AirHitstun = 8;
        [Export] public Vector2I AirBlockKnockback;
        [Export] public int AirBlockKnockbackGravity = 300000;
        [Export] public int AirBlockKnockbackTime = 8;
        [Export] public int AirBlockstun = 8;
        

        //Throw settings
        [ExportGroup("Throw Hit Settings")]
        [Export] public bool GroundThrow;
        [Export] public bool AirThrow;
        [Export] public int ThrowHitReaction = -1;

        //Pushback settings
        [ExportGroup("Pushback Settings")]
        [Export] public bool AllowSelfPushback = true;
        [Export] public int SelfPushbackForce;
        [Export] public int SelfPushbackDuration;

        //Extra settings
        [ExportGroup("Extra Settings")]
        [Export] public int HitConfirmState = -1;
        [Export] public bool AllowInertia = true;
        [Export] public int BounceXTime = 0;
        [Export] public int BounceXIntensity = 100;
        [Export] public int BounceXState = -1;
        [Export] public int BounceYTime = 0;
        [Export] public int BounceYIntensity = 100;
        [Export] public int BounceYState = -1;
    }
}
