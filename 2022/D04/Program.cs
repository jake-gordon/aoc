using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
	string fname = "./ranges.dat";
	// part 1
	int[] contains = CheckOneContainsTheOther(fname);
	Console.WriteLine($"The number of range pairs where one is contained in the other is {contains.Sum()}");

	// part 2
	int[] ovlps = CheckNonEmptyIntersections(fname);
	Console.WriteLine($"The number of range pairs that overlap is {ovlps.Sum()}");
    }

    public static int[] CheckOneContainsTheOther(string fname)
    {
	string[] contents = System.IO.File.ReadAllText(fname)
	    .Split("\n");
	var ranges = (from line in contents
		      let rangeEnds = line.Split(",")
		      let r1 = (from s in rangeEnds[0].Split("-")
				select int.Parse(s)).ToArray()
		      let r2 = (from s in rangeEnds[1].Split("-")
				select int.Parse(s)).ToArray()
		      select new IEnumerable<int>[] { Enumerable.Range(r1[0],r1[1]-r1[0]+1),
			      Enumerable.Range(r2[0],r2[1]-r2[0]+1)}).ToArray();
	return (from rs in ranges
		let intersection = Enumerable.Intersect(rs[0],rs[1])
		let secondContainsFirst = rs[0].SequenceEqual(intersection)
		let firstContainsSecond = rs[1].SequenceEqual(intersection)
		select secondContainsFirst || firstContainsSecond ? 1 : 0).ToArray();
    }
    public static int[] CheckNonEmptyIntersections(string fname)
    {
	string[] contents = System.IO.File.ReadAllText(fname)
	    .Split("\n");
	var ranges = (from line in contents
		      let rangeEnds = line.Split(",")
		      let r1 = (from s in rangeEnds[0].Split("-")
				select int.Parse(s)).ToArray()
		      let r2 = (from s in rangeEnds[1].Split("-")
				select int.Parse(s)).ToArray()
		      select new IEnumerable<int>[] { Enumerable.Range(r1[0],r1[1]-r1[0]+1),
			      Enumerable.Range(r2[0],r2[1]-r2[0]+1)}).ToArray();
	return  (from rs in ranges
		 let intersection = Enumerable.Intersect(rs[0],rs[1]).Any()
		 select intersection ? 1 : 0).ToArray();
    }
}
