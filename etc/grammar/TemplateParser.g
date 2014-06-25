/*

FastTemplate

The MIT License (MIT)

Copyright (c) 2014 Jeff Panici

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

parser grammar TemplateParser;

options {
    language=CSharp3;
	tokenVocab=TemplateLexer;
	TokenLabelType=CommonToken;
    output=AST;
    ASTLabelType=CommonTree;
    backtrack=false;
    k=1;
}

tokens {
    Block;
    Statement;
    Nested;
    FnArgs;
    Constant;
    Control;
    DefaultBlock;
    ConstDict;
    ConstList;
    Unary;
    Document;
    Empty;
    LoopArgs;
    TargetedBlock;
    Invoke;
    Error;
    Indexer;
    Passthrough;
}

@namespace { PaniciSoftware.FastTemplate.Common }

@header{
    // The variable 'name' is declared but never used
    #pragma warning disable 168
        }

@members {
    public ErrorList Errors { get; set; }

    public override void DisplayRecognitionError(string[] tokenNames, RecognitionException e)
    {
        var header = GetErrorHeader(e);
        var message = GetErrorMessage(e, tokenNames);
        Errors.ErrorRecognition( header, message );
        this.EmitErrorMessage( header + " " + message );
    }

    public override string GetErrorMessage(RecognitionException e, string[] tokenNames)
    {
      string str1 = e.Message;
      if (e is UnwantedTokenException)
      {
        UnwantedTokenException unwantedTokenException = (UnwantedTokenException) e;
        string str2 = unwantedTokenException.Expecting != -1 ? tokenNames[unwantedTokenException.Expecting] : "EndOfFile";
        str1 = "extraneous input " + this.GetTokenErrorDisplay(unwantedTokenException.UnexpectedToken) + " expecting " + str2;
      }
      else if (e is MissingTokenException)
      {
        MissingTokenException missingTokenException = (MissingTokenException) e;
        str1 = "missing " + (missingTokenException.Expecting != -1 ? tokenNames[missingTokenException.Expecting] : "EndOfFile") + " at " + this.GetTokenErrorDisplay(e.Token);
      }
      else if (e is MismatchedTokenException)
      {
        MismatchedTokenException mismatchedTokenException = (MismatchedTokenException) e;
        string str2 = mismatchedTokenException.Expecting != -1 ? tokenNames[mismatchedTokenException.Expecting] : "EndOfFile";
        str1 = "mismatched input " + this.GetTokenErrorDisplay(e.Token) + " expecting " + str2;
      }
      else if (e is MismatchedTreeNodeException)
      {
        MismatchedTreeNodeException treeNodeException = (MismatchedTreeNodeException) e;
        string str2 = treeNodeException.Expecting != -1 ? tokenNames[treeNodeException.Expecting] : "EndOfFile";
        str1 = "mismatched tree node: " + (treeNodeException.Node != null ? treeNodeException.Node.ToString() ?? string.Empty : string.Empty) + " expecting " + str2;
      }
      else if (e is NoViableAltException)
        str1 = "no viable alternative at input " + this.GetTokenErrorDisplay(e.Token);
      else if (e is EarlyExitException)
        str1 = "required (...)+ loop did not match anything at input " + this.GetTokenErrorDisplay(e.Token);
      else if (e is MismatchedSetException)
      {
        MismatchedSetException mismatchedSetException = (MismatchedSetException) e;
        str1 = string.Concat(new object[4]
        {
          (object) "mismatched input ",
          (object) this.GetTokenErrorDisplay(e.Token),
          (object) " expecting set ",
          (object) mismatchedSetException.Expecting
        });
      }
      else if (e is MismatchedNotSetException)
      {
        MismatchedNotSetException mismatchedNotSetException = (MismatchedNotSetException) e;
        str1 = string.Concat(new object[4]
        {
          (object) "mismatched input ",
          (object) this.GetTokenErrorDisplay(e.Token),
          (object) " expecting set ",
          (object) mismatchedNotSetException.Expecting
        });
      }
      else if (e is FailedPredicateException)
      {
        FailedPredicateException predicateException = (FailedPredicateException) e;
        str1 = "rule " + predicateException.RuleName + " failed predicate: {" + predicateException.PredicateText + "}?";
      }
      return str1;
    }
}

public
document: section* EOF -> ^( Document section* );

block: section* -> ^( Block section* );

section
    : statement -> ^( Statement statement )
    | passthrough -> ^( Passthrough passthrough )
    | Literal -> Literal
    | Unparsed -> Unparsed
    | control -> ^( Control control )
    ;

statement
    : EStart ( LBrace Root sSection* RBrace | Root sSection* ) -> ^( EStart Root sSection* )
    | MStart ( LBrace Root sSection* RBrace | Root sSection* ) -> ^( MStart Root sSection* )
    ;

passthrough
    : EPass argument_expression_list? RP -> ^( EPass argument_expression_list? )
    | MPass argument_expression_list? RP -> ^( MPass argument_expression_list? )
    ;

sSection
    : Prop -> Prop
    | LP argument_expression_list? RP -> ^( Invoke argument_expression_list? )
    | LBracket argument_expression_list? RBracket -> ^( Indexer argument_expression_list? )
    ;

control
    : loop
    | assert
    | pragma
    | foreach
    | set
    | include
    | parse
    | if
    | Stop
    | Break
    | Continue
    ;

constant
    : StringLiteral
    | SignedLong
    | Integer
    | UnsignedInteger
    | Double
    | Decimal
    | Hex
    | True
    | False
    | Null
    | list -> ^( ConstList list )
    | dict -> ^( ConstDict dict )  
    ;

tuple:  dictKey Assign expression -> dictKey expression;

dictKey
    : Keyword
    | StringLiteral
    ;

dict
    : LBrace ( RBrace -> Empty
        | tuple ( Comma tuple )* RBrace -> tuple+ )
    ;

list
    : LBracket ( RBracket -> Empty
        | expression ( Comma expression )* RBracket -> expression+ )
    ;

dynamicString
    : DynamicString dynamicStringSection* DynamicString
        -> dynamicStringSection*
    ;

dynamicStringSection
    : Literal -> Literal
    | statement -> ^( Statement statement )
    | passthrough -> ^( Passthrough passthrough )
    ;

expression
	: logical_and_expression ( OR^ logical_and_expression )*
	;

logical_and_expression
	: equality_expression ( AND^ equality_expression )*
	;

equality_expression
	: relational_expression ( ( EQ | NEQ )^ relational_expression )*
	;

relational_expression
	: additive_expression ( ( LT | GT | LE | GE )^ additive_expression )*
	;

additive_expression
	: multiplicative_expression ( ( Minus | Add )^ multiplicative_expression )*
	;

multiplicative_expression
	: unary_expression ( ( Mul | Div | Mod )^ unary_expression )*
	;

unary_expression
	: primary_expression -> primary_expression
	| Plus unary_expression -> ^( Unary Plus unary_expression )
    | Minus unary_expression -> ^( Unary Minus unary_expression )
    | Not unary_expression -> ^( Unary Not unary_expression )
	;

primary_expression
    : statement -> ^( Statement statement )
    | passthrough -> ^( Passthrough passthrough )
    | dynamicString -> ^( DynamicString dynamicString )
    | constant -> ^( Constant constant )
	| LP expression RP -> ^( Nested expression )
	;

if
    : If LP ifCond=expression RP
        ifBlock=block
        ( ElseIf LP elseIfCond+=expression RP
            elseIfBlock+=block )*
        ( Else
            elseBlock=block )?
        End
        -> ^( If $ifCond $ifBlock
            ^( Else $elseBlock )?
            ^( ElseIf $elseIfCond $elseIfBlock )* )
    ;

set: Set LP statement Assign expression RP -> ^( Set ^( Statement statement ) expression );

include: Include LP argument_expression_list? RP -> ^( Include argument_expression_list? );

parse: Parse LP argument_expression_list? RP -> ^( Parse argument_expression_list? );

assert: Assert LP cond=expression Comma message=expression RP -> ^( Assert $cond $message );

pragma: Pragma LP dictKey ( Comma dictKey )* RP -> ^( Pragma dictKey+ );

loop
    : Loop LP lowerBound=expression
        To upperBound=expression
        As var=statement
        ( Step step=expression )? RP
        d=block
        ( i+=LoopDirective b+=block )*
        End -> ^( Loop ^( LoopArgs $lowerBound $upperBound ^( Statement $var ) $step? )
            ^( DefaultBlock $d )
            ^( TargetedBlock $i $b )* )
    ;

foreach
    : Foreach LP var=statement In source=expression RP
        d=block
        ( i+=LoopDirective b+=block )*
        End
        -> ^( Foreach ^( Statement $var ) $source ^( DefaultBlock $d ) ^( TargetedBlock $i $b )* )
    ;

argument_expression_list
    : expression ( Comma expression )* -> ^( FnArgs expression+ )
    ;
