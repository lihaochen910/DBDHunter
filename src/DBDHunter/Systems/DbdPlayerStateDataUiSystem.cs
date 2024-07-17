using System.Numerics;
using System.Text;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;
using DBDHunter.Utilities;
using Murder;
using Murder.Core.Graphics;
using Murder.Services;


namespace DBDHunter.Systems;

[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Write, typeof( DbdPlayerAndCharacterStateDataComponent ) )]
public class DbdPlayerStateDataUiSystem : IMurderRenderSystem {

	private List< Entity > _sortedEntities = new ( 5 );
	private StringBuilder _perkStrBuilder = new ();
	private StringBuilder _addonStrBuilder = new ();
	
	public void Draw( RenderContext render, Context context ) {
		if ( context.World.TryGetUniqueEntity< DbdGNamesComponent >() is null ) {
			return;
		}
		
		var dbdGNamesComponent = context.World.GetUnique< DbdGNamesComponent >();
		
		_sortedEntities.Clear();

		foreach ( var entity in context.Entities ) {
			_sortedEntities.Add( entity );
		}
		
		_sortedEntities.Sort( ( a, b ) => {
			if ( a.GetDbdPlayerAndCharacterStateData().PlayerRole is EPlayerRole.VE_Slasher &&
				 b.GetDbdPlayerAndCharacterStateData().PlayerRole is EPlayerRole.VE_Camper ) {
				return -1;
			}
			if ( a.GetDbdPlayerAndCharacterStateData().PlayerRole is EPlayerRole.VE_Camper &&
				 b.GetDbdPlayerAndCharacterStateData().PlayerRole is EPlayerRole.VE_Slasher ) {
				return 1;
			}

			return 0;
		} );

		const float fontSize = 12f;
		const float fontScale = 2f;
		var drawInfo = new DrawInfo( Color.BrightGray, 0.1f ) { Scale = Vector2.One * fontScale };
		var slasherDrawInfo = new DrawInfo( Color.Cyan, 0.1f ) { Scale = Vector2.One * fontScale };
	
		var lineCounter = 1;
		void Line( string line ) {
			render.UiBatch.DrawText( 103, line, new Vector2( 2, Game.Height / 5f * 4f + lineCounter++ * fontSize * fontScale ), drawInfo );
		}
		
		void SlasherLine( string line ) {
			render.UiBatch.DrawText( 103, line, new Vector2( 2, Game.Height / 5f * 4f + lineCounter++ * fontSize * fontScale ), slasherDrawInfo );
		}
		
		foreach ( var entity in _sortedEntities ) {
			var data = entity.GetDbdPlayerAndCharacterStateData();
			if ( data.Name != null && data.Name.Length > 0xFFF ) { // invalid data
				continue;
			}
			
			var equipedFavorName = DBDFavors.GetFavorName( UEHelper.GetFNameByComparisonIndex( data.EquipedFavorId ) );
			string characterName = "None";
			string perksStr = string.Empty;
			string addonStr = string.Empty;
			_perkStrBuilder.Clear();
			_addonStrBuilder.Clear();
			foreach ( var perkId in data.EquipedPerkIds ) {
				_perkStrBuilder.AppendFormat( "[{0}] ", DBDPerks.GetPerkName( UEHelper.GetFNameByComparisonIndex( perkId ) ) );
			}
			perksStr = _perkStrBuilder.ToString();
			
			switch ( data.PlayerRole ) {
				case EPlayerRole.VE_Slasher:
					characterName = DBDKillers.GetKillerName( data.SelectedSlasherIndex );
					foreach ( var addonId in data.AddonIds ) {
						_addonStrBuilder.AppendFormat( "'{0}' ", DBDAddons.GetAddonName( UEHelper.GetFNameByComparisonIndex( addonId ) ) );
					}
					addonStr = _addonStrBuilder.ToString();
					SlasherLine( $"{data.Name} {characterName} P{data.PrestigeLevel:000} {perksStr} {addonStr} {equipedFavorName} [{data.Platform}]" );
					break;
				case EPlayerRole.VE_Camper:
					characterName = DBDCampers.GetCampersName( data.SelectedCamperIndex ) ?? data.SelectedCamperIndex.ToString();
					Line( $"{data.Name} {characterName} P{data.PrestigeLevel:000} {perksStr} {addonStr} {equipedFavorName} [{data.Platform}]" );
					break;
				default:
					characterName = data.PlayerRole.ToString();
					break;
			}
			
		}
	}
}
