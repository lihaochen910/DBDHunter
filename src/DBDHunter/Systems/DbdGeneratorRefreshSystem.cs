using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;
using DBDHunter.Utilities;
using Murder;
using vmmsharp;


namespace DBDHunter.Systems; 

[Filter( ContextAccessorFilter.AnyOf, typeof( DbdGeneratorComponent ) )]
internal class DbdGeneratorRefreshSystem : IStartupSystem, IExitSystem, IUpdateSystem {

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
			handle.Prepare< bool >( entity.GetDbdGenerator().Addr + Offsets.AGenerator_Activated );
			handle.Prepare< float >( entity.GetDbdGenerator().Addr + Offsets.AGenerator_NativePercentComplete );
		}

		handle.Execute();
		
		foreach ( var entity in context.Entities ) {
			entity.SetDbdGenerator(
				entity.GetDbdGenerator().Addr,
				handle.Read< bool >( entity.GetDbdGenerator().Addr + Offsets.AGenerator_Activated ),
				handle.Read< float >( entity.GetDbdGenerator().Addr + Offsets.AGenerator_NativePercentComplete )
			);
		}
		
		handle.Clear( Driver.ProcessPid, Vmm.FLAG_NOCACHE );
	}
	
}
