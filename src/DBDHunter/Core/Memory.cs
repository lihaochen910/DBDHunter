using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using vmmsharp;


namespace DBDHunter; 

internal static class Memory {

    public static T Read< T >( ulong address, bool logError = false ) where T : struct {
        int size = Marshal.SizeOf< T >();
        byte[] buffer = new byte[size];
        unsafe {
            fixed ( byte* pBuffer = buffer ) {
                if ( Driver.vmm.MemRead( Driver.ProcessPid, address, ( uint )size, ( nint )pBuffer ) == size ) {
                    return ByteArrayToStructure< T >( buffer );
                }
                else {
                    if ( logError ) {
                        Logger.Error( $"Failed to read {typeof( T ).Name} from memory!" );

                        StackTrace trace = new(true);

                        const int traceCount = 7;
                        int _traceCounter = traceCount;
                        StackFrame[] frames = trace.GetFrames();

                        StringBuilder stack = new();
                        for ( var i = 1; i < frames.Length && i < traceCount + 1; i++ ) {
                            stack.AppendLine(
                                $"\t{frames[ i ].GetMethod()?.DeclaringType?.FullName ?? string.Empty}.{frames[ i ].GetMethod()?.Name ?? "Unknown"}:line {frames[ i ].GetFileLineNumber()}" );

                            if ( _traceCounter-- <= 0 ) {
                                break;
                            }
                        }

                        Logger.Debug( $"At:\n{stack}" );
                    }
                    
                    return default;
                }
            }
        }
    }


    public static T Read< T >( ulong address, VmmScatter scatterHandle, bool logError = false ) where T : struct {
        int size = Marshal.SizeOf< T >();
        byte[] buffer = scatterHandle.Read( address, ( uint )size );
        if ( buffer != null && buffer.Length == size ) {
            return ByteArrayToStructure< T >( buffer );
        }
        else {
            if ( logError ) {
                Logger.Error( $"Failed to read {typeof( T ).Name} from memory!" );

                StackTrace trace = new(true);

                const int traceCount = 7;
                int _traceCounter = traceCount;
                StackFrame[] frames = trace.GetFrames();

                StringBuilder stack = new();
                for ( var i = 1; i < frames.Length && i < traceCount + 1; i++ ) {
                    stack.AppendLine(
                        $"\t{frames[ i ].GetMethod()?.DeclaringType?.FullName ?? string.Empty}.{frames[ i ].GetMethod()?.Name ?? "Unknown"}:line {frames[ i ].GetFileLineNumber()}" );

                    if ( _traceCounter-- <= 0 ) {
                        break;
                    }
                }

                Logger.Debug( $"At:\n{stack}" );
            }
                    
            return default;
        }
        
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read< T >( this VmmScatter scatterHandle, ulong address, bool logError = false ) where T : struct => Read< T >( address, scatterHandle, logError );

    
    public static T[] ReadTArray< T >( ulong address, int length ) where T : struct {
        int size = Marshal.SizeOf< T >() * length;
        T[] buffer = new T[length];
        unsafe {
            fixed ( T* pBuffer = buffer ) {
                var readedCount = Driver.vmm.MemRead( Driver.ProcessPid, address, ( uint )size, ( nint )pBuffer );
                if ( readedCount == size ) {
                    return buffer;
                }
                else {
                    if ( readedCount is 0 ) {
                        return default;
                    }
                    
                    Logger.Error( $"Failed to read {typeof( T ).Name}[] from memory! read: {readedCount} but need: {size}" );

                    StackTrace trace = new(true);

                    const int traceCount = 7;
                    int _traceCounter = traceCount;
                    StackFrame[] frames = trace.GetFrames();

                    StringBuilder stack = new();
                    for ( var i = 1; i < frames.Length && i < traceCount + 1; i++ ) {
                        stack.AppendLine(
                            $"\t{frames[ i ].GetMethod()?.DeclaringType?.FullName ?? string.Empty}.{frames[ i ].GetMethod()?.Name ?? "Unknown"}:line {frames[ i ].GetFileLineNumber()}" );

                        if ( _traceCounter-- <= 0 ) {
                            break;
                        }
                    }

                    Logger.Debug( $"At:\n{stack}" );

                    // throw new InvalidOperationException($"Failed to read {typeof(T).Name} from memory.");
                    return default;
                }
            }
        }
    }


    public static T[] ReadTArray< T >( ulong address, VmmScatter scatterHandle, int length ) where T : struct {
        int size = Marshal.SizeOf< T >();
        T[] result = new T[length];

        for ( var i = 0; i < length; i++ ) {
            byte[] buffer = scatterHandle.Read( address + ( ulong )( i * size ), ( uint )size );
            if ( buffer != null && buffer.Length == size ) {
                result[ i ] = ByteArrayToStructure< T >( buffer );
            }
            else {
                Logger.Error( $"Failed to read {typeof( T ).Name}[] from memory!" );

                StackTrace trace = new(true);

                const int traceCount = 7;
                int _traceCounter = traceCount;
                StackFrame[] frames = trace.GetFrames();

                StringBuilder stack = new();
                for ( var j = 1; j < frames.Length && j < traceCount + 1; j++ ) {
                    stack.AppendLine(
                        $"\t{frames[ j ].GetMethod()?.DeclaringType?.FullName ?? string.Empty}.{frames[ j ].GetMethod()?.Name ?? "Unknown"}:line {frames[ j ].GetFileLineNumber()}" );

                    if ( _traceCounter-- <= 0 ) {
                        break;
                    }
                }

                Logger.Debug( $"At:\n{stack}" );

                result[ i ] = default;
            }
        }

        return result;
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ReadTArray< T >( this VmmScatter scatterHandle, ulong address, int length ) where T : struct => ReadTArray< T >( address, scatterHandle, length );

    
    public static T[] ReadUETArray< T >( ulong address ) where T : struct {
        var arraySize = Read< int >( address + Offsets.TArray_NumElements );
        if ( arraySize is 0 ) {
            return [];
        }
        
        if ( arraySize is > 0 and < 0xFFFF ) {
            var arrayAddr = Read< ulong >( address + Offsets.TArray_Data );
            if ( arrayAddr != 0 ) {
                return ReadTArray< T >( arrayAddr, arraySize );
            }
        }

        Logger.Error( $"Failed to read UETArray<{typeof( T ).Name}> from memory! size: {arraySize}" );
        return null;
    }


    public static string ReadFString( ulong address ) {

        var size = Read< int >( address + 0x8, false );
        if ( size is <= 0 or > 0xFFF ) {
            return null;
        }

        var byteSize = size * sizeof( ushort );
        byte[] buffer = new byte[byteSize];
        unsafe
        {
            fixed (byte* pBuffer = buffer) {
                var readedCount = Driver.vmm.MemRead( Driver.ProcessPid, Read< ulong >( address, true ), ( uint )byteSize, ( nint )pBuffer );
                if ( readedCount == byteSize)
                {
                    return System.Text.Encoding.Unicode.GetString( buffer );
                }
                else
                {
                    if ( readedCount is 0 ) {
                        return null;
                    }
                    
                    Logger.Error($"Failed to read FString from memory!");
                        
                    StackTrace trace = new(true);

                    const int traceCount = 7;
                    int _traceCounter = traceCount;
                    StackFrame[] frames = trace.GetFrames();

                    StringBuilder stack = new();
                    for (var i = 1; i < frames.Length && i < traceCount + 1; i++)
                    {
                        stack.AppendLine($"\t{frames[i].GetMethod()?.DeclaringType?.FullName ?? string.Empty}.{frames[i].GetMethod()?.Name ?? "Unknown"}:line {frames[i].GetFileLineNumber()}");

                        if (_traceCounter-- <= 0)
                        {
                            break;
                        }
                    }
                        
                    Logger.Debug($"At:\n{stack}");
                    return default;
                }
            }
        }
    }


    public static bool Write< T >( ulong address, T data ) where T : struct {
        var buffer = StructureToByteArray( data );
        unsafe {
            fixed ( byte* pBuffer = buffer ) {
                var result = Driver.vmm.MemWrite( Driver.ProcessPid, address, ( uint )buffer.Length, ( nint )pBuffer );
                if ( result ) {
                    Logger.Debug( $"write: {data} at: 0x{address:X8} size: {buffer.Length} result: success!" );
                    return true;
                }

                Logger.Error( $"write: {data} at: 0x{address:X8} size: {buffer.Length} result: failed!" );
                return false;
            }
        }
    }


    public static byte[] StructureToByteArray< T >( T structure ) where T : struct {
        int size = Marshal.SizeOf< T >();
        byte[] array = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal( size );
        try {
            Marshal.StructureToPtr( structure, ptr, false );
            Marshal.Copy( ptr, array, 0, size );
        }
        finally {
            Marshal.FreeHGlobal( ptr );
        }

        return array;
    }


    public static T ByteArrayToStructure< T >( byte[] byteArray ) where T : struct {
        T structure;
        int size = Marshal.SizeOf< T >();
        if ( byteArray is null || byteArray.Length < 1 /* || byteArray.Length != size */ ) {
            return default;
        }

        IntPtr ptr = Marshal.AllocHGlobal( size );
        try {
            Marshal.Copy( byteArray, 0, ptr, size );
            structure = Marshal.PtrToStructure< T >( ptr );
        }
        finally {
            Marshal.FreeHGlobal( ptr );
        }

        return structure;
    }


    private static StringBuilder _printByteArrayBuilder = new ();
    public static void PrintByteArray( byte[] bytes, ulong addressHint = 0x0, int bytePerRow = 0x10 ) {
        var counter = 0;
        var byteOffset = 0ul;
        var ansiView = string.Empty;
        _printByteArrayBuilder.AppendLine();
        _printByteArrayBuilder.Append( $"0x{( addressHint + byteOffset ):X8} | " );
        foreach ( var b in bytes ) {
            _printByteArrayBuilder.Append( $"{b:X2} " );
            int c = b;
            ansiView += c is >= 32 and < 128 ? Convert.ToChar( c ) : '.';
            
            byteOffset++;
            
            if ( ++counter >= bytePerRow ) {
                _printByteArrayBuilder.Append( $" | {ansiView}\n" );
                _printByteArrayBuilder.Append( $"0x{( addressHint + byteOffset ):X8} | " );
                ansiView = string.Empty;
                counter = 0;
            }
        }
        
        Logger.Debug( _printByteArrayBuilder.ToString() );
        _printByteArrayBuilder.Clear();
    }


    public static void PrintStructureByteArray< T >( T structure ) where T : struct {
        PrintByteArray( StructureToByteArray( structure ) );
    }


    public static void PeekAddressBytes( ulong addr, int size = 0xff, ulong addressHint = 0x0 ) {
        var bytes = ReadTArray< byte >( addr, size );
        if ( bytes != null && bytes.Length > 0 ) {
            PrintByteArray( bytes, addressHint );
        }
        else {
            Logger.Error( $"Failed peek: {addr}." );
        }
    }

    
    private static bool IsNum( char c ) => ( uint )( c - '0' ) <= 9;
    private static bool IsHexUpper( char c ) => ( uint )( c - 'A' ) <= 5;
    private static bool IsHexLower( char c ) => ( uint )( c - 'a' ) <= 5;
    
    /// <summary>
    /// Parses the hex string into a <see cref="uint"/>, skipping all characters except for valid digits.
    /// </summary>
    /// <param name="value">Hex String to parse</param>
    /// <returns>Parsed value</returns>
    public static uint GetHexValue( string value ) {
        uint result = 0;
        if ( string.IsNullOrEmpty( value ) )
            return result;

        if ( value.StartsWith( "0x" ) ) {
            value = value.TrimStart( '0', 'x' );
        }

        foreach ( var c in value ) {
            if ( IsNum( c ) ) {
                result <<= 4;
                result += ( uint )( c - '0' );
            }
            else if ( IsHexUpper( c ) ) {
                result <<= 4;
                result += ( uint )( c - 'A' + 10 );
            }
            else if ( IsHexLower( c ) ) {
                result <<= 4;
                result += ( uint )( c - 'a' + 10 );
            }
        }

        return result;
    }


    private static readonly Regex sWhitespace = new ( @"\s+" );

    public static string ReplaceWhitespace( string input, string replacement ) {
        return sWhitespace.Replace( input, replacement );
    }


    public static byte[] StringToByteArrayFastest( string hex ) {
        // if ( hex.StartsWith( "0x" ) ) {
        //     hex = hex.TrimStart( '0', 'x' );
        // }

        hex = ReplaceWhitespace( hex, string.Empty );

        // if ( hex.Length % 2 == 1 ) {
        //     Logger.Error( "The binary key cannot have an odd number of digits" );
        //     return null;
        // }

        byte[] arr = new byte[ hex.Length >> 1 ];
        for ( var i = 0; i < hex.Length >> 1; ++i ) {
            arr[ i ] = ( byte )( ( GetHexVal( hex[ i << 1 ] ) << 4 ) + ( GetHexVal( hex[ ( i << 1 ) + 1 ] ) ) );
        }

        return arr;
    }

    private static int GetHexVal( char hex ) {
        int val = ( int )hex;

        //For uppercase A-F letters:
        //return val - (val < 58 ? 48 : 55);
        //For lowercase a-f letters:
        //return val - (val < 58 ? 48 : 87);
        //Or the two combined, but a bit slower:
        return val - ( val < 58 ? 48 : ( val < 97 ? 55 : 87 ) );
    }


    public static T ByteArrayStrToStructure< T >( string byteArrayStr ) where T : struct {
        return ByteArrayToStructure< T >( StringToByteArrayFastest( byteArrayStr ) );
    }
}
