using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator
{
    public interface ISkillSimulator
    {
        double GetDamageOfGCDAttack(StatsGroup stats);
        double GetDamageOfDoTTick(StatsGroup stats);
        double NextDouble();
    }
}
