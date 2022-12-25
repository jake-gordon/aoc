using Weather;

class Program
{
    static void Main(string[] args)
    {
	var fname = "./blizzard.dat";

	// part 1
	var blizzard = new Blizzard(fname);
	Console.WriteLine("Trekking to the finish!");
	blizzard.BFS(blizzard.Finish);

	// part 2
	Console.WriteLine("Going back to start for the snacks!");
	blizzard.BFS(blizzard.Start);
	Console.WriteLine("Going back to the finish!");
	blizzard.BFS(blizzard.Finish);
    }
}
