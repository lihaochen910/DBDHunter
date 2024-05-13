using DigitalRune.Mathematics;


namespace DBDHunter.Components; 

public readonly struct DbdGeneratorComponent : IComponent {
	public readonly ulong Addr;
	public readonly bool Activated;
	public readonly float NativePercentComplete;

	public bool IsDone => Numeric.IsGreaterOrEqual( NativePercentComplete, 1f );
    	
	public DbdGeneratorComponent( ulong addr, bool activated, float nativePercentComplete ) {
		Addr = addr;
		Activated = activated;
		NativePercentComplete = nativePercentComplete;
	}
}
