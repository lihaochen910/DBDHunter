using System;
using ImGuiNET;
using Murder.Editor.CustomFields;
using Murder.Editor.Reflection;


namespace DBDHunter.Editor.CustomFields; 

[CustomFieldOf( typeof( ulong ) )]
internal class UInt64Field : CustomField {
	public override (bool modified, object? result) ProcessInput( EditorMember member, object? fieldValue ) {
		bool modified = false;
		ulong number = Convert.ToUInt64( fieldValue );
		ImGui.Text( $"0x{number:X8}" );
		return ( false, number );
	}
}
