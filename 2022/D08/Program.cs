using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
	// part 1
	TreeMap heightMap = new TreeMap("./map.dat");
	// TreeMap heightMap = new TreeMap("./testmap.dat");
	// heightMap.PrintMap();
	// heightMap.PrintVisibleMap();
	// heightMap.PrintScenicMap();
	Console.WriteLine($"There are {heightMap.CountVisible()} trees visible along cardinal directions outside patch.");

	// part 2
	Console.WriteLine($"The highest scenic score on the patch is {heightMap.BestScenicScore()}.");
	Console.WriteLine($"The highest scenic score for invisible sites on the patch is {heightMap.BestInvisibleScenicScore()}.");
    }
    class TreeMap
    {
	public int[,] treeHeights;
	public TreeMap(string fname)
	{
	    string[] input = System.IO.File.ReadAllText(fname).Split("\n");
	    int[,] heights = new int[input.Length,input[0].Length]; // rectangular data
	    for (int j = 0; j < heights.GetLength(0); j++)
	    {
		string line = input[j];
		for (int k = 0; k < heights.GetLength(1); k++)
		{
		    heights[j,k] = int.Parse($"{line[k]}");
		}
	    }
	    treeHeights = heights;
	}
	public void PrintMap()
	{
	    for (int j = 0; j < treeHeights.GetLength(0); j++)
	    {
		for (int k = 0; k < treeHeights.GetLength(1); k++)
		{
		    Console.Write($"{treeHeights[j,k]} ");
		}
		Console.Write("\n");
	    }
	    Console.Write("\n");
	}
	public int[] Row(int j)
	{
	    return  (from k in Enumerable.Range(0,treeHeights.GetLength(1))
		     select treeHeights[j,k]).ToArray();
	}
	public int[] Column(int k)
	{
	    return  (from j in Enumerable.Range(0,treeHeights.GetLength(0))
		     select treeHeights[j,k]).ToArray();
	}
	public bool IsEdge(int j, int k)
	{
	    return (j == 0 || j == treeHeights.GetLength(0)-1 || k == 0 || k == treeHeights.GetLength(1)-1);
	}
	public void PrintEdgeMap()
	{
	    for (int j = 0; j < treeHeights.GetLength(0); j++)
	    {
		for (int k = 0; k < treeHeights.GetLength(1); k++)
		{
		    var marker = IsEdge(j,k) ? 1 : 0;
		    Console.Write($"{marker} ");
		}
		Console.Write("\n");
	    }
	    Console.Write("\n");
	}
	public bool IsVisible(int j, int k)
	{
	    int[] row = this.Row(j), col = this.Column(k);
	    int[] left = new ArraySegment<int>(row,0,k).ToArray();
	    int[] right = new ArraySegment<int>(row,k+1,row.Length-k-1).ToArray();
	    int[] top = new ArraySegment<int>(col,0,j).ToArray();
	    int[] bottom = new ArraySegment<int>(col,j+1,col.Length-j-1).ToArray();
	    bool visibleLeft = true, visibleRight = true, visibleTop = true, visibleBottom = true;
	    for (int l = left.Length-1; l >= 0; l--) // <-
	    {
		if (left[l] >= treeHeights[j,k])
		{
		    visibleLeft = false; // found an equal or higher tree
		    break;
		}
	    }
	    for (int r = 0; r < right.Length; r++) // ->
	    {
		if (right[r] >= treeHeights[j,k])
		{
		    visibleRight = false; // found an equal or higher tree
		    break;
		}
	    }
	    for (int t = top.Length-1; t >= 0; t--) // <-
	    {
		if (top[t] >= treeHeights[j,k])
		{
		    visibleTop = false; // found an equal or higher tree
		    break;
		}
	    }
	    for (int b = 0; b < bottom.Length; b++) // ->
	    {
		if (bottom[b] >= treeHeights[j,k])
		{
		    visibleBottom = false; // found an equal or higher tree
		    break;
		}
	    }
	    return visibleLeft || visibleRight || visibleTop || visibleBottom;
	}
	public void Print1D(int[] slice)
	{
	    Console.Write("[ ");
	    foreach (int s in slice) Console.Write($"{s} ");
	    Console.Write("]\n");
	}
	public void PrintVisibleMap()
	{
	    for (int j = 0; j < treeHeights.GetLength(0); j++)
	    {
		for (int k = 0; k < treeHeights.GetLength(1); k++)
		{
		    var marker = IsVisible(j,k) ? 1 : 0;
		    Console.Write($"{marker} ");
		}
		Console.Write("\n");
	    }
	    Console.Write("\n");
	}
	public int CountVisible()
	{
	    int count = 0;
	    for (int j = 0; j < treeHeights.GetLength(0); j++)
	    {
		for (int k = 0; k < treeHeights.GetLength(1); k++)
		{
		    if (IsVisible(j,k)) count += 1;
		}
	    }
	    return count;
	}
	public int ScenicScore(int j, int k)
	{
	    int[] row = this.Row(j), col = this.Column(k);
	    int[] left = new ArraySegment<int>(row,0,k).ToArray();
	    int[] right = new ArraySegment<int>(row,k+1,row.Length-k-1).ToArray();
	    int[] top = new ArraySegment<int>(col,0,j).ToArray();
	    int[] bottom = new ArraySegment<int>(col,j+1,col.Length-j-1).ToArray();
	    int viewLeft = 0, viewRight = 0, viewTop = 0, viewBottom = 0;
	    for (int l = left.Length-1; l >= 0; l--) // <-
	    {
		viewLeft += 1;
		if (left[l] >= treeHeights[j,k]) break;
	    }
	    for (int r = 0; r < right.Length; r++) // ->
	    {
		viewRight += 1;
		if (right[r] >= treeHeights[j,k]) break;
	    }
	    for (int t = top.Length-1; t >= 0; t--) // <-
	    {
		viewTop += 1;
		if (top[t] >= treeHeights[j,k]) break;
	    }
	    for (int b = 0; b < bottom.Length; b++) // ->
	    {
		viewBottom += 1;
		if (bottom[b] >= treeHeights[j,k]) break;
	    }
	    return viewLeft*viewRight*viewTop*viewBottom;
	}
	public void PrintScenicMap()
	{
	    for (int j = 0; j < treeHeights.GetLength(0); j++)
	    {
		for (int k = 0; k < treeHeights.GetLength(1); k++)
		{
		    var marker = ScenicScore(j,k);
		    Console.Write($"{marker} ");
		}
		Console.Write("\n");
	    }
	    Console.Write("\n");
	}
	public int BestScenicScore()
	{
	    return  (from j in Enumerable.Range(0,treeHeights.GetLength(0))
		     from k in Enumerable.Range(0,treeHeights.GetLength(0))
		     let score = ScenicScore(j,k)
		     orderby score descending
		     select score).First();
	}
	public int BestInvisibleScenicScore()
	{
	    return  (from j in Enumerable.Range(0,treeHeights.GetLength(0))
		     from k in Enumerable.Range(0,treeHeights.GetLength(0))
		     where !IsVisible(j,k)
		     let score = ScenicScore(j,k)
		     orderby score descending
		     select score).First();
	}
    }
}
