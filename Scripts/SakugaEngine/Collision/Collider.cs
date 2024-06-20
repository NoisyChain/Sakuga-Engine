using Godot;

namespace SakugaEngine.Collision
{
    public partial class Collider
    {
        [Export] public Vector2I Center;
        [Export] public Vector2I Size;
        public bool Active => Size != Vector2I.Zero;

        public Collider() {}

        public bool IsOverlapping(Collider other)
        {
            if (!Active) return false;
            if (!other.Active) return false;

            bool collisionX = Center.X - (Size.X / 2) <= other.Center.X + (other.Size.X / 2) &&
                    Center.X + (Size.X / 2) >= other.Center.X - (other.Size.X / 2);

            bool collisionY = Center.Y - (Size.Y / 2) <= other.Center.Y + (other.Size.Y / 2) &&
                Center.Y + (Size.Y / 2) >= other.Center.Y - (other.Size.Y / 2);

            return collisionX && collisionY;
        }

        public void UpdateCollider(Vector2I center, Vector2I size)
        {
            Center = center;
            Size = size;
        }
    }
}
