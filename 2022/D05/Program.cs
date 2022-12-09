using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
	// part 1
	char[] tops = ExecuteStackInstructions("./moves.dat");
	Console.WriteLine("The top of each stack after the sequence of moves is: ");
	foreach (char c in tops) Console.Write($"{c}");
	Console.Write("\n");

	// part 2
	char[] newtops = ExecuteModifiedInstructions("./moves.dat");
	Console.WriteLine("The top of each stack after the sequence of moves is: ");
	foreach (char c in newtops) Console.Write($"{c}");
	Console.Write("\n");
    }

    public static char[] ExecuteStackInstructions(string fname)
    {
	string[] contents = System.IO.File.ReadAllText(fname)
	    .Split("\n");
	int moveStartIndex = contents.Select((line,ind) => new {ind,line})
	    .Where(t => !string.IsNullOrEmpty(t.line))
	    .Where(t => t.line[0] == 'm')
	    .Select(t => t.ind )
	    .First();
	int stackNumberingIndex = moveStartIndex-2;
	int numStacks = contents[stackNumberingIndex].Split("  ").Length;
	string[] stackData = (from j in Enumerable.Range(0,stackNumberingIndex)
			      select contents[j]).ToArray();
	string[][] stackEntries = (from line in stackData
				   select (from j in Enumerable.Range(0,numStacks)
					   select line.Substring(4*j+1,1)).ToArray()).ToArray();
	Stack<char>[] stackList = (from j in Enumerable.Range(0,numStacks)
				   select new Stack<char>()).ToArray();
	for (int j = stackEntries.Length-1; j >= 0; j--)
	{
	    for (int k = 0; k < numStacks; k++)
	    {
		char entry = char.Parse(stackEntries[j][k]);
		if (entry != ' ') stackList[k].Push(entry);
	    }
	}
	string[] instructionData = (from line in contents
				    where !string.IsNullOrEmpty(line)
				    where line[0] == 'm'
				    select line).ToArray();
	int[][] instructions = (from instr in instructionData
				let transit = instr.Split("from")
				let	num = int.Parse(transit[0].Split(" ")[1])
				let src = int.Parse(transit[1].Split(" ")[1])-1
				let dst = int.Parse(transit[1].Split(" ")[3])-1
				select new int[] {src, dst, num}).ToArray();
	foreach (int[] instr in instructions)
	{
	    int srcStack = instr[0], dstStack = instr[1], numOps = instr[2];
	    char working = ' ';
	    for (int n = 0; n < numOps; n++)
	    {
		working = stackList[srcStack].Pop();
		stackList[dstStack].Push(working);
	    }
	}
	return (from stack in stackList
		    select stack.Pop()).ToArray();
    }
    public static char[] ExecuteModifiedInstructions(string fname)
    {
	string[] contents = System.IO.File.ReadAllText(fname)
	    .Split("\n");
	int moveStartIndex = contents.Select((line,ind) => new {ind,line})
	    .Where(t => !string.IsNullOrEmpty(t.line))
	    .Where(t => t.line[0] == 'm')
	    .Select(t => t.ind )
	    .First();
	int stackNumberingIndex = moveStartIndex-2;
	int numStacks = contents[stackNumberingIndex].Split("  ").Length;
	string[] stackData = (from j in Enumerable.Range(0,stackNumberingIndex)
			      select contents[j]).ToArray();
	string[][] stackEntries = (from line in stackData
				   select (from j in Enumerable.Range(0,numStacks)
					   select line.Substring(4*j+1,1)).ToArray()).ToArray();
	Stack<char>[] stackList = (from j in Enumerable.Range(0,numStacks)
				   select new Stack<char>()).ToArray();
	for (int j = stackEntries.Length-1; j >= 0; j--)
	{
	    for (int k = 0; k < numStacks; k++)
	    {
		char entry = char.Parse(stackEntries[j][k]);
		if (entry != ' ') stackList[k].Push(entry);
	    }
	}
	string[] instructionData = (from line in contents
				    where !string.IsNullOrEmpty(line)
				    where line[0] == 'm'
				    select line).ToArray();
	int[][] instructions = (from instr in instructionData
				let transit = instr.Split("from")
				let	num = int.Parse(transit[0].Split(" ")[1])
				let src = int.Parse(transit[1].Split(" ")[1])-1
				let dst = int.Parse(transit[1].Split(" ")[3])-1
				select new int[] {src, dst, num}).ToArray();
	foreach (int[] instr in instructions)
	{
	    int srcStack = instr[0], dstStack = instr[1], numOps = instr[2];
	    char[] poppedItems = new char[] {};
	    for (int n = 0; n < numOps; n++)
	    {
		poppedItems = poppedItems.Append(stackList[srcStack].Pop()).ToArray();
	    }
	    for (int n = numOps-1; n >= 0; n--)
	    {
		stackList[dstStack].Push(poppedItems[n]);
	    }
	}
	return (from stack in stackList
		select stack.Pop()).ToArray();
    }
}
