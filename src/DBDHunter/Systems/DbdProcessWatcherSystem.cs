using System.Runtime.InteropServices;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;
using Microsoft.Extensions.Logging;
using Murder;
using vmmsharp;


namespace DBDHunter.Systems; 

[Filter( ContextAccessorFilter.AllOf, typeof( VmmComponent ), typeof( DbdGameProcessComponent ) )]
public class DbdProcessWatcherSystem : IFixedUpdateSystem {

	private const float Time = 5f;
	private float _timer;

	public void FixedUpdate( Context context ) {
		_timer -= Game.FixedDeltaTime;
		if ( _timer < 0 ) {
			_timer = Time;
			CheckGameProcessKilled( context );
		}
	}


	private unsafe void CheckGameProcessKilled( Context context ) {
		if ( context.World.TryGetUnique< VmmComponent >() is not {} vmmComponent ) {
			return;
		}
		
		if ( context.World.TryGetUnique< DbdGameProcessComponent >() is {} gameProcessComponent ) {
			bool success = false;
			ulong cbENTRY = (ulong)Marshal.SizeOf<vmmi.VMMDLL_PROCESS_INFORMATION>();
			fixed ( byte* pb = new byte[ cbENTRY ] ) {
				Marshal.WriteInt64( new IntPtr( pb + 0 ), unchecked( ( long ) vmmi.VMMDLL_PROCESS_INFORMATION_MAGIC ) );
				Marshal.WriteInt16( new IntPtr( pb + 8 ), unchecked( ( short ) vmmi.VMMDLL_PROCESS_INFORMATION_VERSION ) );
				success = vmmi.VMMDLL_ProcessGetInformation( vmmComponent.Vmm.HandleVMM, gameProcessComponent.Pid, pb, ref cbENTRY );
				if ( !success ) {
					Logger.Debug( "detect GameProcess killed." );

					if ( context.World.TryGetUniqueEntity< DbdGameProcessComponent >() is {} gameProcessEntity ) {
						gameProcessEntity.RemoveDbdGWorldOffset();
						gameProcessEntity.RemoveDbdGNamesOffset();
						gameProcessEntity.RemoveDbdGObjectsOffset();
						gameProcessEntity.RemoveDbdGamePlatform();
						gameProcessEntity.RemoveDbdGameProcess();
					}
				}
			}
		}
	}
	
}
