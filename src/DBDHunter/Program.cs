global using System;
global using System.Collections.Immutable;
global using System.Collections.Generic;
global using Logger = DigitalRune.Logger;
global using World = Bang.World;
global using Entity = Bang.Entities.Entity;
global using IComponent = Bang.Components.IComponent;

using Murder.Diagnostics;
using Pixpil.Editor.Data;


namespace DBDHunter;

public static class Program {

    [STAThread]
    static void Main() {
        try {
            var gamePrototype = new DBDHunterMurderGame();
            using var game = new Murder.Game( gamePrototype, new DBDHunterDataManager( gamePrototype ) );
            game.Run();
        }
        catch ( Exception ex ) when ( GameLogger.CaptureCrash( ex ) ) { }
    }

}
