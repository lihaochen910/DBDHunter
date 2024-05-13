namespace DBDHunter.Components; 

public readonly struct DbdHatchComponent : IComponent {
	public readonly ulong Addr;
	public readonly EHatchState State;

    public DbdHatchComponent( ulong addr, EHatchState state ) {
    	Addr = addr;
		State = state;
	}
}
