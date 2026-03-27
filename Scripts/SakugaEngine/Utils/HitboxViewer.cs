using Godot;
using SakugaEngine.Collision;
using SakugaEngine.Game;
using SakugaEngine.Global;

namespace SakugaEngine.Utils
{
    public partial class HitboxViewer : Node3D
    {
        [Export] private Sprite3D[] hitboxGraphics;
        [Export] private PhysicsBody body;

        public override void _Process(double delta)
        {
            if (body == null) return;

            Visible = GameManager.Instance != null && GameManager.Instance.ShowHitboxes;
            GlobalPosition = GlobalFunctions.ToScaledVector3(body.FixedPosition);
            GlobalRotation = Vector3.Zero;

            UpdateHitboxes();
            UpdatePushbox();
        }

        public void UpdateHitboxes()
        {
            for(int j = 0; j < hitboxGraphics.Length; j++)
            {
                if (body.CurrentHitbox == null || body.CurrentHitbox.HitboxData == null || body.CurrentHitbox.HitboxData.Hitboxes == null || j >= body.CurrentHitbox.HitboxData.Hitboxes.Length)
                    hitboxGraphics[j].Hide();
                else
                {
                    hitboxGraphics[j].Visible = body.Hitboxes[j].Active;

                    switch (body.CurrentHitbox.HitboxData.Hitboxes[j].HitboxType)
                    {
                        case HitboxType.HURTBOX:
                            hitboxGraphics[j].SortingOffset = 1;
                            hitboxGraphics[j].Modulate = new Color(0.0f, 1.0f, 0.0f);
                            break;
                        case HitboxType.HITBOX:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(1.0f, 0.0f, 0.0f);
                            break;
                        case HitboxType.PROJECTILE:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(1.0f, 0.64f, 0.0f);
                            break;
                        case HitboxType.PROXIMITY_BLOCK:
                            hitboxGraphics[j].SortingOffset = 4;
                            hitboxGraphics[j].Modulate = new Color(1.0f, 0.0f, 1.0f);
                            break;
                        case HitboxType.THROW:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(0.0f, 0.0f, 1.0f);
                            break;
                        case HitboxType.COUNTER:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(0.5f, 0.5f, 0.5f);
                            break;
                        case HitboxType.DEFLECT:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(1.0f, 0.0f, 0.5f);
                            break;
                        /*case Global.HitboxType.PARRY:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(0.5f, 0.5f, 0.5f);
                            break;*/
                    }
                    hitboxGraphics[j].GlobalPosition = GlobalFunctions.ToScaledVector3(body.Hitboxes[j].Center);
                    hitboxGraphics[j].Scale = GlobalFunctions.ToScaledVector3(body.Hitboxes[j].Size, 1f);
                }
            }
        }

        public void UpdatePushbox()
        {
            int collisionViewer = hitboxGraphics.Length - 1;
            hitboxGraphics[collisionViewer].Visible = body.Pushbox.Active;
            hitboxGraphics[collisionViewer].SortingOffset = 3;
            hitboxGraphics[collisionViewer].Modulate = new Color(1.0f, 1.0f, 0.0f);
            hitboxGraphics[collisionViewer].GlobalPosition = GlobalFunctions.ToScaledVector3(body.Pushbox.Center);
            hitboxGraphics[collisionViewer].Scale = GlobalFunctions.ToScaledVector3(body.Pushbox.Size, 1f);
        }
    }
}
