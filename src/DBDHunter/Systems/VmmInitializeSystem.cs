using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Assets;
using DBDHunter.Components;
using DBDHunter.Services;
using vmmsharp;


namespace DBDHunter.Systems; 

[Filter]
public class VmmInitializeSystem : IStartupSystem, IExitSystem/*, IFixedUpdateSystem*/ {
	
	public void Start( Context context ) {
		Init( context );
	}
	
	public void Exit( Context context ) {
		if ( context.World.TryGetUnique< VmmComponent >() is {} vmmComponent ) {
			vmmComponent.Vmm.Close();
			Driver.vmm = null;
		}
	}

	public void FixedUpdate( Context context ) {
		if ( context.World.TryGetUniqueEntity< VmmComponent >() is null ) {
			Init( context );
		}
	}

	private void Init( Context context ) {
		Vmm vmm = default;
		try {
			vmm = new Vmm( "-printf", "-v", "-device", "fpga" );
		}
		catch ( Exception e ) {
			Logger.Error( e.Message );
		}

		if ( vmm != null ) {
			Logger.Debug( "Init Vmm Successful." );
			
			var vmmEntity = LibraryServices.GetLibrary().SpawnPrefab( nameof( LibraryAsset.Vmm ), context.World );
			vmmEntity.SetVmm( vmm );
			vmmEntity.SetCameraFollow();
			Driver.vmm = vmm;
		}
	}
	
}
