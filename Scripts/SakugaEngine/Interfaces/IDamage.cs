using Godot;
using SakugaEngine.Resources;
using SakugaEngine.Global;

namespace SakugaEngine
{
    public partial interface IDamage
    {
        void BaseDamage(SakugaActor target, AttackProfile profile, Vector2I contact);
        void HitTrade(SakugaActor target, AttackProfile profile, Vector2I contact);
        void ThrowDamage(SakugaActor target, AttackProfile profile, Vector2I contact);
        void ThrowTrade();
        void HitboxClash(SakugaActor target, AttackProfile profile, Vector2I contact);
        void ProjectileClash(SakugaActor target, AttackProfile profile, Vector2I contact);
        void ProjectileDeflect(SakugaActor target, AttackProfile profile, Vector2I contact);
        void CounterHit(SakugaActor target, AttackProfile profile, Vector2I contact);
        //void ParryHit(SakugaActor target, HitboxElement box, Vector2I contact);
        void ProximityBlock(HitType type = HitType.MEDIUM);
        void OnHitboxExit();
    }
}