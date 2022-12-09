using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
	// part 1
	string[][] bags = ParseInputPart1("./baggage.dat");
	string[] inters = GetIntersectionsPart1(bags);
	int priority = (from i in inters
			select GetPriorites(i)[0]).Sum();
	Console.WriteLine($"The sum of item priorities in the bag is {priority}.");

	// part 2
	string[] intersections = ParseInputPart2("./baggage.dat");
	int priority = (from i in intersections
			select GetPriorites(i)[0]).Sum();
	Console.WriteLine($"The sum of badge priorities of the groups is {priority}.");
    }
    public static int[] GetPriorites(string encoded)
    {
	return  (from c in encoded
		 select (int)(Char.IsUpper(c) ? c-'A' + 26 : c-'a') + 1).ToArray();
    }
    public static string[][] ParseInputPart1(string fname)
	{
	    string contents = System.IO.File.ReadAllText(fname);
	    return (from line in contents.Split("\n")
		    let firstHalf = line.Substring(0,line.Length/2)
		    let secHalf = line.Substring(line.Length/2,line.Length/2)
		    select new string[] { firstHalf, secHalf }).ToArray();
	}
    public static string[] GetIntersectionsPart1(string[][] bags)
    {
	return  (from b in bags
		 let intersection = new string((from c1 in b[0] select c1)
					       .Intersect(b[1]).ToArray())
		 select intersection).ToArray();
    }
    public static string[] ParseInputPart2(string fname)
    {
	string[] contents = System.IO.File.ReadAllText(fname)
	    .Split("\n");
	string[] intersections = new string[] {};
	for (int j = 0; j < contents.Length/3; j++)
	{
	    string intersection = IntersectStrings(contents[3*j],
						   IntersectStrings(contents[3*j+1],
								    contents[3*j+2]));
	    intersections = intersections.Append(intersection).ToArray();
}
	return intersections;
    }
    public static string IntersectStrings(string str1, string str2)
    {
	return new string((from c1 in str1 select c1).Intersect(str2).ToArray());
    }
}
