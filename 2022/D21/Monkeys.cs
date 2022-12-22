namespace Monkeys;

using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public class Hollering
{
    // part 1
    Dictionary<string,long> loneMonkeys;
    Dictionary<string,(string dep1, string op, string dep2)> depMonkeys;
    // part 2: evaluating all expressions not involving the human
    Dictionary<string,long> evalMonkeys;
    Dictionary<string,(string dep1, string op, string dep2)> unevalMonkeys;
    public Hollering(string fname)
    {
	Regex rex = new Regex(@"\d+");
	var monkeyList =  System.IO.File.ReadAllText(fname).Split("\n");

	loneMonkeys = new Dictionary<string,long>();
	var lM = (from m in monkeyList
		  let matches = rex.Matches(m)
		  where matches.Count == 1
		  select (m.Split(":")[0],long.Parse(matches.First().Value)));
	foreach (var l in lM) loneMonkeys[l.Item1] = l.Item2;

	depMonkeys = new Dictionary<string,(string dep1, string op, string dep2)>();
	var dM = (from m in monkeyList
		  let matches = rex.Matches(m)
		  where matches.Count == 0
		  select (m.Split(":")[0],
			  m.Split(":")[1].Split(" ")[1],
			  m.Split(":")[1].Split(" ")[2],
			  m.Split(":")[1].Split(" ")[3]));
	foreach (var d in dM) depMonkeys[d.Item1] = (d.Item2,d.Item3,d.Item4);

	evalMonkeys = new Dictionary<string,long>();
	unevalMonkeys = new Dictionary<string,(string dep1, string op, string dep2)>();
    }
    public void EvaluateRoot()
    {
	var result = EvaluateDependentExpression(depMonkeys["root"]);
	Console.WriteLine($"The value of the root expression is {result}.");
    }
    public long EvaluateDependentExpression((string dep1, string op, string dep2) dep)
    {
	// simple recursive evaluation, substituting the actual values when appropriate
	bool dep1Dependent = depMonkeys.ContainsKey(dep.dep1);
	bool dep2Dependent = depMonkeys.ContainsKey(dep.dep2);
	long dep1Val = dep1Dependent ? EvaluateDependentExpression(depMonkeys[dep.dep1]) : loneMonkeys[dep.dep1];
	long dep2Val = dep2Dependent ? EvaluateDependentExpression(depMonkeys[dep.dep2]) : loneMonkeys[dep.dep2];
	switch (dep.op)
	{
	    case "+":
		return dep1Val + dep2Val;
	    case "-":
		return dep1Val - dep2Val;
	    case "*":
		return dep1Val * dep2Val;
	    case "/":
		return dep1Val / dep2Val;
	    default:
		throw new ArgumentException($"Invalid operation {dep.op}.");
	}
    }
    public void EvaluateDeterminedExpressions()
    {
	// go through each dependent expression, and evaluate it completely if it doesn't depend
	// on the value the human shouts; for the real input this brought the number of unevaluated
	// expressions from ~2000 down to ~100, and actually revealed that one half of the root
	// expression is independent of the human value!
	foreach (var d in loneMonkeys)
	    if (d.Key != "humn") evalMonkeys[d.Key] = d.Value;
	foreach (var d in depMonkeys)
	    if (!ExpressionDependsOnHuman(d.Value)) evalMonkeys[d.Key] = EvaluateDependentExpression(d.Value);
	foreach (var d in depMonkeys)
	    if (!evalMonkeys.ContainsKey(d.Key)) unevalMonkeys[d.Key] = d.Value;
    }
    public bool ExpressionDependsOnHuman((string dep1, string op, string dep2) dep)
    {
	// recursive check if this expression or any of its sub-expressions depend on the value
	// the human shouts
	if ( (dep.dep1 == "humn") || (dep.dep2 == "humn") ) return true;
	// otherwise, if it's a dependent expression, check if it depends on human
	if (depMonkeys.ContainsKey(dep.dep1) && ExpressionDependsOnHuman(depMonkeys[dep.dep1]))
	    return true;
	if (depMonkeys.ContainsKey(dep.dep2) && ExpressionDependsOnHuman(depMonkeys[dep.dep2]))
	    return true;
	// if none of these encountered the human, it's not a dependency
	return false;
    }
    public void EvaluateModifiedRoot()
    {
	var root = depMonkeys["root"];
	Console.WriteLine($"Checking if {root.dep1} == {root.dep2}...");

	// in both the test and real input, one of the inputs was completely independent of human
	// so we really only needed to evaluate the dependent side in whatever way to find what to yell
	bool d1 = evalMonkeys.ContainsKey(root.dep1),  d2 = evalMonkeys.ContainsKey(root.dep2);
	string eval, uneval;
	if (d1)
	{
	    eval = root.dep1; uneval = root.dep2;
	}
	else
	{
	    eval = root.dep2; uneval = root.dep1;
	}
	Console.WriteLine($"The expression {eval} is independent of the human and equal to {evalMonkeys[eval]}.");
	Console.WriteLine($"The expression {uneval} depends on the human and starts with {unevalMonkeys[uneval]}.");
	Console.WriteLine($"By partially evaluating there are only {unevalMonkeys.Count}/{evalMonkeys.Count+unevalMonkeys.Count} unevaluated.");

	// some values we could yell to get the difference between the two outputs to be > 0, < 0
	// perform a silly scaling BFS to get a positive and negative value to seed bisection
	// (it's advent of code, gotta fit in BFS somehow)
	long hc = 10, scl = 10;
	long hm = 0, hp = 0;
	bool haveNegative = false, havePositive = false;
	var queue = new Queue<long>();
	queue.Enqueue(hc);
	while (queue.Count > 0)
	{
	    hc = queue.Dequeue();
	    if ( haveNegative && havePositive ) continue;
	    var current = DifferenceInOutputs(hc,uneval,eval);
	    if ( current > 0 )
	    {
		hp = hc;
		havePositive = true;
	    }
	    if ( current < 0 )
	    {
		hm = hc;
		haveNegative = true;
	    }
	    if ( current == 0 ) Console.WriteLine($"Got lucky at h = {hc}.");
	    queue.Enqueue(-hc);
	    queue.Enqueue(-scl*hc);
	}
	Console.WriteLine($"Found {hm} with {DifferenceInOutputs(hm,uneval,eval)} and {hp} with {DifferenceInOutputs(hp,uneval,eval)}.");

	// perform bisection method, function is a continuous function of h and should have an integer root
	long n = 0, Nmax = 100; // number of iterations
	while ( n <= Nmax )
	{
	    // midpoints, checking both just in case we miss the root via rounding
	    var mid1 = (hm + hp)/2;
	    var mid2 = mid1+1;
	    var midEval1 = DifferenceInOutputs(mid1,uneval,eval);
	    var midEval2 = DifferenceInOutputs(mid2,uneval,eval);
	    if ( midEval1 == 0 )
	    {
		Console.WriteLine($"Solution found at {mid1}!");
		break;
	    }
	    if ( midEval2 == 0 )
	    {
		Console.WriteLine($"Solution found at {mid2}!");
		break;
	    }
	    // squeeze the interval
	    if ( midEval1 < 0)  hm = mid1;
	    else hp = mid1;
	    n += 1;
	}

    }
    public long DifferenceInOutputs(long h, string uneval, string eval)
    {
	// given the two sides of the root expression, form their difference
	evalMonkeys["humn"] = h;
	return EvaluateHumanExpression(unevalMonkeys[uneval])-evalMonkeys[eval];
    }
    public long EvaluateHumanExpression((string dep1, string op, string dep2) dep)
    {
	// same recursive definition as in part 1, just using the maximally evaluated expressions instead
	bool dep1Dependent = unevalMonkeys.ContainsKey(dep.dep1);
	bool dep2Dependent = unevalMonkeys.ContainsKey(dep.dep2);
	long dep1Val = dep1Dependent ? EvaluateHumanExpression(unevalMonkeys[dep.dep1]) : evalMonkeys[dep.dep1];
	long dep2Val = dep2Dependent ? EvaluateHumanExpression(unevalMonkeys[dep.dep2]) : evalMonkeys[dep.dep2];
	switch (dep.op)
	{
	    case "+":
		return dep1Val + dep2Val;
	    case "-":
		return dep1Val - dep2Val;
	    case "*":
		return dep1Val * dep2Val;
	    case "/":
		return dep1Val / dep2Val;
	    default:
		throw new ArgumentException($"Invalid operation {dep.op}.");
	}
    }
}
