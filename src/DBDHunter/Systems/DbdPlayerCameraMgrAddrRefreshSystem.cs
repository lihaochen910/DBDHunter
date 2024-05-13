using Bang;
using Bang.Components;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Assets;
using DBDHunter.Components;
using DBDHunter.Services;


namespace DBDHunter.Systems; 

[Messager( typeof( RequestRefreshMessage ) )]
[Filter( typeof( DbdLocalPlayersComponent ) )]
public class DbdPlayerCameraMgrAddrRefreshSystem : IMessagerSystem {

	public void OnMessage( World world, Entity entity, IMessage message ) {
		
		var localPlayer0 = Memory.Read< ulong >( entity.GetDbdLocalPlayers().Addr );
		if ( localPlayer0 != 0 ) {
			
			var playerController = Memory.Read< ulong >( localPlayer0 + Offsets.UPlayer_PlayerController );
			if ( playerController != 0 ) {
				
				if ( world.TryGetUniqueEntity< DbdPlayerCameraManagerComponent >() is null ) {
                	var playerCameraMgrEntity = LibraryServices.GetLibrary().SpawnPrefab( nameof( LibraryAsset.PlayerCameraManager ), world );
					playerCameraMgrEntity.SetDbdPlayerCameraManager( 0ul, default );
				}
				
				var playerCameraEntity = world.GetUniqueEntity< DbdPlayerCameraManagerComponent >();
				var cameraManagerAddr = Memory.Read< ulong >( playerController + Offsets.APlayerController_PlayerCameraManager );
				if ( cameraManagerAddr != 0 ) {
					playerCameraEntity.SetDbdPlayerCameraManager(
						cameraManagerAddr,
						Memory.Read< FCameraCacheEntry >( cameraManagerAddr + Offsets.APlayerCameraManager_CameraCachePrivate )
					);
				}
				else {
					playerCameraEntity.Destroy();
				}
			}
			else {
				if ( world.TryGetUniqueEntity< DbdPlayerCameraManagerComponent >() is {} playerCameraEntity ) {
					playerCameraEntity.Destroy();
				}
			}
					
		}
	}
}