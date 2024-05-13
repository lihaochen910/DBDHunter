using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using DBDHunter.Components;
using ImGuiNET;
using Murder.Editor;
using Murder.Editor.Attributes;
using Murder.Editor.CustomComponents;
using Murder.Serialization;


namespace DBDHunter.Editor.CustomComponents; 

[CustomComponentOf( typeof( DbdActorMeshSocketsComponent ) )]
public class DbdActorMeshSocketsComponentEditor : CustomComponent {
	
	protected override bool DrawAllMembersWithTable( ref object target, bool sameLineFilter ) {

		var playingInEditor = Architect.Instance != null && Architect.Instance.IsPlayingGame;
		if ( playingInEditor ) {
			var dbdActorMeshSocketsComponent = ( DbdActorMeshSocketsComponent )target;
			if ( !dbdActorMeshSocketsComponent.SocketsData.IsDefaultOrEmpty ) {
				ImGui.Text( $"SocketsData size: {dbdActorMeshSocketsComponent.SocketsData.Length}" );
				ImGui.SameLine();
				if ( ImGui.Button( "Dump SocketsData" ) ) {
					var dumpedSocketsData =
						dbdActorMeshSocketsComponent.SocketsData.Select( s => {
							return new USkeletalMeshSocketDumped {
								SocketName = s.SocketName.ToString(),
								BoneName = s.BoneName.ToString(),
								RelativeLocation = s.RelativeLocation,
								RelativeRotation = s.RelativeRotation,
								RelativeScale = s.RelativeScale
							};
						} ).ToImmutableArray();
					var text = FileManager.SerializeToJson( new DbdActorMeshSocketsDumpedComponent( dumpedSocketsData ) );
					File.WriteAllText( "SocketsData.json", text );
				}
			}
			
			return false;
		}

		return base.DrawAllMembersWithTable( ref target, sameLineFilter );
	}
	
}
