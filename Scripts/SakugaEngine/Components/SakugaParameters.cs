using Godot;
using SakugaEngine.Global;
using SakugaEngine.Resources;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class SakugaParameters : Node
    {
        protected SakugaActor _owner;
        [Export] public SakugaHealth Health;
        [Export] public SakugaSuperGauge SuperGauge;
        [Export] public SakugaSuperArmor SuperArmor;
        [Export] public SakugaProrations Prorations;
        [Export] public CombatTracker Tracker;
        [Export] public FrameTimer[] Timers;
        [Export] public CustomVariable[] Variables;
        [Export] public SoundQueue[] SoundSources;

        public void Initialize(SakugaActor owner)
        {
            _owner = owner;
            if (Health != null) Health.Initialize(_owner.Data);
            if (SuperGauge != null) SuperGauge.Initialize(_owner.Data);
            if (SuperArmor != null) SuperArmor.Initialize();
            if (Prorations != null) Prorations.Initialize(_owner.Data);
            if (Tracker != null) Tracker.Initialize(_owner);

            if (HasVariables())
            {
                for (int v = 0; v < Variables.Length; v++)
                    Variables[v].Initialize();
            }
        }

        public void Reset()
        {
            if (Health != null) Health.Initialize(_owner.Data);
            if (SuperGauge != null) SuperGauge.Initialize(_owner.Data);
            if (SuperArmor != null) SuperArmor.Initialize();
            if (Prorations != null) Prorations.Initialize(_owner.Data);
            if (Tracker != null) Tracker.Reset();

            if (HasTimers())
            {
                for (int t = 0; t < Timers.Length; t++)
                    Timers[t].Stop();
            }

            if (HasVariables())
            {
                for (int v = 0; v < Variables.Length; v++)
                {
                    if (!Variables[v].Reset) continue;

                    Variables[v].Initialize();
                }
            }
        }

        public void Tick()
        {
            if (SuperGauge != null) SuperGauge.Tick();

            UpdateProrations();
            UpdateVariables();
            UpdateTimers();
        }

        public void Clear()
        {
            if (Tracker != null) Tracker.Reset();
            if (Health != null) Health.UpdateLostHealth();
            if (Prorations != null) Prorations.ResetProrations();
        }

        public void TakeDamage(int damage, int superGaugeGain, bool isKilingBlow)
        {
            if (Health != null)
            {
                if (!isKilingBlow && Health.CurrentValue - damage <= 1)
                    Health.SetHealth(1);
                else
                    Health.RemoveHealth(damage);
            }
            if (SuperGauge != null)
                SuperGauge.AddSuperGauge(superGaugeGain);
            
            ChangeVariablesBehavior(CustomVariableBehaviorTarget.ON_DAMAGE);
        }

        public void HitConfirm(int superGaugeGain)
        {
            if (SuperGauge != null)
                SuperGauge.AddSuperGauge(superGaugeGain);
            
            ChangeVariablesBehavior(CustomVariableBehaviorTarget.ON_HIT);
        }

        public int CalculateProrations(int damage, int attack, int defense, ushort damageScalingLoss)
        {
            int finalDamage = damage;
            if (Prorations != null)
                finalDamage = Prorations.CalculateCompleteDamage(damage, attack, defense, damageScalingLoss);
            
            return finalDamage;
        }

        public void ArmorDamage(int damage, int healthDamage)
        {
            SuperArmor.RemoveArmor(damage);
            Health.RemoveHealth(healthDamage);
        }

        public FrameTimer GetTimer(int index) => Timers[index];
        public FrameTimer GetTimer(string name)
        {
            for (int i = 0; i < Timers.Length; i++)
            {
                if (Timers[i].Name == name)
                    return Timers[i];
            }

            return null;
        }

        public CustomVariable GetVariable(int index) => Variables[index];
        public CustomVariable GetVariable(string name)
        {
            for (int i = 0; i < Variables.Length; i++)
            {
                if (Variables[i].Name == name)
                    return Variables[i];
            }

            return null;
        }

        public void UpdateTimers()
        {
            if (!HasTimers()) return;

            for (int t = 0; t < Timers.Length; t++)
            {
                FrameTimer timer = Timers[t];
                if (timer.Manual) continue;

                timer.Run();
            }
        }

        public void UpdateVariables()
        {
            if (!HasVariables()) return;

            for (int v = 0; v < Variables.Length; v++)
            {
                switch (Variables[v].CurrentMode)
                {
                    case CustomVariableMode.IDLE:
                        break;
                    case CustomVariableMode.INCREASE:
                        Variables[v].Add(Variables[v].CurrentFactor);
                        break;
                    case CustomVariableMode.DECREASE:
                        Variables[v].Subtract(Variables[v].CurrentFactor);
                        break;
                }
            }
        }

        public void UpdateProrations()
        {
            if (Prorations == null) return;

            Prorations.UpdateDamageScaling(_owner.Body != null && _owner.Body.IsOnWall);
        }

        public void ChangeVariablesBehavior(CustomVariableBehaviorTarget CompareTarget)
        {
            if (!HasVariables()) return;

            for (int v = 0; v < Variables.Length; v++)
                Variables[v].ChangeBehavior(CompareTarget);
        }

        public void SetVariables(CustomVariableChange[] ChangeConditions)
        {
            if (ChangeConditions == null || ChangeConditions.Length <= 0) return;
            
            foreach (CustomVariableChange condition in ChangeConditions)
            {
                CustomVariable variable = null;
                if (condition.ByIndex >= 0)
                    variable = GetVariable(condition.ByIndex);
                else if (condition.ByName == "")
                    variable = GetVariable(condition.ByName);

                if (variable == null) return;
                
                variable.ChangeVariable(condition);
            }
        }

        public bool CompareVariables(CustomVariableCondition[] CompareConditions)
        {
            if (CompareConditions == null || CompareConditions.Length <= 0) return true;
            foreach (CustomVariableCondition condition in CompareConditions)
            {
                CustomVariable variable = null;
                if (condition.ByIndex >= 0)
                    variable = GetVariable(condition.ByIndex);
                else if (condition.ByName == "")
                    variable = GetVariable(condition.ByName);
                
                if (!variable.CompareVariable(condition)) return false;
            }
            return true;
        }


        public void PlaySound(int soundChannel, AudioStream sound)
        {
            if (!HasSounds()) return;

            SoundSources[soundChannel].QueueSound(sound);
        }

        public bool HasTimers() => Timers != null && Timers.Length > 0;
        public bool HasVariables() => Variables != null && Variables.Length > 0;
        public bool HasSounds() => SoundSources != null && SoundSources.Length > 0;
    }
}