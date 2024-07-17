using System.Linq;
using System.Runtime.InteropServices;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Assets;
using DBDHunter.Components;
using DBDHunter.Services;
using DBDHunter.Utilities;
using DigitalRune.Mathematics.Algebra;
using Murder;
using vmmsharp;


namespace DBDHunter.Systems; 

[Filter( ContextAccessorFilter.AllOf, typeof( DbdGNamesComponent ), typeof( DbdGameStateComponent ) )]
public class DbdPlayerStateRefreshSystem : IUpdateSystem {

	private const float RefreshTime = 1f;
	private float _timer;
	
	public void Update( Context context ) {
		if ( context.Entities.Length < 1 ) {
			return;
		}

		_timer -= Game.DeltaTime;
		if ( _timer < 0f ) {
			DestroyAllRelatedEntity( context );
			DoRefresh( context );
			_timer = RefreshTime;
		}
	}

	private void DoRefresh( Context context ) {
		if ( context.World.TryGetUniqueEntity< DbdGameStateComponent >() is null ) {
			return;
		}
		
		var dbdGameStateComponent = context.World.GetUnique< DbdGameStateComponent >();
		if ( dbdGameStateComponent.PlayerStates.Length < 1 ) {
			return;
		}
		
		var handle = Driver.CreateScatterHandle();
		for ( var i = 0; i < dbdGameStateComponent.PlayerStates.Length; i++ ) {
			var playerState = dbdGameStateComponent.PlayerStates[ i ];
			if ( playerState is 0 ) {
				continue;
			}
			
			handle.Prepare< int >( playerState + Offsets.ADBDPlayerState_PlayerNamePrivate + 0x8 );
			handle.Prepare< byte >( playerState + Offsets.ADBDPlayerState_GameRole );
			handle.Prepare< int >( playerState + Offsets.ADBDPlayerState_SelectedCamperIndex );
			handle.Prepare< int >( playerState + Offsets.ADBDPlayerState_SelectedSlasherIndex );
			handle.Prepare< FName >( playerState + Offsets.ADBDPlayerState_PlayerData + Offsets.FPlayerStateData_EquipedFavorId );
			handle.Prepare< int >( playerState + Offsets.ADBDPlayerState_PlayerData + Offsets.FPlayerStateData_EquipedPerkIds + 0x08 );
			handle.Prepare< int >( playerState + Offsets.ADBDPlayerState_PlayerData + Offsets.FPlayerStateData_PrestigeLevel );
			handle.Prepare< uint >( playerState + Offsets.ADBDPlayerState_Platform );
		}
		
		handle.Execute();
		
		for ( var i = 0; i < dbdGameStateComponent.PlayerStates.Length; i++ ) {
			var playerState = dbdGameStateComponent.PlayerStates[ i ];
			if ( playerState is 0 ) {
				continue;
			}
			
			var name = Memory.ReadFString( playerState + Offsets.ADBDPlayerState_PlayerNamePrivate );
				
			var role = ( EPlayerRole )Memory.Read< byte >( playerState + Offsets.ADBDPlayerState_GameRole, handle );
			var selectedCamperIndex = Memory.Read< int >( playerState + Offsets.ADBDPlayerState_SelectedCamperIndex, handle );
			var selectedSlasherIndex = Memory.Read< int >( playerState + Offsets.ADBDPlayerState_SelectedSlasherIndex, handle );
			
			var equipedFavorId = Memory.Read< FName >( playerState + Offsets.ADBDPlayerState_PlayerData + Offsets.FPlayerStateData_EquipedFavorId, handle );
			// var equipedFavorIdName = Offsets.GetFNameByComparisonIndex( equipedFavorId.ComparisonIndex, dbdGNamesComponent.Addr );

			var equipedPerkIds = Array.Empty< FName >();
			var equipedPerkIdsLen = Memory.Read< int >( playerState + Offsets.ADBDPlayerState_PlayerData + Offsets.FPlayerStateData_EquipedPerkIds + 0x8, handle );
			if ( equipedPerkIdsLen is > 0 and < 0xF ) {
				equipedPerkIds = Memory.ReadTArray< FName >(
					Memory.Read< ulong >(playerState + Offsets.ADBDPlayerState_PlayerData + Offsets.FPlayerStateData_EquipedPerkIds),
					equipedPerkIdsLen );
			}
			
			var prestigeLevel = Memory.Read< int >( playerState + Offsets.ADBDPlayerState_PlayerData + Offsets.FPlayerStateData_PrestigeLevel, handle );
			
			var addonIds = Array.Empty< FName >();
			if ( role is EPlayerRole.VE_Slasher ) {
				var addonIdsLen = Memory.Read< int >( playerState + Offsets.ADBDPlayerState_SlasherData + Offsets.FCharacterStateDataInfo_AddonIds + 0x8 );
				if ( addonIdsLen is > 0 and < 0xF ) {
					addonIds = Memory.ReadTArray< FName >(
						Memory.Read< ulong >(playerState + Offsets.ADBDPlayerState_SlasherData + Offsets.FCharacterStateDataInfo_AddonIds),
						addonIdsLen );
				}
			}
			
			var platform = ( EPlatformFlag )Memory.Read< uint >( playerState + Offsets.ADBDPlayerState_Platform, handle );

			var playerStateEntity = LibraryServices.GetLibrary().SpawnPrefab( nameof( LibraryAsset.PlayerState ), context.World );
			playerStateEntity.SetDbdPlayerAndCharacterStateData(
				playerStateAddr: playerState,
				name: name,
				playerRole: role,
				selectedCamperIndex: selectedCamperIndex,
				selectedSlasherIndex: selectedSlasherIndex,
				equipedFavorId: equipedFavorId.ComparisonIndex,
				equipedPerkIds: equipedPerkIds != null ? equipedPerkIds.Select( id => id.ComparisonIndex ).ToImmutableArray() : ImmutableArray< int >.Empty,
				prestigeLevel: prestigeLevel,
				addonIds: equipedPerkIds != null ? addonIds.Select( id => id.ComparisonIndex ).ToImmutableArray() : ImmutableArray< int >.Empty,
				platform: platform
			);
		}
		
		handle.SafeClose();
	}
	
	
	private void DestroyAllRelatedEntity( Context context ) {
		foreach ( var entity in context.World.GetEntitiesWith( ContextAccessorFilter.AnyOf,
					 typeof( DbdPlayerAndCharacterStateDataComponent )
					 // typeof( DbdGeneratorComponent ),
					 // typeof( DbdHatchComponent ),
					 // typeof( DbdTotemComponent ),
					 // typeof( DbdSearchableComponent ),
					 // typeof( DbdPalletComponent )
					) ) {
			entity.Destroy();
		}
	}
	
}
