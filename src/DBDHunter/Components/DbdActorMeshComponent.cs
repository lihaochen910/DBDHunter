namespace DBDHunter.Components; 

public readonly struct DbdActorMeshComponent : IComponent {
	
	public readonly ulong ActorAddr;
	public readonly ulong ActorMeshAddr;             // ACharacter::Mesh(USkeletalMeshComponent)
	public readonly FTransform ActorMeshComponentToWorld;
	public readonly ulong SkeletalMesh;         // USkinnedMeshComponent::SkeletalMesh(USkeletalMesh)
	public readonly ulong CachedBoneSpaceTransforms; // USkeletalMeshComponent::CachedBoneSpaceTransforms(TArray<struct FTransform>)
	public readonly int CachedBoneSpaceTransformsNumElements; // USkeletalMeshComponent::CachedBoneSpaceTransforms(TArray<struct FTransform>)
	
	// public DbdActorMeshComponent( ulong actorAddr, ulong actorMeshAddr, ulong skeletalMeshAsset, ulong cachedBoneSpaceTransforms, int cachedBoneSpaceTransformsNumElements )
	// 	: this( actorAddr, actorMeshAddr, skeletalMeshAsset, cachedBoneSpaceTransforms, cachedBoneSpaceTransformsNumElements, ImmutableArray< FTransform >.Empty, ImmutableArray< USkeletalMeshSocket >.Empty ) {
	// }

	public DbdActorMeshComponent( ulong actorAddr, ulong actorMeshAddr, FTransform actorMeshComponentToWorld, ulong skeletalMesh,
								  ulong cachedBoneSpaceTransforms, int cachedBoneSpaceTransformsNumElements ) {
		ActorAddr = actorAddr;
		ActorMeshAddr = actorMeshAddr;
		ActorMeshComponentToWorld = actorMeshComponentToWorld;
		SkeletalMesh = skeletalMesh;
		CachedBoneSpaceTransforms = cachedBoneSpaceTransforms;
		CachedBoneSpaceTransformsNumElements = cachedBoneSpaceTransformsNumElements;
	}

	// public DbdActorMeshComponent( ulong actorAddr, ulong actorMeshAddr, ulong skeletalMeshAsset,
	// 							  ulong cachedBoneSpaceTransforms, int cachedBoneSpaceTransformsNumElements,
	// 							  ImmutableArray< FTransform > cachedBoneSpaceTransformsData )
	// 	: this( actorAddr, actorMeshAddr, skeletalMeshAsset, cachedBoneSpaceTransforms,
	// 		cachedBoneSpaceTransformsNumElements, cachedBoneSpaceTransformsData,
	// 		ImmutableArray< USkeletalMeshSocket >.Empty ) {}

	// public DbdActorMeshComponent( ulong actorAddr, ulong actorMeshAddr, ulong skeletalMeshAsset,
	// 							  ulong cachedBoneSpaceTransforms, int cachedBoneSpaceTransformsNumElements,
	// 							  ImmutableArray< USkeletalMeshSocket > socketsData )
	// 	: this( actorAddr, actorMeshAddr, skeletalMeshAsset, cachedBoneSpaceTransforms, cachedBoneSpaceTransformsNumElements, ImmutableArray< FTransform >.Empty, socketsData ) {}

	
	// public DbdActorMeshComponent SetCachedBoneSpaceTransformsData( ImmutableArray< FTransform > cachedBoneSpaceTransformsData ) {
	// 	return new DbdActorMeshComponent(
	// 		ActorAddr,
	// 		ActorMeshAddr,
	// 		SkeletalMeshAsset,
	// 		CachedBoneSpaceTransforms,
	// 		CachedBoneSpaceTransformsNumElements,
	// 		cachedBoneSpaceTransformsData,
	// 		SocketsData
	// 	);
	// }
	//
	//
	// public DbdActorMeshComponent SetSocketsData( ImmutableArray< USkeletalMeshSocket > socketsData ) {
	// 	return new DbdActorMeshComponent(
	// 		ActorAddr,
	// 		ActorMeshAddr,
	// 		SkeletalMeshAsset,
	// 		CachedBoneSpaceTransforms,
	// 		CachedBoneSpaceTransformsNumElements,
	// 		CachedBoneSpaceTransformsData,
	// 		socketsData
	// 	);
	// }
	
}
