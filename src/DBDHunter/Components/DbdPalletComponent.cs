namespace DBDHunter.Components; 

public readonly struct DbdPalletComponent : IComponent {
	public readonly ulong Addr;
	public readonly EPalletState State;
    	
	public DbdPalletComponent( ulong addr, EPalletState state ) {
		Addr = addr;
		State = state;
	}
}
