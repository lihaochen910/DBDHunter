using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;
using DBDHunter.Utilities;
using Murder;
using vmmsharp;


namespace DBDHunter.Systems; 

[Filter( ContextAccessorFilter.AnyOf, typeof( DbdTotemComponent ) )]
internal class DbdTotemRefreshSystem : IStartupSystem, IExitSystem, IUpdateSystem {

	private const float RefreshTime = 2f;
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
			handle.Prepare< byte >( entity.GetDbdTotem().Addr + Offsets.ATotem_TotemState );
		}

		handle.Execute();
		
		foreach ( var entity in context.Entities ) {
			var totemState = ( ETotemState )handle.Read< byte >( entity.GetDbdTotem().Addr + Offsets.ATotem_TotemState );
			entity.SetDbdTotem(
				entity.GetDbdTotem().Addr,
				totemState
			);
		}
		
		handle.Clear( Driver.ProcessPid, Vmm.FLAG_NOCACHE );
	}
	
}
