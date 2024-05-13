using Bang.Components;


namespace DBDHunter.Components; 

[Unique]
public readonly struct DbdGObjectsComponent : IComponent {
	public readonly ulong Addr;
	
	public DbdGObjectsComponent( ulong addr ) {
		Addr = addr;
	}
}


[Unique]
public readonly struct DbdGObjectsOffsetComponent : IComponent {
	public readonly ulong Offset;
	
	public DbdGObjectsOffsetComponent( ulong offset ) {
		Offset = offset;
	}

}
