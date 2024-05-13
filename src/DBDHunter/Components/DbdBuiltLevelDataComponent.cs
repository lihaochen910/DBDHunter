using Bang.Components;


namespace DBDHunter.Components; 

[Unique]
public readonly struct DbdBuiltLevelDataComponent : IComponent {
	public readonly FName ThemeName;
	public readonly FName ThemeWeather;
	public readonly ulong MapName;
    	
	public DbdBuiltLevelDataComponent( FName themeName, FName themeWeather, ulong mapName ) {
		ThemeName = themeName;
		ThemeWeather = themeWeather;
		MapName = mapName;
	}
}
