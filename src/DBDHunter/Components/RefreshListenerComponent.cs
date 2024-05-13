using Bang.Components;


namespace DBDHunter.Components; 

public readonly struct RefreshListenerComponent : IComponent {
	public readonly float RefreshTime;
	
	public RefreshListenerComponent( float refreshTime ) {
		RefreshTime = refreshTime;
	}
}


public readonly struct RefreshTimerComponent : IComponent {
	public readonly float Time;
	
	public RefreshTimerComponent( float time ) {
		Time = time;
	}
}


public readonly struct RequestRefreshMessage : IMessage;
