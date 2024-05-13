using System;
using DigitalRune.Mathematics.Algebra;
using Murder.Assets;
using Murder.Attributes;

namespace DBDHunter.Assets;

public class DBDHunterProfile : GameProfile {
	
	[GameAssetId( typeof( LibraryAsset ) )]
	public readonly Guid Library;


	public readonly Vector2I EspViewportSize = new ( 1920, 1080 );
}
