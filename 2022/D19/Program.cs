using Robots;

class Program
{
    static void Main(string[] args)
    {
	var fname = "./blueprints.dat";

	// part 1
	var escavation = new Escavation(fname);
	escavation.SimulateQualityLevels();

	// part 2
	escavation.SimulateLargest();
    }
}
