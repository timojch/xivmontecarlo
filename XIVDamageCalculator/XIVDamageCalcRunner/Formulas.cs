using System;

namespace XIVDamageCalculator
{
    public static class Formulas
    {
        public static double CalculateDamageFromGCD(double critRate, double critDamage, double speedFactor, double directHitRate, double determinationFactor, double tenacityFactor=1.0)
        {
            double critFactor = GetCritFactor(critRate, critDamage);
            double directHitFactor = (directHitRate * 1.25) + (1 - directHitRate);
            return critFactor * directHitFactor * determinationFactor * tenacityFactor * speedFactor;
        }

        public static double CalculateDamageFromGCD(StatsGroup stats)
        {
            double critRate = RatingConversions.GetCritChance(stats.CritRating);
            double critMult = RatingConversions.GetCritDamageMultiplier(stats.CritRating);
            double speedFactor = RatingConversions.GetSpeedFactor(stats.SpeedRating);
            double directHitRate = RatingConversions.GetDirectHitChance(stats.DirectHitRating);
            double determinationFactor = RatingConversions.GetDeterminationFactor(stats.Determination);
            double tenacityFactor = RatingConversions.GetTenacityFactor(stats.Tenacity);

            return CalculateDamageFromGCD(critRate, critMult, speedFactor, directHitRate, determinationFactor, tenacityFactor);
        }

        public static double CalculateDamageFromDoT(StatsGroup stats)
        {
            double critRate = RatingConversions.GetCritChance(stats.CritRating);
            double critMult = RatingConversions.GetCritDamageMultiplier(stats.CritRating);
            double speedFactor = RatingConversions.GetSpeedDoTContribution(stats.SpeedRating);
            double directHitRate = RatingConversions.GetDirectHitChance(stats.DirectHitRating);
            double determinationFactor = RatingConversions.GetDeterminationFactor(stats.Determination);
            double tenacityFactor = RatingConversions.GetTenacityFactor(stats.Tenacity);

            return CalculateDamageFromGCD(critRate, critMult, speedFactor, directHitRate, determinationFactor, tenacityFactor);
        }

        public static double CalculateDamageFromAbility(StatsGroup stats)
        {
            double critRate = RatingConversions.GetCritChance(stats.CritRating);
            double critMult = RatingConversions.GetCritDamageMultiplier(stats.CritRating);
            double directHitRate = RatingConversions.GetDirectHitChance(stats.DirectHitRating);
            double determinationFactor = RatingConversions.GetDeterminationFactor(stats.Determination);
            double tenacityFactor = RatingConversions.GetTenacityFactor(stats.Tenacity);

            return CalculateDamageFromGCD(critRate, critMult, 1.0, directHitRate, determinationFactor, tenacityFactor);
        }

        public static double GetCritFactor(double critRate, double critMult)
        {
            return ((critRate * critMult) + (1 - critRate)) / 1.02; // Divide by 1.02 so that 0-stat baseline is 1.00 
        }
    }
}
