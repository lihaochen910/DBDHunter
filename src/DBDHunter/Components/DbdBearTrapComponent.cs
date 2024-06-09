using Murder.Utilities.Attributes;


namespace DBDHunter.Components; 

[RuntimeOnly]
public readonly struct DbdBearTrapComponent : IComponent {
	
	public readonly bool IsTrapSet;
	
	public DbdBearTrapComponent( bool isTrapSet ) {
		IsTrapSet = isTrapSet;
	}
	
}
