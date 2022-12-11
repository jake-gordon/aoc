using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
	// part 1
	// Directory filesystem = ExecuteCommands("./commands.dat");
	Directory filesystem = ExecuteCommands("./commands-test.dat");
	// filesystem.ListDirectoryRecursive("\t",0);
	int maxSize = 100000;
	int count = filesystem.CountDirIfSmallerThan(maxSize);
	Console.WriteLine($"The total size of directories smaller than {maxSize} is {count}.");

	// part 2
	int totalDiskSpace = 70000000;
	int neededFreeSpace = 30000000;
	int currentFreeSpace = totalDiskSpace-filesystem.TotalDirectorySize();
	int needToDelete = neededFreeSpace-currentFreeSpace;
	Console.WriteLine($"To free up {neededFreeSpace} we need to delete {needToDelete} in a folder.");
	Directory[] candidates = filesystem.ListDirsLargerThan(needToDelete);
	Directory smallest = (from c in candidates
			      orderby c.TotalDirectorySize() ascending
			      select c).First();
	Console.WriteLine($"The smallest folder {smallest.Name} that accomplishes this frees up {smallest.TotalDirectorySize()}.");
	smallest.ListDirectoryRecursive("\t",0);
    }

    public static Directory ExecuteCommands(string fname)
    {
	string[] input = System.IO.File.ReadAllText(fname).Split("\n");
	int[] commandMarkerIndices = input.Select((line,ind) => new {ind,line})
	    .Where(t => !string.IsNullOrEmpty(t.line))
	    .Where(t => t.line[0] == '$')
	    .Select(t => t.ind )
	    .ToArray();
	int numCommands = commandMarkerIndices.Length;
	int[] commandLengths = (from j in Enumerable.Range(0,numCommands-1)
				select commandMarkerIndices[j+1]-commandMarkerIndices[j]).ToArray();
	commandLengths = commandLengths.Append(input.Length-commandMarkerIndices[numCommands-1]).ToArray();

	Directory filesystem = new Directory("/"); // global object which we can attach stuff to at different paths
	string[] currentPath = new string[] {filesystem.Name}; // context controlled by cd commands
	for (int j = 0; j < numCommands; j++)
	{
	    if (commandLengths[j] == 1) // must be a cd to change directory context, contains exactly one argument and no output
	    {
		string[] command = new ArraySegment<string>(input[commandMarkerIndices[j]].Split(" "),1,2).ToArray();
		if (command[1] == "/")
		{
		    currentPath = new string[] {filesystem.Name};
		}
		else if (command[1] == "..")
		{
		    currentPath = currentPath.Take(currentPath.Length-1).ToArray();
		}
		else
		{
		    currentPath = currentPath.Append(command[1]).ToArray();
		}
	    }
	    else // otherwise, it's ls with zero arguments and a number of outputs specifying the files needed in structure
	    {
		string[] command = new ArraySegment<string>(input[commandMarkerIndices[j]].Split(" "),1,1).ToArray();
		string[] output = new ArraySegment<string>(input,commandMarkerIndices[j]+1,commandLengths[j]-1).ToArray();
		Directory[] newSubDirectories = (from line in output
						 where line.Contains("dir")
						 let dirname = line.Split(" ")[1]
						 select new Directory(dirname)).ToArray();
		foreach (Directory d in newSubDirectories) filesystem.AddSubDirectory(d,currentPath);
		File[] newFiles = (from line in output
				   where !line.Contains("dir")
				   let name = line.Split(" ")[1]
				   let	size = int.Parse(line.Split(" ")[0])
				   select new File(name,size)).ToArray();
		foreach (File f in newFiles) filesystem.AddFile(f,currentPath);
	    }
	    // foreach (int k in Enumerable.Range(1,currentPath.Length-1))
	    // Console.Write($"/{currentPath[k]}"); Console.Write("\n");
	}
	return filesystem;
    }
    public class File
    {
	public string Name { get; set; }
	public int Size { get; set; }
	public File(string name, int size)
	{
	    Name = name;
	    Size = size;
	}
    }
    public class Directory
    {
	public string Name { get; set; }
	public Directory[] SubDirectories { get; set; }
	public File[] Files { get; set; }
	public Directory(string name)
	{
	    Name = name;
	    SubDirectories = new Directory[] {};
	    Files = new File[] {};
	}
	public void ListDirectory(string padstring)
	{
	    Console.WriteLine($"- {Name} (dir, size={this.TotalDirectorySize()})");
	    foreach (Directory d in SubDirectories)
		Console.WriteLine($"{padstring}- {d.Name} (dir, size={d.TotalDirectorySize()})");
	    foreach (File f in Files)
		Console.WriteLine($"{padstring}- {f.Name} (file, size={f.Size})");
	}
	public void ListDirectoryRecursive(string padstring, int depth)
	{
	    string pad1 = string.Concat(Enumerable.Repeat(padstring, depth));
	    string pad2 = string.Concat(pad1,padstring);
	    Console.WriteLine($"{pad1}- {Name} (dir, size={this.TotalDirectorySize()})");
	    foreach (Directory d in SubDirectories)
		d.ListDirectoryRecursive(padstring, depth+1);
	    foreach (File f in Files)
		Console.WriteLine($"{pad2}- {f.Name} (file, size={f.Size})");
	}
	public int TotalDirectorySize()
	{
	    int total =(from f in Files select f.Size).Sum();
	    foreach (Directory d in SubDirectories) total += d.TotalDirectorySize();
	    return total;
	}
	public int FindChildIndex(string dirname)
	{
	    int[] nameMatchIndices = SubDirectories.Select((dir,ind) => new {dir,ind})
		.Where(t => t.dir.Name == dirname)
		.Select(t => t.ind)
		.ToArray();
	    return nameMatchIndices.Length == 1 ? nameMatchIndices[0] : -1;
	}
	public Directory GetChild(int index)
	{
	    return SubDirectories[index];
	}
	public void UpdateChild(int index, Directory updated)
	{
	    SubDirectories[index] = updated;
	}
	public void AddFile(File file) // base case
	{
	    Files = Files.Append(file).ToArray();
	}
	public void AddFile(File file, string[] path) // traversing directories
	{
	    if (path.Length == 1)
	    {
		this.AddFile(file); // invoke base case once path is stripped down
	    }
	    else // we need to traverse the next step in the path
	    {
		string[] nextpath = new ArraySegment<string>(path,1,path.Length-1).ToArray();
		int index = this.FindChildIndex(nextpath[0]);
		SubDirectories[index].AddFile(file,nextpath);
	    }
	}
	public void AddSubDirectory(Directory directory) // base case
	{
	    SubDirectories = SubDirectories.Append(directory).ToArray();
	}
	public void AddSubDirectory(Directory directory, string[] path) // traversing directories
	{
	    if (path.Length == 1)
	    {
		this.AddSubDirectory(directory); // invoke base case once path is stripped down
	    }
	    else // we need to traverse the next step in the path
	    {
		string[] nextpath = new ArraySegment<string>(path,1,path.Length-1).ToArray();
		int index = this.FindChildIndex(nextpath[0]);
		SubDirectories[index].AddSubDirectory(directory,nextpath);
	    }
	}
	public int CountDirIfSmallerThan(int maxSize)
	{
	    int total = 0;
	    foreach (Directory d in SubDirectories)
	    {
		int sum = d.TotalDirectorySize();
		if ( sum <= maxSize )
		{
		    total += sum;
		}
		total += d.CountDirIfSmallerThan(maxSize);
	    }
	    return total;
	}
	public Directory[] ListDirsLargerThan(int needToDelete)
	{
	    Directory[] candidates = new Directory[] {};
	    foreach(Directory d in SubDirectories)
	    {
		int sum = d.TotalDirectorySize();
		if ( sum >= needToDelete )
		{
		    candidates = candidates.Append(d).ToArray();
		    // Console.WriteLine($"Directory {d.Name} takes up {sum}.");
		}
		candidates = candidates.Concat(d.ListDirsLargerThan(needToDelete)).ToArray();
	    }
	    return candidates;
	}
    }
}
