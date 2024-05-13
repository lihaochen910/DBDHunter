using Bang;
using Bang.Components;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;


namespace DBDHunter.Systems; 

[Messager( typeof( RequestRefreshMessage ) )]
[Filter( typeof( DbdPersistentLevelComponent ) )]
public class DbdPersistentLevelRefreshSystem : IMessagerSystem {

	public void OnMessage( World world, Entity entity, IMessage message ) {
		
	}

}
