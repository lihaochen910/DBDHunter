using Bang;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;
using DBDHunter.Utilities;


namespace DBDHunter.Systems; 

[Watch( typeof( DbdGameProcessComponent ) )]
public class DbdEngineInitializeSystem : IReactiveSystem, IExitSystem {

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			InitDbdEngineOffsets( entity );
		}
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			entity.RemoveDbdGWorld();
			entity.RemoveDbdGNames();
			entity.RemoveRefreshListener();
			
			Driver.GWorld = 0;
			Driver.GNamesTable = 0;
		}
	}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {}


	public static void InitDbdEngineOffsets( Entity gameProcessEntity ) {
		if ( gameProcessEntity.TryGetComponent< DbdGameProcessComponent >() is not {} dbdGameProcessComponent ) {
			return;
		}
		
		var gWorld = Memory.Read< ulong >( dbdGameProcessComponent.BaseAddr + gameProcessEntity.GetDbdGWorldOffset().Offset );
		if ( gWorld != 0 ) {
			Logger.Debug( $"GWorld: 0x{gWorld:X8}" );
			Driver.GWorld = gWorld;
			gameProcessEntity.SetDbdGWorld( gWorld );
		}
		else {
			Logger.Error( $"GWorld is nullptr!" );	
		}
			
		ulong gNames = dbdGameProcessComponent.BaseAddr + gameProcessEntity.GetDbdGNamesOffset().Offset;
		if ( gNames != 0 ) {
			Logger.Debug( $"GNames: 0x{gNames:X8}" );
			// debug:
			// Memory.PeekAddressBytes( gNames, 0x0ff );
			// Memory.PrintStructureByteArray(Vector3D.One);
			Driver.GNamesTable = gNames;
			gameProcessEntity.SetDbdGNames( gNames );
		}
		else {
			Logger.Warning( "GNames is nullptr!" );	
		}
			
		var gObjects = dbdGameProcessComponent.BaseAddr + gameProcessEntity.GetDbdGObjectsOffset().Offset;
		if ( gObjects != 0 ) {
			Logger.Debug( $"GObjects: 0x{gObjects:X8}" );
			gameProcessEntity.SetDbdGObjects( gObjects );
		}
		else {
			Logger.Warning( "GObjects is nullptr!" );	
		}
			
		gameProcessEntity.SetRefreshTimer( -1f );
	}

	public void Exit( Context context ) {
		Driver.GWorld = 0;
		Driver.GNamesTable = 0;
		UEHelper.ClearUClassPtrCache();
		Logger.Debug( "DbdEngine Exit." );
	}
}
