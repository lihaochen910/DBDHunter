using Bang.Components;
using DigitalRune.Geometry.Shapes;


namespace DBDHunter.Components; 

public readonly struct DbdActorBoxShapeComponent : IModifiableComponent {

	public readonly BoxShape BoxShape;

	public DbdActorBoxShapeComponent( BoxShape boxShape ) {
		BoxShape = boxShape;
	}

	public void Subscribe( Action notification ) {
		// BoxShape.Changed += ( sender, args ) => notification();
	}

	public void Unsubscribe( Action notification ) {
		// BoxShape.Changed -= ( sender, args ) => notification();
	}
}
