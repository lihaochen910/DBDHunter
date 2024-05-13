using Bang.Components;


namespace DBDHunter.Components; 

[Unique]
public readonly struct DbdPlayerControllerComponent : IComponent {
	public readonly ulong Addr;
	
	public DbdPlayerControllerComponent( ulong addr ) {
		Addr = addr;
	}

}
