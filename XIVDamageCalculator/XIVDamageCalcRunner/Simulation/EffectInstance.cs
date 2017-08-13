using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator.Simulation
{
    public class EffectInstance
    {
        public EffectType Type;
        public double StartTime;
        public double Duration;
        public StatsGroup StatsSnapshot;
        public List<EffectType> EffectsSnapshot;
        public int TicksSoFar;
    }
}
