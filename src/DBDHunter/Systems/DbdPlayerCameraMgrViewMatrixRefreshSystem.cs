using Bang;
using Bang.Components;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;
using vmmsharp;


namespace DBDHunter.Systems;

[Messager( typeof( RequestRefreshMessage ) )]
[Filter( typeof( DbdPlayerCameraManagerComponent ) )]
public class DbdPlayerCameraMgrViewMatrixRefreshSystem : IMessagerSystem {

	public void OnMessage( World world, Entity entity, IMessage message ) {
		var playerCameraManagerAddr = entity.GetDbdPlayerCameraManager().Addr;
		var handle = Driver.CreateScatterHandle();
		handle.Prepare( playerCameraManagerAddr + Offsets.APlayerCameraManager_CameraCachePrivate, 0x07D0 );
		handle.Execute();
		var bytes = handle.Read(
			playerCameraManagerAddr + Offsets.APlayerCameraManager_CameraCachePrivate, 0x07D0 );
		if ( bytes is null || bytes.Length < 1 ) {
			handle.Clear( Driver.ProcessPid, Vmm.FLAG_NOCACHE );
			handle.Close();
			return;
		}
		
		var cameraCacheEntry = Memory.ByteArrayToStructure< FCameraCacheEntry >( bytes );
		entity.SetDbdPlayerCameraManager( playerCameraManagerAddr, cameraCacheEntry );
		
		handle.Clear( Driver.ProcessPid, Vmm.FLAG_NOCACHE );
		handle.Close();
	}
}
