using Murder;
using Murder.Data;


namespace Pixpil.Editor.Data;

public class DBDHunterDataManager : GameDataManager {

	public DBDHunterDataManager( IMurderGame game ) : base( game ) {}

	public override void LoadContent() {
		// ConvertBitmapFontToSpriteFont();
		base.LoadContent();
		
		// ItemTypeServices.Initialize();
	}
	
}
