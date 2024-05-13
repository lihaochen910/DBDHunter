namespace DBDHunter.Components; 

public readonly struct DbdSearchableComponent : IComponent {
	public readonly ulong Addr;
	public readonly ulong SpawnedItem;

	public DbdSearchableComponent( ulong addr, ulong spawnedItem ) {
		Addr = addr;
		SpawnedItem = spawnedItem;
	}
}