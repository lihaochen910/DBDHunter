using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;
using DBDHunter.Messages;
using DBDHunter.Utilities;
using ImGuiNET;
using Murder.Core.Graphics;
using Murder.Editor.Attributes;
using Murder.Editor.Systems;
using vmmsharp;


namespace DBDHunter.Systems; 

[OnlyShowOnDebugView]
[Filter(ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof(DbdGWorldComponent))]
public class DbdDebugSystem : IGuiSystem {

	public record struct FPlayerStateData {
		public string Name;
		public EPlayerRole PlayerRole;
		public int SelectedCamperIndex;
		public int SelectedSlasherIndex;
		public string EquipedFavorId;
		public string[] EquipedPerkIds;
		public int PrestigeLevel;
	}
	
	public record struct FCharacterStateData {
		public string[] AddonIds;
	}
	
	private bool _show;
	private FPlayerStateData[] _playerStateDatas = Array.Empty< FPlayerStateData >();
	private FCharacterStateData[] _characterStateDatas = Array.Empty< FCharacterStateData >();
	
	private int _gObjectSize = -1;
	private int _gObjectChunkSize = -1;
	private ulong _gameState;
	private ulong _gameStatePlayerArray;
	private uint _gameStatePlayerArraySize;
	private ulong[] _playerStateList;
	private ulong _actorArray;
	private int _actorArraySize;

	private string _mapName = string.Empty;
	private bool _showActorMeshDebug = false;

	public void DrawGui( RenderContext render, Context context ) {
		ImGui.SetNextWindowBgAlpha(0.9f);
		ImGui.SetNextWindowSizeConstraints(
			new Vector2(300, 300),
			new Vector2(EditorSystem.WINDOW_MAX_WIDTH, EditorSystem.WINDOW_MAX_HEIGHT)
		);

		if ( ImGui.Begin( "Dbd", ref _show ) ) {
			
			ImGui.SeparatorText( "Engine" );

			var dbdGNamesComponent = context.World.TryGetUnique< DbdGNamesComponent >();
			var dbdGObjectsComponent = context.World.TryGetUnique< DbdGObjectsComponent >();
			
			if ( dbdGNamesComponent.HasValue ) {
				ImGui.Text( $"GNames: 0x{dbdGNamesComponent.Value.Addr:X8}" );
			}
			
			if ( dbdGObjectsComponent.HasValue ) {
				ImGui.Text( $"GObjects: 0x{dbdGObjectsComponent.Value.Addr:X8}" );
				ImGui.Text( $"GObjectsSize: {_gObjectSize}" );
				ImGui.Text( $"GObjectsChunkSize: {_gObjectChunkSize}" );

				if ( ImGui.Button( "Travel GObjects" ) ) {
					_gObjectSize = Memory.Read< int >( dbdGObjectsComponent.Value.Addr + Offsets.TUObjectArray_NumElements );
					_gObjectChunkSize = Memory.Read< int >( dbdGObjectsComponent.Value.Addr + Offsets.TUObjectArray_NumChunks );

					Logger.Verify( _gObjectChunkSize < _gObjectSize );

					// for ( var chunkIndex = 0; chunkIndex < _gObjectChunkSize; chunkIndex++ ) {
					// 	var chunk = Memory.Read< ulong >( dbdGObjectsComponent.Value.Addr + ( ulong )chunkIndex * 0x8 );
					// 	// var uObjectItems = Memory.ReadTArray< FUObjectItem >( Memory.Read< ulong >( chunk ), Offsets.TUObjectArray_ElementsPerChunk );
					// 	var uObjectItems = Memory.ReadTArray< FUObjectItem >( chunk, Offsets.TUObjectArray_ElementsPerChunk );
					// 	if ( uObjectItems is not null ) {
					// 		for ( var i = 0; i < uObjectItems.Length; i++ ) {
					// 			if ( uObjectItems[ i ].Object > 0 ) {
					// 				var uObject = Memory.Read< ulong >( Memory.Read< ulong >( uObjectItems[ i ].Object ) );
					// 				if ( uObject > 0 ) {
					// 					var classPtr = Memory.Read< ulong >( uObject + Offsets.UObject_Class );
					// 					if ( classPtr > 0 ) {
					// 						var classFName = Memory.Read< FName >( classPtr + Offsets.UObject_Name );
					// 						Logger.Debug( $"obj[{chunkIndex}][{i}] class: { Offsets.GetFNameByComparisonIndex( classFName.ComparisonIndex, dbdGNamesComponent.Value.Addr ) }" );
					// 					}
					// 				}
					// 			}
					// 		}
					// 	}
					// }

					(int, int) DecryptGObjectsIndex( int index ) {
						var ChunkIndex = index / Offsets.TUObjectArray_ElementsPerChunk;
						var InChunkIdx = index % Offsets.TUObjectArray_ElementsPerChunk;

						if (ChunkIndex >= _gObjectChunkSize || index >= _gObjectSize)
							return ( -1, -1 );

						return ( ChunkIndex, InChunkIdx );
					}

					for ( var i = 0; i < _gObjectSize; i++ ) {
					// for ( var i = 0; i < 0xFFFF; i++ ) {
						var ( chunkIndex, inChunkIdx ) = DecryptGObjectsIndex( i );
						var chunk = Memory.Read< ulong >( dbdGObjectsComponent.Value.Addr + ( ulong )chunkIndex * 0x8 );
						if ( chunk is 0 ) {
							continue;
						}
						Logger.Verify( chunk > 0 );
						// var uObjectItem = Memory.Read< FUObjectItem >( Memory.Read< ulong >( chunk ) + ( ulong )inChunkIdx * ( ulong )Marshal.SizeOf< FUObjectItem >() );
						// var uObjectItem = Memory.Read< FUObjectItem >( Memory.Read< ulong >( chunk + ( ulong )inChunkIdx * ( ulong )Marshal.SizeOf< FUObjectItem >() ) );
						var uObjectItem = Memory.Read< FUObjectItem >( chunk + ( ulong )inChunkIdx * ( ulong )Marshal.SizeOf< FUObjectItem >() );
						if ( uObjectItem.Object > 0 ) {
							var uObject = Memory.Read< ulong >( uObjectItem.Object );
							if ( uObject > 0 ) {
								var classPtr = Memory.Read< ulong >( uObject + Offsets.UObject_Class );
								if ( classPtr > 0 ) {
									var classFName = Memory.Read< FName >( classPtr + Offsets.UObject_Name );
									var className = UEHelper.GetFNameByComparisonIndex( classFName.ComparisonIndex );
									// if ( classFName.ComparisonIndex >= 0 /* && !string.IsNullOrEmpty( className ) && className != "None"*/ ) {
									// 	Logger.Debug( $"obj[{chunkIndex}][{inChunkIdx}] class: {className}" );
									// }
								}
							}
						}
					}
					
					Logger.Debug( "finished." );
				}
			}
			
			if ( context.World.TryGetUniqueEntity< DbdPersistentLevelComponent >() is {} persistentLevelEntity ) {
				var persistentLevel = persistentLevelEntity.GetDbdPersistentLevel().Addr;
				ImGui.PushID( ( int )persistentLevel );
				if ( ImGui.Button( "Travel PersistentLevel" ) ) {
					var owningActor = Memory.Read< ulong >( persistentLevel + Offsets.UNetConnection_OwningActor );
					_actorArray = owningActor;
					var maxPacket = Memory.Read< uint >( persistentLevel + Offsets.UNetConnection_MaxPacket );
					_actorArraySize = ( int )maxPacket;
					if ( maxPacket is > 2000 or <= 0 ) {
						return;
					}
		
					ulong[] entityList = Memory.ReadTArray< ulong >( owningActor, ( int )maxPacket );
					if ( entityList is null ) {
						return;
					}
		
					var entityListReadHandle = Driver.CreateScatterHandle();
					var entityClassNameReadHandle = Driver.CreateScatterHandle();

					for ( var i = 0; i < entityList.Length; i++ ) {
						var actor = entityList[ i ];
						if ( actor <= 0 ) {
							continue;
						}
				
						entityListReadHandle.Prepare< ulong >( actor + Offsets.UObject_Class );
					}

					entityListReadHandle.Execute();
					
					for ( var i = 0; i < entityList.Length; i++ ) {
						var actor = entityList[ i ];
						if ( actor <= 0 ) {
							continue;
						}

						var classPtr = entityListReadHandle.Read< ulong >( actor + Offsets.UObject_Class );
						entityClassNameReadHandle.Prepare< FName >( classPtr + Offsets.UObject_Name );
					}

					entityClassNameReadHandle.Execute();
					
					for ( var i = 0; i < entityList.Length; i++ ) {
						var actor = entityList[ i ];
						if ( actor <= 0 ) {
							continue;
						}
				
						var actorClass =  entityListReadHandle.Read< ulong >( actor + Offsets.UObject_Class );
						UEHelper.AffirmUClassCache( actor, actorClass );
						// var classFName = Memory.ByteArrayToStructure< FName >(
						// 	entityClassNameReadHandle.Read( classPtr + Offsets.UObject_Name, ( uint )Marshal.SizeOf< FName >() ) );
						// Logger.Debug( $"actor[{i}] class: { Offsets.GetFNameByComparisonIndex( classFName.ComparisonIndex, dbdGNamesComponent.Value.Addr ) }" );
						Logger.Debug( $"actor[{i}] class: {UEHelper.GetActorClassDebugStr( actor, dbdGNamesComponent.Value.Addr ) }" );
						// Logger.Debug( $"actor[{i}] cache: {UEHelper.GetCachedUClassNode( actorClass ).GetInheritanceTreeDebugString()}" );
					}
					
					entityClassNameReadHandle.SafeClose();
					entityListReadHandle.SafeClose();
				}
				ImGui.PopID();

				if ( ImGui.Button( "send DbdMatchStartMessage" ) ) {
					persistentLevelEntity.SendMessage< DbdMatchStartMessage >();
				}
			}

			ImGui.Checkbox( "ShowActorMesh", ref _showActorMeshDebug );
			if ( ImGui.CollapsingHeader( "ActorMesh", ref _showActorMeshDebug ) ) {
				var actorsWithMesh = context.World.GetEntitiesWith( typeof( DbdActorMeshRefSkeletonComponent ) );
				foreach ( var entity in actorsWithMesh ) {
					var rawRefBoneInfo = entity.GetDbdActorMeshRefSkeleton().RawRefBoneInfo;
					if ( rawRefBoneInfo.IsDefaultOrEmpty ) {
						continue;
					}
					
					ImGui.Text( $"RawRefBoneInfo size: {rawRefBoneInfo.Length}" );
					ImGui.PushID( entity.EntityId );	
					if ( ImGui.Button( "Print" ) ) {
						foreach ( var meshBoneInfo in rawRefBoneInfo ) {
							Logger.Debug( $"{meshBoneInfo.Name}" );
						}
					}
					ImGui.PopID();
				}
			}
			
			ImGui.SeparatorText( "PlayerState" );

			var playerStateEntities = context.World.GetEntitiesWith( typeof( DbdPlayerAndCharacterStateDataComponent ) );
			foreach ( var playerStateEntity in playerStateEntities ) {
				var playerState = playerStateEntity.GetDbdPlayerAndCharacterStateData();
				var playerStateAddr = playerState.PlayerStateAddr;
				ImGui.Text( $"player: {playerState.PlayerRole} P{playerState.PrestigeLevel:000} 0x{playerStateAddr:x8}" );
				if ( playerStateAddr != 0 ) {
					if ( ImGui.Button( "Set Role: Slasher" ) ) {
						Memory.Write( playerStateAddr + Offsets.ADBDPlayerState_GameRole, ( byte )EPlayerRole.VE_Slasher );
					}
					if ( ImGui.Button( "Set Role: Camper" ) ) {
						Memory.Write( playerStateAddr + Offsets.ADBDPlayerState_GameRole, ( byte )EPlayerRole.VE_Camper );
					}
				}
			}

			if ( context.World.TryGetUnique< DbdBuiltLevelDataComponent >() is {} dbdBuiltLevelDataComponent ) {
				ImGui.SeparatorText( "BuiltLevelData" );
				
				// ImGui.Text( $"ThemeName: {DBDThemes.GetThemeName( Offsets.GetFNameByComparisonIndex( dbdBuiltLevelDataComponent.ThemeName.ComparisonIndex, dbdGNamesComponent.Value.Addr ) ) }" );
				ImGui.Text( $"ThemeName: {UEHelper.GetFNameByComparisonIndex( dbdBuiltLevelDataComponent.ThemeName.ComparisonIndex ) }" );
				ImGui.Text( $"ThemeNameIdx: {dbdBuiltLevelDataComponent.ThemeName.ComparisonIndex}" );
				ImGui.Text( $"MapName: {_mapName}" );
				if ( ImGui.Button( "decode MapName" ) ) {
					if ( dbdBuiltLevelDataComponent.MapName != 0 ) {
						_mapName = Memory.ReadFString( dbdBuiltLevelDataComponent.MapName );
					}
                }
			}
			
			ImGui.SeparatorText( "Generator" );
			
			var generators = context.World.GetEntitiesWith( typeof( DbdGeneratorComponent ) );
			foreach ( var generator in generators ) {
				var generatorComponent = generator.GetDbdGenerator();
				ImGui.Text( $"generator: 0x{generatorComponent.Addr:x8} {generatorComponent.NativePercentComplete:0.00}" );
				if ( generatorComponent.Addr != 0 ) {
					ImGui.Indent( 8 );
					if ( ImGui.Button( "Set Percent: 0" ) ) {
						Memory.Write( generatorComponent.Addr + Offsets.AGenerator_NativePercentComplete, 0f );
					}
					
					if ( ImGui.Button( "Set Percent: 0.98" ) ) {
						Memory.Write( generatorComponent.Addr + Offsets.AGenerator_NativePercentComplete, 0.98f );
					}
					ImGui.Unindent( 8 );
				}
			}
			
			ImGui.Separator();

			ImGui.SeparatorText( "Pallet" );
			
			var pallets = context.World.GetEntitiesWith( typeof( DbdPalletComponent ) );
			if ( !pallets.IsDefaultOrEmpty ) {
				foreach ( var pallet in pallets ) {
					var dbdPalletComponent = pallet.GetDbdPallet();
					ImGui.Text( $"pallet: 0x{dbdPalletComponent.Addr:x8}" );
					if ( dbdPalletComponent.Addr != 0 ) {
						ImGui.Indent( 8 );
						if ( ImGui.Button( "Set Pallet: Up" ) ) {
							Memory.Write( dbdPalletComponent.Addr + Offsets.APallet_State, ( byte )EPalletState.Up );
						}
						ImGui.SameLine();
						if ( ImGui.Button( "Set Pallet: Fallen" ) ) {
							Memory.Write( dbdPalletComponent.Addr + Offsets.APallet_State, ( byte )EPalletState.Fallen );
						}
						ImGui.Unindent( 8 );
					}
				}
			}
			
			ImGui.Separator();

			// if ( _playerStateList != null ) {
			// 	
			// 	ImGui.Text( $"PlayerStateList len:{_playerStateList.Length}" );
			// 	
			// 	if ( ImGui.Button( "read" ) ) {
			// 		var playerStateDatas = new List< FPlayerStateData >();
			// 		var characterStateDatas = new List< FCharacterStateData >();
			// 		
			// 		for ( var i = 0; i < _playerStateList.Length; i++ ) {
			// 			var playerState = _playerStateList[ i ];
			// 			if ( playerState > 0 ) {
			// 				var name = Memory.ReadFString( playerState + Offsets.ADBDPlayerState_PlayerNamePrivate );
			// 				
			// 				var role = ( EPlayerRole )Memory.Read< byte >( playerState + Offsets.ADBDPlayerState_GameRole );
			// 				var selectedCamperIndex = Memory.Read< int >( playerState + Offsets.ADBDPlayerState_SelectedCamperIndex );
			// 				var selectedSlasherIndex = Memory.Read< int >( playerState + Offsets.ADBDPlayerState_SelectedSlasherIndex );
			// 				
			// 				var equipedFavorId = Memory.Read< FName >( playerState + Offsets.ADBDPlayerState_PlayerData + Offsets.FPlayerStateData_EquipedFavorId );
			// 				var equipedFavorIdName = Offsets.GetFNameByComparisonIndex( equipedFavorId.ComparisonIndex, dbdGNamesComponent.Value.Addr );
			//
			// 				FName[] equipedPerkIds = Array.Empty< FName >();
			// 				var equipedPerkIdsLen = Memory.Read< int >( playerState + Offsets.ADBDPlayerState_PlayerData + Offsets.FPlayerStateData_EquipedPerkIds + 0x8 );
			// 				if ( equipedPerkIdsLen > 0 ) {
			// 					equipedPerkIds = Memory.ReadTArray< FName >(
			// 						Memory.Read< ulong >(playerState + Offsets.ADBDPlayerState_PlayerData + Offsets.FPlayerStateData_EquipedPerkIds),
			// 						equipedPerkIdsLen );
			// 				}
			// 				
			// 				var prestigeLevel = Memory.Read< int >( playerState + Offsets.ADBDPlayerState_PlayerData + Offsets.FPlayerStateData_PrestigeLevel );
			// 				var data = new FPlayerStateData {
			// 					Name = name,
			// 					PlayerRole = role,
			// 					SelectedCamperIndex = selectedCamperIndex,
			// 					SelectedSlasherIndex = selectedSlasherIndex,
			// 					EquipedFavorId = equipedFavorIdName,
			// 					PrestigeLevel = prestigeLevel
			// 				};
			// 				data.EquipedPerkIds = equipedPerkIds.Select( id =>
			// 					Offsets.GetFNameByComparisonIndex( id.ComparisonIndex, dbdGNamesComponent.Value.Addr )
			// 				).ToArray();
			// 				
			// 				playerStateDatas.Add( data );
			//
			// 				if ( role is EPlayerRole.VE_Slasher ) {
			// 					FName[] addonIds = Array.Empty< FName >();
			// 					var addonIdsLen = Memory.Read< int >( playerState + Offsets.ADBDPlayerState_SlasherData + Offsets.FCharacterStateDataInfo_AddonIds + 0x8 );
			// 					if ( addonIdsLen > 0 ) {
			// 						addonIds = Memory.ReadTArray< FName >(
			// 							Memory.Read< ulong >(playerState + Offsets.ADBDPlayerState_SlasherData + Offsets.FCharacterStateDataInfo_AddonIds),
			// 							addonIdsLen );
			// 					}
			// 					
			// 					var characterData = new FCharacterStateData();
			// 					characterData.AddonIds = addonIds.Select( id =>
			// 						Offsets.GetFNameByComparisonIndex( id.ComparisonIndex, dbdGNamesComponent.Value.Addr )
			// 					).ToArray();
			// 					
			// 					characterStateDatas.Add( characterData );
			// 				}
			// 			}
			// 		}
			//
			// 		_playerStateDatas = playerStateDatas.ToArray();
			// 		_characterStateDatas = characterStateDatas.ToArray();
			// 	}
			// 	
			// 	foreach ( var data in _playerStateDatas ) {
			// 		if ( data.EquipedPerkIds is null ) {
			// 			continue;
			// 		}
			// 		ImGui.Text( $"Name: {data.Name}" );
			// 		ImGui.Text( $"SelectedCamperIndex: {data.SelectedCamperIndex}" );
			// 		ImGui.Text( $"SelectedSlasherIndex: {data.SelectedSlasherIndex}" );
			// 		ImGui.Text( $"PrestigeLevel: {data.PrestigeLevel}" );
			// 		ImGui.Text( $"EquipedFavorId: {data.EquipedFavorId}" );
			// 		ImGui.Indent( 8 );
			// 		foreach ( var perk in data.EquipedPerkIds ) {
			// 			ImGui.Text( $"perk: {perk}" );
			// 		}
			// 		ImGui.Unindent( 8 );
			// 	}
			// 	
			// 	foreach ( var data in _characterStateDatas ) {
			// 		if ( data.AddonIds is null ) {
			// 			continue;
			// 		}
			// 		ImGui.Indent( 8 );
			// 		foreach ( var addon in data.AddonIds ) {
			// 			ImGui.Text( $"addon: {addon}" );
			// 		}
			// 		ImGui.Unindent( 8 );
			// 	}
			// }
			
			ImGui.End();
		}
	}
}
