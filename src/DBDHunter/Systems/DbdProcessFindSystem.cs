using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;


namespace DBDHunter.Systems; 

[Filter( ContextAccessorFilter.AllOf, typeof( VmmComponent ) )]
[Filter( ContextAccessorFilter.NoneOf, typeof( DbdGameProcessComponent ) )]
public class DbdProcessFindSystem : IFixedUpdateSystem {

	public void FixedUpdate( Context context ) {
		foreach ( var entity in context.Entities ) {
			var vmm = entity.GetVmm().Vmm;
			if ( vmm.PidGetFromName( GameProcessName.ProcessName_Epic, out var currentPid ) ) {
				InitPlatform( GamePlatform.Epic, currentPid, entity );
				break;
			}
			if ( vmm.PidGetFromName( GameProcessName.ProcessName_Steam, out currentPid ) ) {
				InitPlatform( GamePlatform.Steam, currentPid, entity );
				break;
			}
		}
	}

	private void InitPlatform( GamePlatform gamePlatform, uint pid, Entity entity ) {
		entity.SetDbdGamePlatform( gamePlatform );
		Driver.GamePlatform = gamePlatform;
		
		entity.SetDbdGWorldOffset( gamePlatform is GamePlatform.Steam ? Offsets.GWorld_Steam : Offsets.GWorld_Epic );
		entity.SetDbdGNamesOffset( gamePlatform is GamePlatform.Steam ? Offsets.FNames_Steam : Offsets.FNames_Epic );
		entity.SetDbdGObjectsOffset( gamePlatform is GamePlatform.Steam ? Offsets.GObjects_Steam : Offsets.GObjects_Epic );

		var processInfo = Driver.vmm.ProcessGetInformation( pid );
		Logger.Debug( $"moduleBase: 0x{Driver.vmm.ProcessGetModuleBase( pid, processInfo.szNameLong ):X8}" );
		entity.SetDbdGameProcess( pid, Driver.vmm.ProcessGetModuleBase( pid, processInfo.szNameLong ), processInfo );
		Driver.ProcessPid = pid;
		
		Logger.Debug( $"found Process: {processInfo.szNameLong} {processInfo.dwPID}." );
	}
	
}
