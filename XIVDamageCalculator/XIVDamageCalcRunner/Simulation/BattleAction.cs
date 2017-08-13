using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVDamageCalculator.Simulation
{
    public class BattleAction
    {
        public string Description;
        public double Duration;
        public Skill SkillUsed;
        public bool Trivial = false;
        
        public override string ToString() => Description;
    }
}
