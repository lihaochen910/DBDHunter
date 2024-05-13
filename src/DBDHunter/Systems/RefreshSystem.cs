using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;
using Murder;


namespace DBDHunter.Systems; 

[Filter( typeof( RefreshListenerComponent ) )]
public class RefreshSystem : IUpdateSystem {

	public void Update( Context context ) {
		foreach ( var entity in context.Entities ) {
			if ( !entity.HasRefreshTimer() ) {
				entity.SetRefreshTimer( entity.GetRefreshListener().RefreshTime );
			}
			
			entity.SetRefreshTimer( entity.GetRefreshTimer().Time - Game.DeltaTime );
			if ( entity.GetRefreshTimer().Time < 0 ) {
				entity.SetRefreshTimer( entity.GetRefreshListener().RefreshTime );
				entity.SendMessage< RequestRefreshMessage >();
			}
		}
	}
	
}
