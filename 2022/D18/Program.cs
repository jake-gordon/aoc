using Lava;

class Program
{
    static void Main(string[] args)
    {
	var fname = "./cubes.dat";

	// part 1
	var cubes = new Cubes(fname);
	cubes.ComputeExposedSurfaces();

	// part 2
	cubes.CalculateExteriorArea();
    }
}
