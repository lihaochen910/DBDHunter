using System.Text;
using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Assets;
using DBDHunter.Components;
using DBDHunter.Services;
using DBDHunter.Utilities;
using vmmsharp;


namespace DBDHunter.Systems; 

[Messager( typeof( RequestRefreshMessage ) )]
[Filter( typeof( DbdGWorldComponent ), typeof( DbdGNamesComponent ) )]
public class DbdGameInstanceRefreshSystem : IMessagerSystem, IExitSystem {

	
	public void OnMessage( World world, Entity entity, IMessage message ) {

		// DestroyAllDbdActors( world );

		var gWorldAddr = entity.GetDbdGWorld().Addr;
		// var gNamesTable = entity.GetDbdGNames().Addr;
		
		// if ( world.TryGetUniqueEntity< DbdPersistentLevelComponent >() is null ) {
		// 	var persistentLevelEntity = LibraryServices.GetLibrary().SpawnPrefab( nameof( LibraryAsset.PersistentLevel ), world );
		// 	persistentLevelEntity.SetDbdPersistentLevel( 0ul );
		// }
		
		// var persistentLevel = Memory.Read< ulong >( gWorldAddr + Offsets.UWorld_PersistentLevel );
		// if ( persistentLevel != 0 ) {
		// 	// Logger.Debug( $"PersistentLevel: 0x{persistentLevel:X8}" );
		//
		// }

		var owningGameInstance = Memory.Read< ulong >( gWorldAddr + Offsets.UWorld_OwningGameInstance );
		if ( owningGameInstance != 0 ) {
			// Logger.Debug( $"OwningGameInstance: 0x{owningGameInstance:X8}" );
			entity.SetDbdGameInstance( owningGameInstance );
			
			var localPlayers = Memory.Read< ulong >( owningGameInstance + Offsets.UGameInstance_LocalPlayers );
			if ( localPlayers != 0 ) {
				
				// 假定LocalPlayers的地址不会发生改变
				if ( world.TryGetUniqueEntity< DbdLocalPlayersComponent >() is {} localPlayersArrayEntity ) {
					localPlayersArrayEntity.SetDbdLocalPlayers( localPlayers );
				}
				else {
					localPlayersArrayEntity = LibraryServices.GetLibrary().SpawnPrefab( nameof( LibraryAsset.LocalPlayers ), world );
					localPlayersArrayEntity.SetDbdLocalPlayers( 0ul );
				}
				
				// var localPlayerCount =
				// 	Memory.Read< uint >( owningGameInstance + Offsets.UGameInstance_LocalPlayers + 0x8 );
				// Logger.Debug( $"LocalPlayers count = : {localPlayerCount}" );
			}
			else {
				if ( world.TryGetUniqueEntity< DbdLocalPlayersComponent >() is {} localPlayersArrayEntity ) {
					localPlayersArrayEntity.SetDbdLocalPlayers( 0ul );
				}
			}
			
		}
		else {
			entity.RemoveDbdGameInstance();
		}
		
	}

	public void Exit( Context context ) {
		
	}
	
	

	
	// public void Draw( RenderContext render, Context context ) {
	// 	const float fontSize = 12f;
	// 	const float fontScale = 2f;
	// 	var drawInfo = new DrawInfo( Color.Green, 0.1f ) { Scale = Vector2.One * fontScale };
	//
	// 	var lineCounter = 1;
	// 	void Line( string line ) {
	// 		render.UiBatch.DrawText( 103, line, new Vector2( 2, Game.Height / 3f + lineCounter++ * fontSize * fontScale ), drawInfo );
	// 	}
	// 	
	// 	Line( $"UWorld::GameState = 0x{_gameState:X8}" );
	// 	Line( $"UWorld::GameState::PlayerArray = 0x{_gameStatePlayerArray:X8}" );
	// 	Line( $"UWorld::GameState::PlayerArray len = {_gameStatePlayerArraySize}" );
	// 	Line( $"ActorArray = 0x{_actorArray:X8}" );
	// 	Line( $"ActorArraySize = {_actorArraySize}" );
	// }
	
}
