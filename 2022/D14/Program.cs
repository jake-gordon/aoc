using Sand;

class Program
{
    static void Main(string[] args)
    {
	string fname = "./coordinates.dat";

	// part 1
	Cave c = new Cave(fname,false); // no floor, stare into the abyss
	c.DropSand();

	// part 2
	c = new Cave(fname, true); // infinite floor, look for a safe place to hide!
	c.DropSandWithFloor();
    }
}
