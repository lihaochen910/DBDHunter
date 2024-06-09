using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;
using DBDHunter.Utilities;
using Murder;
using vmmsharp;


namespace DBDHunter.Systems; 

[Filter( ContextAccessorFilter.AllOf, typeof( DbdActorComponent ), typeof( DbdBearTrapComponent ) )]
internal class DbdBearTrapRefreshSystem : IStartupSystem, IExitSystem, IUpdateSystem {

	private const float RefreshTime = 1f;
	private float _timer;
	private VmmScatter _handle;
	
	public void Start( Context context ) {
		_handle = Driver.CreateScatterHandle();
	}

	public void Exit( Context context ) {
		_handle.Close();
	}
	
	public void Update( Context context ) {
		if ( context.Entities.Length < 1 ) {
			return;
		}

		_timer -= Game.DeltaTime;
		if ( _timer < 0f ) {
			DoRefresh( context );
			_timer = RefreshTime;
		}
	}

	private void DoRefresh( Context context ) {
		var handle = _handle;
		foreach ( var entity in context.Entities ) {
			handle.Prepare< bool >( entity.GetDbdActor().ActorAddr + Offsets.ABaseTrap_IsTrapSet );
		}

		handle.Execute();
		
		foreach ( var entity in context.Entities ) {
			entity.SetDbdBearTrap(
				handle.Read< bool >( entity.GetDbdActor().ActorAddr + Offsets.ABaseTrap_IsTrapSet )
			);
		}
		
		handle.Clear( Driver.ProcessPid, Vmm.FLAG_NOCACHE );
	}
	
}
