using Bang.Components;


namespace DBDHunter.Components; 

[Unique]
public readonly struct DbdLocalPlayersComponent : IComponent {
	public readonly ulong Addr;
	
	public DbdLocalPlayersComponent( ulong addr ) {
		Addr = addr;
	}

}
