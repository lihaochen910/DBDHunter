using Bang.Components;
using vmmsharp;


namespace DBDHunter.Components; 

[Unique]
public readonly struct DbdGameProcessComponent : IComponent {
	public readonly uint Pid;
	public readonly ulong BaseAddr;
	public readonly Vmm.PROCESS_INFORMATION ProcessInformation;

	public DbdGameProcessComponent( uint pid, ulong baseAddr, in Vmm.PROCESS_INFORMATION processInformation ) {
		Pid = pid;
		BaseAddr = baseAddr;
		ProcessInformation = processInformation;
	}
}
