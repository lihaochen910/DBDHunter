using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;


namespace DBDHunter.Systems; 

[Watch( typeof( DbdActorMeshRefSkeletonComponent ) )]
public class DbdActorSkeletonConnectionInitializeSystem : IReactiveSystem {

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			BuildSkeletonJointConnection( entity );
		}
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			entity.RemoveDbdActorSkeletonJointConnectionDefine();
		}
	}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			BuildSkeletonJointConnection( entity );
		}
	}


	private void BuildSkeletonJointConnection( Entity entity ) {
		var actorMeshRefSkeletonComponent = entity.GetDbdActorMeshRefSkeleton();
		var list = new List< DbdSkeletonJointConnection >();
		foreach ( var pair in Offsets.DbdCamperJointConnections ) {
			var foundBoneA = actorMeshRefSkeletonComponent.FindBoneInfoByKeyword( pair.First );
			var foundBoneB = actorMeshRefSkeletonComponent.FindBoneInfoByKeyword( pair.Second );
			if ( foundBoneA.HasValue && foundBoneB.HasValue ) {
				list.Add( new DbdSkeletonJointConnection(
					actorMeshRefSkeletonComponent.RawRefBoneInfo.IndexOf( foundBoneA.Value ),
					actorMeshRefSkeletonComponent.RawRefBoneInfo.IndexOf( foundBoneB.Value )
				) );
			}
		}
		entity.SetDbdActorSkeletonJointConnectionDefine( [..list] );
	}
	
}
