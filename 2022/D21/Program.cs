using Monkeys;

class Program
{
    static void Main(string[] args)
    {
	var fname = "./yells.dat";

	// part 1
	var hollerin = new Hollering(fname);
	hollerin.EvaluateRoot();

	// part 2
	hollerin.EvaluateDeterminedExpressions();
	hollerin.EvaluateModifiedRoot();
    }
}
