using Godot;
using SakugaEngine.Resources;

namespace SakugaEngine
{
    public partial interface IDamage
    {
        void BaseDamage(HitboxElement box, Vector2I contact);
        void ThrowDamage(HitboxElement box);
        void ProjectileDamage(HitboxElement box, Vector2I contact, int priority);
        void HitboxClash(HitboxElement box, Vector2I contact, int priority);
        void ProjectileDeflect(HitboxElement box);
        void CounterHit(HitboxElement box, Vector2I contact);
        void ProximityBlock();
        void OnHitboxExit();
    }
}