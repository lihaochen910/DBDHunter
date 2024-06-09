using Murder.Utilities.Attributes;


namespace DBDHunter.Components; 

[RuntimeOnly]
public readonly struct DbdPalletComponent : IComponent {

	public readonly EPalletState State;
    	
	public DbdPalletComponent( EPalletState state ) {
		State = state;
	}
}
