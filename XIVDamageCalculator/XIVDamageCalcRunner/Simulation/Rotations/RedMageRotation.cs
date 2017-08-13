using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator.Simulation.Rotations
{
    public class RedMageRotation : WarriorOfLight
    {
        private List<Skill> OffGlobalDamageAbilities = new List<Skill>();
        private bool Chainspell;
        private bool InMelee = true;
        private Skill LastSkill;

        private int WhiteMana;
        private int BlackMana;

        private bool UseAccelerate;

        private IEnumerator<BattleAction> ActionQueue;

        private Dictionary<Skill, Skill> ComboPrerequisites = new Dictionary<Skill, Skill>
        {
            { EnchantedZwer, EnchantedRiposte },
            { Redoublement, EnchantedZwer },
            { Verholy, Redoublement },
            { Verflare,Redoublement },
        };

        static RedMageRotation()
        {
            EmboldenEffects = new EffectType[5];
            for(int i = 0; i < 5; ++i)
            {
                EmboldenEffects[i] = new EffectType
                {
                    Name = $"Embolden:{5 - i}",
                    Duration = 4,
                    DamageMultiplier = 1.2 - (.04 * i)
                };
                EmboldenEffects[i].OnExpired += OnEmboldenExpired;
            }
            Embolden = new Skill
            {
                Name = "Embolden",
                AppliesEffect = EmboldenEffects[0],
                IsGCD = false,
                RecastTime = 120,
            };
        }

        public RedMageRotation(Battle battle, StatsGroup stats, bool useAccelerate) : base(battle, stats)
        {
            this.UseAccelerate = useAccelerate;
            OffGlobalDamageAbilities.Add(Fleche);
            OffGlobalDamageAbilities.Add(ContraSixte);
        }

        public override BattleAction ChooseBattleAction()
        {
            // If we already have a plan, execute it.
            if(ActionQueue != null)
            {
                var nextAction = NextActionInQueue();
                if(nextAction != null)
                {
                    return nextAction;
                }
            }

            // First: If we're on GCD
            if(!base.CanUseGCD)
            {
                // Use oGCDs if we can
                return 
                    UseSkillIfPossible(Acceleration) ??
                    UseSkillIfPossible(Fleche) ??
                    UseSkillIfPossible(ContraSixte) ??
                    WaitForGCD();
            }

            // Priority list: If 80/80...
            if (WhiteMana >= 80 && BlackMana >= 80)
            {
                // ...and unbalanced: Go to melee combo!
                if (WhiteMana != BlackMana || WhiteMana == 100)
                {
                    ActionQueue = MeleeCombo();
                    return NextActionInQueue();
                }
                // ...otherwise: Try to find a way to become unbalanced
                else if (Chainspell) // If we have chainspell, use it to become unbalanced
                {
                    UseChainspell();
                }
                else return // Otherwise, try swiftcast. Failing that, do whatever we can.
                        UseSkillIfPossible(Swiftcast) ??
                        UseSkillIfPossible(Verstone) ??
                        UseSkillIfPossible(Verfire) ??
                        UseSkillIfPossible(Impact) ??
                        UseSkill(Jolt2);
            }

            // If 40/40 and we have manafication up: use it
            // We need to get into the special case once either mana hits 40,
            // to prepare for manafication.
            if ((WhiteMana > 40 || BlackMana > 40) && CanUseSkill(Manafication))
            {
                // If both manas get above 50, we've lost the manafication window.
                if(WhiteMana < 50 || BlackMana < 50)
                {
                    // If both manas are above 40 and we've used our chainspell, Manaficate!
                    if(WhiteMana > 40 && BlackMana > 40 && !Chainspell)
                    {
                        return UseSkill(Manafication);
                    }
                    else
                    {
                        // If both manas are above 39, 
                        if (WhiteMana > 39 && BlackMana > 39)
                        {
                            if (WhiteMana > BlackMana)
                            {
                                return
                                    UseSkillIfPossible(Veraero) ??
                                    UseSkill(Manafication);
                            }
                            else
                            {
                                return
                                    UseSkillIfPossible(Verthunder) ??
                                    UseSkill(Manafication);
                            }
                        }
                    }
                }
            }

            // If in chainspell, use it
            if (Chainspell)
            {
                return UseChainspell();
            }

            // Otherwise, build combo
            return UseAppropriateCastTimeSpell();
        }
        
        private BattleAction UseAppropriateCastTimeSpell()
        {
            if (CanUseSkill(Verfire) && CanUseSkill(Verstone))
            {
                if (BlackMana > WhiteMana)
                {
                    return UseSkill(Verstone);
                }
                else
                {
                    return UseSkill(Verfire);
                }
            }
            else return
                    UseSkillIfPossible(Verstone) ??
                    UseSkillIfPossible(Verfire) ??
                    UseSkillIfPossible(Impact) ??
                    UseSkillIfPossible(Jolt2);
        }

        private BattleAction NextActionInQueue()
        {
            if(ActionQueue?.MoveNext() ?? false)
            {
                return ActionQueue.Current;
            }
            ActionQueue = null;
            return null;
        }

        private IEnumerator<BattleAction> MeleeCombo()
        {
            yield return UseSkill(EnchantedRiposte);
            yield return WaitForGCD();
            yield return UseSkill(EnchantedZwer);
            yield return UseSkillIfPossible(Embolden);
            yield return WaitForGCD();
            yield return UseSkill(Redoublement);
            yield return UseSkillIfPossible(Embolden);
            yield return WaitForGCD();
            if(BlackMana > WhiteMana)
            {
                yield return UseSkill(Verholy);
            }
            else
            {
                yield return UseSkill(Verflare);
            }
        }

        private BattleAction UseChainspell()
        {
            if(CanUseSkill(Verfire) && !CanUseSkill(Verstone))
            {
                return UseSkill(Veraero);
            }
            else if (CanUseSkill(Verstone) && !CanUseSkill(Verfire))
            {
                return UseSkill(Verthunder);
            }
            else if(BlackMana > WhiteMana)
            {
                return UseSkill(Veraero);
            }
            else
            {
                return UseSkill(Verthunder);
            }
        }

        protected override void PaySkillCosts(Skill toUse)
        {
            base.PaySkillCosts(toUse);

            if (toUse == Verfire)
            {
                Battle.ExpireEffect(VerfireReady);
            }
            else if(toUse == Verstone)
            {
                Battle.ExpireEffect(VerstoneReady);
            }
            else if(toUse == Impact)
            {
                Battle.ExpireEffect(ImpactReady);
            }

            if (toUse.IsGCD)
            {
                LastSkill = toUse;
            }

            if(toUse.IsGCD && Chainspell)
            {
                Chainspell = false;
            }
            else if(toUse.IsGCD && toUse.CastTime > 0 && !Chainspell)
            {
                Chainspell = true;
            }
            else if(toUse == Swiftcast)
            {
                Chainspell = true; // It's almost the same thing.
            }

            if (toUse == Manafication)
            {
                WhiteMana *= 2;
                BlackMana *= 2;

                // Generate 0 mana to put ourselves back under cap.
                GenerateWhiteMana(0);
                GenerateBlackMana(0);
            }

            DoReadyProcs(toUse);
            GenerateMana(toUse);
        }

        public override bool CanUseSkill(Skill toUse, out string reason)
        {
            reason = "";
            if ((toUse == Veraero || toUse == Verthunder) && !Chainspell)
            {
                reason = $"Never hardcast {toUse}.";
                return false;
            }
            if ((toUse == Redoublement || toUse == EnchantedZwer || toUse == EnchantedRiposte))
            {
                if (!InMelee)
                {
                    reason = $"Cannot use {toUse} while not in melee.";
                    return false;
                }

                int lowerMana = Math.Min(WhiteMana, BlackMana);
                if ((lowerMana < 25 && (toUse == Redoublement || toUse == EnchantedZwer))
                    || (lowerMana < 30 && toUse == EnchantedRiposte))
                {
                    reason = $"Not enough mana to use {toUse}";
                    return false;
                }
            }
            if (ComboPrerequisites.ContainsKey(toUse) && ComboPrerequisites[toUse] != LastSkill)
            {
                reason = $"Cannot use {toUse} without using ${ComboPrerequisites[toUse]} first";
                return false;
            }
            if (toUse == Verfire && Battle.GetRemainingEffectDuration(VerfireReady) <= 0)
            {
                reason = $"Cannot use {toUse} without Verfire Ready";
                return false;
            }
            if (toUse == Verstone && Battle.GetRemainingEffectDuration(VerstoneReady) <= 0)
            {
                reason = $"Cannot use {toUse} without Verstone Ready";
                return false;
            }
            if (toUse == Impact && Battle.GetRemainingEffectDuration(ImpactReady) <= 0)
            {
                reason = $"Cannot use {toUse} without Impactful";
                return false;
            }
            if(toUse == Acceleration && !this.UseAccelerate)
            {
                reason = $"{toUse} is disabled in this mode.";
                return false;
            }

            return base.CanUseSkill(toUse, out reason);
        }

        private void DoReadyProcs(Skill toUse)
        {
            
            if(toUse == Veraero || toUse == Verthunder)
            {
                bool makeReady = Battle.Simulator.NextDouble() > (UseAccelerate ? .5 : .58);
                if(Battle.GetRemainingEffectDuration(Acceleration.AppliesEffect) > 0)
                {
                    Battle.ExpireEffect(Acceleration.AppliesEffect);
                    makeReady = true;
                }

                if (makeReady)
                {
                    if (toUse == Veraero)
                    {
                        Battle.ApplyEffect(VerstoneReady, Stats);
                    }
                    else
                    {
                        Battle.ApplyEffect(VerfireReady, Stats);
                    }
                }
            }
            
            if(toUse == Verholy || toUse == Verflare)
            {
                bool makeReady = Battle.Simulator.NextDouble() > .8;

                if (toUse == Verholy && (makeReady || BlackMana > WhiteMana))
                {
                    Battle.ApplyEffect(VerstoneReady, Stats);
                }
                if (toUse == Verflare && (makeReady || WhiteMana > BlackMana))
                {
                    Battle.ApplyEffect(VerfireReady, Stats);
                }
            }
        }

        private void GenerateMana(Skill toUse)
        {
            int manaGenerated = 0;
            switch(toUse.Name)
            {
                case "Verfire":
                case "Verstone":
                    manaGenerated = 9;
                    break;
                case "Verthunder":
                case "Veraero":
                    manaGenerated = 11;
                    break;
                case "Verholy":
                case "Verflare":
                    manaGenerated = 21;
                    break;
                case "Jolt":
                    manaGenerated = 3;
                    break;
                case "Impact":
                    manaGenerated = 4;
                    break;
                case "Enchanted Riposte":
                    manaGenerated = -30;
                    break;
                case "Enchanted Zwerchhau":
                case "Enchanted Redoublement":
                    manaGenerated = -25;
                    break;
            }

            int whiteManaGen = 0;
            int blackManaGen = 0;

            switch(toUse.Name)
            {
                case "Verstone":
                case "Veraero":
                case "Verholy":
                    whiteManaGen = manaGenerated;
                    break;
                case "Verfire":
                case "Verthunder":
                case "Verflare":
                    blackManaGen = manaGenerated;
                    break;
                case "Jolt":
                case "Impact":
                case "Enchanted Riposte":
                case "Enchanted Zwerchhau":
                case "Enchanted Redoublement":
                    blackManaGen = manaGenerated;
                    whiteManaGen = manaGenerated;
                    break;
            }

            GenerateWhiteMana(whiteManaGen);
            GenerateBlackMana(blackManaGen);
        }

        private void GenerateWhiteMana(int amount)
        {
            if(BlackMana > WhiteMana+30 && amount > 0)
            {
                amount /= 2;
            }
            WhiteMana += amount;
            WhiteMana = Math.Min(WhiteMana, 100);
        }

        private void GenerateBlackMana(int amount)
        {
            if (WhiteMana > BlackMana + 30)
            {
                amount /= 2;
            }
            BlackMana += amount;
            BlackMana = Math.Min(BlackMana, 100);
        }

        static void OnEmboldenExpired(Battle battle, EffectInstance instance)
        {
            int stackCount = int.Parse(instance.Type.Name.Split(':')[1]);
            stackCount--;
            if(stackCount > 0)
            {
                battle.ApplyEffect(EmboldenEffects[5 - stackCount], instance.StatsSnapshot);
            }
        }

        #region Skills
        static Skill Fleche = new Skill
        {
            Name = "Fleche",
            IsGCD = false,
            Potency = 420,
            RecastTime = 25,
        };
        static Skill ContraSixte = new Skill
        {
            Name = "ContraSixte",
            IsGCD = false,
            Potency = 300,
            RecastTime = 45
        };
        static Skill Swiftcast = new Skill
        {
            Name = "Swiftcast",
            IsGCD = false,
            RecastTime = 60,
        };
        static Skill Acceleration = new Skill
        {
            Name = "Acceleration",
            IsGCD = false,
            RecastTime = 35,
            AppliesEffect = new EffectType
            {
                Name = "Acceleration",
                Duration = 10
            }
        };
        static Skill Embolden; // Initialized in static constructor
        static Skill Manafication = new Skill
        {
            Name = "Manafication",
            IsGCD = false,
            RecastTime = 120,
        };
        static Skill Jolt2 = new Skill
        {
            Name = "Jolt2",
            IsGCD = true,
            CastTime = 2.0,
            Potency = 240
        };
        static Skill Impact = new Skill
        {
            Name = "Impact",
            IsGCD = true,
            CastTime = 2.0,
            Potency = 270
        };
        static Skill Veraero = new Skill
        {
            Name = "Veraero",
            IsGCD = true,
            Potency = 300
        };
        static Skill Verthunder = new Skill
        {
            Name = "Verthunder",
            IsGCD = true,
            Potency = 300
        };
        static Skill Verstone = new Skill
        {
            Name = "Verstone",
            IsGCD = true,
            CastTime = 2.0,
            Potency = 270
        };
        static Skill Verfire = new Skill
        {
            Name = "Verfire",
            IsGCD = true,
            CastTime = 2.0,
            Potency = 270
        };
        static Skill Verholy = new Skill
        {
            Name = "Verholy",
            IsGCD = true,
            Potency = 650
        };
        static Skill Verflare = new Skill
        {
            Name = "Verflare",
            IsGCD = true,
            Potency = 650
        };
        static Skill EnchantedRiposte = new Skill
        {
            Name = "Enchanted Riposte",
            IsGCD = true,
            RecastTime = 1.5,
            IsAffectedBySpeed = false,
            Potency = 210,
        };
        static Skill EnchantedZwer = new Skill
        {
            Name = "Enchanted Zwerchhau",
            IsGCD = true,
            RecastTime = 1.5,
            IsAffectedBySpeed = false,
            Potency = 290,
        };
        static Skill Redoublement = new Skill
        {
            Name = "Enchanted Redoublement",
            IsGCD = true,
            RecastTime = 2.2,
            IsAffectedBySpeed = false,
            Potency = 470,
        };
        #endregion

        #region Effects
        static EffectType VerfireReady = new EffectType
        {
            Name = "Verfire Ready",
            Duration = 30,
        };
        static EffectType VerstoneReady = new EffectType
        {
            Name = "Verstone Ready",
            Duration = 30,
        };
        static EffectType ImpactReady = new EffectType
        {
            Name = "Impactful",
            Duration = 30
        };

        static EffectType[] EmboldenEffects;
        #endregion
    }
}

