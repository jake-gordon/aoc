namespace Sand;

using System.Collections.Generic;
using System.Linq;

public class Cave
{
    char[,] caveMap;
    int[] xvals, yvals; // xvals[j] is physical	position at index j
    int[][] rocks;
    int xmin,xmax,ymin,ymax;
    int height,width;
    int xSpout, spoutInd;
    int sandDropped; // number of grains dropped successfully
    public Cave(string fname, bool infiniteFloor)
    {
	string[] obstructions = System.IO.File.ReadAllText(fname).Split("\n");
	var lists = (from o in obstructions
		     let pts = (from p in o.Split(" -> ")
				let split = p.Split(",")
				select new int[] { int.Parse(split[0]), int.Parse(split[1])}).ToArray()
		     select pts).ToArray();

	sandDropped = 0;
	xSpout = 500;

	xvals = new int[] {};
	yvals = new int[] {};
	foreach (var l in lists)
	{
	    foreach (var p in l)
	    {
		xvals = xvals.Append(p[0]).ToArray();
		yvals = yvals.Append(p[1]).ToArray();
	    }
	}

	Array.Sort(xvals);
	Array.Sort(yvals);
	xmin = xvals.First();
	xmax = xvals.Last();
	ymin = Math.Min(yvals.First(),0);
	ymax = yvals.Last();
	if (infiniteFloor)
	{
	    ymax += 2; // make our array 2 deeper
	    xmin = Math.Min(xmin, xSpout-ymax); // widen to accommodate widest possible spread
	    xmax = Math.Max(xmax, xSpout+ymax);
	}

	width = xmax-xmin+1;
	height = ymax-ymin+1;

	xvals = (from j in Enumerable.Range(0,height)
		 select xmin + j).ToArray();
	yvals = (from j in Enumerable.Range(0,width)
		 select ymin + j).ToArray();

	spoutInd = IndexOfDistance(xSpout);

	rocks = new int[][] {};
	foreach (var l in lists)
	{
	    foreach (int j in Enumerable.Range(0,l.Length-1))
	    {
		// Console.WriteLine($"{l[j][0]} {l[j][1]} -> {l[j+1][0]} {l[j+1][1]}");
		if (l[j][0] == l[j+1][0]) // vertical
		{
		    int miny = Math.Min(l[j][1],l[j+1][1]);
		    int maxy = Math.Max(l[j][1],l[j+1][1]);
		    foreach (var k in Enumerable.Range(miny,maxy-miny+1))
		    {
			rocks = rocks.Append(new int[] {k,l[j][0]-xmin}).ToArray();
		    }
		}
		if (l[j][1] == l[j+1][1]) // horizontal
		{
		    int minx = Math.Min(l[j][0],l[j+1][0])-xmin;
		    int maxx = Math.Max(l[j][0],l[j+1][0])-xmin;
		    foreach (var k in Enumerable.Range(minx,maxx-minx+1))
		    {
			rocks = rocks.Append(new int[] {l[j][1], k}).ToArray();
		    }
		}
	    }
	}
	if (infiniteFloor)
	{
	    foreach (int j in Enumerable.Range(0,width))
		rocks = rocks.Append(new int[] {ymax,j}).ToArray();
	}

	caveMap = new char[height,width];
	foreach (int j in Enumerable.Range(0,height))
	{
	    foreach (int k in Enumerable.Range(0,width))
	    {
		caveMap[j,k] = '.';
		if (j == 0 && k == spoutInd) caveMap[j,k] = '+';
	    }
	}
	foreach(int[] r in rocks)
	{
	    caveMap[r[0],r[1]] = '#';
	}
    }
    public int IndexOfDistance(int x)
    {
	return  (from p in xvals.Select((v,j) => new {j,v})
		 where p.v == xSpout
		 select p.j).First();
    }
    public void PrintCave()
    {
	foreach (int j in Enumerable.Range(0,height))
	{
	    foreach (int k in Enumerable.Range(0,width))
	    {
		Console.Write($"{caveMap[j,k]} ");
	    }
	    Console.Write("\n");
	}
    }
    public void DropSand()
    {
	bool drop = DropSandGrain(); // test if we need to drop more
	while(drop)
	{
	    drop = DropSandGrain();
	}
	Console.WriteLine($"After dropping {sandDropped-1} grains of sand, the stream has started entering the abyss.");
	// PrintCave();
    }
    public bool DropSandGrain() // value indicates if we need to drop more sand
    {
	bool enteredAbyss = false;
	bool isStopped = false;
	int[] current = new int[] {0,spoutInd};
	int[] next = new int[] {};
	while(!isStopped && !enteredAbyss)
	{
	    next = NextMove(current);
	    if ( next[0] == -1 && next[1] == -1)
	    {
		// Console.WriteLine($"{current[0]} {current[1]}");
		enteredAbyss = true;
		caveMap[current[0],current[1]] = 'A';
	    }
	    if ( next[0] == -1 && next[1] == 0 )
	    {
		// Console.WriteLine($"{current[0]} {current[1]}");
		isStopped = true;
		caveMap[current[0],current[1]] = 'o';
	    }
	    else // try the next step
	    {
		// Console.WriteLine($"{current[0]} {current[1]}");
		current = next;
	    }
	}
	sandDropped += 1;
	if (enteredAbyss) return false; // no need to drop any more, we've reached the tipping point
	else if (isStopped) return true; // sand dropped is stopped normally, keep going
	else throw new ArgumentException("Neither stopped nor in the abyss!?!");
    }
    public void DropSandWithFloor()
    {
	bool drop = DropSandGrainWithFloor(); // do we need to drop more?
	while (drop)
	{
	    drop = DropSandGrainWithFloor();
	}
	Console.WriteLine($"After dropping {sandDropped} grains of sand, the spout is blocked.");
	// PrintCave();
    }
    public bool DropSandGrainWithFloor() // value indicates if we need to drop more sand
    {
	// try dropping just before so it attempts moving down into spout
	int[] current = new int[] {-1,spoutInd};
	int[] next = NextMove(current);
	// Console.WriteLine($"{next[0]} {next[1]}");
	// if the next move is on either side of the spout position, it's blocked
	if ( (next[0] == 0 && next[1] == current[1]-1) || (next[0] == 0 && next[1] == current[1]+1) )
	{
	    // Console.WriteLine($"BLOCKED {current[0]} {current[1]}");
	    caveMap[0,spoutInd] = 'B';
	    return false; // can't drop any more sand, spout is blocked
	}
	else //	not stuck, proceed to loop through moves and record at the end
	{
	    bool isStopped = false;
	    while ( !isStopped )
	    {
		current = next;
		next = NextMove(current);
		if ( next[0] == -1 && next[1] == 0 ) // stuck
		{
		    // Console.WriteLine($"STUCK {current[0]} {current[1]}");
		    caveMap[current[0],current[1]] = 'o';
		    isStopped = true;
		}
		else // keep looping
		{
		    // Console.WriteLine($"MOVE FROM {current[0]} {current[1]}");
		}
	    }
	    sandDropped += 1;
	    return true; // got stuck in a normal position, need to drop more sand
	}
    }
    public int[] NextMove(int[] point) // get next point based on current state of map
    {
	// try to move down, if unsuccessful try to move down diagonally
	// grid is constructed such that it's only as wide as the extremal rocks
	// if when I try to move down I go beyond the height, I have entered the abyss
	// if when I try to move diagonally I go beyond the width or height, I have entered the abyss
	// Console.WriteLine($"Trying down from ({point[0]},{point[1]})!");
	if ( point[0] + 1 == height ) // we've made it to the bottom without encountering obstacles
	{
	    // Console.WriteLine("Next move is abyss!");
	    return new int[] {-1,-1}; // marker to signify abyss
	}
	else if ( caveMap[point[0]+1,point[1]] == '.' || caveMap[point[0]+1,point[1]] == '+') // all clear, move down
	{
	    return new int[] {point[0]+1,point[1]};
	}
	else //	try moving downwards diagonally left then right, checking for the abyss
	{
	    // Console.WriteLine($"Trying left from ({point[0]},{point[1]})!");
	    if ( (point[0]+1==height) || (point[1]-1<0)) // made it clear to the left side
	    {
		// Console.WriteLine("Next move is abyss!");
		return new int[] {-1,-1}; // marker to signify abyss
	    }
	    else if ( caveMap[point[0]+1,point[1]-1] == '.' ) // all clear, move left diagonally
	    {
		// Console.WriteLine("Moving left!");
		return new int[] {point[0]+1,point[1]-1};
	    }
	    else // try moving diagonally right
	    {
		// Console.WriteLine($"Trying right from ({point[0]},{point[1]})!");
		if ( (point[0]+1==height) || (point[1]+1==width)) // made it clear to the right side
		{
		    // Console.WriteLine("Next move is abyss!");
		    return new int[] {-1,-1}; // marker to signify abyss
		}
		else if ( caveMap[point[0]+1,point[1]+1] == '.' ) // all clear, move right diagonally
		{
		    // Console.WriteLine("Moving right!");
		    return new int[] {point[0]+1,point[1]+1};
		}
		else //	no possible moves, we're stuck
		{
		    // Console.WriteLine("Stuck!");
		    return new int[] {-1,0}; // marker to signify we're out of moves
		}
	    }
	}
    }
}
