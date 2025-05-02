using Godot;
using SakugaEngine.Resources;

namespace SakugaEngine
{
    public partial interface IDamage
    {
        void BaseDamage(SakugaActor target, HitboxElement box, Vector2I contact);
        void HitTrade(HitboxElement box, Vector2I contact);
        void ThrowDamage(SakugaActor target, HitboxElement box, Vector2I contact);
        void ThrowTrade();
        void HitboxClash(HitboxElement box, Vector2I contact);
        void ProjectileClash(HitboxElement box, Vector2I contact);
        void ProjectileDeflect(SakugaActor target, HitboxElement box, Vector2I contact);
        void CounterHit(SakugaActor target, HitboxElement box, Vector2I contact);
        //void ParryHit(SakugaActor target, HitboxElement box, Vector2I contact);
        void ProximityBlock(HitboxElement box);
        void OnHitboxExit();
    }
}