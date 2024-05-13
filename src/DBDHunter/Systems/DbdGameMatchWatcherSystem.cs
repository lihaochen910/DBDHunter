using Bang.Systems;
using DBDHunter.Components;
using DBDHunter.Messages;


namespace DBDHunter.Systems; 

[Watch( typeof( DbdBuiltLevelDataComponent ) )]
[Filter( typeof( DbdGWorldComponent ), typeof( DbdGNamesComponent ) )]
public class DbdGameMatchWatcherSystem : IReactiveSystem {

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		var builtLevelDataComponent = world.GetUnique< DbdBuiltLevelDataComponent >();
		var uniqueEntity = world.GetUniqueEntity< DbdGWorldComponent >();
		// var started = levelStateComponent.GameLevelLoaded && levelStateComponent.GameLevelCreated;
		if ( builtLevelDataComponent.ThemeName.ComparisonIndex != 0 ) {
			uniqueEntity.SendMessage< DbdMatchStartMessage >();
		}
		else {
			uniqueEntity.SendMessage< DbdMatchEndMessage >();
		}
	}
}