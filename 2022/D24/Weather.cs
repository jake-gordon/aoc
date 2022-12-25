namespace Weather;

using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public class Blizzard
{
    int[,] map;
    int height, width;
    (int h, int w) start, finish;
    public (int h, int w) Start
    {
	get { return start; }
    }
    public (int h, int w) Finish
    {
	get { return finish; }
    }
    (int h, int w, int t) currentState;
    public (int h, int w, int t) State
    {
	get { return currentState; }
    }
    List<(int h, int w, char d)> storms;
    // pre-compute blizzard positions and index by time
    List<List<(int h, int w, char d)>> activity;

    public Blizzard(string fname)
    {
	var lines = (from l in System.IO.File.ReadAllText(fname).Split("\n")
		     select l.ToCharArray()).ToArray();

	height = lines.Length;
	width = lines[0].Length;
	map = new int[height,width];
	foreach (var j in Enumerable.Range(0,height))
	    foreach (var k in Enumerable.Range(0,width))
		map[j,k] = (lines[j][k] == '#') ? 1 : 0;
	start = (from k in Enumerable.Range(0,width)
		 where map[0,k] == 0
		 select (0,k)).First();
	currentState = (start.h,start.w,0);
	finish = (from k in Enumerable.Range(0,width)
		  where map[height-1,k] == 0
		  select (height-1,k)).First();

	storms = (from j in Enumerable.Range(0,height)
		  from k in Enumerable.Range(0,width)
		  let c = lines[j][k]
		  where (c != '#') && (c != '.')
		  select (j,k,c)).ToList();

	// pre-compute storm positions
	int t = 0, tMax = 1500;
	var current = storms;
	activity = new List<List<(int h, int w, char d)>>();
	activity.Add(current);
	while (t < tMax)
	{
	    current = UpdateStorms(current);
	    activity.Add(current);
	    t += 1;
	}
    }
    public void UpdateStorms()
    {
	storms = UpdateStorms(storms);
    }
    public List<(int h, int w, char d)> UpdateStorms(List<(int h, int w, char d)> stormState)
    {
	var newState = new List<(int h, int w, char d)>();
	foreach (int j in Enumerable.Range(0,stormState.Count))
	{
	    var p = stormState[j];
	    // baking in some assumptions here, we'll never encounter a situation
	    // where a storm reaches the edge of the map, so I just need to check
	    // if the next position is blocked and wrap around predictably; this is
	    // find because all the storms in the start & end columns move L <-> R
	    switch (p.d)
	    {
		case '>':
		    if (map[p.h,p.w+1] == 1) newState.Add((p.h,1,p.d));
		    else newState.Add((p.h,p.w+1,p.d));
		    break;
		case '<':
		    if (map[p.h,p.w-1] == 1) newState.Add((p.h,width-2,p.d));
		    else newState.Add((p.h,p.w-1,p.d));
		    break;
		case 'v':
		    if (map[p.h+1,p.w] == 1) newState.Add((1,p.w,p.d));
		    else newState.Add((p.h+1,p.w,p.d));
		    break;
		case '^':
		    if (map[p.h-1,p.w] == 1) newState.Add((height-2,p.w,p.d));
		    else newState.Add((p.h-1,p.w,p.d));
		    break;
	    }
	}
	return newState;
    }
    public void BFS((int h, int w) end)
    {
	// try to perform a BFS using the current position, path length, and time indexing state of storm
	Console.WriteLine($"Considering path {currentState} -> {end}.");
	(int h, int w, int t) state = currentState;
	var visited = new HashSet<(int h, int w, int t)>();
	var queue = new Queue<(int h, int w, int t)>();
	queue.Enqueue(state);

	while (queue.Count > 0)
	{
	    state = queue.Dequeue();

	    // we're at the destination
	    if (state.h == end.h && state.w == end.w) break;

	    // skip if we've seen it before
	    if (visited.Contains(state)) continue;
	    else visited.Add(state);

	    // position of storm at next point in time
	    var stormPositions = (from s in activity[state.t+1]
				  select (s.h,s.w)).ToList();

	    // we could stay where we are if no storm exists there now
	    if ( !stormPositions.Contains((state.h,state.w)) )
		queue.Enqueue((state.h,state.w,state.t+1));
	    // move right if no storm exists there now
	    if ( !stormPositions.Contains((state.h,state.w+1)) && map[state.h,state.w+1] == 0 )
		queue.Enqueue((state.h,state.w+1,state.t+1));
	    // move left if no storm exists there now
	    if ( !stormPositions.Contains((state.h,state.w-1)) && map[state.h,state.w-1] == 0 )
		queue.Enqueue((state.h,state.w-1,state.t+1));
	    // move down if no storm exists there now
	    if ( !stormPositions.Contains((state.h+1,state.w)) && (state.h < height-1) && map[state.h+1,state.w] == 0 )
		queue.Enqueue((state.h+1,state.w,state.t+1));
	    // move up if no storm exists there now
	    if ( !stormPositions.Contains((state.h-1,state.w)) && (state.h >= 1) && map[state.h-1,state.w] == 0 )
		queue.Enqueue((state.h-1,state.w,state.t+1));
	}
	Console.WriteLine($"Reached destination after minute {state.t}: {(state.h, state.w)}.");
	currentState = state;
    }
    public void PrintMap()
    {
	PrintMap(storms,(start.h,start.w-1)," ");
    }
    public void PrintMap(List<(int h, int w, char d)> stormState, (int h, int w) current, string pad)
    {
	List<(int h, int w, char d)> stChars = (from s in stormState
						let cnt = (from p in stormState
							   where ( s.h == p.h && s.w == p.w )
							   select p).Count()
						let c =	cnt == 1 ? s.d : ( cnt < 9 ? Char.Parse($"{cnt}") : '+')
						select (s.h, s.w, c)).ToList();
	foreach (var j in Enumerable.Range(0,height))
	{
	    foreach (var k in Enumerable.Range(0,width))
	    {
		var stc  = (from s in stChars
			    where (s.h == j && s.w == k)
			    select s.d);
		if ( stc.Count() > 0 ) Console.Write($"{stc.First()}{pad}");
		else if ((current.h == j) && (current.w == k)) Console.Write($"o{pad}");
		else if ((start.h == j) && (start.w == k)) Console.Write($"S{pad}");
		else if ((finish.h == j) && (finish.w == k)) Console.Write($"F{pad}");
		else
		{
		    switch (map[j,k])
		    {
			case 0:
			    Console.Write($".{pad}");
			    break;
			case 1:
			    Console.Write($"#{pad}");
			    break;
		    }
		}
	    }
	    Console.Write("\n");
	}
    }
}
