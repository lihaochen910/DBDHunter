namespace DBDHunter.Components; 

public readonly struct DbdPlayerAndCharacterStateDataComponent : IComponent {

	public readonly ulong PlayerStateAddr;
	public readonly string Name;
	public readonly EPlayerRole PlayerRole;
	public readonly int SelectedCamperIndex;
	public readonly int SelectedSlasherIndex;
	public readonly int EquipedFavorId;
	public readonly ImmutableArray< int > EquipedPerkIds;
	public readonly int PrestigeLevel;
	public readonly ImmutableArray< int > AddonIds;
	
	public DbdPlayerAndCharacterStateDataComponent( ulong playerStateAddr,
													string name,
													EPlayerRole playerRole,
													int selectedCamperIndex,
													int selectedSlasherIndex,
													int equipedFavorId,
													ImmutableArray< int > equipedPerkIds,
													int prestigeLevel,
													ImmutableArray< int > addonIds
													) {
		PlayerStateAddr = playerStateAddr;
		Name = name;
		PlayerRole = playerRole;
		SelectedCamperIndex = selectedCamperIndex;
		SelectedSlasherIndex = selectedSlasherIndex;
		EquipedFavorId = equipedFavorId;
		EquipedPerkIds = equipedPerkIds;
		PrestigeLevel = prestigeLevel;
		AddonIds = addonIds;
	}

}
