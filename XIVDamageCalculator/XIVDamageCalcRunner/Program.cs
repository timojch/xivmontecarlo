using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XIVDamageCalculator.Simulation;
using XIVDamageCalculator.Simulation.Rotations;

namespace XIVDamageCalculator
{
    public static class Program
    {
        const double sampleDuration = 600;
        const int monteCarloCount = 65536;
        static TimeSpan monteCarloDuration = TimeSpan.FromSeconds(3);
        const int statDelta = 200;

        static StatsGroup CurrentStats;

        static KeyValuePair<string, RotationFactory> CurrentRotation = new KeyValuePair<string, RotationFactory>("Not Selected", null);

        static ISkillSimulator CurrentSimulationRules = new DeterministicSimulator();
        
        delegate WarriorOfLight RotationFactory(Battle battle, StatsGroup stats);

        private static Dictionary<string, RotationFactory> rotationFactories = new Dictionary<string, RotationFactory>
        {
            {"White Mage", (battle, stats)=>new WhiteMageRotation(battle, stats, true) },
            {"Astrologian", (battle, stats)=>new AstrologianRotation(battle, stats) },
            {"Red Mage", (battle, stats)=>new RedMageRotation(battle, stats, true)},
            {"Red Mage (No Accel)", (battle, stats)=>new RedMageRotation(battle, stats, false)},
        };

        private static Dictionary<String, ISkillSimulator> simulationRules = new Dictionary<string, ISkillSimulator>
        {
            {"Deterministic", new DeterministicSimulator() },
            {"MonteCarlo", new RandomSimulator() }
        };

        [STAThread]
        public static void Main(string[] args)
        {
            MainMenu();
        }

        static void MainMenu()
        {
            bool terminate = false;
            while (!terminate)
            {
                char selected;
                Console.Clear();
                Console.WriteLine($"Stats Loaded: {CurrentStats != null}");
                Console.WriteLine($"Rotation Selected: {CurrentRotation.Key}");
                Console.WriteLine($"Rulesset Selected: {CurrentSimulationRules.GetType().Name}");
                Console.WriteLine("Choose an Action");
                Console.WriteLine("1 - Load Stats");
                Console.WriteLine("2 - Select Rotation");
                Console.WriteLine("3 - Change Rules");
                Console.WriteLine("4 - Enter Stats Manually");
                if (CurrentStats != null && CurrentRotation.Value != null)
                {
                    Console.WriteLine("G - Go!");
                }
                Console.WriteLine("Q - Quit");
                selected = Console.ReadKey(true).KeyChar;
                switch(selected)
                {
                    case '1':
                        SelectStats();
                        break;
                    case '2':
                        SelectRotation();
                        break;
                    case '3':
                        SelectRules();
                        break;
                    case '4':
                        ModifyStats();
                        break;
                    case '0':
                        CurrentStats = StatsGroup.Baseline;
                        break;
                    case 'G':
                    case 'g':
                        RunBasicSimulation();
                        break;
                    case 'R':
                    case 'r':
                        RunAllRotations();
                        break;
                    case 'S':
                    case 's':
                        RunStatSimulation();
                        break;
                    case 'V':
                    case 'v':
                        RunGraph();
                        break;
                    case 'P':
                    case 'p':
                        SaveStats();
                        break;
                    case 'I':
                    case 'i':
                        PrintInfo();
                        break;
                    case 'Q':
                    case 'q':
                        terminate = true;
                        break;
                    default:
                        selected = '\0';
                        break;
                }
            }
        }

        private static void PrintInfo()
        {
            Console.WriteLine($"Crit Chance:  {RatingConversions.GetCritChance(CurrentStats.CritRating) * 100:0.0}%");
            Console.WriteLine($"Crit Mult:    {RatingConversions.GetCritDamageMultiplier(CurrentStats.CritRating) * 100:0.0}%");
            Console.WriteLine($"DH   Chance:  {RatingConversions.GetDirectHitChance(CurrentStats.DirectHitRating) * 100:0.0}%");
            Console.WriteLine($"GCD:          {2.5 / RatingConversions.GetSpeedFactor(CurrentStats.SpeedRating):0.00}");
            Console.WriteLine($"Ten/Det Mult: {RatingConversions.GetDeterminationFactor(CurrentStats.Determination) * RatingConversions.GetTenacityFactor(CurrentStats.Tenacity):0.00}");
            
            Console.WriteLine();
            Console.ReadKey(true);
        }

        private static void RunGraph()
        {
            foreach (var statNum in Enum.GetValues(typeof(Stat)))
            {
                var stat = (Stat)statNum;
                Console.WriteLine($"{(int)(stat + 1)} - {stat.ToString()}");
            }

            int choice = int.Parse(Console.ReadKey(true).KeyChar.ToString()) - 1;
            FileInfo outFile = new FileInfo("out.csv");
            using (Stream outStream = outFile.OpenWrite())
            {
                StreamWriter writer = new StreamWriter(outStream);
                double baseResult = 0;
                for (int stat = StatsGroup.Baseline[(Stat)choice]; stat < 5000; stat+=10)
                {
                    StatsGroup stats = CurrentStats.Copy();
                    stats[(Stat)choice] = stat;
                    double result = RunSimulation(stats, CurrentRotation.Value);

                    if (baseResult == 0)
                    {
                        baseResult = result;
                    }
                    
                    Console.WriteLine($"{stat}\t{result / baseResult}");
                    writer.WriteLine($"{stat}\t{result / baseResult}");
                }
                writer.Flush();
            }
        }

        private static void EnterStats()
        {
            CurrentStats = StatsGroup.Baseline;
            ModifyStats();
        }

        private static void ModifyStats()
        {
            var stats = (CurrentStats ?? StatsGroup.Baseline).Copy();
            foreach (var statNum in Enum.GetValues(typeof(Stat)))
            {
                var stat = (Stat)statNum;
                Console.Write($"{stat.ToString()} = {stats[stat]}+");
                try
                {
                    int amount = int.Parse(Console.ReadLine());
                    stats[stat] += amount;
                }
                catch
                {
                }
            }
            CurrentStats = stats;
        }

        static void SelectStats()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Character stat files (*.char)|*.char|All files (*.*)|*.*";
            openFile.ShowDialog();
            string file = openFile.FileName;

            try
            {
                CurrentStats = StatsGroup.Load(new FileInfo(file));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void SaveStats()
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Character stat files (*.char)|*.char|All files (*.*)|*.*";
            saveFile.ShowDialog();
            string file = saveFile.FileName;

            try
            {
                CurrentStats.Save(new FileInfo(file));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void SelectRotation()
        {
            int i = 1;
            Console.WriteLine("Choose a job/rotation");
            Dictionary<char, KeyValuePair<string, RotationFactory>> list = new Dictionary<char, KeyValuePair<string, RotationFactory>>();
            foreach (var pair in rotationFactories)
            {
                list[(char)('0' + i++)] = pair;
            }
            while (true)
            {
                foreach (var pair in list)
                {
                    Console.WriteLine($"{pair.Key} - {pair.Value.Key}");
                }
                char selected = Console.ReadKey(true).KeyChar;
                Console.WriteLine();
                if (list.ContainsKey(selected))
                {
                    CurrentRotation = list[selected];
                    return;
                }
            }
        }

        static void SelectRules()
        {
            int i = 1;
            Console.WriteLine("Choose a ruleset");
            Dictionary<char, KeyValuePair<string, ISkillSimulator>> list = new Dictionary<char, KeyValuePair<string, ISkillSimulator>>();
            foreach (var pair in simulationRules)
            {
                list[(char)('0' + i++)] = pair;
            }
            while (true)
            {
                foreach (var pair in list)
                {
                    Console.WriteLine($"{pair.Key} - {pair.Value.Key}");
                }
                char selected = Console.ReadKey(true).KeyChar;
                Console.WriteLine();
                if (list.ContainsKey(selected))
                {
                    CurrentSimulationRules = list[selected].Value;
                    return;
                }
            }
        }

        static void RunBasicSimulation()
        {
            double totalPotency = RunSimulation(CurrentStats, CurrentRotation.Value, true);
            Console.WriteLine($"My total potency is is {totalPotency} ({totalPotency/sampleDuration:0.0} pps)");
            Console.ReadKey(true);
        }

        static void RunAllRotations()
        {
            Console.WriteLine("Simulating all rotations at these stats");
            foreach(var rotation in rotationFactories)
            {
                double potency = RunSimulation(CurrentStats, rotation.Value);
                Console.WriteLine($"{rotation.Key} - {potency}");
            }
            Console.ReadKey(true);
        }

        static void RunStatSimulation()
        {
            bool done = false;
            while (!done)
            {
                try
                {
                    Console.WriteLine("Simulating stat variance");
                    Console.Write("Potential Loss: ");
                    int potentialLoss = int.Parse(Console.ReadLine());
                    Console.Write("Potential Gain: ");
                    int potentialGain = int.Parse(Console.ReadLine());
                    Dictionary<string, StatsGroup> possibleStats = new Dictionary<string, StatsGroup>();
                    if(potentialLoss > 0 && potentialGain > 0)
                    {
                        foreach(var lostStat in StatsGroup.GetEachDPSStat())
                        {
                            foreach (var gainedStat in StatsGroup.GetEachDPSStat())
                            {
                                if (lostStat == gainedStat) continue;
                                string name = $"{lostStat}->{gainedStat}";
                                StatsGroup newStats = CurrentStats.Copy();
                                newStats[lostStat] -= potentialLoss;
                                newStats[gainedStat] += potentialGain;
                                possibleStats[name] = newStats;
                            }
                        }
                    }
                    else if(potentialLoss > 0)
                    {
                        foreach (var lostStatNum in Enum.GetValues(typeof(Stat)))
                        {
                            Stat lostStat = (Stat)lostStatNum;
                            string name = $"-{potentialLoss} {lostStat}";
                            StatsGroup newStats = CurrentStats.Copy();
                            newStats[lostStat] -= potentialLoss;
                            possibleStats[name] = newStats;
                        }
                    }
                    else if(potentialGain > 0)
                    {
                        foreach (var gainedStatNum in Enum.GetValues(typeof(Stat)))
                        {
                            Stat gainedStat = (Stat)gainedStatNum;
                            string name = $"+{potentialGain} {gainedStat}";
                            StatsGroup newStats = CurrentStats.Copy();
                            newStats[gainedStat] += potentialGain;
                            possibleStats[name] = newStats;
                        }
                    }

                    done = true;

                    double baseline = RunSimulation(CurrentStats, CurrentRotation.Value, true);
                    Console.WriteLine($"Baseline is {baseline}");
                    foreach (var stats in possibleStats)
                    {
                        double result = RunSimulation(stats.Value, CurrentRotation.Value);
                        double delta = result - baseline;
                        double percentage = delta / baseline * 100;
                        string sign = result > baseline ? "+" : "";
                        Console.WriteLine($"{stats.Key}: {sign}{percentage:0.00}% ({sign}{delta:#.0})");
                    }
                    Console.WriteLine();
                    Console.ReadKey(true);
                }
                catch { }
            }

        }

        static double RunSimulation(StatsGroup stats, RotationFactory rotation, bool enableLogging = false)
        {
            double totalPotency = 0;
            if (CurrentSimulationRules is DeterministicSimulator)
            {
                Battle battle = new Battle(new DeterministicSimulator(), sampleDuration);
                if (enableLogging)
                {
                    battle.Logger += Console.WriteLine;
                }
                WarriorOfLight hero = rotation(battle, stats);
                battle.Simulate(hero);
                totalPotency = battle.TotalPotency;

                if(enableLogging)
                {
                    Console.WriteLine();
                    Console.WriteLine("Breakdown by Ability");
                    foreach(var breakdown in battle.PotencyBySource)
                    {
                        double percent = breakdown.Value / battle.TotalPotency * 100;
                        Console.WriteLine($"  {breakdown.Key}: {breakdown.Value:#.0} ({percent:0.00}%)");
                    }
                }
            }
            else if(CurrentSimulationRules is RandomSimulator)
            {
                double[] potencies = new double[monteCarloCount];
                Stopwatch timer = new Stopwatch();
                timer.Start();
                int count = 0;

                Battle firstBattle = new Battle(new RandomSimulator(), sampleDuration);
                {
                    WarriorOfLight hero = rotation(firstBattle, stats);
                    firstBattle.Simulate(hero);
                    potencies[0] = firstBattle.TotalPotency;
                }

                Parallel.For(1, monteCarloCount, (i) =>
                {
                    if(timer.Elapsed > monteCarloDuration)
                    {
                        return;
                    }
                    Battle battle = new Battle(new RandomSimulator(), sampleDuration);
                    WarriorOfLight hero = rotation(battle, stats);
                    battle.Simulate(hero);
                    potencies[i] = battle.TotalPotency;
                });
                timer.Stop();

                double minPotency = double.MaxValue, maxPotency = double.MinValue;
                for(int i = 0; i < monteCarloCount; ++i)
                {
                    totalPotency += potencies[i];
                    if (potencies[i] != 0)
                    {
                        minPotency = Math.Min(minPotency, potencies[i]);
                        maxPotency = Math.Max(maxPotency, potencies[i]);
                        count++;
                    }
                }
                totalPotency /= count;

                if (enableLogging)
                {
                    foreach (var breakdown in firstBattle.PotencyBySource)
                    {
                        double percent = breakdown.Value / firstBattle.TotalPotency * 100;
                        Console.WriteLine($"  {breakdown.Key}: {breakdown.Value:#.0} ({percent:0.00}%)");
                    }
                    Console.WriteLine();
                    Console.WriteLine($"Over {count} trials, PPS ranged from {minPotency / sampleDuration:#.0} to {maxPotency / sampleDuration:#.0}");
                }
            }

            return totalPotency;
        }
        
    }
}
