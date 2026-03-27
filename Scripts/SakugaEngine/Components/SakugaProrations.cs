using Godot;
using SakugaEngine.Global;
using SakugaEngine.Resources;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class SakugaProrations : Node
    {
        public ushort BaseAttack;
        public ushort BaseDefense;

        public ushort CurrentAttack;
        public ushort CurrentDefense;

        public ushort CurrentDamageScaling;
        public ushort CurrentBaseDamageScaling;
        public ushort CurrentCornerDamageScaling;

        public ushort CurrentSameMoveProration;
        public ushort CurrentGravityProration;
        public ushort CurrentHitstunProration;

        public byte GravityDecayFactor = 0;
        public byte HitstunDecayFactor = 0;

        public ushort SelectDamageScaling(bool condition) => condition ? CurrentCornerDamageScaling : CurrentBaseDamageScaling;

        public void Initialize(DataContainer data)
        {
            BaseAttack = data.BaseAttack;
            BaseDefense = data.BaseDefense;

            ResetProrations();
        }

        public void UpdateDamageScaling(bool condition)
        {
            CurrentDamageScaling = SelectDamageScaling(condition);
        }

        public void RemoveDamageScaling(ushort value)
        {
            if (CurrentBaseDamageScaling - value < GlobalVariables.BaseMinDamageScaling)
                CurrentBaseDamageScaling = GlobalVariables.BaseMinDamageScaling;
            else CurrentBaseDamageScaling -= value;

            if (CurrentCornerDamageScaling - value < GlobalVariables.CornerMinDamageScaling)
                CurrentCornerDamageScaling = GlobalVariables.CornerMinDamageScaling;
            else CurrentCornerDamageScaling -= value;
        }

        public void ResetDamageScaling()
        {
            CurrentBaseDamageScaling = GlobalVariables.BaseMaxDamageScaling;
            CurrentCornerDamageScaling = GlobalVariables.CornerMaxDamageScaling;
        }

        public void ResetProrations()
        {
            CurrentAttack = BaseAttack;
            CurrentDefense = BaseDefense;

            ResetDamageScaling();

            CurrentSameMoveProration = 100;
            CurrentGravityProration = 100;
            CurrentHitstunProration = 100;

            GravityDecayFactor = 0;
            HitstunDecayFactor = 0;
        }

        public int CalculateCompleteDamage(int damage, int attackValue, int defenseValue, ushort damageScalingLoss)
        {
            var damageFactor = attackValue + (defenseValue - 100);
            var scaledDamage = (damage * CurrentDamageScaling) / 100;
            var finalDamage = (scaledDamage * damageFactor) / 100;
            if (finalDamage > 0) RemoveDamageScaling(damageScalingLoss);
            return finalDamage;
        }
    }
}