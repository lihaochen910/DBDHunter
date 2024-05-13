using Bang.Components;


namespace DBDHunter.Components; 

[Unique]
public readonly struct DbdGameInstanceComponent : IComponent {
	public readonly ulong Addr;
	
	public DbdGameInstanceComponent( ulong addr ) {
		Addr = addr;
	}

}
