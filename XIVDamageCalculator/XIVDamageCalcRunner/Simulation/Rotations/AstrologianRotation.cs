using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator.Simulation.Rotations
{
    public class AstrologianRotation : SimpleRotation
    {
        static Skill Combust3 = new Skill
        {
            Name = "Combust II",
            Potency = 0,
            IsGCD = true,
            RecastTime = 2.5,
            IsAffectedBySpeed = true,
            AppliesEffect = new EffectType
            {
                Name = "Combust II",
                Duration = 30,
                DotPotency = 50,
            }
        };

        static Skill Malefic3 = new Skill
        {
            Name = "Malefic III",
            Potency = 220,
            IsGCD = true,
            RecastTime = 2.5,
            CastTime = 2.5,
            IsAffectedBySpeed = true,
        };

        public AstrologianRotation(Battle battle, StatsGroup stats) : base(battle, stats)
        {
            this.DoTSkills.Add(Combust3);
            this.FillerSkill = Malefic3;
        }
    }
}
