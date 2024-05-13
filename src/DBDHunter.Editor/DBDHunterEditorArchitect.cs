using Murder.Editor;
using Murder.Editor.Data;


namespace DBDHunter; 

public class DBDHunterEditorArchitect : Architect {

	public DBDHunterEditorArchitect( IMurderArchitect? game = null, EditorDataManager? editorDataManager = null ) : base( game, editorDataManager ) {}

	protected override void ApplyGameSettingsImpl() {
		
		base.ApplyGameSettingsImpl();

		if ( Data.GameProfile.IsVSyncEnabled ) {
			_graphics.SynchronizeWithVerticalRetrace = true;
			IsFixedTimeStep = true;
		}
	}

	// protected override void Draw( GameTime gameTime ) {
	// 	base.Draw( gameTime );
	// 	
	// }
	//
	// protected override void EndDraw() {
	// 	GraphicsDevice.SetRenderTarget( null );
	// 	base.EndDraw();
	// }

}
