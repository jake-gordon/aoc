namespace Climbing;

using System.Collections.Generic;
using System.Linq;

class Map
{
    int height, width;
    int[,] elevation;
    char[,] elevationSymbols;
    int[] start, finish;
    int[][] possibleStarts;
    int[][] moveDirections = new int[][] {
	new int[] {+1,0},
	new int[] {-1,0},
	new int[] {0,+1},
	new int[] {0,-1}};
    public int[] Start
    {
	get { return start; }
	set { start = value; }
    }
    public int[] Finish
    {
	get { return finish; }
	set { finish = value; }
    }
    public Map(string fname)
    {
	char[][] symbols = (from line in System.IO.File.ReadAllText(fname).Split("\n")
			    where line.Length > 0
			    select line.ToCharArray()).ToArray();
	height = symbols.Length;
	width = symbols[0].Length;
	elevation = new int[height,width];
	elevationSymbols = new char[height,width];
	start = new int[2];
	finish = new int[2];
	foreach(int j in Enumerable.Range(0,height))
	{
	    foreach (int k in Enumerable.Range(0,width))
	    {
		if (symbols[j][k] == 'S')
		{
		    start = new int[] {j,k};
		    elevation[j,k] = ((int)('a'-'a')); // a = 0, b = 1, ... z = 25;
		    elevationSymbols[j,k] = 'a';
		}
		else if (symbols[j][k] == 'E')
		{
		    finish = new int[] {j,k};
		    elevation[j,k] = ((int)('z'-'a'));
		    elevationSymbols[j,k] = 'z';
		}
		else
		{
		    elevation[j,k] = ((int)(symbols[j][k]-'a'));
		    elevationSymbols[j,k] = symbols[j][k];
		}
	    }
	}
	possibleStarts = (from p in FindLevelSet(0)
			  let t = new Tuple<int,int>(p[0],p[1])
			  where FindNextMoves(t).Length > 0
			  select p).ToArray(); // starting points that aren't trapped
    }
    public void PrintElevations(bool markers)
    {
	if (markers) Console.WriteLine("Marked map:");
	else Console.WriteLine("Unmarked map:");
	foreach(int j in Enumerable.Range(0,height))
	{
	    foreach(int k in Enumerable.Range(0,width))
	    {
		if (markers && start[0] == j && start[1] == k)
		    Console.Write($"S");
		else if (markers && finish[0] == j && finish[1] == k)
		    Console.Write($"E");
		else
		{
		    Console.Write($"{elevationSymbols[j,k]}");
		}
	    }
	    Console.Write("\n");
	}
	Console.Write("\n");
    }
    public void PrintOptimalSolution(bool map)
    {
	// int[][] onePath = DijkstraAlgorithm(start); // fine for part 1 but too slow for part 2, BFS instead
	int[][] onePath = BFSAlgorithm(start);
	Console.WriteLine($"An optimal path starting at ({start[0]},{start[1]}) and ending at ({finish[0]},{finish[1]}) has length {onePath.Length-1}.");
	char[,] optimalMap = new char[height,width];
	foreach (int j in Enumerable.Range(0,height))
	{
	    foreach (int k in Enumerable.Range(0,width))
	    {
		optimalMap[j,k] = '.'; // elevationSymbols[j,k];
	    }
	}
	foreach (int j in Enumerable.Range(0,onePath.Length-1)) // replace characters with direction
	{
	    int[] linkDifference = new int[] {onePath[j+1][0]-onePath[j][0],onePath[j+1][1]-onePath[j][1]};
	    if (linkDifference[0] == moveDirections[0][0] && linkDifference[1] == moveDirections[0][1])
		optimalMap[onePath[j][0],onePath[j][1]] = 'v';
	    if (linkDifference[0] == moveDirections[1][0] && linkDifference[1] == moveDirections[1][1])
		optimalMap[onePath[j][0],onePath[j][1]] = '^';
	    if (linkDifference[0] == moveDirections[2][0] && linkDifference[1] == moveDirections[2][1])
		optimalMap[onePath[j][0],onePath[j][1]] = '>';
	    if (linkDifference[0] == moveDirections[3][0] && linkDifference[1] == moveDirections[3][1])
		optimalMap[onePath[j][0],onePath[j][1]] = '<';
	}
	optimalMap[start[0],start[1]] = 'S';
	optimalMap[finish[0],finish[1]] = 'E';
	foreach(int j in Enumerable.Range(0,height))
	{
	    foreach(int k in Enumerable.Range(0,width))
	    {
		if (map) Console.Write($"{optimalMap[j,k]}");
	    }
	    if (map) Console.Write("\n");
	}
	if (map) Console.Write("\n");
    }
    public void PrintGlobalOptimalSolution(bool map)
    {
	int[][] onePath = (from p in possibleStarts
			   let path = BFSAlgorithm(p)
			   where path.Length > 0
			   orderby path.Length ascending
			   select path).First();

	var optStart = onePath[0];
	var optFinish = onePath[onePath.Length-1];
	Console.WriteLine($"A global optimal path starting at ({optStart[0]},{optStart[1]}) and ending at ({optFinish[0]},{optFinish[1]}) has length {onePath.Length-1}.");

	char[,] optimalMap = new char[height,width];
	foreach (int j in Enumerable.Range(0,height))
	{
	    foreach (int k in Enumerable.Range(0,width))
	    {
		optimalMap[j,k] = '.'; // elevationSymbols[j,k];
	    }
	}
	foreach (int j in Enumerable.Range(0,onePath.Length-1)) // replace characters with direction
	{
	    int[] linkDifference = new int[] {onePath[j+1][0]-onePath[j][0],onePath[j+1][1]-onePath[j][1]};
	    if (linkDifference[0] == moveDirections[0][0] && linkDifference[1] == moveDirections[0][1])
		optimalMap[onePath[j][0],onePath[j][1]] = 'v';
	    if (linkDifference[0] == moveDirections[1][0] && linkDifference[1] == moveDirections[1][1])
		optimalMap[onePath[j][0],onePath[j][1]] = '^';
	    if (linkDifference[0] == moveDirections[2][0] && linkDifference[1] == moveDirections[2][1])
		optimalMap[onePath[j][0],onePath[j][1]] = '>';
	    if (linkDifference[0] == moveDirections[3][0] && linkDifference[1] == moveDirections[3][1])
		optimalMap[onePath[j][0],onePath[j][1]] = '<';
	}
	optimalMap[optStart[0],optStart[1]] = 'S';
	optimalMap[optFinish[0],optFinish[1]] = 'E';
	foreach(int j in Enumerable.Range(0,height))
	{
	    foreach(int k in Enumerable.Range(0,width))
	    {
		if (map) Console.Write($"{optimalMap[j,k]}");
	    }
	    if (map) Console.Write("\n");
	}
	if (map) Console.Write("\n");
    }
    public int[][] DijkstraAlgorithm(int[] begin) // not ideal for unweighted graphs, but fine for part 1
    {
	// key is coordinate {j,k} and value is distance, each edge of graph has unit weight
	int bigval = (int)Math.Pow(width*height,2); // infinity
	Dictionary<Tuple<int,int>,int> visited = new Dictionary<Tuple<int,int>,int>();
	Dictionary<Tuple<int,int>,int> unvisited = new Dictionary<Tuple<int,int>,int>();
	foreach (int j in Enumerable.Range(0,height))
	{
	    foreach (int k in Enumerable.Range(0,width))
	    {
		unvisited.Add(new Tuple<int,int>(j,k), ((j == begin[0]) && (k == begin[1])) ? 0 : bigval );
	    }
	}
	var origin = new Tuple<int,int>(begin[0],begin[1]);
	var currentNode = origin; // process start position to begin
	var currentVal = unvisited[currentNode];
	int[][] allowedMoves = FindNextMoves(currentNode);
	foreach (int[] p in allowedMoves)
	{
	    var t = new Tuple<int,int>(p[0],p[1]);
	    if (unvisited.ContainsKey(t) && unvisited[t] > currentVal) unvisited[t] = currentVal + 1;
	}
	unvisited.Remove(currentNode);
	visited.Add(currentNode,currentVal);
	var target = new Tuple<int,int>(finish[0],finish[1]);

	while (!visited.ContainsKey(target)) //	until we've visited the target, keep going
	{
	    currentNode = (from kv in unvisited
			   orderby kv.Value
			   select kv.Key).First(); // visit closest one next
	    currentVal = unvisited[currentNode];
	    allowedMoves = FindNextMoves(currentNode);
	    foreach (int[] p in allowedMoves)
	    {
		var t = new Tuple<int,int>(p[0],p[1]);
		if (unvisited.ContainsKey(t) && unvisited[t] > currentVal) unvisited[t] = currentVal + 1;
	    }
	    unvisited.Remove(currentNode);
	    visited.Add(currentNode,currentVal);
	}
	int pathLength = visited[target];
	int[][] oneOptimalPath = new int[][] { finish };
	while ( pathLength > 0)
	{
	    int[] previousPoint;
	    var previousPoints = (from kv in visited
				  let v = new int[] {kv.Key.Item1,kv.Key.Item2}
				  let d = kv.Value
				  let prev = oneOptimalPath[oneOptimalPath.Length-1]
				  let diff = new int[] {prev[0]-v[0],prev[1]-v[1]}
				  where d == pathLength-1
				  where diff[0]*diff[0] + diff[1]*diff[1] == 1
				  select v).ToArray();
	    if (previousPoints.Length != 0)
	    {
		previousPoint = previousPoints.First();
		oneOptimalPath = oneOptimalPath.Append(previousPoint).ToArray();
	    }
	    pathLength -= 1;
	}
	Array.Reverse(oneOptimalPath);
	return oneOptimalPath;
    }
    public int[][] BFSAlgorithm(int[] begin)
    {
	// discourages backtracking, meant for graphs
	bool[,] visited = new bool[height,width];
	foreach (int j in Enumerable.Range(0,height))
	    foreach (int k in Enumerable.Range(0,width))
		visited[j,k] = false;

	// p1 -> p2 is stored as cameFrom[p2] = p1
	var cameFrom = new Dictionary<Tuple<int,int>,Tuple<int,int>>(); // to reconstruct the path,
	cameFrom[new Tuple<int,int>(begin[0],begin[1])] = new Tuple<int,int>(-1,-1); // null value

	var frontier = new Queue<int[]>();
	frontier.Enqueue(begin);
	visited[begin[0],begin[1]] = true;

	int counter = 0;
	int[] current = start;
	while ( frontier.Count > 0 )
	{
	    current = frontier.Dequeue();
	    if ( (current[0] == finish[0]) && (current[1] == finish[1])) break;

	    int[][] newPositions = FindNextMoves(current);
	    foreach(int[] p in newPositions)
	    {
		if ( !visited[p[0],p[1]] )
		{
		    frontier.Enqueue(p);
		    visited[p[0],p[1]] = true;
		    cameFrom[new Tuple<int,int>(p[0],p[1])] = new Tuple<int,int>(current[0],current[1]);
		}
	    }
	    counter += 1;
	}
	var path = new int[][] {};
	if ( (current[0] == finish[0]) && (current[1] == finish[1]))
	{
	    var curr = new Tuple<int,int>(current[0],current[1]);
	    while ( (curr.Item1 >= 0) && (curr.Item2 >= 0) )
	    {
		path = path.Append(new int[] {curr.Item1, curr.Item2}).ToArray();
		curr = cameFrom[curr];
	    }
	}
	return path.Reverse().ToArray();
    }
    public int[][] FindNextMoves(int[] current)
    {
	return (from m in moveDirections
		let p = new int[] {current[0] + m[0], current[1] + m[1]}
		where (0 <= p[0]) && (p[0] < height)
		where (0 <= p[1]) && (p[1] < width)
		where elevation[p[0],p[1]]-elevation[current[0],current[1]] <= 1
		select p).ToArray();
    }
    public int[][] FindNextMoves(Tuple<int,int> t)
    {
	int[] x = new int[] {t.Item1,t.Item2};
	return (from m in moveDirections
		let p = new int[] {x[0] + m[0], x[1] + m[1]}
		where (0 <= p[0]) && (p[0] < height)
		where (0 <= p[1]) && (p[1] < width)
		where elevation[p[0],p[1]]-elevation[x[0],x[1]] <= 1
		select p).ToArray();
    }
    public int[][] FindLevelSet(int h) // all indices where the elevation is h
    {
	int[][] indices = new int[][] {};
	foreach (int j in Enumerable.Range(0,height))
	{
	    foreach (int k in Enumerable.Range(0,width))
	    {
		if (elevation[j,k] == h) indices = indices.Append(new int[] {j,k}).ToArray();
	    }
	}
	return indices;
    }
}
