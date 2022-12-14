using Parsing;

class Program
{
    static void Main(string[] args)
    {
	string fname = "./packets.dat";
	// part 1
	Parser.FindPairsInOrder(fname);

	// part 2
	Parser.SortAndFindPackets(fname);
    }
}
