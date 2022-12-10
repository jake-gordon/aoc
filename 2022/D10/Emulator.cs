namespace Emulator;

using System.Collections.Generic;
using System.Linq;

class Computer
{
    CPU cpu;
    CRT crt;
    public Computer(CPU cp, CRT cr)
    {
	cpu = cp;
	crt = cr;
    }
    public void Reset()
    {
	cpu.Reset();
	crt.Reset();
    }
    public void RunSystem() // coordinate CRT & CPU by passing X register positions
    {
	while (cpu.state != RunState.Halted)
	{
	    cpu.Step();
	    crt.DrawPixel(cpu.X);
	}
	crt.PrintCanvas();
    }
    public void RunSystemVerbose()
    {
	cpu.PrintState();
	while (cpu.state != RunState.Halted)
	{
	    cpu.Step();
	    cpu.PrintState();
	    crt.DrawPixel(cpu.X);
	}
	crt.PrintCanvas();
    }
}
class CRT
{
    int height, width;
    int currentPixel;
    char[,] canvas;
    public CRT(int h, int w)
    {
	height = h;
	width = w;
	canvas = new char[h,w];
	currentPixel = 0;
    }
    public void Reset() // clear canvas and move to starting pixel
    {
	canvas = new char[this.height, this.width];
	currentPixel = 0;
    }
    public void DrawPixel(int sp)
    {
	var sprite = Enumerable.Range(sp-1,3);
	char pixel = sprite.Contains(currentPixel%width) ? '#' : '.';
	if (currentPixel < width*height)
	{
	    canvas[currentPixel/width,currentPixel%width] = pixel;
	    currentPixel += 1;
	}
    }
    public void PrintCanvas()
    {
	foreach (int h in Enumerable.Range(0,height))
	{
	    foreach(int w in Enumerable.Range(0,width))
	    {
		Console.Write($"{canvas[h,w]} ");
	    }
	    Console.Write("\n");
	}
    }
}
enum RunState
{
    Stopped, // CPU initialized
    Running,
    Halted   // end of instructions
}
class CPU
{
    public int X { get; set; }
    public int cycle { get; set; }
    public RunState state { get; set; }
    int signal; // = cycle*X
    int addCounter; // keeping track of multi-cycle addx instruction
    string currentOp;
    Stack<string> instructions;
    public CPU()
    {
	X = 1;
	cycle = 0;
	signal = cycle*X;
	addCounter = 0;
	currentOp = String.Empty;
	state = RunState.Halted;
	instructions = new Stack<string>();
    }
    public void LoadInstructions(string fname)
    {
	string[] steps = System.IO.File.ReadAllText(fname).Split("\n");
	for (int j = steps.Length - 1; j >= 0; j--) instructions.Push(steps[j]);
	state = RunState.Stopped;
    }
    public void Reset()
    {
	X = 1;
	cycle = 0;
	signal = cycle*X;
	addCounter = 0;
	currentOp = String.Empty;
	state = RunState.Halted;
	instructions = new Stack<string>();
    }
    public void Run()
    {
	while ( state != RunState.Halted ) Step();
    }
    public void RunVerbose()
    {
	PrintState();
	while ( state != RunState.Halted )
	{
	    Step();
	    PrintState();
	}
    }
    public int[] RunAndMeasureSignals(int[] onCycle)
    {
	int[] signals = new int[] {};
	while ( state != RunState.Halted )
	{
	    if ( onCycle.Contains(cycle) ) signals = signals.Append(signal).ToArray();
	    Step();
	}
	return signals;
    }
    public int[] RunAndMeasureSignalsVerbose(int[] onCycle)
    {
	int[] signals = new int[] {};
	PrintState();
	while ( state != RunState.Halted )
	{
	    if ( onCycle.Contains(cycle) ) signals = signals.Append(signal).ToArray();
	    Step();
	    PrintState();
	}
	return signals;
    }
    public void Step() // take the CPU through one clock cycle
    {
	switch(state)
	{
	    case RunState.Stopped: // grab the first instruction and step through it
		currentOp = instructions.Pop();
		state = RunState.Running;
		cycle += 1;
		break;
	    case RunState.Running:
		if ( currentOp.Split(" ").Length == 1) // came from noop, grab next instruction
		{
		    if (instructions.Count() > 0)
		    {
			currentOp = instructions.Pop();
			cycle += 1;
		    }
		    else state = RunState.Halted;
		}
		else // came from addx
		{
		    if (addCounter == 1) // add to X, reset addCounter, grab next instruction
		    {
			X += int.Parse(currentOp.Split(" ")[1]);
			addCounter = 0;
			if (instructions.Count() > 0)
			{
			    currentOp = instructions.Pop();
			    cycle += 1;
			}
			else state = RunState.Halted;
		    }
		    else  // increment addCounter
		    {
			addCounter = 1;
			cycle += 1;
		    }
		}
		break;
	    case RunState.Halted:
		break;
	}
	signal = cycle*X;
    }
    public void PrintInstructions()
	{
	    foreach(string instr in instructions) Console.WriteLine(instr);
	}
    public void PrintInstructions(int N)
    {
	foreach(string instr in instructions.Take(N)) Console.WriteLine(instr);
    }
    public void PrintState()
    {
	switch(state)
	{
	    case RunState.Stopped:
		Console.WriteLine($"[STOPPED] cycle = {cycle} X = {X} signal = {signal}");
		break;
	    case RunState.Running:
		if (currentOp.Split(" ").Length == 1)
		    Console.WriteLine($"[RUNNING] cycle = {cycle} X = {X} signal = {signal} [{currentOp.ToUpper()}]");
		else
		    Console.WriteLine($"[RUNNING] cycle = {cycle} X = {X} signal = {signal} [{currentOp.ToUpper()}] [{addCounter+1}/2]");
		break;
	    case RunState.Halted:
		Console.WriteLine($"[ HALTED] cycle = {cycle} X = {X} signal = {signal}");
		break;
	}
    }
}
