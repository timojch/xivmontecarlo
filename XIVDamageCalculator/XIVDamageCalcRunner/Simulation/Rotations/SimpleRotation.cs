using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator.Simulation.Rotations
{
    public class SimpleRotation : WarriorOfLight
    {
        public List<Skill> DoTSkills { get; private set; } = new List<Skill>();
        public List<Skill> OffGlobalDamageAbilities { get; private set; } = new List<Skill>();
        public Skill FillerSkill { get; set; }

        public override BattleAction ChooseBattleAction()
        {
            foreach(var ability in OffGlobalDamageAbilities)
            {
                if(CanUseSkill(ability))
                {
                    return this.UseSkill(ability);
                }
            }

            if(!CanUseGCD)
            {
                return this.WaitForGCD();
            }

            foreach(var dot in DoTSkills)
            {
                if(this.Battle.GetRemainingEffectDuration(dot.AppliesEffect) <= dot.CastTime)
                {
                    return this.UseSkill(dot);
                }
            }

            return this.UseSkill(FillerSkill);
        }

        public SimpleRotation(Battle battle, StatsGroup stats) : base(battle, stats)
        {
        }
    }
}
