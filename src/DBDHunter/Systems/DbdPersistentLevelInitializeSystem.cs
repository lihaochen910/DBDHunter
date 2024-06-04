using System.Collections;
using System.Linq;
using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.StateMachines;
using Bang.Systems;
using DBDHunter.Assets;
using DBDHunter.Components;
using DBDHunter.Messages;
using DBDHunter.Services;
using DBDHunter.Utilities;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Extensions.Logging;
using Murder.Services;


namespace DBDHunter.Systems;

public record GameStatePlayerData {
	public ulong Actor;
	public ulong RootComponent;
	public ulong PlayerState;
	public ulong ActorMesh;
	public ulong SkeletalMesh;
	public EPlayerRole PlayerRole;
}

[Messager( typeof( DbdMatchStartMessage ), typeof( DbdMatchEndMessage ) )]
[Filter( typeof( DbdGWorldComponent ), typeof( DbdGNamesComponent ) )]
public class DbdPersistentLevelInitializeSystem : IStartupSystem, IExitSystem, IMessagerSystem {
	
	private readonly Dictionary< ulong, GameStatePlayerData > _slasherList = new ( 1 );
	private readonly Dictionary< ulong, GameStatePlayerData > _camperList = new ( 5 );
	private readonly List< ulong > _generatorList = new ( 7 );
	private readonly List< ulong > _hatchList = new ( 1 );
	private readonly List< ulong > _totemList = new ( 5 );
	private readonly List< ulong > _searchablesList = new ( 8 );
	private readonly List< ulong > _palletsList = new ( 30 );

	private Entity _coroutineEntity;
	
	public void Start( Context context ) {
		if ( context.World.TryGetUniqueEntity< DbdPersistentLevelComponent >() is null ) {
			var persistentLevelEntity = LibraryServices.GetLibrary().SpawnPrefab( nameof( LibraryAsset.PersistentLevel ), context.World );
			persistentLevelEntity.SetDbdPersistentLevel( 0ul );
		}

		_coroutineEntity = context.World.AddEntity();
		_coroutineEntity.RunCoroutine( DelayFirstMatchStart( context.World ) );
	}
	
	public void Exit( Context context ) {
		// _handle.Close();
	}

	public void OnMessage( World world, Entity entity, IMessage message ) {
		Logger.Debug( $"OnMessage: {message}" );
		
		var gWorldAddr = entity.GetDbdGWorld().Addr;
		
		world.GetUniqueEntity< DbdPersistentLevelComponent >().SetDbdPersistentLevel(
			Memory.Read< ulong >( gWorldAddr + Offsets.UWorld_PersistentLevel )
		);

		if ( message is DbdMatchStartMessage ) {
			if ( entity.HasDbdBuiltLevelData() ) {
				var dbdBuiltLevelDataComponent = entity.GetDbdBuiltLevelData();
				Logger.Debug( $"Theme: {UEHelper.GetFNameByComparisonIndex( dbdBuiltLevelDataComponent.ThemeName.ComparisonIndex )}" );
				Logger.Debug( $"Name: {dbdBuiltLevelDataComponent.MapName} {Memory.ReadFString( dbdBuiltLevelDataComponent.MapName )}" );
			}
			
			_coroutineEntity.RunCoroutine( LoopTravelPersistentLevel( world, entity ) );
		}

		if ( message is DbdMatchEndMessage ) {
			_coroutineEntity.RemoveStateMachine();
			DestroyAllDbdActors( world );
		}
	}

	private void FilterPersistentLevelActors( World world, Entity _ ) {
		var persistentLevel = world.GetUnique< DbdPersistentLevelComponent >().Addr;
		if ( persistentLevel is 0 ) {
			return;
		}

		var gNamesComponent = world.TryGetUnique< DbdGNamesComponent >();
		
		var owningActor = Memory.Read< ulong >( persistentLevel + Offsets.UNetConnection_OwningActor );
		var maxPacket = Memory.Read< uint >( persistentLevel + Offsets.UNetConnection_MaxPacket );
		if ( maxPacket is > 2000 or <= 0 ) {
			if ( world.TryGetUniqueEntity< DbdPersistentLevelComponent >() is {} persistentLevelEntity ) {
				persistentLevelEntity.SetDbdPersistentLevel( 0ul );
			}
			return;
		}
		
		var entityList = Memory.ReadTArray< ulong >( owningActor, ( int )maxPacket );
		if ( entityList is null || entityList.Length > 1_0000 ) {
			if ( world.TryGetUniqueEntity< DbdPersistentLevelComponent >() is {} persistentLevelEntity ) {
				persistentLevelEntity.SetDbdPersistentLevel( 0ul );
			}
			return;
		}

		var dbdPlayerList = new List< ulong >( 6 );
		
		var validEntityMap = new bool[ entityList.Length ];
		bool EntityValid( int index ) => validEntityMap[ index ];
		void SetEntityValid( int index, bool valid ) => validEntityMap[ index ] = valid;
		
		Array.Fill( validEntityMap, true );
		
		var firstPassReadHandle = Driver.CreateScatterHandle();
		var secondPassReadHandle = Driver.CreateScatterHandle();
		
		// Phase 1: filter non-null Actor
		for ( var i = 0; i < entityList.Length; i++ ) {
			var actor = entityList[ i ];
			if ( actor is 0 ) {
				SetEntityValid( i, false );
				continue;
			}
			
			firstPassReadHandle.Prepare< ulong >( actor + Offsets.APawn_PlayerState );
			firstPassReadHandle.Prepare< ulong >( actor + Offsets.APawn_AcknowledgedPawn );
			firstPassReadHandle.Prepare< ulong >( actor + Offsets.AActor_RootComponent );
			firstPassReadHandle.Prepare< ulong >( actor + Offsets.ACharacter_Mesh );
			// firstPassReadHandle.Prepare< int >( actor + Offsets.AActor_ActorID );
			firstPassReadHandle.Prepare< ulong >( actor + Offsets.UObject_Class );
			// firstPassReadHandle.Prepare< FName >( actor + Offsets.UObject_Name );
		}

		firstPassReadHandle.Execute();
		
		// Phase 2: filter Actor With GameRole
		for ( var i = 0; i < entityList.Length; i++ ) {
			if ( !EntityValid( i ) ) {
				continue;
			}
			
			var actor = entityList[ i ];
			// var rootComponent = firstPassReadHandle.Read< ulong >( actor + Offsets.AActor_RootComponent );
			// if ( rootComponent is 0 ) {
			// 	SetEntityValid( i, false );
			// 	continue;
			// }
			
			if ( gNamesComponent != null ) {
				
				var actorClass = firstPassReadHandle.Read< ulong >( actor + Offsets.UObject_Class );
				// var actorName = Memory.Read< FName >( actor + Offsets.UObject_Name, entityListReadHandle );

				if ( actor.IsA( "DBDPlayer", actorClass ) ) {
					dbdPlayerList.Add( actor );
				}
			
				if ( actor.IsA( Offsets.CLASS_Generator, actorClass ) ) {
					// Logger.Debug( $"found: {actor.GetUClassName()}" );
					_generatorList.Add( actor );
				}
			
				if ( actor.IsA( Offsets.CLASS_Hatch, actorClass ) ) {
					// Logger.Debug( $"found: {actor.GetUClassName()}" );
					_hatchList.Add( actor );
				}
			
				if ( actor.IsA( Offsets.CLASS_Totem, actorClass ) ) {
					_totemList.Add( actor );
				}
			
				if ( actor.IsA( Offsets.CLASS_Chest, actorClass ) ) {
					_searchablesList.Add( actor );
				}
			
				if ( actor.IsA( Offsets.CLASS_Pallet, actorClass ) ) {
					_palletsList.Add( actor );
				}
				
				if ( actor.IsA( Offsets.CLASS_Slasher, actorClass ) ||
					 actor.IsA( Offsets.CLASS_Camper, actorClass ) ||
					 actor.IsA( Offsets.CLASS_DBDMenuPlayer, actorClass ) ) {
					// Logger.Debug( $"found Player: {actor.GetUClassName()}" );
					var playerState = firstPassReadHandle.Read< ulong >( actor + Offsets.APawn_PlayerState );
					secondPassReadHandle.Prepare< byte >( playerState + Offsets.ADBDPlayerState_GameRole );
				}
				else {
					SetEntityValid( i, false );
				}
			}
			else {
				var playerState = firstPassReadHandle.Read< ulong >( actor + Offsets.APawn_PlayerState );
				var acknowledgedPawn = firstPassReadHandle.Read< ulong >( actor + Offsets.APawn_AcknowledgedPawn );
				if ( acknowledgedPawn == 0 && playerState != 0 ) { // players aren't pawns
					secondPassReadHandle.Prepare< byte >( playerState + Offsets.ADBDPlayerState_GameRole );
				}
				else {
					SetEntityValid( i, false );
				}
			}
			
			var meshComponent = firstPassReadHandle.Read< ulong >( actor + Offsets.ACharacter_Mesh );
			if ( meshComponent != 0 ) {
				secondPassReadHandle.Prepare< ulong >( meshComponent + Offsets.USkinnedMeshComponent_SkeletalMesh );
			}
		}

		secondPassReadHandle.Execute();

		// Phase 3: filter surviors and killers, and crate entity
		for ( var i = 0; i < entityList.Length; i++ ) {
			if ( !EntityValid( i ) ) {
				continue;
			}
			
			var actor = entityList[ i ];
			var rootComponent = firstPassReadHandle.Read< ulong >( actor + Offsets.AActor_RootComponent );
			var meshComponent = firstPassReadHandle.Read< ulong >( actor + Offsets.ACharacter_Mesh );
			var skeletalMesh = secondPassReadHandle.Read< ulong >( meshComponent + Offsets.USkinnedMeshComponent_SkeletalMesh );
			var playerState = firstPassReadHandle.Read< ulong >( actor + Offsets.APawn_PlayerState );
			
			var prePlayerData = new GameStatePlayerData {
				Actor = actor,
				RootComponent = rootComponent,
				ActorMesh = meshComponent,
				SkeletalMesh = skeletalMesh,
				PlayerRole = EPlayerRole.VE_None,
				PlayerState = playerState
			};

			if ( gNamesComponent.HasValue ) {
				var actorClass = firstPassReadHandle.Read< ulong >( actor + Offsets.UObject_Class );

				if ( actor.IsA( Offsets.CLASS_Slasher, actorClass ) ) {
					prePlayerData.PlayerRole = EPlayerRole.VE_Slasher;
					_slasherList.Add( actor, prePlayerData );
				}
				else if ( actor.IsA( Offsets.CLASS_Camper, actorClass ) ) {
					prePlayerData.PlayerRole = EPlayerRole.VE_Camper;
					_camperList.Add( actor, prePlayerData );
				}
				else if ( actor.IsA( Offsets.CLASS_DBDMenuPlayer, actorClass ) ) {
					prePlayerData.PlayerRole = EPlayerRole.VE_Camper;
					_camperList.Add( actor, prePlayerData );
				}
			}
			else {
				var playerRole = ( EPlayerRole )secondPassReadHandle.Read< byte >( playerState + Offsets.ADBDPlayerState_GameRole );
				if ( playerRole != EPlayerRole.VE_Camper && playerRole != EPlayerRole.VE_Slasher ) {
					SetEntityValid( i, false );
					continue;
				}
				
				prePlayerData.PlayerRole = playerRole;
				if ( playerRole is EPlayerRole.VE_Slasher ) {
					_slasherList.Add( actor, prePlayerData );
				}
				if ( playerRole is EPlayerRole.VE_Camper ) {
					_camperList.Add( actor, prePlayerData );
				}
			}
		}
		
		firstPassReadHandle.SafeClose();
		 
		secondPassReadHandle.SafeClose();
		 
		world.GetUniqueEntity< DbdPersistentLevelComponent >().SetDbdPersistentLevel( persistentLevel );

		if ( world.TryGetUniqueEntity< DbdGameStateComponent >() is {} gameStateEntity ) {
			gameStateEntity.SetDbdGameState( gameStateEntity.GetDbdGameState().SetPlayerDatas( [.._slasherList.Values.Concat( _camperList.Values )] ) );
			gameStateEntity.SetDbdGameState( gameStateEntity.GetDbdGameState().SetGenerators( [.._generatorList] ) );
			gameStateEntity.SetDbdGameState( gameStateEntity.GetDbdGameState().SetHatches( [.._hatchList] ) );
			gameStateEntity.SetDbdGameState( gameStateEntity.GetDbdGameState().SetTotems( [.._totemList] ) );
			gameStateEntity.SetDbdGameState( gameStateEntity.GetDbdGameState().SetSearchables( [.._searchablesList] ) );
			gameStateEntity.SetDbdGameState( gameStateEntity.GetDbdGameState().SetPallets( [.._palletsList] ) );
			
			AfterPersistentLevelInitialized( world, gameStateEntity );
		}
		else {
			Logger.Error( "no DbdGameState found!" );
		}
	}

	
	private void DestroyAllDbdActors( World world ) {
		foreach ( var entity in world.GetEntitiesWith( ContextAccessorFilter.AnyOf, typeof( DbdActorComponent ) ) ) {
			entity.Destroy();
		}
	}

	private void AfterPersistentLevelInitialized( World world, Entity gameStateEntity ) {
		var dbdGameStateComponent = gameStateEntity.GetDbdGameState();
		
		var handle = Driver.CreateScatterHandle();
		
		foreach ( var playerData in _slasherList.Values.Concat( _camperList.Values ) ) {
			var playerEntity = LibraryServices.GetLibrary().SpawnPrefab( nameof( LibraryAsset.PlayerActor ), world );
			playerEntity.SetDbdActor( playerData.Actor, playerData.RootComponent, default, default, default );
			playerEntity.SetDbdActorBoxShape( new BoxShape( new Vector3F( 80f, 30f, 10f ) ), Vector3F.Zero, 0f );
			playerEntity.SetDbdPlayerRole( playerData.PlayerRole );
			
			if ( playerData.ActorMesh != 0 ) {
				playerEntity.SetDbdActorMesh(
					playerData.Actor,
					playerData.ActorMesh,
					FTransform.Identity,
					playerData.SkeletalMesh,
					0ul, 0 );
			}
		}

		foreach ( var generator in dbdGameStateComponent.Generators ) {
			if ( generator is 0 ) {
				continue;
			}
			
			handle.Prepare< ulong >( generator + Offsets.AActor_RootComponent );
			handle.Prepare< bool >( generator + Offsets.AGenerator_Activated );
			handle.Prepare< float >( generator + Offsets.AGenerator_NativePercentComplete );
		}
		
		foreach ( var hatch in dbdGameStateComponent.Hatches ) {
			if ( hatch is 0 ) {
				continue;
			}
			
			handle.Prepare< ulong >( hatch + Offsets.AActor_RootComponent );
			handle.Prepare< byte >( hatch + Offsets.AHatch_HatchState );
		}
		
		foreach ( var totem in dbdGameStateComponent.Totems ) {
			if ( totem is 0 ) {
				continue;
			}
			
			handle.Prepare< ulong >( totem + Offsets.AActor_RootComponent );
			handle.Prepare< byte >( totem + Offsets.ATotem_TotemState );
		}
		
		foreach ( var searchable in dbdGameStateComponent.Searchables ) {
			if ( searchable is 0 ) {
				continue;
			}
			
			handle.Prepare< ulong >( searchable + Offsets.AActor_RootComponent );
			handle.Prepare< ulong >( searchable + Offsets.ASearchable_SpawnedItem );
		}
		
		foreach ( var pallet in dbdGameStateComponent.Pallets ) {
			if ( pallet is 0 ) {
				continue;
			}
			
			handle.Prepare< ulong >( pallet + Offsets.AActor_RootComponent );
			handle.Prepare< ulong >( pallet + Offsets.APallet_State );
		}

		handle.Execute();
		
		foreach ( var generator in dbdGameStateComponent.Generators ) {
			if ( generator is 0 ) {
				continue;
			}
			
			var rootComponent = handle.Read< ulong >( generator + Offsets.AActor_RootComponent );
			
			var generatorEntity = LibraryServices.GetLibrary().SpawnPrefab( nameof( LibraryAsset.Generator ), world );
			generatorEntity.SetDbdActor( generator, rootComponent, default, Vector3D.Zero, Vector3D.One );
			generatorEntity.SetDbdGenerator(
				generator,
				handle.Read< bool >( generator + Offsets.AGenerator_Activated ),
				handle.Read< float >( generator + Offsets.AGenerator_NativePercentComplete )
			);
		}
		
		foreach ( var hatch in dbdGameStateComponent.Hatches ) {
			if ( hatch is 0 ) {
				continue;
			}
			
			var rootComponent = handle.Read< ulong >( hatch + Offsets.AActor_RootComponent );
			
			var hatchEntity = LibraryServices.GetLibrary().SpawnPrefab( nameof( LibraryAsset.Hatch ), world );
			hatchEntity.SetDbdActor( hatch, rootComponent, default, Vector3D.Zero, Vector3D.One );
			hatchEntity.SetDbdHatch(
				hatch,
				( EHatchState )handle.Read< byte >( hatch + Offsets.AHatch_HatchState )
			);
		}
		
		foreach ( var totem in dbdGameStateComponent.Totems ) {
			if ( totem is 0 ) {
				continue;
			}
			
			var rootComponent = handle.Read< ulong >( totem + Offsets.AActor_RootComponent );
			
			var totemEntity = LibraryServices.GetLibrary().SpawnPrefab( nameof( LibraryAsset.Totem ), world );
			totemEntity.SetDbdActor( totem, rootComponent, default, Vector3D.Zero, Vector3D.One );
			totemEntity.SetDbdTotem(
				totem,
				( ETotemState )handle.Read< byte >( totem + Offsets.ATotem_TotemState )
			);
		}
		
		foreach ( var searchable in dbdGameStateComponent.Searchables ) {
			if ( searchable is 0 ) {
				continue;
			}
			
			var rootComponent = handle.Read< ulong >( searchable + Offsets.AActor_RootComponent );
			
			var searchableEntity = LibraryServices.GetLibrary().SpawnPrefab( nameof( LibraryAsset.Searchable ), world );
			searchableEntity.SetDbdActor( searchable, rootComponent, default, Vector3D.Zero, Vector3D.One );
			searchableEntity.SetDbdSearchable(
				searchable,
				handle.Read< ulong >( searchable + Offsets.ASearchable_SpawnedItem )
			);
		}
        
		foreach ( var pallet in dbdGameStateComponent.Pallets ) {
			if ( pallet is 0 ) {
				continue;
			}
			
			var rootComponent = handle.Read< ulong >( pallet + Offsets.AActor_RootComponent );
			
			var palletEntity = LibraryServices.GetLibrary().SpawnPrefab( nameof( LibraryAsset.Pallet ), world );
			palletEntity.SetDbdActor( pallet, rootComponent, default, Vector3D.Zero, Vector3D.One );
			palletEntity.SetDbdPallet(
				pallet,
				( EPalletState )handle.Read< byte >( pallet + Offsets.APallet_State )
			);
		}
		
		handle.SafeClose();
	}

	private IEnumerator< Wait > DelayFirstMatchStart( World world ) {
		yield return Wait.ForSeconds( 2f );
		
		if ( world.TryGetUniqueEntity< DbdGWorldComponent >() is {} entity ) {
			entity.SendMessage< DbdMatchStartMessage >();
		}
	}

	private const int TargetGeneratorCount = 7;
	private const int TargetHatchCount = 1;
	private const int TargetTotemCount = 5;
	
	private IEnumerator< Wait > LoopTravelPersistentLevel( World world, Entity entity ) {
		var ok = false;

		while ( !ok ) {
			if ( world.TryGetUniqueEntity< DbdGameStateComponent >() is null ) {
				yield return Wait.ForSeconds( 1f );
				continue;
			}
				
			FilterPersistentLevelActors( world, entity );

			if ( ( _totemList.Count >= TargetTotemCount && _hatchList.Count >= TargetHatchCount ) ||
				 ( _slasherList.Count > 0 && _camperList.Count > 0 ) ) {
				ok = true;
			}

			// Logger.Debug( $"Slasher count: {_slasherList.Count}" );
			if ( _slasherList.Count < 1 ) {
				ok = false;
			}
			
			// Logger.Debug( $"Camper count: {_camperList.Count}" );
			if ( _camperList.Count < 1 ) {
				ok = false;
			}
			
			_slasherList.Clear();
			_camperList.Clear();
			_generatorList.Clear();	
			_hatchList.Clear();
			_totemList.Clear();
			_searchablesList.Clear();
			_palletsList.Clear();

			if ( !ok ) {
				DestroyAllDbdActors( world );
			}
			
			yield return Wait.ForSeconds( 1f );
		}
		
		Logger.Debug( "finished LoopTravelPersistentLevel()." );
		yield return Wait.Stop;
	}
	
}
