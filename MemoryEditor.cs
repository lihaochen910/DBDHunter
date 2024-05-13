using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using DigitalRune.Graphics;
using DigitalRune.Windows.Framework;
using ImGuiNET;


namespace DigitalRuneScript.EditorViews;

public class MemoryEditor : ImGuiWindowBase, IEditorWorkspaceDockerChild {
    
    private bool _allowEdits;
    private int _rows;
    private long _dataEditingAddr;
    private bool _dataEditingTakeFocus;
    private byte[] _dataInput = new byte[ 32 ];
    private byte[] _addrInput = new byte[ 32 ];

    public bool DataChanged => _pendingChanges.Count > 0;

    public event Action< IDictionary< long, byte > > ApplyChanges;
    
    private byte[] _memData;
    private int _memSize;
    private long _baseDisplayAddr;

    private readonly Dictionary< int, Dictionary< int, string > > _cachedHexNumber = new ();
    private readonly Dictionary< long, byte > _pendingChanges = new ();
    private readonly Dictionary< long, byte > _changesRec = new ();

    private readonly StringBuilder _stringBufferAsciiVal;
    private readonly ImGuiInputTextFlags _inputTextFlags;

    public MemoryEditor() {
        DisplayName = nameof( MemoryEditor );
        _windowFlags |= ImGuiWindowFlags.NoScrollbar;
        
        _rows = 16;
        _dataEditingAddr = -1;
        _dataEditingTakeFocus = false;
        _allowEdits = true;
        _baseDisplayAddr = 0;

        _cachedHexNumber.Add( 1, new Dictionary< int , string >( byte.MaxValue ) );
        _cachedHexNumber.Add( 2, new Dictionary< int , string >( byte.MaxValue ) );
        _stringBufferAsciiVal = new StringBuilder( 0xFFF );
        _inputTextFlags = ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.EnterReturnsTrue |
                          ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.NoHorizontalScroll |
                          ImGuiInputTextFlags.CallbackAlways;
    }


    public void ResetBaseDisplayAddr() {
        _baseDisplayAddr = 0;
    }
    
    
    public void SetMemoryData( byte[] memData, int memSize, long baseDisplayAddr = 0 ) {
        _memData = memData;
        _memSize = memSize;
        _baseDisplayAddr = baseDisplayAddr;
        
        _pendingChanges.Clear();
        _changesRec.Clear();
        
        var addrDigitsCount = 0;
        for ( var n = _baseDisplayAddr + _memSize - 1; n > 0; n >>= 4 )
            addrDigitsCount++;

        if ( !_cachedHexNumber.ContainsKey( addrDigitsCount ) ) {
            _cachedHexNumber.Add( addrDigitsCount, new Dictionary< int, string >( byte.MaxValue ) );
        }
    }

    public unsafe override void OnImGuiDraw( RenderContext context ) {
        ImGui.SetNextWindowSize( new Vector2( 500, 350 ), ImGuiCond.FirstUseEver );

        if ( _memData is null ) {
            return;
        }
        
        var lineHeight = ImGuiNative.igGetTextLineHeight();
        var lineTotalCount = ( _memSize + _rows - 1 ) / _rows;

        ImGuiNative.igSetNextWindowContentSize( new Vector2( 0.0f, lineTotalCount * lineHeight ) );
        ImGui.BeginChild( "##scrolling", new Vector2( 0, -ImGuiNative.igGetFrameHeightWithSpacing() ), ImGuiChildFlags.None, 0 );

        ImGui.PushStyleVar( ImGuiStyleVar.FramePadding, new Vector2( 0, 0 ) );
        ImGui.PushStyleVar( ImGuiStyleVar.ItemSpacing, new Vector2( 0, 0 ) );

        var addrDigitsCount = 0;
        for ( var n = _baseDisplayAddr + _memSize - 1; n > 0; n >>= 4 ) {
            addrDigitsCount++;
        }

        var glyphWidth = ImGui.CalcTextSize( "F" ).X;
        var cellWidth = glyphWidth * 3; // "FF " we include trailing space in the width to easily catch clicks everywhere

        var dataNext = false;
        
        ImGuiListClipperPtr listClipper = new ImGuiListClipperPtr( ImGuiNative.ImGuiListClipper_ImGuiListClipper() );
        listClipper.Begin( lineTotalCount, lineHeight );

        while ( listClipper.Step() ) {
            
            var visibleStartAddr = listClipper.DisplayStart * _rows;
            var visibleEndAddr = listClipper.DisplayEnd * _rows;

            if ( !_allowEdits || _dataEditingAddr >= _memSize ) {
                _dataEditingAddr = -1;
            }

            var dataEditingAddrBackup = _dataEditingAddr;

            if ( _dataEditingAddr != -1 ) {
                if ( ImGui.IsKeyPressed( ImGui.GetKeyIndex( ImGuiKey.UpArrow ) ) && _dataEditingAddr >= _rows ) {
                    _dataEditingAddr -= _rows;
                    _dataEditingTakeFocus = true;
                }
                else if ( ImGui.IsKeyPressed( ImGui.GetKeyIndex( ImGuiKey.DownArrow ) ) && _dataEditingAddr < _memSize - _rows ) {
                    _dataEditingAddr += _rows;
                    _dataEditingTakeFocus = true;
                }
                else if ( ImGui.IsKeyPressed( ImGui.GetKeyIndex( ImGuiKey.LeftArrow ) ) && _dataEditingAddr > 0 ) {
                    _dataEditingAddr -= 1;
                    _dataEditingTakeFocus = true;
                }
                else if ( ImGui.IsKeyPressed( ImGui.GetKeyIndex( ImGuiKey.RightArrow ) ) && _dataEditingAddr < _memSize - 1 ) {
                    _dataEditingAddr += 1;
                    _dataEditingTakeFocus = true;
                }
            }

            if ( _dataEditingAddr / _rows != dataEditingAddrBackup / _rows ) {
                // Track cursor movements
                float scrollOffset = ( _dataEditingAddr / _rows - dataEditingAddrBackup / _rows ) * lineHeight;
                bool scrollDesired = ( scrollOffset < 0.0f && _dataEditingAddr < visibleStartAddr + _rows * 2 ) ||
                                      ( scrollOffset > 0.0f && _dataEditingAddr > visibleEndAddr - _rows * 2 );
                if ( scrollDesired )
                    ImGuiNative.igSetScrollY_Float( ImGuiNative.igGetScrollY() + scrollOffset );
            }

            for ( long lineI = listClipper.DisplayStart < 0 ? 0 : listClipper.DisplayStart; lineI < listClipper.DisplayEnd; lineI++ ) { // display only visible items

                var addr = lineI * _rows;
                ImGui.Text( $"{FixedHex( _baseDisplayAddr + addr, addrDigitsCount )}:\t" );
                ImGui.SameLine();

                // Draw Hexadecimal
                float lineStartX = ImGuiNative.igGetCursorPosX();

                for ( var n = 0; n < _rows && addr < _memSize; n++, addr++ ) {
                    ImGui.SameLine( lineStartX + cellWidth * n );

                    if ( _dataEditingAddr == addr ) {
                        // Display text input on current byte
                        ImGui.PushID( ( int )addr );

                        // FIXME: We should have a way to retrieve the text edit cursor position more easily in the API, this is rather tedious.
                        int Callback( ImGuiInputTextCallbackData* data ) {
                            int* pCursorPos = ( int* )data->UserData;
                            if ( ImGuiNative.ImGuiInputTextCallbackData_HasSelection( data ) == 0 ) {
                                *pCursorPos = data->CursorPos;
                            }
                            return 0;
                        }

                        var cursorPos = -1;
                        var dataWrite = false;

                        if ( _dataEditingTakeFocus ) {
                            ImGui.SetKeyboardFocusHere();
                            ReplaceChars( _dataInput, FixedHex( _memData[ addr ], 2 ) );
                            ReplaceChars( _addrInput, FixedHex( _baseDisplayAddr + addr, addrDigitsCount ) );
                        }

                        ImGui.PushItemWidth( ImGui.CalcTextSize( "FF" ).X );
                        
                        if ( ImGui.InputText( "##data", _dataInput, ( uint )_dataInput.Length, _inputTextFlags, Callback, ( IntPtr )( &cursorPos ) ) ) {
                            dataWrite = dataNext = true;
                        }
                        if ( !_dataEditingTakeFocus && !ImGui.IsItemActive() ) {
                            _dataEditingAddr = -1;
                        }

                        _dataEditingTakeFocus = false;
                        ImGui.PopItemWidth();

                        if ( cursorPos >= 2 ) {
                            dataWrite = dataNext = true;
                        }

                        if ( dataWrite ) {
                            if ( TryHexParse( _dataInput, out var data ) ) {
                                _changesRec[ _baseDisplayAddr + addr ] = _memData[ addr ];
                                _memData[ addr ] = ( byte )data;
                                _pendingChanges[ _baseDisplayAddr + addr ] = ( byte )data;
                            }
                        }

                        ImGui.PopID();
                    }
                    else {
                        bool isChangedAddr = _pendingChanges.ContainsKey( _baseDisplayAddr + addr );
                        if ( isChangedAddr ) {
                            ImGui.PushStyleColor( ImGuiCol.Text, Microsoft.Xna.Framework.Color.Red.PackedValue );
                        }
                        ImGui.Text( FixedHex( _memData[ addr ], 2 ) );
                        if ( isChangedAddr ) {
                            ImGui.PopStyleColor();
                        }

                        if ( _allowEdits && ImGui.IsItemHovered() && ImGui.IsMouseClicked( 0 ) ) {
                            _dataEditingTakeFocus = true;
                            _dataEditingAddr = addr;
                        }
                    }
                }

                ImGui.SameLine( lineStartX + cellWidth * _rows + glyphWidth * 2 );

                //separator line drawing replaced by printing a pipe char

                // Draw ASCII values
                addr = lineI * _rows;
                var asciiVal = _stringBufferAsciiVal;
                asciiVal.Clear();
                asciiVal.Append( "| " );

                for ( int n = 0; n < _rows && addr < _memSize; n++, addr++ ) {
                    int c = _memData[ addr ];
                    asciiVal.Append( c is >= 32 and < 128 ? Convert.ToChar( c ) : ' ' );
                }

                ImGui.TextUnformatted( asciiVal.ToString() ); //use unformatted, so string can contain the '%' character
            }

        }
        
        listClipper.End();
        ImGuiNative.ImGuiListClipper_destroy( listClipper );
        //clipper.End();  //not implemented
        ImGui.PopStyleVar( 2 );

        ImGui.EndChild();

        if ( dataNext && _dataEditingAddr < _memSize ) {
            _dataEditingAddr += 1;
            _dataEditingTakeFocus = true;
        }

        ImGui.Separator();

        ImGuiNative.igAlignTextToFramePadding();
        ImGui.PushItemWidth( 50 );
        ImGui.PushTabStop( true );
        int rowsBackup = _rows;

        if ( ImGui.DragInt( "##rows", ref _rows, 0.2f, 4, 32, "%.0f rows" ) ) {
            if ( _rows <= 0 ) {
                _rows = 4;
            }
            Vector2 newWindowSize = ImGui.GetWindowSize();
            newWindowSize.X += ( _rows - rowsBackup ) * ( cellWidth + glyphWidth );
            ImGui.SetWindowSize( newWindowSize );
        }

        ImGui.PopTabStop();
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.Text( $" Range {FixedHex( _baseDisplayAddr, addrDigitsCount )} - {FixedHex( _baseDisplayAddr + _memSize - 1, addrDigitsCount )} " );
        ImGui.SameLine();
        ImGui.PushItemWidth( ImGui.CalcTextSize( "FFFFFFFFFFFF" ).X );

        if ( ImGui.InputText( "##addr", _addrInput, 32, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.EnterReturnsTrue, null ) ) {
            if ( TryHexParse( _addrInput, out var gotoAddr ) ) {
                gotoAddr -= _baseDisplayAddr;

                if ( gotoAddr >= 0 && gotoAddr < _memSize ) {
                    ImGui.BeginChild( "##scrolling" );
                    ImGui.SetScrollFromPosY( ImGui.GetCursorStartPos().Y + gotoAddr / _rows * ImGuiNative.igGetTextLineHeight() );
                    ImGui.EndChild();
                    _dataEditingAddr = gotoAddr;
                    _dataEditingTakeFocus = true;
                }
            }
        }

        ImGui.PopItemWidth();

        if ( DataChanged ) {
            ImGui.SameLine();
            if ( ImGui.Button( "Apply" ) ) {
                ImGui.OpenPopup( "apply_change" );
            }
            ImGui.SameLine();
            if ( ImGui.Button( "Discard" ) ) {

                foreach ( var addr in _changesRec.Keys ) {
                    _memData[ addr ] = _changesRec[ _baseDisplayAddr + addr ];
                }
                
                _pendingChanges.Clear();
                _changesRec.Clear();
            }
        }

        if ( ImGui.BeginPopup( "apply_change", ImGuiWindowFlags.NoMove ) ) {
            
            ImGui.Text( "Do you want to apply changes?" );

            int count = 1;
            foreach ( var kv in _pendingChanges ) {
                if ( count > 5 )
                    break;
                ImGui.Indent( 8 );
                ImGui.Text( $"0x{kv.Key:X12} - {kv.Value:X2}" );
                ImGui.Unindent( 8 );
                count++;
            }
            
            if ( ImGui.Button( "Confirm" ) ) {
                ApplyChanges?.Invoke( _pendingChanges );
                _pendingChanges.Clear();
                _changesRec.Clear();
                ImGui.CloseCurrentPopup();
            }
            
            if ( ImGui.Button( "Cancel" ) ) {
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.EndPopup();
        }
        
    }
    
    
    private string FixedHex( int v, int count ) {
        if ( !_cachedHexNumber[ count ].ContainsKey( v ) ) {
            _cachedHexNumber[ count ].Add( v, v.ToString( "X" ).PadLeft( count, '0' ) );
        }
        return _cachedHexNumber[ count ][ v ];
    }

    private static string FixedHex( long v, int count ) {
        return v.ToString( "X" ).PadLeft( count, '0' );
    }

    private static bool TryHexParse( byte[] bytes, out long result ) {
        string input = Encoding.UTF8.GetString( bytes ).ToString();
        return long.TryParse( input, NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out result );
    }

    private static void ReplaceChars( byte[] bytes, string input ) {
        var address = Encoding.ASCII.GetBytes( input );

        for ( var i = 0; i < bytes.Length; i++ ) {
            bytes[ i ] = i < address.Length ? address[ i ] : ( byte ) 0;
        }
    }
    
}
