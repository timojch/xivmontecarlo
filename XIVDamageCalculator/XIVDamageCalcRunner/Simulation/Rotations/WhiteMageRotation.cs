using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator.Simulation.Rotations
{
    public class WhiteMageRotation : SimpleRotation
    {
        static Skill Assize = new Skill
        {
            Name = "Assize",
            Potency = 300,
            IsGCD = false,
            RecastTime = 60,
            IsAffectedBySpeed = false,
        };

        static Skill Aero2 = new Skill
        {
            Name = "Aero II",
            Potency = 50,
            IsGCD = true,
            RecastTime = 2.5,
            IsAffectedBySpeed = true,
            AppliesEffect = new EffectType
            {
                Name = "Aero II",
                Duration = 18,
                DotPotency = 50,
            }
        };

        static Skill Aero3 = new Skill
        {
            Name = "Aero III",
            Potency = 50,
            IsGCD = true,
            RecastTime = 2.5,
            CastTime = 2.5,
            IsAffectedBySpeed = true,
            AppliesEffect = new EffectType
            {
                Name = "Aero III",
                Duration = 24,
                DotPotency = 40,
            }
        };

        static Skill Stone4 = new Skill
        {
            Name = "Stone IV",
            Potency = 250,
            IsGCD = true,
            RecastTime = 2.5,
            CastTime = 2.5,
            IsAffectedBySpeed = true,
        };

        public WhiteMageRotation(Battle battle, StatsGroup stats, bool Aero3First = true) : base(battle, stats)
        {
            if (Aero3First)
            {
                DoTSkills.Add(Aero3);
                DoTSkills.Add(Aero2);
            }
            else
            {
                DoTSkills.Add(Aero2);
                DoTSkills.Add(Aero3);
            }

            OffGlobalDamageAbilities.Add(Assize);
            FillerSkill = Stone4;
        }
    }
}
