using Bang.Components;


namespace DBDHunter.Components; 

[Unique]
public readonly struct DbdGWorldComponent : IComponent {
	public readonly ulong Addr;
	
	public DbdGWorldComponent( ulong addr ) {
		Addr = addr;
	}

}


[Unique]
public readonly struct DbdGWorldOffsetComponent : IComponent {
	public readonly ulong Offset;
	
	public DbdGWorldOffsetComponent( ulong offset ) {
		Offset = offset;
	}

}
