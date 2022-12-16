namespace Network;

using System.Collections.Generic;
using System.Linq;


public class Caverns
{
    Dictionary<Tuple<int,int>,Tuple<int,int>> closestBeacon; // mapping sensor position to closest beacon
    Dictionary<Tuple<int,int>,int> closestBeaconDistance; // mapping sensor position to closest beacon distance
    List<Tuple<int,int>> sensors, beacons;
    // List<Tuple<int,int>> searchSpace;
    int dmin, dmax, wmin, wmax;
    // List<int> dvals, wvals; // all the values from dmin to dmax
    int depth, width;

    public Caverns(string fname, bool boxed)
    {
	closestBeacon = new Dictionary<Tuple<int,int>,Tuple<int,int>>();
	closestBeaconDistance = new Dictionary<Tuple<int,int>,int>();
	sensors = new List<Tuple<int,int>>();
	beacons = new List<Tuple<int,int>>();
	var sensorList = System.IO.File.ReadAllText(fname).Split("\n");
	foreach (var s in sensorList)
	{
	    string[] pair = s.Split(":");
	    int wval = int.Parse(pair[0].Split("at")[1].Split(",")[0].Split("=")[1]); // width, first coordinate (x)
	    int dval = int.Parse(pair[0].Split("at")[1].Split(",")[1].Split("=")[1]); // depth, second coordinate (y)
	    var sensorCoordinate = new Tuple<int,int>(wval, dval);
	    sensors.Add(sensorCoordinate);
	    wval = int.Parse(pair[1].Split("at")[1].Split(",")[0].Split("=")[1]);
	    dval = int.Parse(pair[1].Split("at")[1].Split(",")[1].Split("=")[1]);
	    var beaconCoordinate = new Tuple<int,int>(wval, dval);
	    beacons.Add(beaconCoordinate);
	    closestBeacon.Add(sensorCoordinate,beaconCoordinate);
	    closestBeaconDistance.Add(sensorCoordinate,ManhattenDistance(sensorCoordinate,beaconCoordinate));
	    // Console.WriteLine($"{sensorCoordinate} -> {closestBeacon[sensorCoordinate]} ({closestBeaconDistance[sensorCoordinate]})");
	}

	// find extremal coordinates to get width and depth of map
	var extremes = FindExtremeCoordinates(sensors.Concat(beacons).ToList());
	// Console.WriteLine(extremes);
	dmin = extremes.Item3;
	dmax = extremes.Item4;
	wmin = extremes.Item1;
	wmax = extremes.Item2;
	foreach (var s in sensors) // adjust width to fit the entire exclusion zone of each scanner
	{
	    var xl = s.Item1-closestBeaconDistance[s];
	    if (xl < wmin) wmin = xl;
	    var xr = s.Item1+closestBeaconDistance[s];
	    if (xr > wmax) wmax = xr;
	}
	if (boxed)
	{
	    // wmin = 0; wmax = 20;
	    // dmin = 0; dmax = 20;
	    wmin = 0; wmax = 4000000;
	    dmin = 0; dmax = 4000000;
	}
	width = wmax-wmin+1;
	depth = dmax-dmin+1;
    }
    public void CountAlongHorizontal(int depth)
    {
	Console.WriteLine($"Checking widths {wmin} to {wmax}.");
	int counter = 0;
	for (int w = wmin; w <= wmax; w++)
	{
	    var currentPoint = new Tuple<int,int>(w,depth);
	    // Console.WriteLine($"Checking the current point {currentPoint}.");
	    if ( ListContains(beacons,currentPoint) ) // don't count among impossible locations
	    {
		// Console.WriteLine("Is a beacon point!");
	    }
	    else if ( ListContains(sensors,currentPoint) ) // not a beacon, could be a sensor though which is an impossible location
	    {
		counter += 1;
		// Console.WriteLine("Is a sensor point!");
	    }
	    else // neither a beacon nor a sensor, need to check if it's in the exclusion zone of at least one sensor
	    {
		// Console.WriteLine("Checking sensor exclusion points.");
		bool inExclusion = false;
		foreach (var s in sensors)
		{
		    if (!inExclusion)
		    {
			// Console.WriteLine($"Checking sensor {s}");
			int dBeacon = closestBeaconDistance[s];
			int dPoint = ManhattenDistance(currentPoint,s);
			if ( dPoint <= dBeacon ) // already know it's not a beacon, so inclusive range (no ties)
			{
			    // Console.WriteLine($"Point is in exclusion zone of {s} ({dPoint} <= {dBeacon})");
			    counter += 1;
			    inExclusion = true;
			    break;
			}
		    }
		}
	    }
	}
	Console.WriteLine($"The number of points along y = {depth} that cannot contain a beacon is {counter}.");
    }

    public void GetFrequencyOfMissingBeacon()
    {
	var p = FindMissingBeacon();
	if (p.Item1 < 0) Console.WriteLine("Missing beacon not found in search space.");
	else
	{
	    long frequency = (long)4000000*(long)p.Item1 + (long)p.Item2;
	    Console.WriteLine($"Missing beacon found at point {p} with frequency {frequency}.");
	}
    }
    public Tuple<int,int> FindMissingBeacon()
    {
	var searchSpace = (from s in sensors
			   from e in Frontier(s,closestBeaconDistance[s]+1)
			   select e).ToList();
	double pct = 100*((double)searchSpace.Count)/((double)width*depth);
	Console.WriteLine($"Searching {searchSpace.Count}/{width*depth} points (% {pct:N9}).");
	foreach (var p in searchSpace)
	{
	    bool outsideAllExclusions = true;
	    foreach (var s in sensors)
	    {
		// will become false when we're within the exclusion zone of some s
		outsideAllExclusions =  outsideAllExclusions && (ManhattenDistance(p,s) > closestBeaconDistance[s]);
	    }
	    if (outsideAllExclusions)
	    {
		Console.WriteLine($"Found a point {p}");
		return p; // short-circuit loop
	    }
	}
	return new Tuple<int,int>(-1,-1); // not found
    }
    public Tuple<int,int,int,int> FindExtremeCoordinates(List<Tuple<int,int>> coords)
    {
	var wCoords = (from c in coords
		       let w = c.Item1
		       orderby w ascending
		       select w).ToList();
	int wCoordMin = wCoords.First();
	int wCoordMax = wCoords.Last();
	var dCoords = (from c in coords
		       let d = c.Item2
		       orderby d ascending
		       select d).ToList();
	int dCoordMin = dCoords.First();
	int dCoordMax = dCoords.Last();
	return new Tuple<int,int,int,int>(wCoordMin,wCoordMax,dCoordMin,dCoordMax);
    }
    public bool IsEqual(Tuple<int,int> p1, Tuple<int,int> p2)
    {
	return ManhattenDistance(p1,p2) == 0;
    }
    public int ManhattenNorm(Tuple<int,int> p)
    {
	return Math.Abs(p.Item1) + Math.Abs(p.Item2);
    }
    public int ManhattenDistance(Tuple<int,int> p1, Tuple<int,int> p2)
    {
	return ManhattenNorm(new Tuple<int,int>(p1.Item1-p2.Item1,p1.Item2-p2.Item2));
    }
    public bool ListContains(List<Tuple<int,int>> list, Tuple<int,int> p)
    {
	bool contains = false;
	int counter = 0;
	while ( !contains && (counter < list.Count))
	{
	    if (ManhattenDistance(p,list[counter]) == 0)
	    {
		contains = true;
	    }
	    counter += 1;
	}
	return contains;
    }
    public List<Tuple<int,int>> Frontier(Tuple<int,int> p, int d)  // construct points on the Manhattan circle of size d
    {
	var list = new List<Tuple<int,int>>();
	var current = new Tuple<int,int>(p.Item1,p.Item2-d); // start at lowest position and work around CCW
	list.Add(current);
	foreach (int j in Enumerable.Range(0,d)) // left and up
	{
	    current = new Tuple<int,int>(current.Item1-1,current.Item2+1);
	    list.Add(current);
	}
	foreach (int j in Enumerable.Range(0,d)) // right and up
	{
	    current = new Tuple<int,int>(current.Item1+1,current.Item2+1);
	    list.Add(current);
	}
	foreach (int j in Enumerable.Range(0,d)) // right and down
	{
	    current = new Tuple<int,int>(current.Item1+1,current.Item2-1);
	    list.Add(current);
	}
	foreach (int j in Enumerable.Range(0,d-1)) // left and down, but don't repeat the first coordinate
	{
	    current = new Tuple<int,int>(current.Item1-1,current.Item2-1);
	    list.Add(current);
	}
	// if points fall outside the box, we won't bother checking them
	list.RemoveAll(p => ((p.Item1 < wmin) || (p.Item1 > wmax) ||
			     (p.Item2 < dmin) || (p.Item2 > dmax)));
	return list;
    }
}
