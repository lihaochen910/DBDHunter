namespace DBDHunter.Components; 

public readonly struct DbdTotemComponent : IComponent {
	public readonly ulong Addr;
	public readonly ETotemState State;

	public DbdTotemComponent( ulong addr, ETotemState state ) {
		Addr = addr;
		State = state;
	}
}
