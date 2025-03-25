using Godot;
using SakugaEngine.Collision;

namespace SakugaEngine.Utils
{
    public partial class HitboxViewer : Node3D
    {
        [Export] private Sprite3D[] hitboxGraphics;
        [Export] private PhysicsBody body;

        public override void _Process(double delta)
        {
            if (body == null) return;

            Visible = Global.ShowHitboxes;

            UpdateHitboxes();
            UpdatePushbox();
        }

        public void UpdateHitboxes()
        {
            for(int j = 0; j < hitboxGraphics.Length; j++)
            {
                if (body.CurrentHitbox < 0 || j >= body.GetCurrentHitbox().Hitboxes.Length)
                    hitboxGraphics[j].Hide();
                else
                {
                    hitboxGraphics[j].Visible = body.Hitboxes[j].Active;

                    switch (body.GetCurrentHitbox().Hitboxes[j].HitboxType)
                    {
                        case Global.HitboxType.HURTBOX:
                            hitboxGraphics[j].SortingOffset = 1;
                            hitboxGraphics[j].Modulate = new Color(0.0f, 1.0f, 0.0f);
                            break;
                        case Global.HitboxType.HITBOX:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(1.0f, 0.0f, 0.0f);
                            break;
                        case Global.HitboxType.PROJECTILE:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(1.0f, 0.64f, 0.0f);
                            break;
                        case Global.HitboxType.PROXIMITY_BLOCK:
                            hitboxGraphics[j].SortingOffset = 4;
                            hitboxGraphics[j].Modulate = new Color(1.0f, 0.0f, 1.0f);
                            break;
                        case Global.HitboxType.THROW:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(0.0f, 0.0f, 1.0f);
                            break;
                        case Global.HitboxType.COUNTER:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(0.5f, 0.5f, 0.5f);
                            break;
                        case Global.HitboxType.DEFLECT:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(1.0f, 0.0f, 0.5f);
                            break;
                        /*case Global.HitboxType.PARRY:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(0.5f, 0.5f, 0.5f);
                            break;*/
                    }
                    hitboxGraphics[j].GlobalPosition = Global.ToScaledVector3(body.Hitboxes[j].Center);
                    hitboxGraphics[j].Scale = Global.ToScaledVector3(body.Hitboxes[j].Size, 1f);
                }
            }
        }

        public void UpdatePushbox()
        {
            int collisionViewer = hitboxGraphics.Length - 1;
            hitboxGraphics[collisionViewer].Visible = body.Pushbox.Active;
            hitboxGraphics[collisionViewer].SortingOffset = 3;
            hitboxGraphics[collisionViewer].Modulate = new Color(1.0f, 1.0f, 0.0f);
            hitboxGraphics[collisionViewer].GlobalPosition = Global.ToScaledVector3(body.Pushbox.Center);
            hitboxGraphics[collisionViewer].Scale = Global.ToScaledVector3(body.Pushbox.Size, 1f);
        }
    }
}
