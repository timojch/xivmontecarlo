using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XIVDamageCalculator
{
    public class StatsGroup
    {
        public int CritRating { get; private set; } = 0;
        public int SpeedRating { get; private set; } = 0;
        public int Determination { get; private set; } = 0;
        public int Tenacity { get; private set; } = 0;
        public int DirectHitRating { get; private set; } = 0;

        public int this[Stat stat]
        {
            get
            {
                switch(stat)
                {
                    case Stat.CritRating:
                        return CritRating;
                    case Stat.Determination:
                        return Determination;
                    case Stat.DirectHitRating:
                        return DirectHitRating;
                    case Stat.SpeedRating:
                        return SpeedRating;
                    case Stat.Tenacity:
                        return Tenacity;
                    default:
                        throw new ArgumentException("Not a valid stat");
                }
            }
            set
            {
                switch (stat)
                {
                    case Stat.CritRating:
                        CritRating = value;
                        break;
                    case Stat.Determination:
                        Determination = value;
                        break;
                    case Stat.DirectHitRating:
                        DirectHitRating = value;
                        break;
                    case Stat.SpeedRating:
                        SpeedRating = value;
                        break;
                    case Stat.Tenacity:
                        Tenacity = value;
                        break;
                    default:
                        throw new ArgumentException("Not a valid stat");
                }
            }
        }

        public static StatsGroup Baseline
        {
            get
            {
                return new StatsGroup
                {
                    CritRating = 364,
                    SpeedRating = 364,
                    Determination = 292,
                    Tenacity = 364,
                    DirectHitRating = 364,
                };
            }
        }

        public StatsGroup()
        {

        }

        public StatsGroup(params KeyValuePair<Stat, int>[] stats)
        {
            foreach(var stat in stats)
            {
                this[stat.Key] = stat.Value;
            }
        }

        public StatsGroup Copy()
        {
            return new StatsGroup
            {
                CritRating = CritRating,
                SpeedRating = SpeedRating,
                Determination = Determination,
                Tenacity = Tenacity,
                DirectHitRating = DirectHitRating,
            };
        }

        public static StatsGroup operator +(StatsGroup lhs, StatsGroup rhs)
        {
            StatsGroup ret = new StatsGroup();

            ret.CritRating = lhs.CritRating + rhs.CritRating;
            ret.SpeedRating = lhs.SpeedRating + rhs.SpeedRating;
            ret.Determination = lhs.Determination + rhs.Determination;
            ret.Tenacity = lhs.Tenacity + rhs.Tenacity;
            ret.DirectHitRating = lhs.DirectHitRating + rhs.DirectHitRating;

            return ret;
        }

        public static StatsGroup operator -(StatsGroup lhs, StatsGroup rhs)
        {
            StatsGroup ret = new StatsGroup();

            ret.CritRating = lhs.CritRating - rhs.CritRating;
            ret.SpeedRating = lhs.SpeedRating - rhs.SpeedRating;
            ret.Determination = lhs.Determination - rhs.Determination;
            ret.Tenacity = lhs.Tenacity - rhs.Tenacity;
            ret.DirectHitRating = lhs.DirectHitRating - rhs.DirectHitRating;

            return ret;
        }

        public void Save(FileInfo toFile)
        {
            using (var stream = toFile.OpenWrite())
            {
                var writer = new StreamWriter(stream);
                foreach(var statNumber in Enum.GetValues(typeof(Stat)))
                {
                    Stat stat = (Stat)statNumber;
                    writer.WriteLine($"{stat.ToString()}={this[stat]}");
                }
                writer.Flush();
            }
        }

        public static StatsGroup Load(FileInfo fromFile)
        {
            using (var stream = fromFile.OpenRead())
            {
                var reader = new StreamReader(stream);
                var ret = new StatsGroup();
                while(stream.CanRead)
                {
                    string line = reader.ReadLine();
                    if(line == null)
                    {
                        break;
                    }
                    string[] toks = line.Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (toks.Length >= 2)
                    {
                        Stat stat = (Stat)Enum.Parse(typeof(Stat), toks[0]);
                        ret[stat] = int.Parse(toks[1]);
                    }
                }
                return ret;
            }
        }

        public static IEnumerable<Stat> GetEachDPSStat()
        {
            yield return Stat.CritRating;
            yield return Stat.SpeedRating;
            yield return Stat.Determination;
            yield return Stat.DirectHitRating;
        }

        public static IEnumerable<Stat> GetEachTankStat()
        {
            yield return Stat.CritRating;
            yield return Stat.SpeedRating;
            yield return Stat.Determination;
            yield return Stat.DirectHitRating;
            yield return Stat.Tenacity;
        }
    }

    public enum Stat
    {
        CritRating,
        SpeedRating,
        Determination,
        DirectHitRating,
        Tenacity
    }
}
