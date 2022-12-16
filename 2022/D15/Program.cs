using Network;

class Program
{
    static void Main(string[] args)
    {
	string fname = "./beacons.dat";

	// part 1
	Caverns caverns = new Caverns(fname,false);
	// caverns.CountAlongHorizontal(10);
	caverns.CountAlongHorizontal(2000000);

	// part 2
	caverns = new Caverns(fname,true);
	caverns.GetFrequencyOfMissingBeacon();
    }
}
