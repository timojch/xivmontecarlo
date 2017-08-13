using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator.Simulation.Rotations
{
    public class BlackMageRotation : WarriorOfLight
    {
        public AstralUmbral CurrentAstralUmbral;

        public BlackMageRotation(Battle battle, StatsGroup stats) : base(battle, stats)
        {
        }

        public override bool CanUseSkill(Skill toUse, out string reason)
        {
            if(toUse is AdvancedSkill<BlackMageRotation>)
            {
                var advToUse = (AdvancedSkill<BlackMageRotation>)toUse;
                if(!advToUse.IsUsableBy(this))
                {
                    reason = "Black Mage skill returned unusable.";
                    return false;
                }
            }
            return base.CanUseSkill(toUse, out reason);
        }

        public static class Skills
        {
            public static Skill Fire1 = new AdvancedSkill<BlackMageRotation>
            {

            };
            public static Skill Fire3 = new AdvancedSkill<BlackMageRotation>
            {

            };
            public static Skill Fire4 = new AdvancedSkill<BlackMageRotation>
            {

            };
            public static Skill Blizzard1 = new AdvancedSkill<BlackMageRotation>
            {

            };
            public static Skill Blizzard3 = new AdvancedSkill<BlackMageRotation>
            {

            };
            public static Skill Blizzard4 = new AdvancedSkill<BlackMageRotation>
            {

            };
            public static Skill Thunder3 = new AdvancedSkill<BlackMageRotation>
            {

            };
            public static Skill Foul = new AdvancedSkill<BlackMageRotation>
            {

            };
            public static Skill Enochian = new AdvancedSkill<BlackMageRotation>
            {

            };
            public static Skill Sharpcast = new AdvancedSkill<BlackMageRotation>
            {

            };
            public static Skill Swiftcast = new AdvancedSkill<BlackMageRotation>
            {

            };
            public static Skill Triplecast = new AdvancedSkill<BlackMageRotation>
            {

            };
            public static Skill Thundercloud = new AdvancedSkill<BlackMageRotation>
            {

            };
            public static Skill Firestarter = new AdvancedSkill<BlackMageRotation>
            {

            };
        }

        public class BlackMageSkill : AdvancedSkill<BlackMageRotation>
        {
            public virtual int MpCost { get; set; }
        }

        public class FireSkill : BlackMageSkill
        {
            public BlackMageRotation BlackMage;
            public override double Potency { get => base.Potency; set => base.Potency = value; }
            public override int MpCost { get => base.MpCost; set => base.MpCost = value; }
            public int BaseMPCost;
        }

        public enum AstralUmbral
        {
            None,
            AstralFire1,
            AstralFire2,
            AstralFire3,
            UmbralIce1,
            UmbralIce2,
            UmbralIce3,
        }
    }
}
