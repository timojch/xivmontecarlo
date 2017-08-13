using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator
{
    public class DeterministicSimulator : ISkillSimulator
    {
        public double GetDamageOfDoTTick(StatsGroup stats)
        {
            double critRate = RatingConversions.GetCritChance(stats.CritRating);
            double critMult = RatingConversions.GetCritDamageMultiplier(stats.CritRating);
            double directHitRate = RatingConversions.GetDirectHitChance(stats.DirectHitRating);
            double speedFactor = RatingConversions.GetSpeedDoTContribution(stats.SpeedRating);
            double determinationFactor = RatingConversions.GetDeterminationFactor(stats.Determination);
            double tenacityFactor = RatingConversions.GetTenacityFactor(stats.Tenacity);

            double critFactor = (1 - critRate) + (critRate * critMult);
            double directHitFactor = (1 - directHitRate) + (directHitRate * 1.25);

            return critFactor * directHitFactor * determinationFactor * tenacityFactor * speedFactor;
        }

        public double GetDamageOfGCDAttack(StatsGroup stats)
        {
            double critRate = RatingConversions.GetCritChance(stats.CritRating);
            double critMult = RatingConversions.GetCritDamageMultiplier(stats.CritRating);
            double directHitRate = RatingConversions.GetDirectHitChance(stats.DirectHitRating);
            double determinationFactor = RatingConversions.GetDeterminationFactor(stats.Determination);
            double tenacityFactor = RatingConversions.GetTenacityFactor(stats.Tenacity);

            double critFactor = (1 - critRate) + (critRate * critMult);
            double directHitFactor = (1 - directHitRate) + (directHitRate * 1.25);

            return critFactor * directHitFactor * determinationFactor * tenacityFactor;
        }

        private Random random = new Random(0);
        
        public double NextDouble()
        {
            return random.NextDouble();
        }
    }
}
