using System;
using Irony.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Irony.Interpreter.Ast;


namespace DigitalRuneScript;

public class MyMoeLanguageRuntime : LanguageRuntime {

	public Func< string, ulong > OnGetProcessModuleBaseAddress;
	public Func< ulong, ulong? > OnDerefPtr;

	public MyMoeLanguageRuntime( LanguageData language ) : base( language ) {}

	public ulong? GetProcessModuleBaseAddress( string moduleName ) {
		return OnGetProcessModuleBaseAddress?.Invoke( moduleName );
	}

	public ulong? DerefPtr( ulong addr ) {
		return OnDerefPtr?.Invoke( addr );
	}
}


[Language( "MemoryOffsetExpression", "0.1", "Memory offset expr format" )]
public class MyMemoryOffsetExpressionGrammar : Grammar {
	
	public MyMemoryOffsetExpressionGrammar() {
		
		LanguageFlags |= LanguageFlags.CreateAst;

		CommentTerminal SingleLineComment = new CommentTerminal( "SingleLineComment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029" );
		NonGrammarTerminals.Add( SingleLineComment );
		
		// Terminals
		var moe_number = new NumberLiteral( "number_offset", NumberOptions.None, typeof( MoeNumberExpressionNode ) );
		moe_number.Options = NumberOptions.IntOnly;
		moe_number.DefaultIntTypes = [ TypeCode.UInt64 ];
		moe_number.AddPrefix( "0x", NumberOptions.Hex );
		// var moe_module_identifier = TerminalFactory.CreateCSharpIdentifier( "module_identifier" );
		IdentifierTerminal moe_module_identifier = new IdentifierTerminal("module_identifier", ".-" );
		moe_module_identifier.AstConfig.NodeType = typeof( MoeModuleExpressionNode );
		
		// Nonterminals
		var literal = new NonTerminal( "literal", typeof( LiteralNode ) );
		// var unary_operator = new NonTerminal( "unary_operator" );
		var unary_expression = new NonTerminal( "unary_expression", typeof( UnaryExpressionNode ) );
		var deref_expression = new NonTerminal( "deref_expression", typeof( DerefExpressionNode ) );
		var offset_expression = new NonTerminal( "offset_expression", typeof( OffsetExpressionNode ) );

		RegisterOperators( 1, "+" );
		
		// Rules
		literal.Rule = moe_number | moe_module_identifier;
		// unary_operator.Rule = ToTerm("+") | "-";
		unary_expression.Rule = offset_expression + ToTerm( "+" ) + offset_expression;
		deref_expression.Rule = "[" + offset_expression + "]";
		offset_expression.Rule =
			literal
			| unary_expression
			| deref_expression
			| Empty;
		
		// Set grammar root
		Root = offset_expression;
	}
	
}


public class OffsetExpressionNode : AstNode {
	
	public override void Init( AstContext context, ParseTreeNode treeNode ) {
		base.Init( context, treeNode );

		foreach ( var node in treeNode.ChildNodes ) {
			switch ( node.Term.Name ) {
				case "literal":
				case "unary_expression":
				case "deref_expression":
					AddChild( NodeUseType.Parameter, "param", node );
					break;
			}
		}
	}

	protected override object DoEvaluate( ScriptThread thread ) {
		if ( ChildNodes.Count > 0 ) {
			return ChildNodes[ 0 ].Evaluate( thread );
		}

		return null;
	}
}


public class DerefExpressionNode : AstNode {
	
	public override void Init( AstContext context, ParseTreeNode treeNode ) {
		base.Init( context, treeNode );

		if ( treeNode.ChildNodes.Count > 0 ) {
			AddChild( NodeUseType.Parameter, "param", treeNode.ChildNodes[ 1 ] );
		}
	}

	protected override object DoEvaluate( ScriptThread thread ) {
		if ( thread.Runtime is MyMoeLanguageRuntime moeLanguageRuntime ) {
			var target = ( ulong? )ChildNodes[ 0 ].Evaluate( thread );
			return target.HasValue ? moeLanguageRuntime.DerefPtr( target.Value ) : null;
		}
		
		return null;
	}
}


public class UnaryExpressionNode : AstNode {
	
	public override void Init( AstContext context, ParseTreeNode treeNode ) {
		base.Init( context, treeNode );
		
		if ( treeNode.ChildNodes.Count > 0 ) {
			AddChild( NodeUseType.Parameter, "param1", treeNode.ChildNodes[ 0 ] );
			AddChild( NodeUseType.Parameter, "param2", treeNode.ChildNodes[ 2 ] );
		}
	}

	protected override object DoEvaluate( ScriptThread thread ) {
		var p0 = ( ulong? )ChildNodes[ 0 ].Evaluate( thread );
		var p1 = ( ulong? )ChildNodes[ 1 ].Evaluate( thread );
		if ( p0.HasValue && p1.HasValue ) {
			return p0.Value + p1.Value;
		}
		
		return null;
	}
}


public class LiteralNode : AstNode {
	
	public override void Init( AstContext context, ParseTreeNode treeNode ) {
		base.Init( context, treeNode );
		
		AddChild( NodeUseType.Parameter, "param", treeNode.ChildNodes[ 0 ] );
	}

	protected override object DoEvaluate( ScriptThread thread ) {
		return ChildNodes[ 0 ].Evaluate( thread );
	}
}


public class MoeNumberExpressionNode : AstNode {

	public ulong Value { get; private set; }

	public override void Init( AstContext context, ParseTreeNode treeNode ) {
		base.Init( context, treeNode );

		Value = ( ulong )treeNode.Token.Value;
		// Logger.Debug( $"treeNodeTokenValue = {treeNode.Token.Value} -> {Value}" );
	}

	public override bool IsConstant() => true;

	protected override object DoEvaluate( ScriptThread thread ) {
		return Value;
	}
	
}


public class MoeModuleExpressionNode : AstNode {
	
	public string ModuleName { get; private set; }

	public override void Init( AstContext context, ParseTreeNode treeNode ) {
		base.Init( context, treeNode );

		ModuleName = ( string )treeNode.Token.Value;
	}
	
	public override bool IsConstant() => true;

	protected override object DoEvaluate( ScriptThread thread ) {
		if ( thread.Runtime is MyMoeLanguageRuntime moeLanguageRuntime ) {
			return moeLanguageRuntime.GetProcessModuleBaseAddress( ModuleName );
		}
		
		return null;
	}
}
