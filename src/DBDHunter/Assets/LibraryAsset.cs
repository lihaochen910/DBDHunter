using System.Numerics;
using System.Reflection;
using Murder;
using Murder.Assets;
using Murder.Attributes;
using Murder.Utilities;


namespace DBDHunter.Assets; 

public class LibraryAsset : GameAsset {
	
	public override string EditorFolder => "#\uf02dLibraries";

	public override char Icon => '\uf02d';

	public override Vector4 EditorColor => "#FA5276".ToVector4Color();

	[GameAssetId< UiSkinAsset >]
	public readonly Guid UiSkin = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid Vmm = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid PersistentLevel = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid PlayerCameraManager = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid LocalPlayers = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid PlayerActor = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid PlayerState = Guid.Empty;

	[GameAssetId< PrefabAsset >]
	public readonly Guid Generator = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid Hatch = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid Totem = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid Searchable = Guid.Empty;

	[GameAssetId< PrefabAsset >]
	public readonly Guid Pallet = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid BearTrap = Guid.Empty;

	
	
	public Entity SpawnPrefab( string fieldName, World world ) {
		var prefabAssetObj = GetType().GetField( fieldName, BindingFlags.Public | BindingFlags.Instance )?.GetValue( this );
		if ( prefabAssetObj is Guid prefabGuid && prefabGuid != Guid.Empty ) {
			if ( Game.Data.TryGetAsset< PrefabAsset >( prefabGuid ) is {} prefabAsset ) {
				return prefabAsset.CreateAndFetch( world );
			}
		}

		return null;
	}
	
}
