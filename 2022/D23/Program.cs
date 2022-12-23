using Diffusion;

class Program
{
    static void Main(string[] args)
    {
	var fname = "./elves.dat";

	// part 1
	var simulation = new Simulation(fname);
	simulation.ExecuteRounds(10);

	// part 2
	simulation = new Simulation(fname);
	simulation.ExecuteUntilStalled();
    }
}
