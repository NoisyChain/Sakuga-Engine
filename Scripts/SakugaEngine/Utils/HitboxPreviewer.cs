using Godot;
using SakugaEngine.Global;
using SakugaEngine.Resources;

namespace SakugaEngine.Utils
{
    [Tool] [GlobalClass]
    public partial class HitboxPreviewer : Node3D
    {
        [Export] private Sprite3D[] hitboxGraphics;
        [Export] public AnimationViewer Animator;
        [Export] public AnimationData Data;
        [Export] public bool AutoRun;
        [Export(PropertyHint.Range, "0, 99999")] public int Frame;
        
        public override void _Process(double delta)
        {
            if (!Engine.IsEditorHint()) return;
            if (Animator == null || Data == null)
            {
                for(int j = 0; j < hitboxGraphics.Length; j++)
                    hitboxGraphics[j].Visible = false;
                return;
            }

            if (AutoRun)
            {
                Frame++;
                if (Frame >= Data.Duration) Frame = 0;
            }

            Animator.ViewAnimations(GetCurrentAnimationSettings(), Frame);

            var hitboxData = GetCurrentHitboxSettings();
            PreviewHitboxes(hitboxData);
            PreviewPushbox(hitboxData);
        }

        public void PreviewHitboxes(HitboxState previewData)
        {
            for(int j = 0; j < hitboxGraphics.Length; j++)
            {
                if (previewData == null || previewData.HitboxData == null || 
                    previewData.HitboxData.Hitboxes == null || previewData.HitboxData.Hitboxes.Length == 0 || 
                    j >= previewData.HitboxData.Hitboxes.Length || previewData.HitboxData.Hitboxes[j] == null)
                    {
                        hitboxGraphics[j].Hide();
                        continue;
                    }

                hitboxGraphics[j].Visible = previewData.HitboxData.Hitboxes[j].Size != Vector2I.Zero;

                switch (previewData.HitboxData.Hitboxes[j].HitboxType)
                {
                    case HitboxType.HURTBOX:
                        hitboxGraphics[j].SortingOffset = 1;
                        hitboxGraphics[j].Modulate = new Color(0.0f, 1.0f, 0.0f);
                        break;
                    case HitboxType.HITBOX:
                        hitboxGraphics[j].SortingOffset = 2;
                        hitboxGraphics[j].Modulate = new Color(1.0f, 0.0f, 0.0f);
                        break;
                    case HitboxType.PROXIMITY_BLOCK:
                        hitboxGraphics[j].SortingOffset = 4;
                        hitboxGraphics[j].Modulate = new Color(1.0f, 0.0f, 1.0f);
                        break;
                    case HitboxType.PROJECTILE:
                        hitboxGraphics[j].SortingOffset = 2;
                        hitboxGraphics[j].Modulate = new Color(1.0f, 0.64f, 0.0f);
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
                hitboxGraphics[j].GlobalPosition = GlobalFunctions.ToScaledVector3(previewData.HitboxData.Hitboxes[j].Center);
                hitboxGraphics[j].Scale = GlobalFunctions.ToScaledVector3(previewData.HitboxData.Hitboxes[j].Size, 1f);
            }
        }

        public void PreviewPushbox(HitboxState previewData)
        {
            int collisionViewer = hitboxGraphics.Length - 1;
            if (previewData == null || previewData.HitboxData == null)
            {
                hitboxGraphics[collisionViewer].Visible = false;
                return;
            }
            
            hitboxGraphics[collisionViewer].Visible = previewData.HitboxData.Pushbox.Size != Vector2I.Zero;
            hitboxGraphics[collisionViewer].SortingOffset = 3;
            hitboxGraphics[collisionViewer].Modulate = new Color(1.0f, 1.0f, 0.0f);
            hitboxGraphics[collisionViewer].GlobalPosition = GlobalFunctions.ToScaledVector3(previewData.HitboxData.Pushbox.Center);
            hitboxGraphics[collisionViewer].Scale = GlobalFunctions.ToScaledVector3(previewData.HitboxData.Pushbox.Size, 1f);
        }

        public AnimationSettings GetCurrentAnimationSettings()
        {
            if (Data == null) return null;
            if (Data.Animations == null || Data.Animations.Length <= 0) return null;
            if (Data.Animations.Length == 1)
                return Data.Animations[0];

            int anim = 0;

            for (int i = 0; i < Data.Animations.Length; i++)
            {
                if (Data.Animations[i] == null) continue;
                int nextFrame = (i >= Data.Animations.Length - 1 || Data.Animations[i + 1] == null) ?
                                Data.Duration - 1 :
                                Data.Animations[i + 1].AtFrame - 1;
                
                if (Frame >= Data.Animations[i].AtFrame && Frame <= nextFrame)
                    anim = i;
            }

            return Data.Animations[anim];
        }

        public HitboxState GetCurrentHitboxSettings()
        {
            if (Data == null) return null;
            if (Data.Hitboxes == null || Data.Hitboxes.Length <= 0) return null;
            if (Data.Hitboxes.Length == 1)
                return Data.Hitboxes[0];

            int anim = 0;

            for (int i = 0; i < Data.Hitboxes.Length; i++)
            {
                int nextFrame = (i >= Data.Hitboxes.Length - 1) ?
                                int.MaxValue :
                                Data.Hitboxes[i + 1].AtFrame - 1;
                
                if (Frame >= Data.Hitboxes[i].AtFrame && Frame <= nextFrame)
                    anim = i;
            }

            return Data.Hitboxes[anim];
        }
    }
}
