using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator
{
    static class Optimizations
    {
        public static double GetContribution(this StatsGroup self, Stat stat)
        {
            StatsGroup lower = self.Copy();
            lower[stat] = StatsGroup.Baseline[stat];

            return Formulas.CalculateDamageFromGCD(self) - Formulas.CalculateDamageFromGCD(lower);
        }

        public static double GetStatValue(this StatsGroup self, Stat stat)
        {
            StatsGroup higher = self.Copy();
            higher[stat] += 1;
            return Formulas.CalculateDamageFromGCD(higher) - Formulas.CalculateDamageFromGCD(self);
        }
    }
}
