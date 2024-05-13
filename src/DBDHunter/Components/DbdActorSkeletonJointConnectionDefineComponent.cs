namespace DBDHunter.Components;

public record struct DbdSkeletonJointConnection( int BoneA, int BoneB ) {
	// public int BoneA; // bone index define in RawRefBoneInfo
	// public int BoneB;
}

public readonly struct DbdActorSkeletonJointConnectionDefineComponent : IComponent {

	public readonly ImmutableArray< DbdSkeletonJointConnection > Connections;
	
	public DbdActorSkeletonJointConnectionDefineComponent( ImmutableArray< DbdSkeletonJointConnection > connections ) {
		Connections = connections;
	}

}
