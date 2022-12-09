using Elf;

class Program
{
    static void Main(string[] args)
    {
	string inputFile = "./calories.dat";
	int[][] stashData = ParseElfStashes(inputFile);
	ElfPack epack = new ElfPack("Dudes",stashData);
	// epack.PrintStashes();
	// part 1
	epack.FindTopStashes(1);

	// part 2
	epack.FindTopStashes(3);
    }
    static int[][] ParseElfStashes(string fname)
    {
	string fileContents = System.IO.File.ReadAllText(fname);
	string[] lines = fileContents.Split('\n');
	int[] inds = Enumerable.Range(0,lines.Length)
	    .Where(j => lines[j] == "")
	    .ToArray();

	string[] segment = new ArraySegment<string>(lines,0,inds[0]).ToArray();
	int[] segCals = segment.Select(s => int.Parse(s)).ToArray();
	int[][] calories = new int[][] { segCals };

	for (int j = 0; j < inds.Length-1; j++)
	{
	    segment = new ArraySegment<string>(lines,inds[j]+1,inds[j+1]-inds[j]-1).ToArray();
	    segCals = segment.Select(s => int.Parse(s)).ToArray();
	    calories = calories.Append(segCals).ToArray();
	}
	return calories;
    }
}
