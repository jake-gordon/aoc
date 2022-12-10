using Emulator;

class Program
{
    static void Main(string[] args)
    {
	CPU cpu = new CPU();

	// part 1
	cpu.LoadInstructions("./instructions.dat");

	int[] onCycle = new int[] {20,60,100,140,180,220};
	int[] signals = cpu.RunAndMeasureSignals(onCycle);
	int totalSignal = signals.Sum();

	Console.WriteLine($"Sampling the following signals yield a total of {totalSignal}.\n");
	Console.WriteLine("cycle\tsignal");
	foreach (int j in Enumerable.Range(0,onCycle.Length))
	{
	    Console.WriteLine($"{onCycle[j]}\t{signals[j]}");
	}
	cpu.Reset();

	// part 2
	cpu.LoadInstructions("./instructions.dat");
	CRT crt = new CRT(6,40);
	Computer computer = new Computer(cpu,crt);
	Console.WriteLine("\nThe output of the CRT is:\n");
	computer.RunSystem();
    }
}
