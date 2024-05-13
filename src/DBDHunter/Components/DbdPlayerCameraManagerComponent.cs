using Bang.Components;


namespace DBDHunter.Components; 

[Unique]
public readonly struct DbdPlayerCameraManagerComponent : IComponent {
	public readonly ulong Addr;
	public readonly FCameraCacheEntry CameraEntry;
	
	public DbdPlayerCameraManagerComponent( ulong addr, FCameraCacheEntry cameraEntry ) {
		Addr = addr;
		CameraEntry = cameraEntry;
	}

}
