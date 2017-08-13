using System;
using System.Collections.Generic;
using System.Text;

namespace XIVDamageCalculator
{
    public static class RatingConversions
    {
        public const int BaseCritRating = 364;
        public const int BaseDirectHitRating = 364;
        public const int BaseSpeedRating = 364;
        public const int BaseDetermination = 292;
        public const int BaseTenacity = 364;

        public static int GetBaseStat(Stat stat)
        {
            switch (stat)
            {
                case Stat.CritRating:
                    return BaseCritRating;
                case Stat.Determination:
                    return BaseDetermination;
                case Stat.DirectHitRating:
                    return BaseDirectHitRating;
                case Stat.SpeedRating:
                    return BaseSpeedRating;
                case Stat.Tenacity:
                    return BaseTenacity;
                default:
                    throw new ArgumentException("Not a valid stat");
            }
        }

        public static double GetCritChance(int critRating)
        {
            return .05 + (critRating - BaseCritRating) / (double)10850;
        }

        public static double GetCritDamageMultiplier(int critRating)
        {
            return 1.40 + (critRating - BaseCritRating) / (double)10850;
        }

        public static double GetDirectHitChance(int dhRating)
        {
            return 0 + (dhRating - BaseDirectHitRating) / (double)3910;
        }

        public static double GetDirectHitDamageFactor(int dhRating)
        {
            return 1.25;
        }

        public static double GetDeterminationFactor(int determination)
        {
            return 1 + (determination - BaseDetermination) / (double)16700;
        }

        public static double GetGCDDuration(int speedRating)
        {
            return 2.5 - ((speedRating - BaseSpeedRating) / (double)6700);
        }

        public static double GetSpeedFactor(int speedRating)
        {
            return 2.5 / GetGCDDuration(speedRating);
        }

        public static double GetSpeedDoTContribution(int speedRating)
        {
            return 1 + (speedRating - BaseSpeedRating) / (double)16700;
        }

        public static double GetTenacityFactor(int tenacityRating)
        {
            return 1 + (tenacityRating - BaseTenacity) / (double)21700;
        }
    }
}
