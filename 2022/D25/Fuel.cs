namespace Fuel;

using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public class Balloon
{
    List<SNAFU> fuelLevels;
    public Balloon(string fname)
    {
	fuelLevels = (from s in System.IO.File.ReadAllText(fname).Split("\n")
		      select new SNAFU(s)).ToList();
    }
    public void TotalFuelLevelsSNAFU()
    {
	long sum = (from s in fuelLevels
		    select s.DecimalValue).Sum();
	var sn = new SNAFU(sum);
	Console.WriteLine($"The total fuel level is {sn.DecimalValue} or {sn.SNAFUValue} in snafu.");
    }
    public void PrintFuelDecimal()
    {
	Console.WriteLine("The current fuel levels in decimal are:");
	foreach (var n in Enumerable.Range(0,fuelLevels.Count))
	    Console.WriteLine($"{n}: {fuelLevels[n].DecimalValue}");
    }
    public void PrintFuelSNAFU()
    {
	Console.WriteLine("The current fuel levels in snafu are:");
	foreach (var n in Enumerable.Range(0,fuelLevels.Count))
	    Console.WriteLine($"{n}: {fuelLevels[n].SNAFUValue}");
    }
}
public class SNAFU
{
    long dval, b;
    public long DecimalValue
    {
	get { return dval; }
    }
    public string SNAFUValue
    {
	get
	{
	    // find the smallest power that includes our number
	    // in the allowable range, based on inverting (5^n-1)/2
	    var n = Math.Ceiling(Math.Log(2*Math.Abs(dval)+1,b));
	    // shift it to an equivalent positive range covered by base 5
	    var dmod = dval + ((long)Math.Pow(b,n)-1)/2;
	    // decompose this number in base 5
	    var digits = new List<long>();
	    var quotient = dmod;
	    while (quotient != 0)
	    {
		digits.Add(quotient%b);
		quotient = (quotient-digits.Last())/b;
	    }
	    digits.Reverse();
	    // map the base 5 digits onto snafu, 4 = 2, 0 = =
	    var chars = new List<char>();
	    foreach (var d in digits)
	    {
		switch (d)
		{
		    case 4:
			chars.Add('2');
			break;
		    case 3:
			chars.Add('1');
			break;
		    case 2:
			chars.Add('0');
			break;
		    case 1:
			chars.Add('-');
			break;
		    case 0:
			chars.Add('=');
			break;
		}
	    }
	    return String.Join("",chars);
	}
    }
    public SNAFU(long x)
    {
	b = 5;
	dval = x;
    }
    public SNAFU(string v)
    {
	var chars = new Stack<char>(v.ToCharArray());
	dval = 0;
	b = 5;
	int p = 0;
	while (chars.Count > 0)
	{
	    switch (chars.Pop())
	    {
		case '2':
		    dval += 2*(long)Math.Pow(b,p);
		    break;
		case '1':
		    dval += 1*(long)Math.Pow(b,p);
		    break;
		case '0':
		    break;
		case '-':
		    dval -= 1*(long)Math.Pow(b,p);
		    break;
		case '=':
		    dval -= 2*(long)Math.Pow(b,p);
		    break;
	    }
	    p += 1;
	}
    }
}
