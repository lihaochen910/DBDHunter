using Bang.Components;


namespace DBDHunter.Components; 

[Unique]
public readonly struct DbdGamePlatformComponent : IComponent {
	
	public readonly GamePlatform Platform;

	public DbdGamePlatformComponent( GamePlatform gamePlatform ) => Platform = gamePlatform;
}
