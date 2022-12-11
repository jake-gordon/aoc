using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
	string[] steps = GetHeadSteps("./motion.dat");
	// string[] steps = GetHeadSteps("./motion-test1.dat");
	// string[] steps = GetHeadSteps("./motion-test2.dat");

	// part 1
	KnotBridge bridge = new KnotBridge(2);
	bridge.LoadInstructions(steps);
	bridge.ExecuteInstructions();
	Console.WriteLine($"The two-knot tail visited {bridge.NumUniqueTailPositions()} unique positions.");

	// part 2
	bridge = new KnotBridge(10);
	bridge.LoadInstructions(steps);
	bridge.ExecuteInstructions();
	Console.WriteLine($"The 10-knot tail visited {bridge.NumUniqueTailPositions()} unique positions.");
    }
    static string[] GetHeadSteps(string fname)
    {
	string[][] sequences = (from line in System.IO.File.ReadAllText(fname).Split("\n")
				let direction = line.Split(" ")[0]
				let number = int.Parse(line.Split(" ")[1])
				select Enumerable.Repeat(direction,number).ToArray()).ToArray();
	string[] steps = new string[] {};
	foreach (string[] s in sequences) steps = steps.Concat(s).ToArray();
	return steps;
    }
    class KnotBridge // to carry current state of objects carried through instructions
    {
	Point[] knotPositions;
	Point[][] knotHistory;
	Point[][] knotUniqueHistory;
	Stack<string> instructions;
	public KnotBridge(int M) // number of knots >= 2
	{
	    int[] origin = new int[] {0,0};
	    knotPositions = Enumerable.Repeat(new Point(origin),M).ToArray();
	    knotHistory = Enumerable.Repeat(new Point[] {new Point(origin)}, M).ToArray();
	    knotUniqueHistory = Enumerable.Repeat(new Point[] {new Point(origin)}, M).ToArray();
	    instructions = new Stack<string>();
	}
	public void AddToHistory(int j, Point p)
	{
	    knotHistory[j] = knotHistory[j].Append(p).ToArray();
	    bool existsInStore = (from q in knotUniqueHistory[j]
				  where q.Equals(p)
				  select q).Count() > 0 ? true : false;
	    if (!existsInStore) knotUniqueHistory[j] = knotUniqueHistory[j].Append(p).ToArray();
	}
	public void LoadInstructions(string[] steps) // reverse order
	{
	    for (int j = steps.Length-1; j >= 0; j--) instructions.Push(steps[j]);
	}
	public void ExecuteInstructions()
	{
	    while (instructions.Count > 0) ExecuteStep(instructions.Pop());
	}
	public void ExecuteStep(string step)
	{
	    switch(step) // move the leader & record new position
	    {
		case "U":
		    knotPositions[0] += new Point(new int[] {0,1});
		    break;
		case "D":
		    knotPositions[0] -= new Point(new int[] {0,1});
		    break;
		case "L":
		    knotPositions[0] -= new Point(new int[] {1,0});
		    break;
		case "R":
		    knotPositions[0] += new Point(new int[] {1,0});
		    break;
	    }
	    this.AddToHistory(0,knotPositions[0]);
	    for (int j = 1; j < knotPositions.Length; j++) // remaining M-1 points
	    {
		Point currentDiff = knotPositions[j-1]-knotPositions[j];
		int distanceSquared = currentDiff.NormSquared();
		if (distanceSquared == 4) // need to move in Cartesian direction
		{
		    knotPositions[j] += currentDiff/2; // unit step in Cartesian direction
		}
		else if (distanceSquared > 4) // need to move diagonally to follow, just need quadrant
		{
		    int signX = currentDiff.X > 0 ? 1 : -1;
		    int signY = currentDiff.Y > 0 ? 1 : -1;
		    knotPositions[j] += new Point(new int[] {signX,signY});
		}
		else // H within 3x3 block, either beside or diagonal & no action needed
		{
		}
		this.AddToHistory(j,knotPositions[j]);
	    }
	}
	public int NumUniquePositions(int j)
	{
	    return knotUniqueHistory[j].Length;
	}
	public int NumUniqueTailPositions()
	{
	    return NumUniquePositions(knotPositions.Length-1);
	}
    }
    class Point // 2D point for convenience
    {
	public int X, Y;
	public Point(int[] p)
	{
	    X = p[0]; Y = p[1];
	}
	public int InnerProd(Point p)
	{
	    return this.X*p.X + this.Y*p.Y;
	}
	public int InnerProd(Point p1, Point p2)
	{
	    return p1.X*p2.X + p1.Y*p2.Y;
	}
	public static Point operator +(Point l, Point r)
	{
	    return new Point(new int[] {l.X + r.X, l.Y + r.Y});
	}
	public static Point operator -(Point l, Point r)
	{
	    return new Point(new int[] {l.X - r.X, l.Y - r.Y});
	}
	public static Point operator /(Point l, int r)
	{
	    return new Point(new int[] {l.X/r, l.Y/r});
	}
	public bool Equals(Point p)
	{
	    return this.X == p.X && this.Y == p.Y;
	}
	public int NormSquared()
	{
	    return this.InnerProd(this);
	}
	public double Norm()
	{
	    return Math.Sqrt(NormSquared());
	}
	public double Distance(Point p)
	{
	    return Math.Sqrt(InnerProd(this-p,this-p));
	}
	public void Print()
	{
	    Console.WriteLine($"({this.X}, {this.Y})");
	}
    }
}
