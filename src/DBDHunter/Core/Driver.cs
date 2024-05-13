using vmmsharp;


namespace DBDHunter; 

internal class Driver {
	
#pragma warning disable CS8618

	public static Vmm vmm;
	public static ulong GameAssembly;
	public static uint ProcessPid;
	public static GamePlatform GamePlatform;
	public static ulong GWorld;
	public static ulong GNamesTable;


	public static VmmScatter CreateScatterHandle() => vmm.Scatter_Initialize( ProcessPid, Vmm.FLAG_NOCACHE );
}
