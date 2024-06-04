namespace DBDHunter.Components; 

public readonly struct DbdActorMeshRefSkeletonComponent : IComponent {

	public readonly ImmutableArray< FMeshBoneInfo > RawRefBoneInfo;

	public DbdActorMeshRefSkeletonComponent( ImmutableArray< FMeshBoneInfo > rawRefBoneInfo ) {
		RawRefBoneInfo = rawRefBoneInfo;
	}

	public FMeshBoneInfo? FindBoneInfoByKeyword( string keyword ) {
		foreach ( var meshBoneInfo in RawRefBoneInfo ) {
			var boneName = meshBoneInfo.Name.ToString();
			if ( boneName != null && boneName.Contains( keyword ) ) {
				return meshBoneInfo;
			}
		}

		return null;
	}

}
