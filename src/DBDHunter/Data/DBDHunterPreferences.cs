using System.Runtime.CompilerServices;
using Murder;
using Murder.Save;


namespace DBDHunter.Data;

/// <summary>
/// Tracks preferences of the current session. This is unique per run.
/// </summary>
public class DBDHunterPreferences : GamePreferences {

    private static readonly float[] TimeSpeedPresets = [ 1f / 120f, 1f / 60f, 1f / 30f, 1f / 15f, 1f / 3f ];

    public const float TheDuskTime = 0.7f;

    [Bang.Serialize]
    protected float _speedOfTime = 1f;

    public float SpeedOfTime => _speedOfTime;
    
    /// <summary>
    /// 人口食物消耗(每人)
    /// </summary>
    public int PopulationFoodConsume { get; protected set; }
    
    public DBDHunterPreferences() : base() { }

    public override void OnPreferencesChangedImpl() {
        // Game.Sound.SetVolume(id: PGGame.Profile.MusicBus, _musicVolume);
        // TODO: Implement sound.
        // Game.Sound.SetVolume(id: LDGame.Profile.SoundBus, _soundVolume);
        
        SetTimeFliesSpeed( TimeFliesSpeedType.VeryFast );

        PopulationFoodConsume = 20;
    }

    public void SetTimeFliesSpeed( TimeFliesSpeedType timeFliesSpeedType ) {
        switch ( timeFliesSpeedType ) {
            case TimeFliesSpeedType.Slow:     _speedOfTime = TimeSpeedPresets[ 0 ]; break;
            case TimeFliesSpeedType.Normal:   _speedOfTime = TimeSpeedPresets[ 1 ]; break;
            case TimeFliesSpeedType.Fast:     _speedOfTime = TimeSpeedPresets[ 2 ]; break;
            case TimeFliesSpeedType.VeryFast: _speedOfTime = TimeSpeedPresets[ 3 ]; break;
            case TimeFliesSpeedType.AMoment:  _speedOfTime = TimeSpeedPresets[ 4 ]; break;
        }
    }
    
}


public static class GamePrototypePreferencesMurderGameExtensions {
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DBDHunterPreferences GetPreferences( this Game game ) => Game.Preferences as DBDHunterPreferences;
    
}


public enum TimeFliesSpeedType : byte {
    Slow,
    Normal,
    Fast,
    VeryFast,
    AMoment
}
