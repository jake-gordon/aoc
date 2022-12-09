class Program
{
    public static class Translations
    {
	public static Dictionary<string,string> opponentMap = new Dictionary<string,string>(){{"A","Rock"},{"B","Paper"},{"C","Scissors"}};
	public static Dictionary<string,string> myMap = new Dictionary<string,string>(){{"X","Rock"},{"Y","Paper"},{"Z","Scissors"}};
	public static Dictionary<string,int> scoreMap = new Dictionary<string,int>(){{"Rock",1},{"Paper",2},{"Scissors",3}};
	public static Dictionary<string,int> outcomeMap = new Dictionary<string,int>(){{"Lost",0},{"Draw",3},{"Won",6}};
	public static Dictionary<string,string> elfMap = new Dictionary<string,string>(){{"X","Lose"},{"Y","Draw"},{"Z","Win"}};
    }
    static void Main(string[] args)
    {
	// part 1: misinterpreted the elf!
	// string[][] plays = ParseStrategyList("./strategy.dat");
	// int score = 0;
	// foreach (string[] p in plays) score += MyScore(p[0],p[1]);
	// Console.WriteLine($"If I follow this strategy, my score would be {score}.");

	// part 2: mapping my old function from the new interpretation
	string[][] calls = ParseStrategyList("./strategy.dat");
	string[][] plays = FindMyPlays(calls);
	int score = 0;
	foreach (string[] p in plays) score += MyScore(p[0],p[1]);
	Console.WriteLine($"If I follow these instructions, my score would be {score}.");
    }
    static string[][] ParseStrategyList(string fname)
    {
	string fileContents = System.IO.File.ReadAllText(fname);
	string[] lines = fileContents.Split('\n');
	return lines.Select(l => l.Split(" ")).ToArray();
    }
    static string[][] FindMyPlays(string[][] calls)
    {
	string[][] instructions = calls
	    .Select(s => new string[] {s[0],Translations.elfMap[s[1]]})
	    .ToArray();
	for (int j = 0; j < instructions.Length; j++)
	{
	    string desiredOutcome = instructions[j][1];
	    string oppPlay = Translations.opponentMap[instructions[j][0]];
	    string myPlay = "";
	    if ( desiredOutcome == "Draw" )
	    {
		myPlay = Translations.myMap.FirstOrDefault(e => e.Value == oppPlay).Key;
		instructions[j] = new string[] {instructions[j][0],myPlay};
	    }
	    else if ( desiredOutcome == "Win" ) // ugh, should have done with cyclic permutations for comparison
	    {
		if ( oppPlay == "Rock" )
		{
		    myPlay = Translations.myMap.FirstOrDefault(e => e.Value == "Paper").Key;
		}
		else if ( oppPlay == "Paper" )
		{
		    myPlay = Translations.myMap.FirstOrDefault(e => e.Value == "Scissors").Key;
		}
		else if ( oppPlay == "Scissors" )
		{
		    myPlay = Translations.myMap.FirstOrDefault(e => e.Value == "Rock").Key;
		}
	    }
	    else
	    {
		if ( oppPlay == "Rock" )
		{
		    myPlay = Translations.myMap.FirstOrDefault(e => e.Value == "Scissors").Key;
		}
		else if ( oppPlay == "Paper" )
		{
		    myPlay = Translations.myMap.FirstOrDefault(e => e.Value == "Rock").Key;
		}
		else if ( oppPlay == "Scissors" )
		{
		    myPlay = Translations.myMap.FirstOrDefault(e => e.Value == "Paper").Key;
		}
	    }
	    instructions[j] = new string[] {instructions[j][0],myPlay};
	}
	return instructions;
    }
    static int MyScore(string opponentPlay, string myPlay)
    {
	var opp = Translations.opponentMap[opponentPlay];
	var myp = Translations.myMap[myPlay];
	int score = Translations.scoreMap[myp];
	string outcome = "";
	if ( opp == myp )
	{
	    outcome = "Draw";
	}
	else if ( opp == "Rock" )
	{
	    if ( myp == "Paper" )
	    {
		outcome = "Won";
	    }
	    else {

	    }
	    {
		outcome = "Lost";
	    }
	}
	else if ( opp == "Paper" )
	{
	    if ( myp == "Rock" )
	    {
		outcome = "Lost";
	    }
	    else
	    {
		outcome = "Won";
	    }
	}
	else if ( opp == "Scissors" )
	{
	    if ( myp == "Rock" )
	    {
		outcome = "Won";
	    }
	    else
	    {
		outcome = "Lost";
	    }
	}
	score += Translations.outcomeMap[outcome];
	return score;
    }
}
