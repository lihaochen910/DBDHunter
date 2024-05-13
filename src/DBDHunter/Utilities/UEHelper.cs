using System.Diagnostics;
using System.Text;


namespace DBDHunter.Utilities; 

public static class UEHelper {

	[DebuggerDisplay("{ClassName}::{Super?.ClassName} 0x{Addr}")]
	public record UClassNode ( ulong addr, string className ) {
		public readonly ulong Addr = addr;
		public readonly string ClassName = className;
		public UClassNode Super;

		public string GetInheritanceTreeDebugString() {
			_actorClassDebugStrBuilder.Clear();
			var klassNode = this;
			while ( klassNode != null ) {
				_actorClassDebugStrBuilder.Append( $"->{klassNode.ClassName}" );
				klassNode = klassNode.Super;
			}

			return _actorClassDebugStrBuilder.ToString();
		}
	}
	
	
	private static Dictionary< ulong, UClassNode > UClassNodeMap = new ();

	
	public static void ClearUClassPtrCache() => UClassNodeMap.Clear();

	
	internal static void AffirmUClassCache( ulong uObject, ulong uObjectKlass ) {
		if ( uObject is 0 ) {
			return;
		}
		
		if ( uObjectKlass is 0 ) {
			Logger.Warning( $"invalid UObject: {uObject:X8}" );
			return;
		}
		
		if ( UClassNodeMap.ContainsKey( uObjectKlass ) ) {
			return;
		}
		
		var uObjectKlassNameComparisonIndex = Memory.Read< int >( uObjectKlass + Offsets.UObject_Name );
		var uClassNode = new UClassNode( uObjectKlass, GetFNameByComparisonIndex( uObjectKlassNameComparisonIndex ) );
		UClassNodeMap[ uObjectKlass ] = uClassNode;
		
		LinkedList< UClassNode > childToSuper = new ();
		var currentNode = childToSuper.AddFirst( uClassNode );
		
		while ( uObjectKlass != 0 ) {
			
			uObjectKlass = Memory.Read< ulong >( uObjectKlass + Offsets.UStruct_Super );
			if ( uObjectKlass is 0 ) {
				break;
			}
			
			if ( !UClassNodeMap.ContainsKey( uObjectKlass ) ) {
				uObjectKlassNameComparisonIndex = Memory.Read< int >( uObjectKlass + Offsets.UObject_Name );
				uClassNode = new UClassNode( uObjectKlass, GetFNameByComparisonIndex( uObjectKlassNameComparisonIndex ) );
				UClassNodeMap[ uObjectKlass ] = uClassNode;
			}
			else {
				uClassNode = UClassNodeMap[ uObjectKlass ];
			}
			
			currentNode = childToSuper.AddAfter( currentNode, uClassNode );
		}

		var linkedNode = childToSuper.First;
		while ( linkedNode != null ) {
			if ( linkedNode.Next is null ) {
				break;
			}

			linkedNode.Value.Super = linkedNode.Next.Value;
			linkedNode = linkedNode.Next;
		}
		
	}


	internal static UClassNode GetCachedUClassNode( ulong uObjectKlass ) {
		if ( UClassNodeMap.ContainsKey( uObjectKlass ) ) {
			return UClassNodeMap[ uObjectKlass ];
		}

		return null;
	}


	public static bool IsA( this ulong uObject, string klassName, ulong uObjectKlass ) {
		if ( uObject is 0 || uObjectKlass is 0 ) {
			return false;
		}
		
		if ( !UClassNodeMap.ContainsKey( uObjectKlass ) ) {
			AffirmUClassCache( uObject, uObjectKlass );
			Logger.Verify( UClassNodeMap.ContainsKey( uObjectKlass ), "Failed to AffirmUClassCache." );
		}

		var uClassNode = UClassNodeMap[ uObjectKlass ];
		while ( uClassNode != null ) {
			if ( !string.IsNullOrEmpty( uClassNode.ClassName ) && uClassNode.ClassName.Equals( klassName ) ) {
				return true;
			}

			uClassNode = uClassNode.Super;
		}

		return false;
	}
	
	
	public static bool IsA( this ulong uObject, string klassName ) {
		var uObjectKlass = Memory.Read< ulong >( uObject + Offsets.UObject_Class, true );
		if ( uObjectKlass is 0 ) {
			return false;
		}

		return IsA( uObject, klassName, uObjectKlass );
	}


	public static string GetUClassName( this ulong uObject ) {
		var uObjectKlass = Memory.Read< ulong >( uObject + Offsets.UObject_Class, true );
		if ( uObjectKlass is 0 ) {
			return null;
		}
		
		if ( !UClassNodeMap.ContainsKey( uObjectKlass ) ) {
			Logger.Warning( $"UClassCache not ready for UObject: 0x{uObject:X8}" );
			return null;
		}

		return UClassNodeMap[ uObjectKlass ].ClassName;
	}
	
	
	private static StringBuilder _actorClassDebugStrBuilder = new ();
	public static string GetActorClassDebugStr( ulong actor, ulong gNamesTable ) {
		_actorClassDebugStrBuilder.Clear();
		var actorKlass = Memory.Read< ulong >( actor + Offsets.UObject_Class );
		while ( actorKlass != 0 ) {
			var actorKlassName = Memory.Read< FName >( actorKlass + Offsets.UObject_Name );
			_actorClassDebugStrBuilder.Append( $"->{UEHelper.GetFNameByComparisonIndex( actorKlassName.ComparisonIndex )}" );
			
			actorKlass = Memory.Read< ulong >( actorKlass + Offsets.UStruct_Super );
		}

		return _actorClassDebugStrBuilder.ToString();
	}


	#region FName Query

	private static readonly Dictionary< int, string > _getNameCacheFromFNameIndex = new ();
	
	public static string GetFNameByComparisonIndex( int id ) {
		if ( _getNameCacheFromFNameIndex.TryGetValue( id, out var found ) ) {
			return found;
		}

		ulong gNamesTable = Driver.GNamesTable;

		string Method01() {
			uint tableLocation = ( uint )id >> 0x10;
			ushort rowLocation = ( ushort )id;
			ulong tableLocationAddr = Memory.Read< ulong >( gNamesTable + 0x10 + tableLocation * 0x8 ) + (ulong)(4 * rowLocation);
			ulong sLength = ( ulong )Memory.Read< ushort >( tableLocationAddr + 0x4 ) >> 1;
			if ( sLength < 128 ) {
				var strData = Memory.ReadTArray< byte >( tableLocationAddr + 0x6, ( int )sLength );
				var str = System.Text.Encoding.ASCII.GetString( strData );
				if ( !string.IsNullOrEmpty( str ) ) {
					_getNameCacheFromFNameIndex.Add( id, str );
				}
				return str;
			}
			return null;
		}
		
		string Method02() {
			var FNameEntryHandle_Block = ( uint )id >> 16; // Block - Sidenote: The casting may not be necessary, arithmetic/logical shifting nonsense.
			// var FNameEntryHandle_Offset = ( ushort )comparisonIndex;
			const uint FNameMaxBlockBits = 13;
			const uint FNameBlockOffsetBits = 16;
			const uint FNameMaxBlocks = ( uint )( 1 << ( int )FNameMaxBlockBits );
			const uint FNameBlockOffsets = ( uint )( 1 << ( int )FNameBlockOffsetBits );
			var fNameEntryHandleOffset = ( ulong )( id & ( FNameBlockOffsets - 1 ) );
			var namePoolChunk = Memory.Read< ulong >( gNamesTable + ( FNameEntryHandle_Block + 0 ) * 0x8 + 0x10 );
			var entryOffset = Memory.Read< ulong >( Memory.Read< ulong >( namePoolChunk ) ) + 4ul * fNameEntryHandleOffset;

			var nameLength64 = Memory.Read< ushort >( entryOffset + 0x4 );
			var nameLength = ( ushort )( nameLength64 >> 1 );
			var strData = Memory.ReadTArray< byte >( entryOffset + 0x6, nameLength );
			if ( strData is null || strData.Length < 1 ) {
				return null;
			}
			var str = System.Text.Encoding.Default.GetString( strData );
			return str;
		}
		
		return Method01();
	}

	#endregion
	
}
