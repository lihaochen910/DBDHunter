using Bang.Components;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DBDHunter.Components; 

public readonly struct DbdActorBoxShapeComponent : IModifiableComponent {

	public readonly BoxShape BoxShape;
	public readonly Vector3F Origin;
	public readonly float SphereRadius;

	public DbdActorBoxShapeComponent( BoxShape boxShape, Vector3F origin, float sphereRadius ) {
		BoxShape = boxShape;
		Origin = origin;
		SphereRadius = sphereRadius;
	}

	public DbdActorBoxShapeComponent UpdateBoxShape( Vector3F boxExtent ) {
		BoxShape.Extent = boxExtent;
		return this;
	}

	public void Subscribe( Action notification ) {
		// BoxShape.Changed += ( sender, args ) => notification();
	}

	public void Unsubscribe( Action notification ) {
		// BoxShape.Changed -= ( sender, args ) => notification();
	}
}
