using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
	// part 1
	int startPosition = FindStartMarker("./stream.dat");
	Console.WriteLine($"The first marker occurs after character {startPosition}.");

	// part 2
	int messagePosition = FindMessageMarker("./stream.dat");
	Console.WriteLine($"The message marker occurs after character {messagePosition}.");
    }

    public static int FindStartMarker(string fname)
    {
	char[] stream = System.IO.File.ReadAllText(fname)
	    .ToCharArray();
	Console.WriteLine($"Communication stream: {new string(stream)}");
	int markerPosition = 0;
	for (int j = 0; j < stream.Length-4; j++)
	{
	    char[] segment = new ArraySegment<char>(stream,j,4).ToArray();
	    bool duplicates = segment.GroupBy(c => c)
		.Where(g => g.Count() > 1)
		.Select(c => c).ToArray().Length > 0 ? true : false;
	    if (!duplicates)
	    {
		Console.WriteLine($"Marker found with window: {new string(segment)} starting at position {j} and ending at position {j+4}.");
		markerPosition = j+4;
		break;
	    }
	}
	return markerPosition;
    }
    public static int FindMessageMarker(string fname)
    {
	char[] stream = System.IO.File.ReadAllText(fname)
	    .ToCharArray();
	// Console.WriteLine($"Communication stream: {new string(stream)}");
	int markerPosition = 0;
	for (int j = 0; j < stream.Length-14; j++)
	{
	    char[] segment = new ArraySegment<char>(stream,j,14).ToArray();
	    bool duplicates = segment.GroupBy(c => c)
		.Where(g => g.Count() > 1)
		.Select(c => c).ToArray().Length > 0 ? true : false;
	    if (!duplicates)
	    {
		Console.WriteLine($"Marker found with window: {new string(segment)} starting at position {j} and ending at position {j+14}.");
		markerPosition = j+14;
		break;
	    }
	}
	return markerPosition;
    }
}
