using Climbing;

class Program
{
    static void Main(string[] args)
    {
	// part 1
	Map map = new Map("./elevation.dat");
	map.PrintOptimalSolution(false);

	// part 2
	map.PrintGlobalOptimalSolution(false);
    }
}
