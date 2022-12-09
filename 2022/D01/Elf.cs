using System.Linq;

namespace Elf
{
    class ElfStash
    {
	public string Name {get; set; }
	public int NumItems { get; set; }
	public int TotalCalories { get; set; }
	private int[] calories;
	public ElfStash(int[] inventory)
	{
	    var rng = new Random();
	    Name = $"Elf{rng.Next(1000):D4}";
	    calories = inventory;
	    NumItems = inventory.Length;
	    TotalCalories = (int)inventory.Sum();
	}
	public ElfStash(string name, int[] inventory)
	{
	    Name = name;
	    calories = inventory;
	    NumItems = inventory.Length;
	    TotalCalories = (int)inventory.Sum();
	}
	public void AddItem(int food)
	{
	    calories = calories.Append(food).ToArray();
	    NumItems += 1;
	    TotalCalories += food;
	}
	public void PrintInventory()
	{
	    Console.WriteLine($"{Name} has {NumItems} items with {TotalCalories} total calories:");
	    Console.Write("\t{{ ");
	    foreach (var cal in calories) Console.Write($"{cal} ");
	    Console.Write("}}.\n");
	}
    }
    class ElfPack
    {
	public string clanName { get; set; }
	public int NumElfs { get; set; }
	public int TotalCalories { get; set; }
	public int MaxIndividualCalories { get; set; }
	private ElfStash[] stashes;
	private int[] maxcalories;
	public ElfPack(string name)
	{
	    clanName = name;
	    stashes = new ElfStash[] {};
	    NumElfs = stashes.Length;
	    maxcalories = new int[] {};
	    TotalCalories = 0;
	    MaxIndividualCalories = 0;
	}
	public ElfPack(string name, int[][] stashData)
	{
	    clanName = name;
	    stashes = stashData.Select(s => new ElfStash(s)).ToArray();
	    stashes = stashes.OrderBy(e => e.TotalCalories).ToArray();
	    Array.Reverse(stashes);
	    NumElfs = stashes.Length;
	    maxcalories = stashes.Select(s => s.TotalCalories).ToArray();
	    TotalCalories = maxcalories.Sum();
	    MaxIndividualCalories = maxcalories.Max();
	}
	public ElfPack(string name, ElfStash[] mystashes)
	{
	    clanName = name;
	    stashes = mystashes.OrderBy(e => e.TotalCalories).ToArray();
	    Array.Reverse(stashes);
	    NumElfs = stashes.Length;
	    maxcalories = stashes.Select(s => s.TotalCalories).ToArray();
	    TotalCalories = maxcalories.Sum();
	    MaxIndividualCalories = maxcalories.Max();
	}
	public void AddStash(ElfStash estash)
	{
	    stashes = stashes.Append(estash).OrderBy(e => e.TotalCalories).ToArray();
	    Array.Reverse(stashes);
	    NumElfs += 1;
	    maxcalories = stashes.Select(s => s.TotalCalories).ToArray();
	    TotalCalories = maxcalories.Sum();
	    MaxIndividualCalories = maxcalories.Max();
	}
	public void PrintStashes()
	{
	    ElfStash estsh = stashes[0];
	    Console.WriteLine($"The collection of {clanName} has {TotalCalories} calories split among {NumElfs} elves.");
	    Console.WriteLine($"{estsh.Name} has the most calories on hand with {estsh.TotalCalories}.");
	    foreach (ElfStash estash in stashes) estash.PrintInventory();
	}
	public void FindTopStashes(int N)
	{
	    ElfStash[] topStashes = new ArraySegment<ElfStash>(stashes,0,N).ToArray();
	    int total = topStashes.Select(e => e.TotalCalories).Sum();
	    Console.WriteLine($"The top {N} calorie holders of {clanName} have {total} calories among them.");
	    foreach (ElfStash es in topStashes) es.PrintInventory();
	}
    }
}
