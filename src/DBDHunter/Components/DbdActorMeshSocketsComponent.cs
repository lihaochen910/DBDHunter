namespace DBDHunter.Components; 

public readonly struct DbdActorMeshSocketsComponent : IComponent {
	
	public readonly ImmutableArray< ulong > Sockets;
	public readonly ImmutableArray< USkeletalMeshSocket > SocketsData;
	
	public DbdActorMeshSocketsComponent( ImmutableArray< ulong > sockets, ImmutableArray< USkeletalMeshSocket > socketsData ) {
		Sockets = sockets;
		SocketsData = socketsData;
	}

	
	public USkeletalMeshSocket FindBoneByName( string boneName ) {
		if ( !SocketsData.IsDefaultOrEmpty ) {
			foreach ( var socket in SocketsData ) {
				if ( socket.BoneName.ToString().Equals( boneName ) ) {
					return socket;
				}
			}
		}

		return null;
	}
	
	public USkeletalMeshSocket FindBoneByFName( in FName boneName ) {
		if ( !SocketsData.IsDefaultOrEmpty ) {
			foreach ( var socket in SocketsData ) {
				if ( socket.BoneName.ComparisonIndex == boneName.ComparisonIndex ) {
					return socket;
				}
			}
		}

		return null;
	}

}


public readonly struct DbdActorMeshSocketsDumpedComponent : IComponent {
	
	public readonly ImmutableArray< USkeletalMeshSocketDumped > SocketsData;
	
	public DbdActorMeshSocketsDumpedComponent( ImmutableArray< USkeletalMeshSocketDumped > socketsData ) {
		SocketsData = socketsData;
	}
}
