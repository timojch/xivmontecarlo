using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator.Simulation
{
    public class Battle
    {
        public ISkillSimulator Simulator { get; private set; }
        public double CurrentTime { get; private set; }
        public double BattleDuration { get; private set; }
        public double TotalPotency { get; private set; }

        public double Latency { get; set; } = 0.0;

        public event Action<string> Logger;

        private Dictionary<EffectType, EffectInstance> effects = new Dictionary<EffectType, EffectInstance>();

        public Dictionary<string, double> PotencyBySource { get; private set; } = new Dictionary<string, double>();
        
        public Battle(ISkillSimulator simulator, double duration)
        {
            this.Simulator = simulator;
            this.BattleDuration = duration;
        }

        public bool IsOver { get { return CurrentTime >= BattleDuration; } }

        public void Simulate(WarriorOfLight hero)
        {
            while(!IsOver)
            {
                BattleAction heroAction = hero.ChooseBattleAction();
                if (heroAction != null)
                {
                    if(heroAction.SkillUsed != null)
                    {
                        if(heroAction.SkillUsed.IsGCD)
                        {
                            AdvanceTime(Latency);
                        }
                        //heroAction.Duration = Math.Max(heroAction.Duration, Skill.MinimumTime);
                    }
                    AdvanceTime(heroAction.Duration);
                    if (!IsOver)
                    {
                        if (heroAction.SkillUsed != null)
                        {
                            HeroUsesSkill(heroAction.SkillUsed, hero.Stats);
                        }
                        if (!heroAction.Trivial)
                        {
                            Log(heroAction.Description);
                        }
                    }
                }
            }
            foreach(var effect in effects.Keys.ToList())
            {
                if (GetRemainingEffectDuration(effect) <= 0)
                {
                    ExpireEffect(effect);
                }
            }
        }

        public double GetRemainingEffectDuration(EffectType effect)
        {
            return GetEffectDurationAt(effect, this.CurrentTime);
        }

        public void ApplyEffect(EffectType effect, StatsGroup withStats)
        {
            ExpireEffect(effect);
            effects[effect] = new EffectInstance
            {
                Type = effect,
                Duration = effect.Duration,
                EffectsSnapshot = effects.Keys.ToList(),
                StartTime = CurrentTime,
                StatsSnapshot = effect.DotPotency == 0 ? null : withStats.Copy()
            };
        }

        public void ExpireEffect(EffectType effect)
        {
            if (effects.ContainsKey(effect))
            {
                var instance = effects[effect];
                double durationRemaining = GetRemainingEffectDuration(effect);

                if (effect.DotPotency > 0)
                {
                    int ticks = GetDoTTickCount(instance, this.CurrentTime);
                    double dotTotalPotency = 0;
                    for (int i = 0; i < ticks; ++i)
                    {
                        double potency = effect.DotPotency * Simulator.GetDamageOfDoTTick(instance.StatsSnapshot);
                        DealPotency(potency, $"{effect.Name} Tick", instance.EffectsSnapshot);
                        dotTotalPotency += potency;
                    }
                    Log($"{effect.Name} expired with {durationRemaining:#0.0} remaining ({ticks} ticks and {dotTotalPotency:#.0} potency)");
                }
                else
                {
                    //Log($"{effect.Name} expired with {durationRemaining:#0.0})");
                }

                effect.Expire(this, instance);
                effects.Remove(effect);
            }
        }

        private void AdvanceTime(double time)
        {
            if (time < 0) throw new ArgumentException("No matter how much speed rating you stack, only Alexander can travel back in time.");
            CurrentTime += time;
            foreach (var effect in effects.Keys.ToList())
            {
                if (GetRemainingEffectDuration(effect) <= 0)
                {
                    double store = CurrentTime;
                    CurrentTime = effects[effect].StartTime + effect.Duration;
                    ExpireEffect(effect);
                    CurrentTime = store;
                }
            }
        }

        private void HeroUsesSkill(Skill skill, StatsGroup currentStats)
        {
            foreach (var effect in skill.AppliesEffects)
            {
                ApplyEffect(effect, currentStats);
            }
            if (skill.Potency > 0)
            {
                DealPotency(Simulator.GetDamageOfGCDAttack(currentStats) * skill.Potency, skill.Name, effects.Keys);
            }
        }

        private double GetEffectDurationAt(EffectType effect, double time)
        {
            if (effects.ContainsKey(effect))
            {
                var instance = effects[effect];
                return Math.Max(0, instance.Duration - (time - instance.StartTime));
            }
            else
            {
                return 0;
            }
        }

        private double EvaluateDamageMultipliers(IEnumerable<EffectType> effectsSnapshot)
        {
            double mult = 1;
            foreach(var effect in effectsSnapshot)
            {
                mult *= effect.DamageMultiplier;
            }
            Debug.Assert(mult >= 1);
            return mult;
        }

        /// <summary>
        /// Evaluates the number of times a DoT will have ticked by a given time
        /// </summary>
        /// <param name="dot">The DoT</param
        /// <param name="endTime">The time to check</param>
        /// <returns></returns>
        private int GetDoTTickCount(EffectInstance dot, double endTime)
        {
            double durationUsed = dot.Duration - GetEffectDurationAt(dot.Type, endTime);

            return (int)Math.Floor((durationUsed + 0.5) / 3);
        }

        private void DealPotency(double amount, string source, IEnumerable<EffectType> effectsSnapshot)
        {
            amount *= EvaluateDamageMultipliers(effectsSnapshot);
            if(PotencyBySource.ContainsKey(source))
            {
                PotencyBySource[source] += amount;
            }
            else
            {
                PotencyBySource[source] = amount;
            }
            TotalPotency += amount;
        }

        private void Log(string format, params string[] args)
        {
            string timeStamp = $"{CurrentTime:#000.0}";
            string message = string.Format(format, args);
            Logger?.Invoke($"{timeStamp}: {message}");
        }
    }
}
