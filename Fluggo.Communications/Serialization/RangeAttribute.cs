/*
	Fluggo Communications Library
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
using System.Text;
using Fluggo.CodeGeneration.IL;

namespace Fluggo.Communications.Serialization
{
	/// <summary>
	/// Specifies the action to take if a value falls outside the allowed range for the field.
	/// </summary>
	public enum RangeViolationAction {
		/// <summary>
		/// Throws an exception if a value is outside the allowed range.
		/// </summary>
		ThrowException,
		
		/// <summary>
		/// Substitutes the value inside the allowed range that is closest to the supplied value.
		/// </summary>
		Clamp
	}
	
	/// <summary>
	/// Specifies the range of an integer field.
	/// </summary>
	/// <remarks>This attribute is used to limit the valid values of a field so that fewer bits can be used
	///   to serialize it. This attribute applies to the integral field types (<see cref="Byte"/>, <see cref='Int16'/>,
	///   <see cref='Int32'/>, <see cref='Int64'/>, <see cref='SByte'/>, <see cref='UInt16'/>, <see cref='UInt32'/>,
	///   and <see cref='UInt64'/>) and is ignored on all other types. If it is applied to an array, then it applies to
	///   all the values in the array.</remarks>
	[AttributeUsage( AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Field, AllowMultiple=false )]
	public sealed class RangeAttribute : SerializationAttribute
	{
		ulong _range, _offset;
		bool _addOffset;
		RangeViolationAction _violationAction = RangeViolationAction.ThrowException;

		/// <summary>
		/// Creates a new instance of the <see cref='RangeAttribute'/> class.
		/// </summary>
		/// <param name="minValue">Minimum value of the field or parameter.</param>
		/// <param name="maxValue">Maximum value of the field or parameter.</param>
		/// <exception cref="ArgumentException"><paramref name="maxValue"/> is less than <paramref name="minValue"/>.</exception>
		public RangeAttribute( long minValue, long maxValue ) {
			if( maxValue < minValue )
				throw new ArgumentException( "The specified maximum value was less than the specified minimum value." );

			checked {
				if( minValue < 0 ) {
					_addOffset = true;
					_offset = (ulong)(-minValue);
					
					if( maxValue < 0 )
						_range = (ulong)(maxValue - minValue);
					else
						_range = _offset + (ulong) maxValue;
				}
				else {
					_addOffset = false;
					_offset = (ulong) minValue;
					
					// maxValue must also be greater than or equal to zero
					_range = (ulong) maxValue - (ulong) minValue;
				}
			}
		}

		/// <summary>
		/// Creates a new instance of the <see cref='RangeAttribute'/> class.
		/// </summary>
		/// <param name="minValue">Minimum value of the field or parameter.</param>
		/// <param name="maxValue">Maximum value of the field or parameter.</param>
		/// <exception cref="ArgumentException"><paramref name="maxValue"/> is less than <paramref name="minValue"/>.</exception>
		public RangeAttribute( int minValue, int maxValue )
			: this( (long) minValue, (long) maxValue ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='RangeAttribute'/> class.
		/// </summary>
		/// <param name="minValue">Minimum value of the field or parameter.</param>
		/// <param name="maxValue">Maximum value of the field or parameter.</param>
		/// <exception cref="ArgumentException"><paramref name="maxValue"/> is less than <paramref name="minValue"/>.</exception>
		public RangeAttribute( short minValue, short maxValue )
			: this( (long) minValue, (long) maxValue ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='RangeAttribute'/> class.
		/// </summary>
		/// <param name="minValue">Minimum value of the field or parameter.</param>
		/// <param name="maxValue">Maximum value of the field or parameter.</param>
		/// <exception cref="ArgumentException"><paramref name="maxValue"/> is less than <paramref name="minValue"/>.</exception>
		[CLSCompliant( false )]
		public RangeAttribute( ulong minValue, ulong maxValue ) {
			if( maxValue < minValue )
				throw new ArgumentException( "The specified maximum value was less than the specified minimum value." );

			checked {
				_addOffset = false;
				_offset = minValue;
				_range = maxValue - minValue;
			}
		}

		/// <summary>
		/// Creates a new instance of the <see cref='RangeAttribute'/> class.
		/// </summary>
		/// <param name="minValue">Minimum value of the field or parameter.</param>
		/// <param name="maxValue">Maximum value of the field or parameter.</param>
		/// <exception cref="ArgumentException"><paramref name="maxValue"/> is less than <paramref name="minValue"/>.</exception>
		[CLSCompliant( false )]
		public RangeAttribute( uint minValue, uint maxValue )
			: this( (ulong) minValue, (ulong) maxValue ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='RangeAttribute'/> class.
		/// </summary>
		/// <param name="minValue">Minimum value of the field or parameter.</param>
		/// <param name="maxValue">Maximum value of the field or parameter.</param>
		/// <exception cref="ArgumentException"><paramref name="maxValue"/> is less than <paramref name="minValue"/>.</exception>
		[CLSCompliant( false )]
		public RangeAttribute( ushort minValue, ushort maxValue )
			: this( (ulong) minValue, (ulong) maxValue ) {
		}

		/// <summary>
		/// Gets the maximum range of the field or parameter.
		/// </summary>
		/// <value>The maximum range of the field or parameter.</value>
		[CLSCompliant( false )]
		public ulong RangeLength {
			get {
				return _range;
			}
		}

		/// <summary>
		/// Gets or sets the action taken when a value falls outside the allowed range.
		/// </summary>
		/// <value>The action taken when a value falls outside the allowed range. The default is
		///   <see cref='RangeViolationAction.ThrowException'>RangeViolationAction.ThrowException</see>.</value>
		public RangeViolationAction ViolationAction {
			get {
				return _violationAction;
			}
			set {
				_violationAction = value;
			}
		}
		
		private static bool IsUnsigned( Type integerType ) {
			if( integerType == null )
				throw new ArgumentNullException( "integerType" );
			
			switch( Type.GetTypeCode( integerType ) ) {
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
					return false;
				
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return true;
					
				default:
					throw new ArgumentException( "The type was not an integral type.", "integerType" );
			}
		}
		
		public Expression GetSerializableIntegerExpression( Expression value, string paramName, out Expression preCondExp ) {
			if( value == null )
				throw new ArgumentNullException( "value" );
				
			TypeCode code = Type.GetTypeCode( value.ResultType );
			Expression minExp = null, maxExp = null;
			preCondExp = null;

			// Determine the min and max values and create expressions for them
			if( IsUnsigned( value.ResultType ) ) {
				ulong minValue = _offset, maxValue = _offset + _range;

				switch( code ) {
					case TypeCode.Byte:
						if( minValue != byte.MinValue )
							minExp = new ByteConstantExpression( (byte) minValue );

						if( maxValue != byte.MaxValue )
							maxExp = new ByteConstantExpression( (byte) maxValue );

						break;

					case TypeCode.UInt16:
						if( minValue != ushort.MinValue )
							minExp = new UInt16ConstantExpression( (ushort) minValue );

						if( maxValue != ushort.MaxValue )
							maxExp = new UInt16ConstantExpression( (ushort) maxValue );

						break;

					case TypeCode.UInt32:
						if( minValue != uint.MinValue )
							minExp = new UInt32ConstantExpression( (uint) minValue );

						if( maxValue != uint.MaxValue )
							maxExp = new UInt32ConstantExpression( (uint) maxValue );

						break;

					case TypeCode.UInt64:
						if( minValue != ulong.MinValue )
							minExp = new UInt64ConstantExpression( minValue );

						if( maxValue != ulong.MaxValue )
							maxExp = new UInt64ConstantExpression( maxValue );

						break;

					default:
						throw new UnexpectedException();
				}
			}
			else {
				long minValue, maxValue;
				
				if( _addOffset ) {
					minValue = -((long) _offset);
					maxValue = (long) (_range - _offset);
				}
				else {
					minValue = (long) _offset;
					maxValue = (long) (_range + _offset);
				}

				switch( code ) {
					case TypeCode.SByte:
						if( minValue != sbyte.MinValue )
							minExp = new SByteConstantExpression( (sbyte) minValue );
							
						if( maxValue != sbyte.MaxValue )
							maxExp = new SByteConstantExpression( (sbyte) maxValue );
							
						break;
					
					case TypeCode.Int16:
						if( minValue != short.MinValue )
							minExp = new Int16ConstantExpression( (short) minValue );
							
						if( maxValue != short.MaxValue )
							maxExp = new Int16ConstantExpression( (short) maxValue );
							
						break;
					
					case TypeCode.Int32:
						if( minValue != int.MinValue )
							minExp = new Int32ConstantExpression( (int) minValue );
							
						if( maxValue != int.MaxValue )
							maxExp = new Int32ConstantExpression( (int) maxValue );
							
						break;
					
					case TypeCode.Int64:
						if( minValue != long.MinValue )
							minExp = new Int64ConstantExpression( minValue );
							
						if( maxValue != long.MaxValue )
							maxExp = new Int64ConstantExpression( maxValue );
							
						break;
					
					default:
						throw new Exception( "Er... what happened?" );
				}
			}  // endif( unsigned )

			// Write code on how to use them
			TypeProxy math = new TypeProxy( typeof(Math) );
			
			switch( _violationAction ) {
				case RangeViolationAction.Clamp:
					if( minExp != null ) {
						value = math.Call( "Max", value, minExp );
					}
					
					if( maxExp != null ) {
						value = math.Call( "Min", value, maxExp );
					}
				
					break;

				case RangeViolationAction.ThrowException:
				default: {
					ListExpression expr = new ListExpression( false );
					
					if( minExp != null ) {
						expr.Add(
							new ConditionalExpression( new CompareExpression( value, CompareOperator.LessThan, minExp ),
							new ThrowExpression( new NewObjectExpression(
								typeof(ArgumentOutOfRangeException).GetConstructor( new Type[] { typeof(string) } ),
								new Expression[] { new StringConstantExpression( paramName ) } ) ), null )
						);
					}
					
					if( maxExp != null ) {
						expr.Add(
							new ConditionalExpression( new CompareExpression( value, CompareOperator.GreaterThan, maxExp ),
							new ThrowExpression( new NewObjectExpression(
								typeof( ArgumentOutOfRangeException ).GetConstructor( new Type[] { typeof( string ) } ),
								new Expression[] { new StringConstantExpression( paramName ) } ) ), null )
						);
					}
					
					if( expr.Count != 0 )
						preCondExp = expr;
				}
					break;
			} // endswitch( _violationAction )
			
			// Then write the offset code
			return (minExp != null) ? new ArithmeticExpression( value, ArithmeticOperator.Subtract, minExp, false ) : value;
		}
		
		public Expression GetDeserializedIntegerExpression( Expression value, Type integerType, out Expression preConditionExpression ) {
			if( value == null )
				throw new ArgumentNullException( "value" );

			if( value.ResultType != typeof(ulong) )
				throw new StackArgumentException( "value" );
				
			preConditionExpression = new ConditionalExpression( new CompareExpression( value, CompareOperator.GreaterThan, ILCodeBuilder.ToExpression( _range ) ),
				new ThrowExpression( new NewObjectExpression( typeof(ArgumentOutOfRangeException).GetConstructor( Type.EmptyTypes ), new Expression[0] ) ), null );
				
			if( IsUnsigned( integerType ) ) {
				if( _addOffset )
					throw new InvalidOperationException( "A range with negative values was specified, but an unsigned number is being decoded." );
					
				return new CastExpression( new ArithmeticExpression( value, ArithmeticOperator.Add, ILCodeBuilder.ToExpression( _offset ), false ), integerType, false );
			}
			else {
				return new CastExpression( _addOffset ? 
					new ArithmeticExpression( value, ArithmeticOperator.Subtract, ILCodeBuilder.ToExpression( _offset ), false ) :
					new ArithmeticExpression( value, ArithmeticOperator.Add, ILCodeBuilder.ToExpression( _offset ), false ), integerType, false );
			}
		}
		
		/// <summary>
		/// Converts the given integer to a serializable range.
		/// </summary>
		/// <param name="value">Integer to convert.</param>
		/// <returns>Returns an integer in the range zero to <see cref="RangeLength"/>, inclusive.</returns>
		[CLSCompliant( false )]
		public ulong ToSerializableInteger( long value ) {
			long minValue, maxValue;
			
			if( _addOffset ) {
				minValue = -((long)_offset);
				maxValue = (long)(_range - _offset);
			}
			else {
				minValue = (long) _offset;
				maxValue = (long)(_range + _offset);
			}
		
			switch( _violationAction ) {
				case RangeViolationAction.Clamp:
					if( value < minValue )
						value = minValue;
						
					if( value > maxValue )
						value = maxValue;
					
					break;

				case RangeViolationAction.ThrowException:
				default:
					if( value < minValue || value > maxValue )
						throw new ArgumentOutOfRangeException( "value" );
						
					break;
			}
			
			checked {
				if( _addOffset ) {
					value += (long) _offset;
				}
				else {
					value -= (long) _offset;
				}
				
				return (ulong) value;
			}
		}

		/// <summary>
		/// Converts the given integer to a serializable range.
		/// </summary>
		/// <param name="value">Integer to convert.</param>
		/// <returns>Returns an integer in the range zero to <see cref="RangeLength"/>, inclusive.</returns>
		[CLSCompliant( false )]
		public ulong ToSerializableInteger( int value ) {
			return ToSerializableInteger( (long) value );
		}

		/// <summary>
		/// Converts the given integer to a serializable range.
		/// </summary>
		/// <param name="value">Integer to convert.</param>
		/// <returns>Returns an integer in the range zero to <see cref="RangeLength"/>, inclusive.</returns>
		[CLSCompliant( false )]
		public ulong ToSerializableInteger( ulong value ) {
			if( _addOffset )
				throw new InvalidOperationException( "A range with negative values was specified, but an unsigned number is being encoded." );

			ulong minValue = _offset, maxValue = _offset + _range;

			switch( _violationAction ) {
				case RangeViolationAction.Clamp:
					if( value < minValue )
						value = minValue;

					if( value > maxValue )
						value = maxValue;

					break;

				case RangeViolationAction.ThrowException:
				default:
					if( value < minValue || value > maxValue )
						throw new ArgumentOutOfRangeException( "value" );

					break;
			}

			checked {
				return value - _offset;
			}
		}

		/// <summary>
		/// Converts a serialized signed integer back to its normal range.
		/// </summary>
		/// <param name="value">Serialized integer to restore.</param>
		/// <returns>Returns the restored signed integer as an <see cref="Int64"/>.</returns>
		[CLSCompliant( false )]
		public long FromSerializedInteger( ulong value ) {
			unchecked {
				if( _addOffset ) {
					return (long) (value - _offset);
				}
				else {
					return (long) (value + _offset);
				}
			}
		}

		/// <summary>
		/// Converts a serialized unsigned integer back to its normal range.
		/// </summary>
		/// <param name="value">Serialized integer to restore.</param>
		/// <returns>Returns the restored unsigned integer as a <see cref="UInt64"/>.</returns>
		[CLSCompliant( false )]
		public ulong FromSerializedUnsignedInteger( ulong value ) {
			if( _addOffset )
				throw new InvalidOperationException( "A range with negative values was specified, but an unsigned number is being decoded." );

			unchecked {
				return value + _offset;
			}
		}
	}
}
