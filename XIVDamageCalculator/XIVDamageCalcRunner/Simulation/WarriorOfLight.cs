using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator.Simulation
{
    public class WarriorOfLight
    {
        private double GcdTimeEnd;
        private Dictionary<Skill, double> AbilityRecastAvailableAt = new Dictionary<Skill, double>();

        public StatsGroup Stats { get; private set; }
        
        public Battle Battle { get; set; }

        public WarriorOfLight(Battle battle, StatsGroup stats)
        {
            this.Battle = battle;
            this.Stats = stats;
        }

        public virtual BattleAction ChooseBattleAction()
        {
            return this.WaitForDuration(10.0, "Must be a cutscene or something.");
        }

        protected virtual void PaySkillCosts(Skill toUse)
        {
            string reason;
            if(!CanUseSkill(toUse, out reason))
            {
                throw new CannotUseSkillException(toUse, reason);
            }

            if(toUse.IsGCD)
            {
                GcdTimeEnd = Battle.CurrentTime + GetModifiedRecastTime(toUse) + Battle.Latency;
            }
            else
            {
                AbilityRecastAvailableAt[toUse] = Battle.CurrentTime + GetModifiedRecastTime(toUse);
            }
        }

        public virtual bool CanUseSkill(Skill toUse, out string reason)
        {
            if(toUse.IsGCD && !CanUseGCD)
            {
                reason = "GCD is not ready";
                return false;
            }
            else if (!toUse.IsGCD && GetSecondsLeftOnRecast(toUse) > 0)
            {
                reason = "Recast time is not ready";
                return false;
            }
            reason = "";
            return true;
        }

        public bool CanUseSkill(Skill toUse)
        {
            string reason;
            return CanUseSkill(toUse, out reason);
        }

        public double GetSecondsLeftOnRecast(Skill toUse)
        {
            if(AbilityRecastAvailableAt.ContainsKey(toUse))
            {
                return Math.Max(AbilityRecastAvailableAt[toUse] - Battle.CurrentTime, 0);
                
            }
            else
            {
                return 0;
            }
        }

        public bool CanUseGCD
        {
            get
            {
                return (GcdTimeEnd <= Battle.CurrentTime);
            }
        }

        public bool CanWeaveAbility
        {
            get
            {
                return (GcdTimeEnd <= Battle.CurrentTime + Skill.MinimumTime);
            }
        }
        
        protected BattleAction WaitForGCD()
        {
            return new BattleAction
            {
                Description = "WaitForGCD",
                Duration = GcdTimeEnd - Battle.CurrentTime,
                Trivial = true
            };
        }

        protected BattleAction WaitForDuration(double duration, string reason = "Waiting")
        {
            return new BattleAction
            {
                Description = $"Waiting ({reason})",
                Duration = duration,
                Trivial = false // Dead GCDs are never trivial.
            };
        }

        protected BattleAction UseSkill(Skill toUse)
        {
            this.PaySkillCosts(toUse);
            return new BattleAction
            {
                Description = toUse.Name,
                Duration = this.GetModifiedCastTime(toUse),
                SkillUsed = toUse
            };
        }

        protected BattleAction UseSkillIfPossible(Skill toUse)
        {
            if(this.CanUseSkill(toUse))
            {
                this.PaySkillCosts(toUse);
                return new BattleAction
                {
                    Description = toUse.Name,
                    Duration = this.GetModifiedCastTime(toUse),
                    SkillUsed = toUse
                };
            }
            else
            {
                return null;
            }
        }

        protected virtual double GetModifiedCastTime(Skill toUse)
        {
            if(toUse.IsAffectedBySpeed ?? true)
            {
                return GetModifiedTime(toUse.CastTime);
            }
            else
            {
                return toUse.CastTime;
            }
        }

        protected virtual double GetModifiedRecastTime(Skill toUse)
        {
            if(toUse.IsAffectedBySpeed ?? toUse.IsGCD)
            {
                return GetModifiedTime(toUse.RecastTime);
            }
            else
            {
                return toUse.RecastTime;
            }
        }

        private double GetModifiedTime(double baseTime)
        {
            return baseTime / RatingConversions.GetSpeedFactor(this.Stats.SpeedRating);
        }

        public class CannotUseSkillException : Exception
        {
            public Skill ToUse;
            public string Reason;

            public CannotUseSkillException(Skill toUse, string reason)
                :base($"Attempted to use {toUse.Name} but couldn't because {reason}.")
            {
                ToUse = toUse;
                Reason = reason;
            }
        }
    }
}
