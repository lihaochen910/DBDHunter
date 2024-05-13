using System.Numerics;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;
using Murder;
using Murder.Core.Graphics;
using Murder.Services;


namespace DBDHunter.Systems; 

/// <summary>
/// 游戏的GWorld从大厅切换到游玩状态后地址会发生改变
/// </summary>
[Filter( ContextAccessorFilter.AllOf, typeof( DbdGameProcessComponent ), typeof( DbdGWorldComponent ), typeof( DbdGWorldOffsetComponent ) )]
public class DbdGWorldWatchSystem : IFixedUpdateSystem, IMurderRenderSystem {
	
	private const float RefreshTime = 1f;
	private float _timer;
	
	public void FixedUpdate( Context context ) {
		if ( context.Entities.Length < 1 ) {
			return;
		}

		_timer -= Game.FixedDeltaTime;
		if ( _timer < 0f ) {
			DoCheckGWorld( context );
			_timer = RefreshTime;
		}
	}
	
	public void Draw( RenderContext render, Context context ) {
		if ( context.World.TryGetUnique< DbdGWorldComponent >() is not {} dbdGWorldComponent ) {
			return;
		}
		
		var drawInfo = new DrawInfo( Color.Gray, 0.1f ) { Scale = Vector2.One * 1f };
		render.UiBatch.DrawText( 103, $"GWorld: 0x{dbdGWorldComponent.Addr:X8}", new Vector2( 2, 28f ), drawInfo );
	}

	private void DoCheckGWorld( Context context ) {
		var gWorldAddr = Memory.Read< ulong >( context.Entity.GetDbdGameProcess().BaseAddr + context.Entity.GetDbdGWorldOffset().Offset );
		if ( gWorldAddr != 0 && gWorldAddr != context.Entity.GetDbdGWorld().Addr ) {
			Logger.Debug( "[DbdGWorldWatchSystem] GWorld changed." );
			DbdEngineInitializeSystem.InitDbdEngineOffsets( context.Entity );
		}
	}
	
	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		Logger.Debug( "[DbdGWorldWatchSystem] detect DbdPersistentLevelComponent added." );
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
		Logger.Debug( "[DbdGWorldWatchSystem] detect DbdPersistentLevelComponent removed." );
	}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		Logger.Debug( "[DbdGWorldWatchSystem] detect DbdPersistentLevelComponent modified." );
	}

	
}
