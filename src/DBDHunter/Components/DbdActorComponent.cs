using DigitalRune.Mathematics.Algebra;


namespace DBDHunter.Components; 

public readonly struct DbdActorComponent : IComponent {
	public readonly ulong ActorAddr;
	public readonly ulong RootComponent;
	public readonly Vector3D RelativeLocation;
	public readonly Vector3D RelativeRotation;
	public readonly Vector3D RelativeScale3D;
	
	public DbdActorComponent( ulong actorAddr, ulong rootComponent, Vector3D relativeLocation, Vector3D relativeRotation, Vector3D relativeScale3D ) {
		ActorAddr = actorAddr;
		RootComponent = rootComponent;
		RelativeLocation = relativeLocation;
		RelativeRotation = relativeRotation;
		RelativeScale3D = relativeScale3D;
	}
}


public readonly struct DbdActorIDComponent : IComponent {
	public readonly int ID;

	public DbdActorIDComponent( int id ) {
		ID = id;
	}
}


public readonly struct DbdActorNameComponent : IComponent {
	public readonly FName Name;

	public DbdActorNameComponent( FName name ) {
		Name = name;
	}
}
