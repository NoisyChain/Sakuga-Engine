using Godot;
using SakugaEngine.Resources;

namespace SakugaEngine
{
    public partial interface IDamage
    {
        void BaseDamage(HitboxElement box, Vector2I contact);
        void HitConfirmReaction(HitboxElement box, Vector2I contact);
        void ThrowDamage(HitboxElement box, Vector2I contact);
        void ProjectileDamage(HitboxElement box, Vector2I contact, int priority);
        void HitboxClash(HitboxElement box, Vector2I contact);
        void ProjectileClash(HitboxElement box, Vector2I contact);
        void ProjectileDeflect(HitboxElement box, Vector2I contact);
        void CounterHit(HitboxElement box, Vector2I contact);
        void ProximityBlock();
        void OnHitboxExit();
    }
}