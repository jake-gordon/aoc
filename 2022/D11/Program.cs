using Monkeys;

class Program
{
    static void Main(string[] args)
    {
	// part 1
	Barrel barrel = new Barrel("./monkeyitems.dat",true);
	barrel.ExecuteRounds(20);
	barrel.PrintMonkeyBusiness();

	// part 2
	barrel = new Barrel("./monkeyitems.dat",false);
	barrel.ExecuteRounds(10000);
	barrel.PrintMonkeyBusiness();
    }
}
