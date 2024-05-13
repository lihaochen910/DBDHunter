using System.Runtime.InteropServices;
using vmmsharp;


namespace DBDHunter.Utilities; 

public static class VmmScatterExtensions {
	
	public static bool Prepare< T >( this VmmScatter handle, ulong qwA ) where T : struct {
		return handle.Prepare( qwA, ( uint ) Marshal.SizeOf< T >() );
	}

	public static void SafeClose( this VmmScatter handle ) {
		handle.Clear( Driver.ProcessPid, Vmm.FLAG_NOCACHE );
		handle.Close();
	}
}
