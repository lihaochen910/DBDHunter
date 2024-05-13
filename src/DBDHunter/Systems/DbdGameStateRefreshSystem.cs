using System.Numerics;
using Bang;
using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;
using Murder;
using Murder.Core.Graphics;
using Murder.Services;


namespace DBDHunter.Systems; 

[Messager( typeof( RequestRefreshMessage ) )]
[Filter( typeof( DbdGWorldComponent ) )]
public class DbdGameStateRefreshSystem : IMessagerSystem, IMurderRenderSystem {

	public void OnMessage( World world, Entity entity, IMessage message ) {
		var gWorldAddr = entity.GetDbdGWorld().Addr;
		
		var gameState = Memory.Read< ulong >( gWorldAddr + Offsets.UWorld_GameState );
		if ( gameState != 0 ) {

			// var levelReadToPlay = Memory.Read< bool >( gameState + Offsets.ADBDGameState_LevelReadyToPlay );
			// if ( levelReadToPlay ) {
			// 	entity.SetDbdGameStateLevelReadyToPlay();
			// }
			// else {
			// 	entity.RemoveDbdGameStateLevelReadyToPlay();
			// }
			
			// entity.SetDbdGameStateLevelState(
			// 	Memory.Read< bool >( gameState + Offsets.ADBDGameState_GameLevelLoaded ),
			// 	Memory.Read< bool >( gameState + Offsets.ADBDGameState_GameLevelCreated ),
			// 	Memory.Read< bool >( gameState + Offsets.ADBDGameState_GameLevelEnded ),
			// 	( EEndGameReason )Memory.Read< byte >( gameState + Offsets.ADBDGameState_GameEndedReason )
			// );

			var builtLevelData = Memory.Read< FBuiltLevelData >( gameState + Offsets.ADBDGameState_BuiltLevelData );
			var mapName = Memory.Read< ulong >( gameState + Offsets.ADBDGameState_BuiltLevelData + Offsets.ADBDGameState_BuiltLevelData_MapName );
			entity.SetDbdBuiltLevelData( builtLevelData.ThemeName, builtLevelData.ThemeWeather, mapName );

			var playerStates = Memory.ReadUETArray< ulong >( gameState + Offsets.AGameStateBase_PlayerArray );
			if ( playerStates != null ) {
				if ( !entity.HasDbdGameState() ) {
					entity.SetDbdGameState(
						gameState,
						[],
						[..playerStates],
						[],
						[],
						[],
						[],
						[],
						[],
						[],
						[]
					);
				}
				else {
					entity.SetDbdGameState( entity.GetDbdGameState().SetPlayerStates( [..playerStates] ) );
				}
				
				return;
			}
			
			// var meatHooks = Memory.ReadUETArray< ulong >( gameState + Offsets.ADBDGameState_MeatHooks );
			// var searchables = Memory.ReadUETArray< ulong >( gameState + Offsets.ADBDGameState_Searchables );
			// var generators = Memory.ReadUETArray< ulong >( gameState + Offsets.ADBDGameState_Generators );
			// var escapeDoors = Memory.ReadUETArray< ulong >( gameState + Offsets.ADBDGameState_EscapeDoors );
			// var hatches = Memory.ReadUETArray< ulong >( gameState + Offsets.ADBDGameState_Hatches );
			// var pallets = Memory.ReadUETArray< ulong >( gameState + Offsets.ADBDGameState_Pallets );
			// var windows = Memory.ReadUETArray< ulong >( gameState + Offsets.ADBDGameState_Windows );
			// var totems = Memory.ReadUETArray< ulong >( gameState + Offsets.ADBDGameState_Totems );
			//
			// entity.SetDbdGameState(
			// 	gameState,
			// 	[..playerStates],
			// 	[..meatHooks],
			// 	[..searchables],
			// 	[..generators],
			// 	[..escapeDoors],
			// 	[..hatches],
			// 	[..pallets],
			// 	[..windows],
			// 	[..totems]
			// );
			//
			// return;
		}
		
		// TODO: clear
// ClearGameState:
		entity.RemoveDbdGameState();
	}

	
	public void Draw( RenderContext render, Context context ) {
		if ( context.World.TryGetUnique< DbdGameStateComponent >() is not {} dbdGameStateComponent ) {
			return;
		}
		
		const float fontSize = 12f;
		const float fontScale = 1f;
		var drawInfo = new DrawInfo( Color.Gray, 0.1f ) { Scale = Vector2.One * fontScale };
	
		var lineCounter = 1;
		void Line( string line ) {
			render.UiBatch.DrawText( 103, line, new Vector2( 2, 28f + lineCounter++ * fontSize * fontScale ), drawInfo );
		}
		
		// Line( $"GameState: 0x{dbdGameStateComponent.Addr:X8}" );

		// var readyToPlay = context.World.TryGetUnique< DbdGameStateLevelReadyToPlayComponent >() is not null;
		// Line( $"ReadyToPlay? {readyToPlay}" );
		
		Line( $"PlayerStates: {dbdGameStateComponent.PlayerStates.Length}" );
		Line( $"PlayerDatas: {dbdGameStateComponent.PlayerDatas.Length}" );
		Line( $"Searchables: {dbdGameStateComponent.Searchables.Length}" );
		// Line( $"Generators: {dbdGameStateComponent.Generators.Length}" );
		// Line( $"EscapeDoors: {dbdGameStateComponent.EscapeDoors.Length}" );
		// Line( $"Hatches: {dbdGameStateComponent.Hatches.Length}" );
		Line( $"Pallets: {dbdGameStateComponent.Pallets.Length}" );
		// Line( $"Windows: {dbdGameStateComponent.Windows.Length}" );
		// Line( $"Totems: {dbdGameStateComponent.Totems.Length}" );
	}
}
