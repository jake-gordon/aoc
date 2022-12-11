namespace Monkeys;

using System.Collections.Generic;
using System.Linq;

public delegate ulong Map(ulong w);

class Barrel // ha ha
{
    public uint Round { get; set; }
    Monkey[] monkeys;
    public Barrel(string fname, bool worryManaged)
    {
	Round = 0;
	monkeys = new Monkey[] {};
	foreach(string summary in System.IO.File.ReadAllText(fname).Split("\n\n"))
	    monkeys = monkeys.Append(new Monkey(summary)).ToArray();
	var divisors = (from m in monkeys select m.Divisor);
	ulong wrapInt = divisors.Aggregate((ulong)1,(a,d) => a*d);
	if (!worryManaged)
	{
	    foreach (Monkey m in monkeys) m.SetWorryReduction(wrapInt);
	}

    }
    public void ExecuteRound()
    {
	foreach (Monkey m in monkeys)
	{
	    ulong[,] itemsToThrow = m.HandleItems();
	    foreach (int j in Enumerable.Range(0,itemsToThrow.GetLength(0)))
	    {
		ulong dst = itemsToThrow[j,0];
		ulong w = itemsToThrow[j,1];
		monkeys[dst].AddItem(w);
	    }
	}
	Round += 1;
    }
    public void ExecuteRounds(int N)
    {
	foreach (int j in Enumerable.Range(0,N)) ExecuteRound();
    }
    public void ExecuteRoundsVerbose(int N)
    {
	foreach (int j in Enumerable.Range(0,N))
	{
	    ExecuteRound();
	    PrintMonkeys();
	}
    }
    public void PrintMonkeys()
    {
	Console.WriteLine($"After round {Round} the monkeys have the following stats:");
	foreach (Monkey m in monkeys) m.PrintSummary();
	Console.Write("\n");
    }
    public void PrintMonkeyBusiness()
    {
	Console.WriteLine($"After {Round} rounds of stuff-slinging the two most active monkeys are:\n");
	Monkey[] top = (from m in monkeys
			orderby m.NumberHandled descending
			select m).ToArray();
	ulong monkeyBusiness = top.Take(2).Aggregate((ulong)1,(a,m) => a*m.NumberHandled);
	foreach (Monkey m in monkeys) m.PrintSummary();
	Console.WriteLine($"\nThis amounts to a monkey business level of {monkeyBusiness}.\n");
    }
}

class Monkey
{
    public uint Number { get; set; }
    public uint NumberHandled { get; set; }
    public ulong Divisor { get; set; }
    Queue<ulong> items;
    Map worryFunction, passFunction;
    Map worryReduction;
    public Monkey(string summary)
    {
	string[] lines = summary.Split("\n");

	Number = uint.Parse(lines[0].Split(" ")[1].Trim(':'));
	NumberHandled = 0;

	items = new Queue<ulong>();
	foreach (var w in lines[1].Split(":")[1].Trim().Split(", "))
	    items.Enqueue(ulong.Parse(w));

	string worryOpString = lines[2].Split("=")[1].Trim().Replace("old","w");
	worryFunction = new Map(w => w);
	if (worryOpString.Count(c => c == 'w') > 1) // squaring operation
	{
	    worryFunction = new Map(w => w*w);
	}
	else // w {*,+} c
	{
	    char Op = worryOpString.ToCharArray()[2];
	    ulong c = ulong.Parse(worryOpString.Split(" ")[2]);
	    switch (Op)
	    {
		case '*':
		    worryFunction = new Map(w => w*c);
		    break;
		case '+':
		    worryFunction = new Map(w => w+c);
		    break;
	    }
	}
	Divisor = ulong.Parse(lines[3].Split("by")[1]);
	ulong[] passTo =  new ulong[] {ulong.Parse(lines[4].Split("monkey")[1]),ulong.Parse(lines[5].Split("monkey")[1])};
	passFunction = new Map( w => ( (w % Divisor == 0) ? passTo[0] : passTo[1]) );

	worryReduction = new Map( w => w/3 ); // defaults to w/3 unless we override with a modulo
    }
    public void AddItem(ulong w)
    {
	items.Enqueue(w);
    }
    public ulong[,] HandleItems() // return list of items to throw {dst,item}
    {
	ulong[,] itemsToThrow = new ulong[items.Count,2];
	foreach (int j in Enumerable.Range(0,items.Count))
	{
	    ulong w = items.Dequeue();
	    w = worryFunction(w);
	    w =	worryReduction(w);
	    ulong dst = passFunction(w);
	    itemsToThrow[j,0] = dst;
	    itemsToThrow[j,1] = w;
	    NumberHandled += 1;
	}
	return itemsToThrow;
    }
    public void SetWorryReduction(ulong M)
    {
	worryReduction = new Map( w => w%M );
    }
    public void PrintSummary()
    {
	Console.Write($"Monkey {Number} inspected items {NumberHandled} times and holds [ ");
	foreach (var w in items) Console.Write($"{w} ");
	Console.Write("]\n");
    }
}
