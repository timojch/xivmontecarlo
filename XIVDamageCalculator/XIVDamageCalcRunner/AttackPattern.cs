using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator
{
    public class AttackPattern
    {
        public int GcdPotency;
        public List<DamageAbility> DamageAbilities = new List<DamageAbility>();
        public List<DamageOverTimeSkill> DoTSkills = new List<DamageOverTimeSkill>();

        public class DamageAbility
        {
            public int Potency;
            public int Recast;
            public string Name;
            public double CastTime = 0;
        }

        public class DamageOverTimeSkill
        {
            public int TotalPotency;
            public int TickCount;
            public double Duration;
            public double CastTime;
            public string Name;
        }

        public static AttackPattern WhiteMageAttackPattern = new AttackPattern
        {
            GcdPotency = 250, // Stone IV
            DamageAbilities = new List<DamageAbility>
            {
                new DamageAbility
                {
                    Potency = 300,
                    Recast = 60,
                    Name = "Assize"
                }
            },
            DoTSkills = new List<DamageOverTimeSkill>
            {
                new DamageOverTimeSkill
                {
                    TotalPotency = 370,
                    Duration = 24,
                    Name = "Aero III",
                    CastTime = 2.5,
                    TickCount = 9
                },
                new DamageOverTimeSkill
                {
                    TotalPotency = 350,
                    Duration = 18,
                    Name = "Aero II",
                    CastTime = 2.5,
                    TickCount = 7
                }
            }
        };
        public static AttackPattern RedMageAttackPattern = new AttackPattern
        {
            GcdPotency = 295,
            DoTSkills = new List<DamageOverTimeSkill>() { },
            DamageAbilities = new List<DamageAbility>
            {
                new DamageAbility
                {
                    Name = "Fleche",
                    Recast = 25,
                    Potency = 420
                },
                new DamageAbility
                {
                    Name = "Contra-Sixte",
                    Recast = 45,
                    Potency = 300
                },
                new DamageAbility
                {
                    Name = "Melee-Combo",
                    Recast = 40,
                    Potency = 970,
                    CastTime = 5
                }
            },
        };
        public static AttackPattern AstrologianAttackPattern = new AttackPattern
        {
            GcdPotency = 220, // Stone IV

            DamageAbilities = new List<DamageAbility>
            {
                // womp womp
            },
            DoTSkills = new List<DamageOverTimeSkill>
            {
                new DamageOverTimeSkill
                {
                    TotalPotency = 500,
                    Duration = 30,
                    Name = "Malefic III",
                    CastTime = 2.5,
                    TickCount = 10
                },
            }
        };
    }
}
