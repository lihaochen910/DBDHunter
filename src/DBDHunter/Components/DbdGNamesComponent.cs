using Bang.Components;


namespace DBDHunter.Components; 

[Unique]
public readonly struct DbdGNamesComponent : IComponent {
	public readonly ulong Addr;
	
	public DbdGNamesComponent( ulong addr ) {
		Addr = addr;
	}
}


[Unique]
public readonly struct DbdGNamesOffsetComponent : IComponent {
	public readonly ulong Offset;
	
	public DbdGNamesOffsetComponent( ulong offset ) {
		Offset = offset;
	}

}
