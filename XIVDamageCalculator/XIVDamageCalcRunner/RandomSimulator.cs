using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator
{
    public class RandomSimulator : ISkillSimulator
    {
        protected Random rng = new Random();

        public int TotalCrits { get; set; }
        public int TotalDirectHits { get; set; }
        public int TotalAttacks { get; set; }

        public double GetDamageOfGCDAttack(StatsGroup stats)
        {
            bool isCrit;
            bool isDirectHit;
            return GetDamageOfGCDAttack(stats, out isCrit, out isDirectHit);
        }

        public double GetDamageOfGCDAttack(StatsGroup stats, out bool isCrit, out bool isDirectHit)
        {
            double critRate = RatingConversions.GetCritChance(stats.CritRating);
            double critMult = RatingConversions.GetCritDamageMultiplier(stats.CritRating);
            double directHitRate = RatingConversions.GetDirectHitChance(stats.DirectHitRating);
            double determinationFactor = RatingConversions.GetDeterminationFactor(stats.Determination);
            double tenacityFactor = RatingConversions.GetTenacityFactor(stats.Tenacity);

            isCrit = (rng.NextDouble() < critRate);
            isDirectHit = (rng.NextDouble() < directHitRate);

            double damage = 1;
            if (isCrit) damage *= critMult;
            if (isDirectHit) damage *= RatingConversions.GetDirectHitDamageFactor(stats.DirectHitRating);
            damage *= determinationFactor;
            damage *= tenacityFactor;

            TotalCrits += isCrit ? 1 : 0;
            TotalDirectHits += isDirectHit ? 1 : 0;
            TotalAttacks++;

            return damage;
        }

        public double GetDamageOfDoTTick(StatsGroup stats)
        {
            double critRate = RatingConversions.GetCritChance(stats.CritRating);
            double critMult = RatingConversions.GetCritDamageMultiplier(stats.CritRating);
            double directHitRate = RatingConversions.GetDirectHitChance(stats.DirectHitRating);
            double directHitMult = RatingConversions.GetDirectHitDamageFactor(stats.DirectHitRating);
            double determinationFactor = RatingConversions.GetDeterminationFactor(stats.Determination);
            double tenacityFactor = RatingConversions.GetTenacityFactor(stats.Tenacity);
            double speedFactor = RatingConversions.GetSpeedDoTContribution(stats.SpeedRating);

            bool isCrit = (rng.NextDouble() < critRate);
            bool isDirectHit = (rng.NextDouble() < directHitRate);

            double damage = 1;
            if (isCrit) damage *= critMult;
            if (isDirectHit) damage *= directHitMult;
            damage *= determinationFactor;
            damage *= tenacityFactor;
            damage *= speedFactor;
            return damage;
        }

        public double NextDouble()
        {
            return rng.NextDouble();
        }
    }
}
