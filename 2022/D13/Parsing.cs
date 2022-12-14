namespace Parsing;

using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public class CustomComparer : IComparer<string>
{
    public int Compare(string? s1, string? s2)
    {
	if ( s1 == null || s2 == null ) throw new ArgumentException("Don't compare null things.");
	return -Parser.InOrder(s1,s2);
    }
}

public class Parser
{
    public static void FindPairsInOrder(string fname)
    {
	var pairs = ParseIntoPairs(fname);
	var locations = (from j in Enumerable.Range(0,pairs.Length)
			 where Parser.InOrder(pairs[j][0],pairs[j][1]) > 0
			 select j+1); // 1 is start of problem
	Console.WriteLine($"The sum of ordered pair indices is {locations.Sum()}");
    }
    public static string[][] ParseIntoPairs(string fname)
    {
	return (from s in System.IO.File.ReadAllText(fname).Split("\n\n")
		let pair = s.Split("\n")
		select pair).ToArray();
    }
    public static string[] ParseIntoList(string fname)
    {
	string[] entries = (from entry in System.IO.File.ReadAllText(fname).Replace("\n\n","\n").Split("\n")
			    select entry).ToArray();
	entries = entries.Append("[[2]]").Append("[[6]]").ToArray(); // divider packets
	return entries;
    }
    public static void SortAndFindPackets(string fname)
    {
	var entries = ParseIntoList(fname).ToList();
	entries = entries.ToList();
	entries.Sort(new CustomComparer());
	var symbolPositions = (from j in Enumerable.Range(0,entries.Count)
			       let e = entries[j]
			       where e == "[[2]]" || e == "[[6]]"
			       select j+1);
	int product = symbolPositions.Aggregate(1,(a,v) => a*v);
	Console.Write("Distress signals found at ");
	foreach (var p in symbolPositions) Console.Write($"{p}, ");
	Console.Write($"with product {product}.\n");
    }
    public static bool ListIsOrdered(string[] entries)
    {
	bool ordered = true;
	foreach (int j in Enumerable.Range(0,entries.Length))
	{
	    foreach (int k in Enumerable.Range(j+1,entries.Length-j-1))
	    {
		if ( InOrder(entries[j],entries[k]) < 0 ) return false;
	    }
	}
	return ordered;
    }
    public static bool IsEmpty(string s)
    {
	return s == "";
    }
    public static bool IsSingleDigit(string s)
    {
	Regex regex = new Regex(@"^\d+$");
	return regex.Matches(s).Count == 1;
    }
    public static bool IsSingleton(string s)
    {
	Regex regex = new Regex(@"^\[\d+\]$");
	return regex.Matches(s).Count == 1;
    }
    public static bool IsEmptySet(string s)
    {
	Regex regex = new Regex(@"^\[\]$");
	return regex.Matches(s).Count == 1;
    }
    public static bool IsDigitArray(string s)
    {
	Regex regex = new Regex(@"^\d+$");
	return (from i in SplitIntoItems(s)
		let m = regex.Matches(i).Count
		select m == 1).Aggregate(true,(a,b) => a && b) || IsEmptySet(s);
    }
    public static int InOrder(int n, int m) // base case, + if true, - if false, 0 if equal
    { // want smaller to be first
	if (n > m) return -1; // a > b not in order
	else if (n < m) return +1; // a < b not in order
	else return 0; // equal
    }
    public static int InOrder(int[] a, int[] b)
    {
	bool aEmpty = (a.Length == 0);
	bool bEmpty = (b.Length == 0);
	if (aEmpty && !bEmpty) return 1; // a ran out first
	else if (aEmpty && bEmpty) return 0; // equal
	else if (!aEmpty && bEmpty) return -1; // b ran out first
	else // both are nonempty, check common length in order
	{
	    foreach (int j in Enumerable.Range(0,Math.Min(a.Length,b.Length)))
	    {
		// before end of common array, if an element of a is bigger than b
		// it's in the right order; we only reach this point if every previous
		// item was equal
		if (InOrder(a[j],b[j]) > 0) return +1; // a < b, in order
		if (InOrder(a[j],b[j]) < 0) return -1; // a > b, not in order
	    }
	    // otherwise, we've ended one of the arrays before it ended without a decision
	    // if they're the same length they're equal
	    // if a is shorter then we're in order, and if b is shorter then we're not
	    if (a.Length == b.Length) return 0;
	    else if (a.Length < b.Length) return +1; // a ran out of items first, in order
	    else return -1; // b ran out of items first, not in order
	}
    }
    public static int InOrder(string s1, string s2) // will pass around strings recursively, parse & pass to real functions when simple
    {
	if ( IsSingleDigit(s1) && IsSingleDigit(s2) )
	{
	    // Console.WriteLine("Comparing single digits.");
	    return InOrder(int.Parse(s1),int.Parse(s2));
	}
	else if ( IsSingleDigit(s1) && IsDigitArray(s2) )
	{
	    // Console.WriteLine("Replacing digit with array.");
	    var a1 = new int[] { int.Parse(s1) };
	    var a2 = new int[] {};
	    if (!IsEmptySet(s2))
	    {
		a2 = (from item in SplitIntoItems(s2)
		      select int.Parse(item)).ToArray();
	    }
	    return InOrder(a1,a2);
	}
	else if ( IsDigitArray(s1) && IsSingleDigit(s2) )
	{
	    // Console.WriteLine("Replacing digit with array.");
	    var a1 = new int[] {};
	    var a2 = new int[] { int.Parse(s2) };
	    if (!IsEmptySet(s1))
	    {
		a1 = (from item in SplitIntoItems(s1)
		      select int.Parse(item)).ToArray();
	    }
	    return InOrder(a1,a2);
	}
	else if ( IsDigitArray(s1) && IsDigitArray(s2) ) // both are [n,m,...], parse & return integer value
	{
	    // Console.WriteLine("Parsing arrays & comparing.");
	    var a1 = new int[] {};
	    var a2 = new int[] {};
	    if (!IsEmptySet(s1))
	    {
		a1 = (from item in SplitIntoItems(s1)
		      select int.Parse(item)).ToArray();
	    }
	    if (!IsEmptySet(s2))
	    {
		a2 = (from item in SplitIntoItems(s2)
		      select int.Parse(item)).ToArray();
	    }
	    return InOrder(a1,a2);
	}
	else
	{
	    // Console.WriteLine("Splitting items & recursing to compare.");
	    // Console.WriteLine($"{s1} {s2}");
	    if (IsEmptySet(s1) && !IsEmptySet(s2)) return 1; // a1 ran out first
	    else if (IsEmptySet(s1) && IsEmptySet(s2)) return 0; // equal
	    else if (!IsEmptySet(s1) && IsEmptySet(s2)) return -1; // a2 ran out first
	    string[] a1 = SplitIntoItems(s1);
	    string[] a2 = SplitIntoItems(s2);
	    foreach (int j in Enumerable.Range(0,Math.Min(a1.Length,a2.Length)))
	    {
		// before end of common array, if an element of a1 is bigger than a2
		// it's in the right order; we only ready this point if every previous
		// item was equal
		if (InOrder(a1[j],a2[j]) > 0) return +1; // a1 < a2, in order
		if (InOrder(a1[j],a2[j]) < 0) return -1; // a1 > a2, not in order
	    }
	    // otherwise, we've ended one of the arrays before it ended without a decision
	    // if they're the same length they're equal
	    // if a1 is shorter then we're in order, and if a2 is shorter then we're not
	    if (a1.Length == a2.Length) return 0;
	    else if (a1.Length < a2.Length) return +1; // a1 ran out of items first, in order
	    else return -1; // a2 ran out of items first, not in order
	}
    }
    public static string[] SplitIntoItems(string s)
    {
	int bracketLevel = 0, position = 0;
	var segmentPositions = new int[] {}; // commas & brackets at level 1
	var tokens = new Stack<char>(s.ToCharArray().Reverse());
	if (IsSingleDigit(s)) return new string[] {s}; // ?
	// if (IsSingleDigit(s)) throw new ArgumentException("Don't split single digits.");
	if (s == "") throw new ArgumentException("Don't split empty strings.");
	while (tokens.Count > 0)
	{
	    switch(tokens.Pop())
	    {
		case '[':
		    if (bracketLevel == 0) segmentPositions = segmentPositions.Append(position).ToArray();
		    bracketLevel += 1;
		    break;
		case ']':
		    bracketLevel -= 1;
		    if (bracketLevel == 0) segmentPositions = segmentPositions.Append(position).ToArray();
		    break;
		case ',':
		    if (bracketLevel == 1) segmentPositions = segmentPositions.Append(position).ToArray();
		    break;
	    }
	    position += 1;
	}
	return (from j in Enumerable.Range(0,segmentPositions.Length-1)
		let item = s.Substring(segmentPositions[j]+1,segmentPositions[j+1]-segmentPositions[j]-1)
		select item).ToArray();
    }
}
