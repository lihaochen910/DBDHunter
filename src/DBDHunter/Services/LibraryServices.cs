using Murder;
using DBDHunter.Assets;


namespace DBDHunter.Services;

public static class LibraryServices {
	
	public static LibraryAsset GetLibrary() {
		return Game.Data.GetAsset< LibraryAsset >( DBDHunterMurderGame.Profile.Library );
	}


	public static UiSkinAsset GetUiSkin() {
		return Game.Data.GetAsset< UiSkinAsset >( GetLibrary().UiSkin );
	}

}
