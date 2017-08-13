using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator
{
    class SimpleSimulator : RandomSimulator
    {
        public double GetDamageOfDoT(StatsGroup stats, int ticks)
        {
            double totalDamage = 0;
            for (int i = 0; i < ticks; ++i)
            {
                totalDamage += GetDamageOfDoTTick(stats);
            }
            return totalDamage / ticks;
        }

        public double Simulate(AttackPattern toSimulate, StatsGroup stats, double duration)
        {
            SimulatedAttackPattern simulated = new SimulatedAttackPattern(toSimulate, stats, this);
            return simulated.SimulateTotalPotencyOver(duration);
        }

        public static double MonteCarlo(AttackPattern toSimulate, StatsGroup stats, double duration, int count)
        {
            double total = 0;
            double[] results = new double[count];
            Parallel.For(0, count, i =>
            {
                SimpleSimulator sim = new SimpleSimulator();
                results[i] = sim.Simulate(toSimulate, stats, duration);
            });
            for(int i = 0; i < count; ++i)
            {
                //results[i] = Simulate(toSimulate, stats, duration);
                total += results[i];
            }
            return total / count;
        }

        private class SimulatedAttackPattern
        {
            AttackPattern ToSimulate;
            SimpleSimulator Sim;
            StatsGroup Stats;

            public SimulatedAttackPattern(AttackPattern toSimulate, StatsGroup stats, SimpleSimulator sim)
            {
                this.ToSimulate = toSimulate;
                this.Stats = stats;
                this.Sim = sim;
            }

            public double SimulateTotalPotencyOver(double duration)
            {
                Dictionary<AttackPattern.DamageAbility, double> lastAbilityTimes = new Dictionary<AttackPattern.DamageAbility, double>();
                Dictionary<AttackPattern.DamageOverTimeSkill, double> lastDoTTimes = new Dictionary<AttackPattern.DamageOverTimeSkill, double>();

                foreach (var dot in ToSimulate.DoTSkills)
                {
                    lastDoTTimes[dot] = double.MinValue;
                }
                foreach (var ability in ToSimulate.DamageAbilities)
                {
                    lastAbilityTimes[ability] = double.MinValue;
                }

                double currentTime = 0;
                double totalPotency = 0;
                int hits = 0;
                int directHits = 0;
                int crits = 0;
                while(currentTime < duration)
                {
                    double timeUsed = 0;
                    double attackPotency = 0;
                    // Use all our oGCDs
                    foreach(var ability in ToSimulate.DamageAbilities)
                    {
                        if(lastAbilityTimes[ability] + ability.Recast < currentTime)
                        {
                            lastAbilityTimes[ability] = currentTime;
                            attackPotency += ability.Potency;
                            // Fudge here - most abilities have a cast time of 0, but RDM melee combo is an "ability" with a cast time
                            // Abilities need to ignore speed stat for that to work.
                            timeUsed = ability.CastTime * RatingConversions.GetSpeedFactor(Stats.SpeedRating);
                            
                        }
                    }
                    
                    // Refresh our DoTs in priority order
                    foreach(var dot in ToSimulate.DoTSkills)
                    {
                        if(timeUsed == 0 && lastDoTTimes[dot] + dot.Duration < currentTime)
                        {
                            lastDoTTimes[dot] = currentTime;
                            attackPotency += dot.TotalPotency * Sim.GetDamageOfDoT(Stats, dot.TickCount);
                            timeUsed += dot.CastTime;
                        }
                    }

                    // If we didn't need to refresh a DoT, use the GCD for damage.
                    if(timeUsed == 0)
                    {
                        bool isCrit, isDirect;
                        timeUsed += 2.5;
                        attackPotency += ToSimulate.GcdPotency * Sim.GetDamageOfGCDAttack(Stats, out isCrit, out isDirect);
                        hits++;
                        if (isCrit) crits++;
                        if (isDirect) directHits++;
                    }

                    totalPotency += attackPotency;
                    currentTime += timeUsed / RatingConversions.GetSpeedFactor(Stats.SpeedRating);
                }
                //Console.WriteLine($"{(double)(crits) / hits * 100:0.00}% Observed Crit Rate");
                //Console.WriteLine($"{(double)(directHits) / hits * 100:0.00}% Observed DH Rate");
                return totalPotency;
            }
        }
    }
}
