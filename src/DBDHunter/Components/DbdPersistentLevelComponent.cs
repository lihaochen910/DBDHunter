using Bang.Components;


namespace DBDHunter.Components; 

[Unique]
public readonly struct DbdPersistentLevelComponent : IComponent {
	public readonly ulong Addr;
	
	public DbdPersistentLevelComponent( ulong addr ) {
		Addr = addr;
	}

}
