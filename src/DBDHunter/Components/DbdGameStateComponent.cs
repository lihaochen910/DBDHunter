using Bang.Components;
using DBDHunter.Systems;


namespace DBDHunter.Components; 

[Unique]
public readonly struct DbdGameStateComponent : IComponent {
	
	public readonly ulong Addr;
	public readonly ImmutableArray< GameStatePlayerData > PlayerDatas;
	public readonly ImmutableArray< ulong > PlayerStates;
	public readonly ImmutableArray< ulong > MeatHooks;
	public readonly ImmutableArray< ulong > Searchables;
	public readonly ImmutableArray< ulong > Generators;
	public readonly ImmutableArray< ulong > EscapeDoors;
	public readonly ImmutableArray< ulong > Hatches;
	public readonly ImmutableArray< ulong > Pallets;
	public readonly ImmutableArray< ulong > Windows;
	public readonly ImmutableArray< ulong > Totems;
	
	public DbdGameStateComponent( ulong addr,
								  ImmutableArray< GameStatePlayerData > playerDatas,
								  ImmutableArray< ulong > playerStates,
								  ImmutableArray< ulong > meatHooks,
								  ImmutableArray< ulong > searchables,
								  ImmutableArray< ulong > generators,
								  ImmutableArray< ulong > escapeDoors,
								  ImmutableArray< ulong > hatches,
								  ImmutableArray< ulong > pallets,
								  ImmutableArray< ulong > windows,
								  ImmutableArray< ulong > totems ) {
		Addr = addr;
		PlayerDatas = playerDatas;
		PlayerStates = playerStates;
		MeatHooks = meatHooks;
		Searchables = searchables;
		Generators = generators;
		EscapeDoors = escapeDoors;
		Hatches = hatches;
		Pallets = pallets;
		Windows = windows;
		Totems = totems;
	}
	
	public DbdGameStateComponent SetPlayerDatas( ImmutableArray< GameStatePlayerData > playerDatas ) {
		return new DbdGameStateComponent(
			Addr,
			playerDatas,
			PlayerStates,
			MeatHooks,
			Searchables,
			Generators,
			EscapeDoors,
			Hatches,
			Pallets,
			Windows,
			Totems
		);
	}
	
	public DbdGameStateComponent SetPlayerStates( ImmutableArray< ulong > playerStates ) {
		return new DbdGameStateComponent(
			Addr,
			PlayerDatas,
			playerStates,
			MeatHooks,
			Searchables,
			Generators,
			EscapeDoors,
			Hatches,
			Pallets,
			Windows,
			Totems
		);
	}

	public DbdGameStateComponent SetMeatHooks( ImmutableArray< ulong > meatHooks ) {
		return new DbdGameStateComponent(
			Addr,
			PlayerDatas,
			PlayerStates,
			meatHooks,
			Searchables,
			Generators,
			EscapeDoors,
			Hatches,
			Pallets,
			Windows,
			Totems
		);
	}
	
	public DbdGameStateComponent SetSearchables( ImmutableArray< ulong > searchables ) {
		return new DbdGameStateComponent(
			Addr,
			PlayerDatas,
			PlayerStates,
			MeatHooks,
			searchables,
			Generators,
			EscapeDoors,
			Hatches,
			Pallets,
			Windows,
			Totems
		);
	}
	
	public DbdGameStateComponent SetGenerators( ImmutableArray< ulong > generators ) {
		return new DbdGameStateComponent(
			Addr,
			PlayerDatas,
			PlayerStates,
			MeatHooks,
			Searchables,
			generators,
			EscapeDoors,
			Hatches,
			Pallets,
			Windows,
			Totems
		);
	}
	
	public DbdGameStateComponent SetEscapeDoors( ImmutableArray< ulong > escapeDoors ) {
		return new DbdGameStateComponent(
			Addr,
			PlayerDatas,
			PlayerStates,
			MeatHooks,
			Searchables,
			Generators,
			escapeDoors,
			Hatches,
			Pallets,
			Windows,
			Totems
		);
	}
	
	public DbdGameStateComponent SetHatches( ImmutableArray< ulong > hatches ) {
		return new DbdGameStateComponent(
			Addr,
			PlayerDatas,
			PlayerStates,
			MeatHooks,
			Searchables,
			Generators,
			EscapeDoors,
			hatches,
			Pallets,
			Windows,
			Totems
		);
	}
	
	public DbdGameStateComponent SetPallets( ImmutableArray< ulong > pallets ) {
		return new DbdGameStateComponent(
			Addr,
			PlayerDatas,
			PlayerStates,
			MeatHooks,
			Searchables,
			Generators,
			EscapeDoors,
			Hatches,
			pallets,
			Windows,
			Totems
		);
	}
	
	public DbdGameStateComponent SetWindows( ImmutableArray< ulong > windows ) {
		return new DbdGameStateComponent(
			Addr,
			PlayerDatas,
			PlayerStates,
			MeatHooks,
			Searchables,
			Generators,
			EscapeDoors,
			Hatches,
			Pallets,
			windows,
			Totems
		);
	}
	
	public DbdGameStateComponent SetTotems( ImmutableArray< ulong > totems ) {
		return new DbdGameStateComponent(
			Addr,
			PlayerDatas,
			PlayerStates,
			MeatHooks,
			Searchables,
			Generators,
			EscapeDoors,
			Hatches,
			Pallets,
			Windows,
			totems
		);
	}

}


[Unique]
public readonly struct DbdGameStateLevelReadyToPlayComponent : IComponent;


[Unique]
public readonly struct DbdGameStateLevelStateComponent : IComponent {
	public readonly bool GameLevelLoaded;
	public readonly bool GameLevelCreated;
	public readonly bool GameLevelEnded;
	public readonly EEndGameReason GameEndedReason = EEndGameReason.None;

	public DbdGameStateLevelStateComponent( bool gameLevelLoaded, bool gameLevelCreated, bool gameLevelEnded, EEndGameReason gameEndedReason ) {
		GameLevelLoaded = gameLevelLoaded;
		GameLevelCreated = gameLevelCreated;
		GameLevelEnded = gameLevelEnded;
		GameEndedReason = gameEndedReason;
	}
}
