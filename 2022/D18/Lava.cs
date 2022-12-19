namespace Lava;

using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

class Cubes
{
    List<(int x, int y, int z)> cubePositions;
    Dictionary<(int x, int y, int z),int> exposedSurfaces;
    int totalSurfaceArea;
    public Cubes(string fname)
    {
	var pts = (from line in System.IO.File.ReadAllText(fname).Split("\n")
		   select (from c in line.Split(",")
			   select int.Parse(c)).ToArray()).ToArray();
	cubePositions = new List<(int x, int y, int z)>();
	foreach (var p in pts) cubePositions.Add(new (p[0],p[1],p[2]));

	exposedSurfaces = new Dictionary<(int x, int y, int z),int>(); // set to maximum until we calculate
	foreach (var p in cubePositions) exposedSurfaces[p] = 6;
	totalSurfaceArea = (from c in exposedSurfaces select c.Value).Sum();
    }
    public int ManhattenDistance((int x, int y, int z) p1, (int x, int y, int z) p2)
    {
	return Math.Abs(p1.x-p2.x)+Math.Abs(p1.y-p2.y)+Math.Abs(p1.z-p2.z);
    }
    public void ComputeExposedSurfaces()
    {
	foreach (var p in cubePositions)
	{
	    var shared = (from q in cubePositions
			  where ManhattenDistance(p,q) == 1
			  select q).Count();
	    exposedSurfaces[p] -= shared;
	}
	totalSurfaceArea = (from c in exposedSurfaces select c.Value).Sum();
	Console.WriteLine($"There are {totalSurfaceArea} faces exposed in this structure.");
    }
    public void CalculateExteriorArea()
    {
	// to calculate it directly we bound the points in some minimal box with padding, then
	// starting from some point on the boundary we use DFS to flood fill the outside
	var xVals = (from p in cubePositions
		     let x = p.x
		     orderby p.x ascending
		     select x).Distinct().ToList();
	var yVals = (from p in cubePositions
		     let y = p.y
		     orderby p.y ascending
		     select y).Distinct().ToList();
	var zVals = (from p in cubePositions
		     let z = p.z
		     orderby p.z ascending
		     select z).Distinct().ToList();
	int xMin = xVals.First()-1, xMax = xVals.Last()+2;
	int yMin = yVals.First()-1, yMax = yVals.Last()+2;
	int zMin = zVals.First()-1, zMax = zVals.Last()+2;
	int Lx = xMax-xMin+1, Ly = yMax-yMin+1, Lz = zMax-zMin+1;
	var boundingBox = new int[Lx,Ly,Lz];
	foreach (int j in Enumerable.Range(0,Lx))
	    foreach (int k in Enumerable.Range(0,Ly))
		foreach (int l in Enumerable.Range(0,Lz))
		    boundingBox[j,k,l] = 0;
	foreach (var p in cubePositions) boundingBox[p.x-xMin+1,p.y-yMin+1,p.z-zMin+1] = 1;

	// now let's perform a BFS, filling in positions encountered; note that each time a lava is
	// encountered by the search, that corresponds to an exposed face, so if we check if we're blocked
	// by the lava, we can get the total at the end of the iteration
	var queue = new Queue<(int x, int y, int z)>();
	(int x, int y, int z) fP = new (0,0,0); // we've padded box, so guaranteed to be outside
	boundingBox[fP.x, fP.y, fP.z] = 2;
	queue.Enqueue(fP);
	int exposed = 0;
	while ( queue.Count > 0)
	{
	    fP = queue.Dequeue();
	    if (fP.x-1 >= 0) // -x
	    {
		if ( boundingBox[fP.x-1,fP.y,fP.z] == 1 ) // lava, proceed no further but count surface
		    exposed += 1;
		else if ( boundingBox[fP.x-1,fP.y,fP.z] == 0 ) // unvisited, mark and add to queue
		{
		    boundingBox[fP.x-1,fP.y,fP.z] = 2;
		    queue.Enqueue(new (fP.x-1,fP.y,fP.z));
		}
	    }
	    if (fP.x+2 <= Lx) // +x
	    {
		if ( boundingBox[fP.x+1,fP.y,fP.z] == 1 ) // lava, proceed no further but count surface
		    exposed += 1;
		else if ( boundingBox[fP.x+1,fP.y,fP.z] == 0 ) // unvisited, mark and add to queue
		{
		    boundingBox[fP.x+1,fP.y,fP.z] = 2;
		    queue.Enqueue(new (fP.x+1,fP.y,fP.z));
		}
	    }
	    if (fP.y-1 >= 0) // -y
	    {
		if ( boundingBox[fP.x,fP.y-1,fP.z] == 1 ) // lava, proceed no further but count surface
		    exposed += 1;
		else if ( boundingBox[fP.x,fP.y-1,fP.z] == 0 ) // unvisited, mark and add to queue
		{
		    boundingBox[fP.x,fP.y-1,fP.z] = 2;
		    queue.Enqueue(new (fP.x,fP.y-1,fP.z));
		}
	    }
	    if (fP.y+2 <= Ly) // +y
	    {
		if ( boundingBox[fP.x,fP.y+1,fP.z] == 1 ) // lava, proceed no further but count surface
		    exposed += 1;
		else if ( boundingBox[fP.x,fP.y+1,fP.z] == 0 ) // unvisited, mark and add to queue
		{
		    boundingBox[fP.x,fP.y+1,fP.z] = 2;
		    queue.Enqueue(new (fP.x,fP.y+1,fP.z));
		}
	    }
	    if (fP.z-1 >= 0) // -z
	    {
		if ( boundingBox[fP.x,fP.y,fP.z-1] == 1 ) // lava, proceed no further but count surface
		    exposed += 1;
		else if ( boundingBox[fP.x,fP.y,fP.z-1] == 0 ) // unvisited, mark and add to queue
		{
		    boundingBox[fP.x,fP.y,fP.z-1] = 2;
		    queue.Enqueue(new (fP.x,fP.y,fP.z-1));
		}
	    }
	    if (fP.z+2 <= Lz) // +z
	    {
		if ( boundingBox[fP.x,fP.y,fP.z+1] == 1 ) // lava, proceed no further but count surface
		    exposed += 1;
		else if ( boundingBox[fP.x,fP.y,fP.z+1] == 0 ) // unvisited, mark and add to queue
		{
		    boundingBox[fP.x,fP.y,fP.z+1] = 2;
		    queue.Enqueue(new (fP.x,fP.y,fP.z+1));
		}
	    }
	}
	Console.WriteLine($"After flood filling the lava blob, we've encountered {exposed} units of area.");
	// foreach (var z in Enumerable.Range(0,Lz))
	// {
	//     foreach (var y in Enumerable.Range(0,Ly))
	//     {
	// 	foreach (var x in Enumerable.Range(0,Lx)) Console.Write($"{boundingBox[x,y,z]} ");
	// 	Console.Write("\n");
	//     }
	//     Console.Write("\n");
	// }
    }
}
