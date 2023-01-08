namespace Robots;

using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

class Escavation
{
    List<(int oRo, int cRo, int obRo, int obRc, int gRo, int gRob)> blueprints;
    public Escavation(string fname)
    {
	var bprintLines = System.IO.File.ReadAllText(fname).Split("\n");
	Regex rex = new Regex(@"\d+");
	var costs = (from l in bprintLines
		     select (from m in rex.Matches(l.Split(":")[1])
			     select int.Parse(m.Value)).ToList()).ToList();
	blueprints = new List<(int oRo, int cRo, int obRo, int obRc, int gRo, int gRob)>();
	foreach (var c in costs) blueprints.Add( new (c[0], c[1], c[2], c[3], c[4], c[5]));
    }
    public void SimulateQualityLevels()
    {
	int qualityLevel = 0;
	foreach (var j in Enumerable.Range(0,blueprints.Count))
	{
	    qualityLevel += (j+1)*SimulateStrategy(blueprints[j],24);
	}
	Console.WriteLine($"The sum of all quality levels of the blueprints is {qualityLevel}.");
    }
    public void SimulateLargest()
    {
	int product = 1;
	var remainingBlueprints = blueprints.Take(3);
	foreach (var b in remainingBlueprints) product *= SimulateStrategy(b,32);
	Console.WriteLine($"The product of the remaining blueprints is {product}.");
    }
    public int SimulateStrategy((int oRo, int cRo, int obRo, int obRc, int gRo, int gRob) bprint, int timeLimit)
    {
	/*
	  Attempt a BFS of the solution by keeping a tuple keeping track of the
	  time, # of robots, and current stock of supplies; will introduce
	  optimizations as needed:

	  1. Since we can only create one robot per turn, there's no point keeping
	  more robots than the most expensive one to create.

	  2. We can trim the stock to whatever is spendable in the remaining time.
	*/

	var tmp = new int[] {bprint.oRo,bprint.cRo,bprint.obRo,bprint.gRo};
	int maxOreRobots = tmp.Max();
	int maxClayRobots = bprint.obRc;
	int maxObsidianRobots = bprint.gRob;

	int maxGeodes = 0;
	(int t, int oRN, int cRN, int obRN, int gRN, int oN, int cN, int obN, int gN) p = (timeLimit,1,0,0,0,0,0,0,0);
	var queue = new Queue<(int t, int oRN, int cRN, int obRN, int gRN, int oN, int cN, int obN, int gN)>();
	queue.Enqueue(p);
	var hashset = new HashSet<(int t, int oRN, int cRN, int obRN, int gRN, int oN, int cN, int obN, int gN)>();

	while (queue.Count > 0)
	{
	    p = queue.Dequeue();
	    maxGeodes = Math.Max(maxGeodes,p.gN);

	    // points to ignore processing:
	    // time's up
	    if (p.t == 0) continue;

	    // we've already encountered it
	    if (hashset.Contains(p)) continue;
	    else hashset.Add(p);

	    // more ore robots than we can ever use
	    p.oRN = Math.Min(p.oRN,maxOreRobots);
	    // more ore than we can ever spend
	    p.oN = Math.Min(p.oN,p.t*maxOreRobots-p.oRN*(p.t-1));

	    // more clay robots than we can ever use
	    p.cRN = Math.Min(p.cRN,maxClayRobots);
	    // more clay than we can ever spend
	    p.cN = Math.Min(p.cN,p.t*maxClayRobots-p.cRN*(p.t-1));

	    // more obsidian robots than we can ever use
	    p.obRN = Math.Min(p.obRN,maxObsidianRobots);
	    // more obsidian than we can ever spend
	    p.obN = Math.Min(p.obN,p.t*maxObsidianRobots-p.obRN*(p.t-1));

	    // evolve in time and add to queue
	    // don't build, just collect
	    queue.Enqueue((p.t-1,p.oRN,p.cRN,p.obRN,p.gRN,p.oN+p.oRN,p.cN+p.cRN,p.obN+p.obRN,p.gN+p.gRN));
	    // build an ore robot if we have enough
	    if (p.oN >= bprint.oRo)
		queue.Enqueue((p.t-1,p.oRN+1,p.cRN,p.obRN,p.gRN,p.oN+p.oRN-bprint.oRo,p.cN+p.cRN,p.obN+p.obRN,p.gN+p.gRN));
	    // build a clay robot if we have enough
	    if (p.oN >= bprint.cRo)
		queue.Enqueue((p.t-1,p.oRN,p.cRN+1,p.obRN,p.gRN,p.oN+p.oRN-bprint.cRo,p.cN+p.cRN,p.obN+p.obRN,p.gN+p.gRN));
	    // build an obsidian robot if we have enough
	    if ((p.oN >= bprint.obRo) && (p.cN >= bprint.obRc))
		queue.Enqueue((p.t-1,p.oRN,p.cRN,p.obRN+1,p.gRN,p.oN+p.oRN-bprint.obRo,p.cN+p.cRN-bprint.obRc,p.obN+p.obRN,p.gN+p.gRN));
	    // build a geode robot if we have enough
	    if ((p.oN >= bprint.gRo) && (p.obN >= bprint.gRob))
		queue.Enqueue((p.t-1,p.oRN,p.cRN,p.obRN,p.gRN+1,p.oN+p.oRN-bprint.gRo,p.cN+p.cRN,p.obN+p.obRN-bprint.gRob,p.gN+p.gRN));
	}
	// Console.WriteLine($"After examining {hashset.Count} states, the maximum number of geodes is {maxGeodes}.");
	return maxGeodes;
    }
    public void PrintBlueprints()
    {
	foreach (int j in Enumerable.Range(0,blueprints.Count))
	{
	    Console.WriteLine($"Blueprint {j:D2}:");
	    Console.WriteLine($"\tEach ore robot costs {blueprints[j].oRo} ore.");
	    Console.WriteLine($"\tEach clay robot costs {blueprints[j].cRo} ore.");
	    Console.WriteLine($"\tEach obsidian robot costs {blueprints[j].obRo} ore and {blueprints[j].obRc} clay.");
	    Console.WriteLine($"\tEach geode robot costs {blueprints[j].gRo} ore and {blueprints[j].gRob} obsidian.");
	}
    }
}
