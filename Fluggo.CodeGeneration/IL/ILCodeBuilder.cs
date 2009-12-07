/*
	Fluggo Code Generation Library
	Copyright (C) 2005-6  Brian J. Crowell

	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.

	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;

namespace Fluggo.CodeGeneration.IL {
	/// <summary>
	/// Base class with properties and methods to facilitate IL generation using expressions.
	/// </summary>
	/// <remarks>The majority of methods on this class are static and do not require an instance. However,
	///   some features require knowing the type of the object on which they are contained.</remarks>
	public class ILCodeBuilder {
		static BlankLineExpression __blankLine = new BlankLineExpression();
		
	#region Calls
		/// <summary>
		/// Creates an expression that calls the given method with the given arguments.
		/// </summary>
		/// <param name="method"><see cref="MethodInfo"/> describing the method to call. This can be an instance or virtual method.</param>
		/// <param name="args">Arguments to the method. If the method is an instance method, the first argument should be an expression
		///   that references the target object.</param>
		/// <returns>A <see cref="CallExpression"/> for calling the given method.</returns>
		protected static CallExpression Call( MethodInfo method, params object[] args )
			{ return new CallExpression( method, ToExpressions( args ) ); }
		
#if false
		/// <summary>
		/// Creates an expression that calls the given virtual method with the given arguments.
		/// </summary>
		/// <param name="method"><see cref="MethodInfo"/> describing the method to call. This can be an instance or virtual method.
		///   If the method is virtual, the method listed in the target's virtual method table is called.</param>
		/// <param name="args">Arguments to the method. If the method is an instance method, the first argument should be an expression
		///   that references the target object.</param>
		/// <returns>A <see cref="CallExpression"/> for calling the given method.</returns>
		[Obsolete]
		protected static CallVirtExpression CallVirt( MethodInfo method, params object[] args )
			{ return new CallVirtExpression( method, ToExpressions( args ) ); }
#endif
		
		/// <summary>
		/// Creates an expression that calls the given constructor with the given arguments.
		/// </summary>
		/// <param name="ctor"><see cref="ConstructorInfo"/> describing the constructor to call.</param>
		/// <param name="args">Arguments to the constructor. The first argument must be a reference to the object.</param>
		/// <returns>A <see cref="CallExpression"/> for calling the given constructor.</returns>
		/// <remarks>This expression only calls the constructor without creating a new object. Use this form inside
		///   constructors for calling base constructors or chaining constructors on the class. Use <see cref="New(Type, object[])"/>
		///   to create a new object.</remarks>
		protected static CallExpression Call( ConstructorInfo ctor, params object[] args )
			{ return new CallExpression( ctor, ToExpressions( args ) ); }
	#endregion

		/// <summary>
		/// Creates an expression that creates a new object.
		/// </summary>
		/// <param name="ctor"><see cref="ConstructorInfo"/> describing the constructor to call on the new object.</param>
		/// <param name="args">Arguments to the constructor, if any.</param>
		/// <returns>A <see cref="NewObjectExpression"/> for creating the object.</returns>
		protected static NewObjectExpression New( ConstructorInfo ctor, params object[] args ) {
			return new NewObjectExpression( ctor, ToExpressions( args ) );
		}

		/// <summary>
		/// Creates an expression that creates a new object.
		/// </summary>
		/// <param name="type"><see cref="Type"/> of the object to create.</param>
		/// <param name="args">Arguments to the constructor, if any.</param>
		/// <returns>A <see cref="NewObjectExpression"/> for creating the object.</returns>
		protected static NewObjectExpression New( Type type, params object[] args ) {
			return new TypeProxy( type ).New( args );
		}

		/// <summary>
		/// Creates an expression that creates a new array.
		/// </summary>
		/// <param name="elementType">Type of the elements in the array.</param>
		/// <param name="count">A value or expression that evaluates to the number of elements in the array.</param>
		/// <returns>A <see cref="NewArrayExpression"/> for creating the array.</returns>
		protected static NewArrayExpression NewArray( Type elementType, object count ) {
			return new NewArrayExpression( elementType, ToExpression( count ) );
		}
		
		/// <summary>
		/// Creates a conditional expression.
		/// </summary>
		/// <param name="condition">Constant or expression for the condition of the expression.</param>
		/// <param name="trueExpression">Constant or expression to evaluate if the condition evaluates to true (nonzero).</param>
		/// <param name="falseExpression">Constant or expression to evaluate if the condition evaluates to false (zero).</param>
		/// <returns>A <see cref="ConditionalExpression"/> for testing the condition.</returns>
		/// <remarks>You can omit one of the cases, but the remaining expression must leave the stack unchanged.
		///   If you supply both cases, they must both have the same <see cref="Expression.ResultType"/>.</remarks>
		protected static ConditionalExpression If( object condition, object trueExpression, object falseExpression )
			{ return new ConditionalExpression( ToBooleanExpression( condition ),
				ToExpression( trueExpression ), ToExpression( falseExpression ) ); }
		
		/// <summary>
		/// Creates a conditional expression that determines if an expression is false.
		/// </summary>
		/// <param name="condition">Constant or expression for the condition of the expression.</param>
		/// <param name="falseExpression">Constant or expression to evaluate if the condition evaluates to false (zero).</param>
		/// <returns>A <see cref="ConditionalExpression"/> for testing the condition.</returns>
		protected static ConditionalExpression IfFalse( object condition, object falseExpression )
			{ return new ConditionalExpression( ToBooleanExpression( condition ),
				null, ToExpression( falseExpression ) ); }

		/// <summary>
		/// Creates a conditional expression that determines if an expression is false.
		/// </summary>
		/// <param name="condition">Constant or expression for the condition of the expression.</param>
		/// <param name="nullExpression">Constant or expression to evaluate if the condition evaluates to null.</param>
		/// <returns>A <see cref="ConditionalExpression"/> for testing the condition.</returns>
		protected static ConditionalExpression IfNull( object condition, object nullExpression ) {
			Expression exp = ToExpression( condition );
			
			if( exp.ResultType.IsValueType )
				throw new StackArgumentException( "condition" );
			
			return new ConditionalExpression( new CompareExpression( exp, CompareOperator.Equals, Null( exp.ResultType ) ), ToExpression( nullExpression ), null );
		}

		/// <summary>
		/// Combines several expressions into one.
		/// </summary>
		/// <param name="args">Expressions to chain.</param>
		/// <returns>A <see cref="ListExpression"/> that evaluates each expression in the given order.</returns>
		protected static ListExpression List( params Expression[] args )
			{ return new ListExpression( args ); }
			
		/// <summary>
		/// Creates an expression that returns from a void method.
		/// </summary>
		/// <returns>A <see cref="ReturnExpression"/> that returns from a void method.</returns>
		protected static ReturnExpression Return()
			{ return new ReturnExpression(); }
		
		/// <summary>
		/// Creates an expression that returns a value.
		/// </summary>
		/// <param name="value">Constant or expression to evaluate and return.</param>
		/// <returns>A <see cref="ReturnExpression"/> that returns the result of the given value.</returns>
		protected static ReturnExpression Return( object value )
			{ return new ReturnExpression( ToExpression( value ) ); }
			
	#region Storing values
		/// <summary>
		/// Creates an expression that stores a value into a local variable.
		/// </summary>
		/// <param name="local"><see cref="LocalBuilder"/> that refers to a local variable.</param>
		/// <param name="value">Constant or expression to evaluate and store.</param>
		/// <returns>A <see cref="StoreLocalExpression"/> for storing the result of the given value.</returns>
		protected static StoreLocalExpression StoreLocal( LocalBuilder local, object value )
			{ return new StoreLocalExpression( local, ToExpression( value ) ); }

		/// <summary>
		/// Creates an expression that stores a value into a local variable.
		/// </summary>
		/// <param name="local"><see cref="LocalExpression"/> that refers to a local variable.</param>
		/// <param name="value">Constant or expression to evaluate and store.</param>
		/// <returns>A <see cref="StoreLocalExpression"/> for storing the result of the given value.</returns>
		protected static StoreLocalExpression StoreLocal( LocalExpression local, object value ) { return new StoreLocalExpression( local, ToExpression( value ) ); }
		
/*		/// <summary>
		/// Creates an expression that stores a value into a local variable.
		/// </summary>
		/// <param name="local">Zero-based index of the local variable.</param>
		/// <param name="value">Constant or expression to evaluate and store.</param>
		/// <returns>A <see cref="StoreLocalExpression"/> for storing the result of the given value.</returns>
		protected static StoreLocalExpression StoreLocal( int local, object value )
			{ return new StoreLocalExpression( local, ToExpression( value ) ); }*/

		/// <summary>
		/// Creates an expression that stores a value into an instance field.
		/// </summary>
		/// <param name="base"><see cref="Expression"/> that refers to the instance for the <paramref name="field"/>.</param>
		/// <param name="field"><see cref="FieldInfo"/> describing the target field.</param>
		/// <param name="value">Constant or expression to evaluate and store.</param>
		/// <returns>A <see cref="StoreFieldExpression"/> for storing the result of the given value.</returns>
		protected static StoreFieldExpression StoreField( Expression @base, FieldInfo field, object value )
			{ return new StoreFieldExpression( @base, field, ToExpression( value ) ); }

		/// <summary>
		/// Creates an expression that stores a value into a static field.
		/// </summary>
		/// <param name="field"><see cref="FieldInfo"/> describing the target field.</param>
		/// <param name="value">Constant or expression to evaluate and store.</param>
		/// <returns>A <see cref="StoreFieldExpression"/> for storing the result of the given value.</returns>
		protected static StoreFieldExpression StoreField( FieldInfo field, object value ) {
			return new StoreFieldExpression( field, ToExpression( value ) );
		}
	#endregion
			
	#region Retrieving values
		/// <summary>
		/// Creates an expression that retrieves the value of an instance field.
		/// </summary>
		/// <param name="baseValue"><see cref="Expression"/> that resolves to an instance containing <paramref name="field"/>.</param>
		/// <param name="field"><see cref="FieldInfo"/> for the field to be read.</param>
		/// <returns>A <see cref="FieldExpression"/> that resolves to the value of the field.</returns>
		protected static FieldExpression Field( Expression baseValue, FieldInfo field )
			{ return new FieldExpression( baseValue, field ); }

		/// <summary>
		/// Creates an expression that retrieves the value of a static field.
		/// </summary>
		/// <param name="field"><see cref="FieldInfo"/> for the field to be read.</param>
		/// <returns>A <see cref="FieldExpression"/> that resolves to the value of the field.</returns>
		protected static FieldExpression Field( FieldInfo field )
			{ return new FieldExpression( field ); }
		
		/// <summary>
		/// Creates an expression that retrieves the value of a local variable.
		/// </summary>
		/// <param name="local"><see cref="LocalBuilder"/> for the local variable to be read.</param>
		/// <returns>A <see cref="LocalExpression"/> that resolves to the value of the local variable.</returns>
		protected static LocalExpression Local( LocalBuilder local )
			{ return new LocalExpression( local ); }
/*		protected static LocalExpression Local( int local )
			{ return new LocalExpression( local ); }*/
	#endregion
		
		/// <summary>
		/// Creates an expression that retrieves the value of a method argument.
		/// </summary>
		/// <param name="type">Type of the argument.</param>
		/// <param name="arg">Index of the argument. For instance methods, zero is the "this" reference and
		///   declared arguments start at one. For static methods, zero is the first declared argument.</param>
		/// <returns>An <see cref="ArgumentExpression"/> that resolves to the value of the argument.</returns>
		/// <remarks>When possible, you should use <see cref="MethodGeneratorContext.Arg">MethodGeneratorContext.Arg</see>
		///   or <see cref="ConstructorGeneratorContext.Arg">ConstructorGeneratorContext.Arg</see>. These methods
		///   will retrieve the type of the argument automatically and ensure that <paramref name="arg"/> is in the
		///   correct range.</remarks>
		protected static ArgumentExpression Arg( Type type, int arg ) {
			return new ArgumentExpression( type, arg );
		}
		
		protected static NullExpression Null( Type type ) {
			return new NullExpression( type );
		}
		
		/// <summary>
		/// Creates an expression that throws an exception.
		/// </summary>
		/// <param name="expr">Expression that evaluates to an exception.</param>
		/// <returns>A <see cref="ThrowExpression"/> that throws the given exception.</returns>
		/// <exception cref="StackArgumentException"><paramref name="expr"/> does not resolve to a type that
		///   derives from <see cref="Exception"/>.</exception>
		protected static ThrowExpression Throw( Expression expr )
			{ return new ThrowExpression( expr ); }
		
		/// <summary>
		/// Creates an expression that throws an exception.
		/// </summary>
		/// <param name="ctor"><see cref="ConstructorInfo"/> for an exception's constructor.</param>
		/// <param name="args">List of arguments to the exception constructor.</param>
		/// <returns>A <see cref="ThrowExpression"/> that throws the given exception.</returns>
		/// <exception cref="StackArgumentException"><paramref name="ctor"/> does not appear on a type that
		///   derives from <see cref="Exception"/>.</exception>
		protected static ThrowExpression Throw( ConstructorInfo ctor, params object[] args )
			{ return new ThrowExpression( ctor, ToExpressions( args ) ); }

		/// <summary>
		/// Creates an expression that throws an exception.
		/// </summary>
		/// <param name="type"><see cref="Type"/> of exception to create.</param>
		/// <param name="args">List of arguments to the exception constructor.</param>
		/// <returns>A <see cref="ThrowExpression"/> that throws the given exception.</returns>
		/// <exception cref="StackArgumentException"><paramref name="ctor"/> does not appear on a type that
		///   derives from <see cref="Exception"/>.</exception>
		protected static ThrowExpression Throw( Type type, params object[] args )
			{ return new ThrowExpression( New( type, args ) ); }
			
		/// <summary>
		/// Creates an expression that throws an <see cref="ArgumentNullException"/>.
		/// </summary>
		/// <param name="argName">Name of the argument that caused the exception.</param>
		/// <returns>A <see cref="ThrowExpression"/> that throws the <see cref="ArgumentNullException"/>.</returns>
		protected static ThrowExpression ThrowArgNull( string argName )
			{ return Throw( typeof(ArgumentNullException).GetConstructor( new Type[] { typeof(string) } ), argName ); }
		
		/// <summary>
		/// Creates an expression that branches to a several targets based on a value.
		/// </summary>
		/// <param name="valueExpr">Integer-based value on which to branch.</param>
		/// <param name="cases">Array of expressions describing the various switch cases. An expression is chosen
		///	  based on its index in this array. For example, if the <paramref name="valueExpr"/> evaluates to two,
		///   the third case is chosen in <paramref name="cases"/> because it is zero-indexed. If the corresponding entry is <see langword='null'/>,
		///   the switch goes to the default case.</param>
		/// <param name="defaultCase">Optional expression to evaluate if none of the cases were chosen.</param>
		/// <returns>A <see cref="SwitchExpression"/> that selects a branch based on the value of the given expression.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='valueExpr'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='cases'/> is <see langword='null'/>.</para></exception>
		/// <exception cref="StackArgumentException"><paramref name="valueExpr"/> has a <see cref='Void'/> result type.</exception>
		protected static SwitchExpression Switch( object valueExpr, Expression[] cases, Expression defaultCase )
			{ return new SwitchExpression( ToExpression( valueExpr ), cases, defaultCase ); }
			
		/// <summary>
		/// Creates an expression that loops over a given expression while a condition is true.
		/// </summary>
		/// <param name="condition">Condition to test before the beginning of each iteration.</param>
		/// <param name="body">Body of the loop to execute when <paramref name="condition"/> is true. This must have a <see cref="Void"/>
		///	  <see cref="Expression.ResultType"/>.</param>
		/// <returns>A <see cref="WhileExpression"/> that iterates over <paramref name="body"/> while <paramref name="condition"/> is true.</returns>
		protected static WhileExpression While( BooleanExpression condition, Expression body )
			{ return new WhileExpression( condition, body ); }
		
		protected static ForLoopExpression For( Expression initExpr, BooleanExpression condition, Expression iterExpr, Expression body )
			{ return new ForLoopExpression( initExpr, condition, iterExpr, body ); }
			
		protected static DeclareLocalExpression Declare( Type localType, string localName, out Local local )
			{ return new DeclareLocalExpression( localType, localName, out local ); }
		protected static DeclareLocalExpression Declare( Type localType, string localName, object initExpr, out Local local )
			{ return new DeclareLocalExpression( localType, localName, ToExpression( initExpr ), out local ); }
		protected static DeclareLocalExpression Declare<T>( string localName, out Local local )
			{ return new DeclareLocalExpression( typeof(T), localName, out local ); }
		protected static DeclareLocalExpression Declare<T>( string localName, object initExpr, out Local local )
			{ return new DeclareLocalExpression( typeof(T), localName, ToExpression( initExpr ), out local ); }
			
		protected static CompareExpression LessThan( Expression leftOperand, object rightOperand )
			{ return new CompareExpression( leftOperand, CompareOperator.LessThan, ToExpression( rightOperand ) ); }
		protected static CompareExpression LessThanEquals( Expression leftOperand, object rightOperand )
			{ return new CompareExpression( leftOperand, CompareOperator.LessThanEquals, ToExpression( rightOperand ) ); }
		protected static CompareExpression GreaterThan( Expression leftOperand, object rightOperand )
			{ return new CompareExpression( leftOperand, CompareOperator.GreaterThan, ToExpression( rightOperand ) ); }
		protected static CompareExpression GreaterThanEquals( Expression leftOperand, object rightOperand )
			{ return new CompareExpression( leftOperand, CompareOperator.GreaterThanEquals, ToExpression( rightOperand ) ); }
		protected static CompareExpression Equals( Expression leftOperand, object rightOperand )
			{ return new CompareExpression( leftOperand, CompareOperator.Equals, ToExpression( rightOperand ) ); }
		protected static CompareExpression NotEquals( Expression leftOperand, object rightOperand )
			{ return new CompareExpression( leftOperand, CompareOperator.NotEquals, ToExpression( rightOperand ) ); }
			
		protected static ArithmeticAssignmentExpression Increment( IDataStore operand )
			{ return new ArithmeticAssignmentExpression( operand, ArithmeticOperator.Add, new Int32ConstantExpression( 1 ), true ); }
			
		protected static TryBlockExpression Try( Expression tryBlock, params CatchBlock[] catchBlocks )
			{ return new TryBlockExpression( tryBlock, catchBlocks, null ); }
		protected static TryBlockExpression TryFinally( Expression tryBlock, Expression finallyBlock )
			{ return new TryBlockExpression( tryBlock, null, finallyBlock ); }
		protected static CatchBlock Catch( Type exceptionType, out Local exceptionLocal )
			{ return new CatchBlock( exceptionType, out exceptionLocal ); }
		protected static CatchBlock Catch( Type exceptionType )
			{ return new CatchBlock( exceptionType ); }
			
		protected static CastExpression Cast( Expression expression, Type targetType )
			{ return new CastExpression( expression, targetType, true ); }
			
		protected static IsInstanceExpression Is( Expression expression, Type targetType )
			{ return new IsInstanceExpression( expression, targetType ); }
		
		/// <summary>
		/// Creates a catch block that catches all types of exceptions.
		/// </summary>
		/// <returns>A <see cref="CatchBlock"/> that catches all types of exceptions.</returns>
		protected static CatchBlock Catch()
			{ return new CatchBlock( null ); }

		/// <summary>
		/// Gets an expression that produces a blank line in the code text.
		/// </summary>
		/// <value>A <see cref="BlankLineExpression"/> that produces a blank line in the code text.</value>
		protected static BlankLineExpression BlankLine {
			get {
				return __blankLine;
			}
		}
		
		/// <summary>
		/// Creates an expression that writes a comment to the code text.
		/// </summary>
		/// <param name="comment">Comment to write.</param>
		/// <returns>A <see cref="CommentExpression"/> that writes the given comment to the code text.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='comment'/> is <see langword='null'/>.</exception>
		protected static CommentExpression Comment( string comment )
			{ return new CommentExpression( comment ); }

		/// <summary>
		/// Creates an expression that writes a comment to the code text.
		/// </summary>
		/// <param name="comment">Format for the comment to write.</param>
		/// <param name="params">Arguments for the comment to write.</param>
		/// <returns>A <see cref="CommentExpression"/> that writes the given comment to the code text.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='comment'/> is <see langword='null'/>.</exception>
		protected static CommentExpression Comment( string comment, params object[] @params )
			{ return new CommentExpression( string.Format( comment, @params ) ); }
		
		/// <summary>
		/// Converts an array of objects into an array of expressions.
		/// </summary>
		/// <param name="args">Arguments to convert.</param>
		/// <returns>An array of <see cref="Expression"/> objects corresponding to the given <paramref name="args"/>.</returns>
		/// <remarks>The conversion is done by the <see cref="ToExpression"/> method.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="args"/> is <see langword='null'/>.</exception>
		public static Expression[] ToExpressions( Array args ) {
			if( args == null )
				throw new ArgumentNullException( "args" );
			
			List<Expression> result = new List<Expression>( args.Length );
			
			for( int i = 0; i < args.Length; i++ ) {
				Expression[] subArgs = args.GetValue( i ) as Expression[];
				
				if( subArgs != null )
					result.AddRange( subArgs );
				else
					result.Add( ToExpression( args.GetValue( i ) ) );
			}
				
			return result.ToArray();
		}

		/// <summary>
		/// Converts an object into an <see cref="BooleanExpression"/>.
		/// </summary>
		/// <param name="arg">Object to convert.</param>
		/// <returns>A <see cref="BooleanExpression"/> representing the given object, if possible.</returns>
		/// <remarks>This method is used to accept many types of expressions as objects in other <see cref="ILCodeBuilder"/>
		///   methods. The conversions possible include:
		///   <list type="bullet">
		///		<item><description><see cref="Expression"/> to <see cref="Expression"/></description></item>
		///		<item><description><see cref="LocalBuilder"/> to <see cref="LocalExpression"/></description></item>
		///		<item><description><see cref="FieldInfo"/> to <see cref="FieldExpression"/></description></item>
		///		<item><description>Array of <see cref="Expression"/> objects to <see cref="ListExpression"/></description></item>
		///		<item><description><see cref="Int32"/> to <see cref="Int32ConstantExpression"/></description></item>
		///		<item><description><see cref="String"/> to <see cref="StringConstantExpression"/></description></item>
		///	  </list></remarks>
		public static BooleanExpression ToBooleanExpression( object arg ) {
			if( arg == null )
				return null;
				
			if( arg is BooleanExpression )
				return (BooleanExpression) arg;

/*			if( arg is Expression )
				return (Expression) arg;
			else if( arg is LocalBuilder )
				return new LocalExpression( (LocalBuilder) arg );
			else if( arg is IDataStore )
				return ((IDataStore) arg).Get();
			else if( arg is FieldInfo )
				return new FieldExpression( (FieldInfo) arg );
			else if( arg is Expression[] )
				return new ListExpression( (Expression[]) arg );
			else if( arg is bool )
				return new BooleanConstantExpression( (bool) arg );
			else if( arg is long )
				return new Int64ConstantExpression( (long) arg );
			else if( arg is int )
				return new Int32ConstantExpression( (int) arg );
			else if( arg is short )
				return new Int16ConstantExpression( (short) arg );
			else if( arg is byte )
				return new ByteConstantExpression( (byte) arg );
			else if( arg is string )
				return new StringConstantExpression( (string) arg );
			else if( arg is ulong )
				return new UInt64ConstantExpression( (ulong) arg );
			else if( arg is uint )
				return new UInt32ConstantExpression( (uint) arg );
			else if( arg is ushort )
				return new UInt16ConstantExpression( (ushort) arg );
			else if( arg is sbyte )
				return new SByteConstantExpression( (sbyte) arg );*/

			throw new NotSupportedException( "The given expression type is not supported." );
		}

		/// <summary>
		/// Converts an object into an <see cref="Expression"/>.
		/// </summary>
		/// <param name="arg">Object to convert.</param>
		/// <returns>An <see cref="Expression"/> representing the given object, if possible.</returns>
		/// <remarks>This method is used to accept many types of expressions as objects in other <see cref="ILCodeBuilder"/>
		///   methods. The conversions possible include:
		///   <list type="bullet">
		///		<item><description><see cref="Expression"/> to <see cref="Expression"/></description></item>
		///		<item><description><see cref="LocalBuilder"/> to <see cref="LocalExpression"/></description></item>
		///		<item><description><see cref="FieldInfo"/> to <see cref="FieldExpression"/></description></item>
		///		<item><description>Array of <see cref="Expression"/> objects to <see cref="ListExpression"/></description></item>
		///		<item><description><see cref="Int32"/> to <see cref="Int32ConstantExpression"/></description></item>
		///		<item><description><see cref="String"/> to <see cref="StringConstantExpression"/></description></item>
		///	  </list></remarks>
		public static Expression ToExpression( object arg ) {
			if( arg == null )
				return null;

			if( arg is Expression )
				return (Expression) arg;
			else if( arg is LocalBuilder )
				return new LocalExpression( (LocalBuilder) arg );
			else if( arg is IDataStore )
				return ((IDataStore)arg).Get();
			else if( arg is FieldInfo )
				return new FieldExpression( (FieldInfo) arg );
			else if( arg is Expression[] )
				return new ListExpression( (Expression[]) arg );
			else if( arg is bool )
				return new BooleanConstantExpression( (bool) arg );
			else if( arg is long )
				return new Int64ConstantExpression( (long) arg );
			else if( arg is int )
				return new Int32ConstantExpression( (int) arg );
			else if( arg is short )
				return new Int16ConstantExpression( (short) arg );
			else if( arg is byte )
				return new ByteConstantExpression( (byte) arg );
			else if( arg is string )
				return new StringConstantExpression( (string) arg );
			else if( arg is ulong )
				return new UInt64ConstantExpression( (ulong) arg );
			else if( arg is uint )
				return new UInt32ConstantExpression( (uint) arg );
			else if( arg is ushort )
				return new UInt16ConstantExpression( (ushort) arg );
			else if( arg is sbyte )
				return new SByteConstantExpression( (sbyte) arg );

			throw new NotSupportedException( "The given expression type is not supported." );
		}
	}
	
	/// <summary>
	/// Provides common methods for generating a new type.
	/// </summary>
	public abstract class ILTypeGenerator : ILCodeBuilder {
		private TypeGeneratorContext _typeCxt;
		private Type _generatedType;

		/// <summary>
		/// Gets a reference to the <see cref="TypeGeneratorContext"/> for the type being generated.
		/// </summary>
		/// <value>A reference to the <see cref="TypeGeneratorContext"/> for the type being generated.</value>
		protected TypeGeneratorContext TypeContext {
			get {
				return _typeCxt;
			}
		}

		/// <summary>
		/// Creates a new instance of the <see cref='ILTypeGenerator'/> class.
		/// </summary>
		/// <param name="moduleCxt">Module in which the new type should be created.</param>
		/// <param name="typeName">Full name of the type to create.</param>
		/// <exception cref='ArgumentNullException'><paramref name='moduleCxt'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='typeName'/> is <see langword='null'/>.</para></exception>
		/// <remarks>After creating a new <see cref="ILTypeGenerator"/>, call <see cref="GenerateType"/>
		///   to build the type and return a reference to it. If the type already exists in the same module, a reference to the
		///   existing type is returned.</remarks>
		protected ILTypeGenerator( ModuleGeneratorContext moduleCxt, string typeName )
			: this( moduleCxt, typeName, TypeAttributes.Class, null ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='ILTypeGenerator'/> class.
		/// </summary>
		/// <param name="moduleCxt">Module in which the new type should be created.</param>
		/// <param name="typeName">Full name of the type to create.</param>
		/// <param name="attributes"><see cref="TypeAttributes"/> describing the type.</param>
		/// <param name="parent">Optional parent of the type.</param>
		/// <exception cref='ArgumentNullException'><paramref name='moduleCxt'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='typeName'/> is <see langword='null'/>.</para></exception>
		/// <remarks>After creating a new <see cref="ILTypeGenerator"/>, call <see cref="GenerateType"/>
		///   to build the type and return a reference to it. If the type already exists in the same module, a reference to the
		///   existing type is returned.</remarks>
		protected ILTypeGenerator( ModuleGeneratorContext moduleCxt, string typeName, TypeAttributes attributes, Type parent ) {
			if( moduleCxt == null )
				throw new ArgumentNullException( "moduleCxt" );

			if( typeName == null )
				throw new ArgumentNullException( "typeName" );

			Type preType = moduleCxt.Module.GetType( typeName, false, false );

			if( preType != null )
				_generatedType = preType;
			else	
				_typeCxt = moduleCxt.DefineType( typeName, attributes, parent );
		}

		/// <summary>
		/// Gets an expression referring to the current object.
		/// </summary>
		/// <value>A <see cref="ThisExpression"/> that evaluates to a reference to the current object when used
		///   in an instance method.</value>
		protected virtual ThisExpression This { get { return _typeCxt.This; } }
		
		protected virtual void BuildType() {
			Expression finalizer = GetFinalizingExpression(), managed = GetManagedDisposeExpression();

			if( finalizer != null || managed != null ) {
				TypeContext.AddInterface( typeof( IDisposable ) );

				if( finalizer != null ) {
					MethodInfo dispose = CreateInternalDisposeMethod( finalizer, managed ).Method;
					CreateManagedDisposeMethod( Call( dispose, This, true ) );
					CreateFinalizer( Call( dispose, This, false ) );
				}
				else {
					CreateManagedDisposeMethod( managed );
				}
			}
		}

		public virtual Type GenerateType() {
			if( _generatedType == null ) {
				BuildType();
				_generatedType = _typeCxt.CreateType();
			}

			return _generatedType;
		}

		protected Field DefineField( string fieldName, Type type, FieldAttributes attributes ) {
			return TypeContext.DefineField( fieldName, type, attributes );
		}
		
		protected ConstructorGeneratorContext DefineConstructor( MethodAttributes attributes, Type[] paramTypes ) {
			return TypeContext.DefineConstructor( attributes, paramTypes );
		}
		
		protected MethodGeneratorContext DefineMethod( string name, MethodAttributes attributes, Type returnType, Type[] paramTypes ) {
			return TypeContext.DefineMethod( name, attributes, returnType, paramTypes );
		}
		
		/// <summary>
		/// Gets the finalizing expression for this class.
		/// </summary>
		/// <returns>An <see cref="Expression"/> for the finalizer of this class, or <see langword="null"/>
		///	  if this class needs no finalization. The base class always returns <see langword="null"/>.</returns>
		protected virtual Expression GetFinalizingExpression() {
			return null;
		}

		/// <summary>
		/// Gets the managed disposing expression for this class.
		/// </summary>
		/// <returns>An <see cref="Expression"/> for the <see cref="IDisposable.Dispose"/> method of this class, or <see langword="null"/>
		///	  if this class needs no finalization. The base class always returns <see langword="null"/>.</returns>
		protected virtual Expression GetManagedDisposeExpression() {
			return null;
		}

		private MethodGeneratorContext CreateInternalDisposeMethod( Expression finalizer, Expression managed ) {
			MethodGeneratorContext methodCxt = TypeContext.DefineMethod( "Dispose", MethodAttributes.Private,
				typeof( void ), new Type[] { typeof( bool ) } );
			Param disposing = methodCxt.DefineParameter( 0, "disposing" );

			methodCxt.AddExpressionRange(
				If( disposing, List(
					managed,
					Call( typeof( GC ).GetMethod( "SuppressFinalize" ), methodCxt.This )
				), null ),
				finalizer
			);

			return methodCxt;
		}

		private MethodGeneratorContext CreateManagedDisposeMethod( Expression managed ) {
			MethodGeneratorContext methodCxt = TypeContext.DefineOverrideMethod( typeof( IDisposable ).GetMethod( "Dispose" ) );
			
			methodCxt.AddExpression( managed );
			
			return methodCxt;
		}

		private MethodGeneratorContext CreateFinalizer( Expression finalizer ) {
			MethodGeneratorContext methodCxt = TypeContext.DefineOverrideMethod( typeof( object ).GetMethod( "Finalize" ) );
			
			methodCxt.AddExpression( finalizer );
			
			return methodCxt;
		}
	}
}