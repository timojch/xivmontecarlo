using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator.Simulation
{
    public class Skill
    {
        public const double MinimumTime = 0.7;

        public bool IsGCD { get; set; } = true;
        public double CastTime { get; set; }
        public bool? IsAffectedBySpeed { get; set; }
        public double RecastTime { get; set; } = 2.5;
        public string Name { get; set; }

        public EffectType AppliesEffect
        {
            get { return AppliesEffects?.FirstOrDefault(); }
            set { AppliesEffects = new EffectType[] { value }; }
        }
        public EffectType[] AppliesEffects { get; set; } = new EffectType[0];
        public virtual double Potency { get; set; }

        public override string ToString() => Name;
    }

    public class AdvancedSkill<Job> : Skill
    {
        public Predicate<Job> IsUsableBy;
        public Action<Job> PayCosts;
    }
}
