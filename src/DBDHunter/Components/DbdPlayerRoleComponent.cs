using Bang.Components;


namespace DBDHunter.Components; 

[Requires(typeof( DbdActorComponent ))]
public readonly struct DbdPlayerRoleComponent : IComponent {
	
	public readonly EPlayerRole Role;
	
	public DbdPlayerRoleComponent( EPlayerRole role ) {
		Role = role;
	}
}
