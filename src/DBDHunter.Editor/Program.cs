using System;


namespace DBDHunter.Editor;

public static class Program {
    
    [STAThread]
    static void Main() {
        var iMurderGame = new DBDHunterArchitect();
        using var editor = new DBDHunterEditorArchitect( iMurderGame, null );
        editor.Run();
    }
    
}
