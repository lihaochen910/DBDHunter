using System.Collections.Immutable;
using Murder.Core.Dialogs;

namespace DBDHunter.Core;

[Blackboard( Name, @default: true )]
public class GameplayBlackboard : IBlackboard {
	
	public const string Name = "Gameplay";

	/// <summary>
	/// 人口
	/// </summary>
	public int Population = 1;
	
	public bool HasPendingConstructBuilding;
	public ImmutableArray< int > PendingConstructBuildings;
	
	
}
