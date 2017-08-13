using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator.Simulation
{
    public class EffectType
    {
        public string Name { get; set; }
        public double Duration { get; set; }
        public double DotPotency { get; set; }

        public double DamageMultiplier { get; set; } = 1;
        public double SpeedMultiplier { get; set; } = 1;
        public double CritChanceAddin { get; set; } = 0;
        public double DirectHitChanceAddin { get; set; } = 0;

        public void Expire(Battle battle, EffectInstance instance)
        {
            this?.OnExpired?.Invoke(battle, instance);
        }

        public event Action<Battle, EffectInstance> OnExpired;
    }
}
