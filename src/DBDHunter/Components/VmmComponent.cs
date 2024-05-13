using Bang.Components;
using vmmsharp;


namespace DBDHunter.Components; 

[Unique]
public readonly struct VmmComponent : IModifiableComponent {

	public readonly Vmm Vmm;

	public VmmComponent( Vmm vmm ) {
		Vmm = vmm;
	}

	public void Subscribe( Action notification ) {
		
	}

	public void Unsubscribe( Action notification ) {
		
	}
}
