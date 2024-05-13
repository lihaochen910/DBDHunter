using Microsoft.Xna.Framework.Input;
using Murder;
using Murder.Assets;
using Murder.Core.Input;
using Murder.Save;
using System.Collections.Immutable;
using System.Text.Json;
using DBDHunter.Assets;
using DBDHunter.Core;
using DBDHunter.Data;


namespace DBDHunter;

public class DBDHunterMurderGame : IMurderGame {
    
    public static DBDHunterProfile Profile => ( DBDHunterProfile )Game.Profile;
    public string Name => "DBDHunter";
    public float Version => 0;

    // public ISoundPlayer CreateSoundPlayer() => new LDGameSoundPlayer();

    // public JsonSerializerOptions Options { get; }
    // public JsonSerializerOptions Options => Murder.Serialization.MurderSerializerOptionsExtensions.Options;
    public JsonSerializerOptions Options => Murder.Serialization.DBDHunterSerializerOptionsExtensions.Options;

    public void Initialize() {
        
        Game.Data.CurrentPalette = Palette.Colors.ToImmutableArray();

        // Register movement axis input.
        Game.Input.RegisterAxes(MurderInputAxis.Movement, GamepadAxis.LeftThumb, GamepadAxis.RightThumb, GamepadAxis.Dpad);

        Game.Input.Register(MurderInputAxis.Movement,
            new InputButtonAxis(Keys.W, Keys.A, Keys.S, Keys.D),
            new InputButtonAxis(Keys.Up, Keys.Left, Keys.Down, Keys.Right));

        Game.Input.Register(MurderInputAxis.Movement,
            new InputButtonAxis(Keys.W, Keys.A, Keys.S, Keys.D),
            new InputButtonAxis(Keys.Up, Keys.Left, Keys.Down, Keys.Right));

        Game.Input.Register(MurderInputAxis.Ui,
            new InputButtonAxis(Keys.W, Keys.A, Keys.S, Keys.D),
            new InputButtonAxis(Keys.Up, Keys.Left, Keys.Down, Keys.Right));

        Game.Input.Register(MurderInputAxis.UiTab,
            new InputButtonAxis(Keys.Q, Keys.Q, Keys.E, Keys.E),
            new InputButtonAxis(Keys.PageUp, Keys.PageUp, Keys.PageDown, Keys.PageDown));

        Game.Input.Register(MurderInputAxis.UiTab,
            new InputButtonAxis(Buttons.LeftShoulder, Buttons.LeftShoulder, Buttons.RightShoulder, Buttons.RightShoulder));

        Game.Input.Register(InputButtons.LockMovement, Keys.LeftShift, Keys.RightShift);
        Game.Input.Register(InputButtons.LockMovement, Buttons.LeftTrigger, Buttons.RightTrigger);

        Game.Input.Register(InputButtons.Attack,
            Keys.Z);
        Game.Input.Register(InputButtons.Attack,
            Buttons.X);

        Game.Input.Register(InputButtons.Submit,
            Keys.Enter, Keys.Space);

        Game.Input.Register(InputButtons.SubmitWithEnter,
            Keys.Enter);

        Game.Input.Register(InputButtons.Cancel,
            Keys.Escape, Keys.Delete, Keys.Back, Keys.BrowserBack);
        
        Game.Input.Register(InputButtons.Cancel,
            MouseButtons.Right);

        Game.Input.Register(InputButtons.Interact, Keys.Space);

        Game.Input.Register(InputButtons.Interact, Buttons.Y);

        Game.Input.Register(InputButtons.Skip,
            Keys.Back, Keys.Escape, Keys.O);
        Game.Input.Register(InputButtons.Skip,
            Buttons.Start);

        Game.Input.Register(InputButtons.ShotcutSelectBuilding01,
            Keys.NumPad1, Keys.D1);

        Game.Input.Register(InputButtons.ShotcutSelectBuilding02,
            Keys.NumPad2, Keys.D2);
        
        Game.Input.Register(InputButtons.ShotcutSelectBuilding03,
            Keys.NumPad3, Keys.D3);
        
        Game.Input.Register(InputButtons.ShotcutSelectBuilding04,
            Keys.NumPad3, Keys.D4);
    }

    public void OnDraw() {
        Game.GraphicsDevice.SetRenderTarget( null );
    }

    // public SaveData CreateSaveData( int slot ) => new GPSaveData( slot, Version );

    public GameProfile CreateGameProfile() => new DBDHunterProfile();

    public GamePreferences CreateGamePreferences() => new DBDHunterPreferences();
}
