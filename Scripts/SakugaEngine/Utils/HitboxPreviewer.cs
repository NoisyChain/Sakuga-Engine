using Godot;
using SakugaEngine.Resources;

namespace SakugaEngine.Utils
{
    [Tool]
    public partial class HitboxPreviewer : Node3D
    {
        [Export] private Sprite3D[] hitboxGraphics;
        [Export] public HitboxSettings previewSettings;
        
        public override void _Process(double delta)
        {
            if (!Engine.IsEditorHint()) return;
            if (previewSettings == null)
            {
                for(int j = 0; j < hitboxGraphics.Length; j++)
                    hitboxGraphics[j].Visible = false;
                return;
            }

            PreviewHitboxes();
            PreviewPushbox();
        }

        public void PreviewHitboxes()
        {
            for(int j = 0; j < hitboxGraphics.Length; j++)
            {
                if (j >= previewSettings.Hitboxes.Length)
                    hitboxGraphics[j].Hide();
                else
                {
                    hitboxGraphics[j].Visible = previewSettings.Hitboxes[j].Size != Vector2I.Zero;

                    switch (previewSettings.Hitboxes[j].HitboxType)
                    {
                        case Global.HitboxType.HURTBOX:
                            hitboxGraphics[j].SortingOffset = 1;
                            hitboxGraphics[j].Modulate = new Color(0.0f, 1.0f, 0.0f);
                            break;
                        case Global.HitboxType.HITBOX:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(1.0f, 0.0f, 0.0f);
                            break;
                        case Global.HitboxType.PROXIMITY_BLOCK:
                            hitboxGraphics[j].SortingOffset = 4;
                            hitboxGraphics[j].Modulate = new Color(1.0f, 0.0f, 1.0f);
                            break;
                        case Global.HitboxType.PROJECTILE:
                            hitboxGraphics[j].SortingOffset = 2;
                            hitboxGraphics[j].Modulate = new Color(1.0f, 0.64f, 0.0f);
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
                    hitboxGraphics[j].GlobalPosition = Global.ToScaledVector3(previewSettings.Hitboxes[j].Center);
                    hitboxGraphics[j].Scale = Global.ToScaledVector3(previewSettings.Hitboxes[j].Size, 1f);
                }
            }
        }

        public void PreviewPushbox()
        {
            int collisionViewer = hitboxGraphics.Length - 1;
            hitboxGraphics[collisionViewer].Visible = previewSettings.PushboxSize != Vector2I.Zero;
            hitboxGraphics[collisionViewer].SortingOffset = 3;
            hitboxGraphics[collisionViewer].Modulate = new Color(1.0f, 1.0f, 0.0f);
            hitboxGraphics[collisionViewer].GlobalPosition = Global.ToScaledVector3(previewSettings.PushboxCenter);
            hitboxGraphics[collisionViewer].Scale = Global.ToScaledVector3(previewSettings.PushboxSize, 1f);
        }
    }
}
