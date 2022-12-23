namespace Diffusion;

using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public class Simulation
{
    List<(int h, int w)> elfPositions; // relative to the arbitrary starting point
    List<(int h, int w)> disp;
    Loop<char> directionCycle;

    public Simulation(string fname)
    {
	var lines = System.IO.File.ReadAllText(fname).Split("\n");
	elfPositions = new List<(int h, int w)>();
	foreach (var j in Enumerable.Range(0,lines.Length))
	{
	    var line = lines[j];
	    elfPositions.AddRange((from k in Enumerable.Range(0,line.Length)
				   where line[k] == '#'
				   select (j,k)).ToList());
	}
	var directions = new List<char>(){'N','S','W','E'};
	directionCycle = new Loop<char>(directions); // keeps track of our ordering
	disp = new List<(int h, int w)>(){(1,0),(1,1),(0,1),(-1,1),(-1,0),(-1,-1),(0,-1),(1,-1)};
    }
    public void ExecuteRounds(int N)
    {
	foreach (var n in Enumerable.Range(1,N))
	{
	    var num = ExecuteRound();
	    // Console.WriteLine($"During round {n}, {num} Elves were moved.");
	}
	Console.WriteLine($"After {N} rounds the number of empty ground tiles in the minimal box is {Area()}.");
    }
    public void ExecuteUntilStalled()
    {
	int num = ExecuteRound(), n = 1;
	while ( num > 0 )
	{
	    // if (n % 50 == 0) Console.WriteLine($"During round {n}, {num} Elves were moved.");
	    num = ExecuteRound();
	    n += 1;
	}
	Console.WriteLine($"At round {n} we need to move {num} Elves so we're done.");
    }
    public int ExecuteRound()
    {
	// part 1: generate position proposals depending on their surroundings
	// and the state of the directionCycle
	var dirPriority = (from n in Enumerable.Range(0,4) select directionCycle[n]).ToList();
	// Console.WriteLine($"Checking directions in priority: {dirPriority[0]} -> {dirPriority[1]} -> {dirPriority[2]} -> {dirPriority[3]}");
	var proposals = new Dictionary<(int h, int w),(int h, int w)>();
	foreach (var p in elfPositions)
	{
	    // Console.WriteLine($"Considering possible moves for point {p}.");
	    var dOcc = (from d in disp
			let newp = (p.h+d.h,p.w+d.w)
			where elfPositions.Contains(newp)
			select d).ToList();
	    // Console.WriteLine($"Elves in {dOcc.Count()}/8 surrounding positions.");
	    if (dOcc.Count() > 0)
	    {
		foreach (var m in Enumerable.Range(0,4))
		{
		    if (!OccupiedAlong(dOcc, dirPriority[m]))
		    {
			switch (dirPriority[m])
			{
			    case 'N':
				// Console.WriteLine($"Propose that {p} move north.");
				proposals[p] = (p.h-1,p.w);
				break;
			    case 'E':
				// Console.WriteLine($"Propose that {p} move east.");
				proposals[p] = (p.h,p.w+1);
				break;
			    case 'S':
				// Console.WriteLine($"Propose that {p} move south.");
				proposals[p] = (p.h+1,p.w);
				break;
			    case 'W':
				// Console.WriteLine($"Propose that {p} move west.");
				proposals[p] = (p.h,p.w-1);
				break;
			}
			break;
		    }
		}
	    }
	    else
	    {
		// Console.WriteLine($"Propose that {p} stay put.");
		proposals[p] = p;
	    }
	}

	// part 2: check for conflicts in the proposals and update values if free
	var numMoved = 0;
	foreach (var p in proposals)
	{
	    // Console.WriteLine($"Checking {p.Key} -> {p.Value} for conflicts.");
	    var conflicted = false;
	    foreach (var q in proposals)
	    {
		if ( ( (p.Key.h != q.Key.h) || (p.Key.w != q.Key.w)) &&
		     (p.Value.h == q.Value.h) && (p.Value.w == q.Value.w) )
		{
		    // Console.WriteLine($"Removing conflicting proposal {q.Key}: {q.Value}");
		    conflicted = true;
		    proposals.Remove(q.Key);
		}
	    }
	    // don't change or count elves staying put
	    if (!conflicted && ((p.Key.h != p.Value.h) || (p.Key.w != p.Value.w)))
	    {
		// Console.WriteLine($"{p.Key} -> {p.Value} is clear to move.");
		elfPositions[elfPositions.IndexOf(p.Key)] = p.Value;
		numMoved += 1;
	    }
	}

	// finally, cycle the direction priorities
	directionCycle.Next();

	// to inform outer loop
	return numMoved;
    }
    public bool OccupiedAlong(List<(int h, int w)> displacements, char direction)
    {
	switch (direction)
	{
	    case 'N':
		return (from d in displacements
			where d.h == -1
			select d).Count() > 0;
	    case 'E':
		return (from d in displacements
			where d.w == +1
			select d).Count() > 0;;
	    case 'S':
		return(from d in displacements
		       where d.h == +1
		       select d).Count() > 0;
	    case 'W':
		return(from d in displacements
		       where d.w == -1
		       select d).Count() > 0;
	    default:
		throw new ArgumentException("Invalid direction.");
	}
    }
    public void PrintPositions()
    {
	PrintPositions("");
    }
    public void PrintPositions(string pad)
    {
	// only prints minimal box
	int boxPad = 0;
	var hvals = (from p in elfPositions
		     orderby p.h ascending
		     select p.h);
	int hmin = hvals.First()-boxPad, hmax = hvals.Last()+boxPad;
	var wvals = (from p in elfPositions
		     orderby p.w ascending
		     select p.w);
	int wmin = wvals.First()-boxPad, wmax = wvals.Last()+boxPad;
	foreach (var h in Enumerable.Range(hmin,hmax-hmin+1))
	{
	    foreach (var w in Enumerable.Range(wmin,wmax-wmin+1))
	    {
		if (elfPositions.Contains((h,w))) Console.Write($"#{pad}");
		else Console.Write($".{pad}");
	    }
	    Console.Write("\n");
	}
    }
    public int Area()
    {
	// get dimensions of smallest minimal box containing current position of elves
	var hvals = (from p in elfPositions
		     orderby p.h ascending
		     select p.h);
	int hmin = hvals.First(), hmax = hvals.Last();
	int height = hmax-hmin+1;
	var wvals = (from p in elfPositions
		     orderby p.w ascending
		     select p.w);
	int wmin = wvals.First(), wmax = wvals.Last();
	int width = wmax-wmin+1;
	return width*height-elfPositions.Count();
    }
}

public class Loop<T>
{
    // keeps track of infinite cycle of a finite set of items
    int index, cycleLength;
    List<T> elements;
    public int CycleLength
    {
	get { return cycleLength; }
    }
    public T Current
    {
	get { return elements[index]; }
    }
    public Loop(List<T> elem)
    {
	index = 0;
	elements = elem;
	cycleLength = elements.Count;
    }
    // indexer
    public T this[int ind]
    {
	get { return elements[(ind+index)%cycleLength]; }
    }
    // increment the repeater and return the next value
    public T Next()
    {
	index = (index+1)%cycleLength;
	return elements[index];
    }
}
