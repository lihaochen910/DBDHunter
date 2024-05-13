namespace DBDHunter.Components; 

public readonly struct DbdActorMeshCachedBoneSpaceTransformsComponent : IComponent {
	
	public readonly ImmutableArray< FTransform > CachedBoneSpaceTransformsData;
	
	public DbdActorMeshCachedBoneSpaceTransformsComponent( ImmutableArray< FTransform > cachedBoneSpaceTransformsData ) {
		CachedBoneSpaceTransformsData = cachedBoneSpaceTransformsData;
	}

}
