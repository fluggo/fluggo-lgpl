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
using System.Collections;
using System.Reflection;
using System.IO;
using Fluggo.CodeGeneration.IL;
using System.Collections.Generic;

namespace Fluggo.Communications.Serialization
{
	/// <summary>
	/// Serializes objects, values, and method parameters in a packed bit format.
	/// </summary>
	public sealed class BitSerializer : ILCodeBuilder, IDisposable {
		/*
		 * BitSerializer format:
		 * 
		 * Integral types are stored as-is in the stream, only using fewer bits for their encoding if
		 * requested in the attributes.
		 * 
		 * Reference types are prefixed with a bit that indicates whether the value is null (0) or
		 * not-null (1). The remainder of the reference type is only stored if not-null. Attributes
		 * can disable this bit for fields that should never be null.
		 * 
		 * Reference types only refer to the specific type of the field unless the attributes specify
		 * a mapping from positive integers to types. If this mapping is specified, the reference type,
		 * if not null, is stored prefixed with the integer representing its type. An error occurs if
		 * the value's type cannot be matched to a mapping. The mappings can be specified with a priority
		 * that indicates which should match if the value matches more than one type. Whatever type is
		 * determined for the field, either from field type or mapping, only the fields from that type
		 * (and no fields from derived types) are serialized.
		 * 
		 * Compound types are stored in alphabetical order, beginning with the declared public instance fields of the base type
		 * followed by the declared public instance fields of the value's type. Attributes can specify an alternate order to the
		 * fields. These attributes associate an integer with the field. Lower ordinals are sorted first, and fields within the
		 * same ordinal are sorted alphabetically. (Therefore, the default sort order is the same as if all the relative orders
		 * were set to zero.)
		 * 
		 * Some types can have a custom serializer. If an attribute with a custom serializer is specified,
		 * it is used. Otherwise, the BitSerializer's internal table is searched for proxies, and finally,
		 * the default serialization is used.
		 */
		 
		BitSerializerOptions _options;
		static readonly BitSerializerOptions __defaultOptions = new BitSerializerOptions();
		TypeGeneratorContext _methodCacheType;
		ModuleGeneratorContext _cacheModule;
		static int _cacheTypeIndex = 0;
		static object _globalLock = new object();
		object _localLock = new object();
		
		Dictionary<Type, MethodInfo> _serializeMethods;
		Dictionary<Type, MethodInfo> _deserializeMethods;
		Dictionary<Type, Type> _receiverTypes;
		Dictionary<Type, Type> _senderTypes;
		
		static int CacheTypeIndex {
			get {
				lock( _globalLock ) {
					return _cacheTypeIndex++;
				}
			}
		}
		
		/// <summary>
		/// Creates a new instance of the <see cref='BitSerializer'/> class with default options.
		/// </summary>
		/// <remarks>You should avoid generating code for recursive types with <see cref="BitSerializer"/> instances created with this constructor.
		///		Without the ability to create subroutines, the <see cref="BitSerializer"/> will continue to recurse through a type until a stack
		///		overflow occurs. This does not occur when doing simple run-time serialization.
		///   <para>In all cases, you should avoid serializing a value that contains a reference to itself, either directly or indirectly.
		///     <see cref="BitSerializer"/> instances can only operate on trees, not general graphs.</para></remarks>
		public BitSerializer()
			: this( null, null ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='BitSerializer'/> class.
		/// </summary>
		/// <param name="options">Optional <see cref="BitSerializerOptions"/> instance to apply to all serialization that this instance performs or generates.</param>
		/// <remarks>You should avoid generating code for recursive types with <see cref="BitSerializer"/> instances created with this constructor.
		///		Without the ability to create subroutines, the <see cref="BitSerializer"/> will continue to recurse through a type until a stack
		///		overflow occurs. This does not occur when doing simple run-time serialization.
		///   <para>In all cases, you should avoid serializing a value that contains a reference to itself, either directly or indirectly.
		///     <see cref="BitSerializer"/> instances can only operate on trees, not general graphs.</para></remarks>
		public BitSerializer( BitSerializerOptions options )
			: this( options, null ) {
		}

		/// <summary>
		/// Creates a new instance of the <see cref='BitSerializer'/> class.
		/// </summary>
		/// <param name="options">Optional <see cref="BitSerializerOptions"/> instance to apply to all serialization that this instance performs or generates.</param>
		/// <param name="cacheModule">Optional <see cref="ModuleGeneratorContext"/> in which to create a type cache.</param>
		/// <remarks>Unless you supply a value for <paramref name="cacheModule"/>, you should avoid generating code for recursive
		///		types with the created <see cref="BitSerializer"/>.
		///		Without the ability to create subroutines, the <see cref="BitSerializer"/> will continue to recurse through a type until a stack
		///		overflow occurs. This does not occur when doing simple run-time serialization.
		///   <para>In all cases, you should avoid serializing a value that contains a reference to itself, either directly or indirectly.
		///     <see cref="BitSerializer"/> instances can only operate on trees, not general graphs.</para></remarks>
		public BitSerializer( BitSerializerOptions options, ModuleGeneratorContext cacheModule ) {
			_options = options;

			if( _options == null )
				_options = __defaultOptions;
			
			if( cacheModule != null ) {
				_cacheModule = cacheModule;
				CreateMethodCache();
					
				_serializeMethods = new Dictionary<Type,MethodInfo>();
				_deserializeMethods = new Dictionary<Type,MethodInfo>();
				_receiverTypes = new Dictionary<Type,Type>();
				_senderTypes = new Dictionary<Type,Type>();
			}
		}
		
		private void CreateMethodCache() {
			lock( _localLock ) {
				_methodCacheType = _cacheModule.DefineType( "__generated.BitSerializer.TypeCache" + CacheTypeIndex.ToString(),
					TypeAttributes.Sealed | TypeAttributes.Public, null );
			}
		}

		/// <summary>
		/// Gets the serializer options for this instance.
		/// </summary>
		/// <value>A <see cref="BitSerializerOptions"/> instance representing serializer options for this <see cref="BitSerializer"/>.</value>
		public BitSerializerOptions Options {
			get {
				return _options;
			}
		}
		
	#region Run-time serialization
		/// <summary>
		/// Reads a value of the given type from the stream.
		/// </summary>
		/// <param name="reader">The source <see cref="BitReader"/>.</param>
		/// <param name="type">The expected type of the value.</param>
		/// <param name="attributes">Optional array of attributes used to change the decoding of the value. These must be the
		///   same significant attributes used to encode the value.</param>
		/// <exception cref='ArgumentNullException'><paramref name='reader'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='type'/> is <see langword='null'/>.</para></exception>
		/// <returns>The deserialized value.</returns>
		public object DeserializeValue( BitReader reader, Type type, object[] attributes ) {
			if( reader == null )
				throw new ArgumentNullException( "reader" );

			if( type == null )
				throw new ArgumentNullException( "type" );

			BitSerializerParameterInfo attrs = new BitSerializerParameterInfo( type, "value", attributes, _options );
			
			if( !type.IsValueType && !attrs.IsRequired ) {
				// Read whether it's null
				if( !reader.ReadBoolean() )
					return null;
			}

			if( type.IsEnum )
				type = Enum.GetUnderlyingType( type );

			switch( Type.GetTypeCode( type ) ) {
				case TypeCode.Boolean:
					return reader.ReadBoolean();

				case TypeCode.Byte:
					return (byte) attrs.FromSerializedUnsignedInteger( reader.ReadUInt64( attrs.Precision ) );

				case TypeCode.Char:
					return reader.ReadChar();

				case TypeCode.DateTime:
					return new DateTime( reader.ReadInt64( 64 ) );

				case TypeCode.Int16:
					return (short) attrs.FromSerializedInteger( reader.ReadUInt64( attrs.Precision ) );

				case TypeCode.Int32:
					return (int) attrs.FromSerializedInteger( reader.ReadUInt64( attrs.Precision ) );

				case TypeCode.Int64:
					return attrs.FromSerializedInteger( reader.ReadUInt64( attrs.Precision ) );

				case TypeCode.SByte:
					return (sbyte) attrs.FromSerializedInteger( reader.ReadUInt64( attrs.Precision ) );

				case TypeCode.String:
					return reader.ReadString( attrs.MaxLength );

				case TypeCode.UInt16:
					return (ushort) attrs.FromSerializedUnsignedInteger( reader.ReadUInt64( attrs.Precision ) );

				case TypeCode.UInt32:
					return (uint) attrs.FromSerializedUnsignedInteger( reader.ReadUInt64( attrs.Precision ) );

				case TypeCode.UInt64:
					return attrs.FromSerializedUnsignedInteger( reader.ReadUInt64( attrs.Precision ) );

				case TypeCode.Single:
					return reader.ReadSingle();

				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.DBNull:
					throw new NotImplementedException( string.Format( "Serialization is not implemented for fields of type {0}.", type.Name ) );

				case TypeCode.Object:
					if( type.IsArray ) {
						Type elementType = type.GetElementType();
						int maxLength = attrs.MaxLength;

						Array array = Array.CreateInstance( elementType, reader.ReadInt32( GetPrecision( (ulong) maxLength ) ) );

						for( int i = 0; i < array.Length; i++ )
							array.SetValue( DeserializeValue( reader, elementType, attrs.GetElementAttributes() ), i );
							
						return array;
					}
					else if( type == typeof(Guid) ) {
						return new Guid( reader.ReadBytes( 16 ) );
					}
					else {
						int typeCode = -1;
						
						if( !type.IsValueType ) {
							typeCode = attrs.DeserializeTypeCode( reader, out type );
							
							if( type == null )
								throw new IOException( "The encoded type code, " + typeCode.ToString() + ", was not present in the code table. You may need to update your metadata or allow a null substitution." );
						}
						
						object obj = Activator.CreateInstance( type, false );
						
						if( typeCode != -1 )
							attrs.StoreTypeCodeFieldValue( obj, typeCode );

						foreach( FieldInfo field in GetSerializableFields( type, true ) ) {
							// Members of a compound type
							field.SetValue( obj, DeserializeValue( reader, field.FieldType, field.GetCustomAttributes( false ) ) );
						}
						
						return obj;
					}

				default:
					throw new Exception( "Unexpected type code." );
			}
		}
		
		/// <summary>
		/// Writes the given value to the stream.
		/// </summary>
		/// <param name="writer">The target <see cref="BitWriter"/>.</param>
		/// <param name="value">Value to write to the target writer.</param>
		/// <param name="type">Optional specific type to serialize. If <paramref name="type"/> is specified, <paramref name="value"/> must be
		///   assignable to this type. In case <paramref name="value"/> is a subclass of <paramref name="type"/>, the value is
		///   serialized as though it were of type <paramref name="type"/>.</param>
		/// <param name="attributes">Optional array of attributes used to change the encoding of the value.</param>
		/// <exception cref='ArgumentNullException'><paramref name='writer'/> is <see langword='null'/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="type"/> is supplied and <paramref name="value"/> is not assignable to that type.</exception>
		public void SerializeValue( BitWriter writer, object value, Type type, object[] attributes ) {
			if( writer == null )
				throw new ArgumentNullException( "writer" );

			if( value == null ) {
				if( type != null && type.IsValueType )
					throw new ArgumentException( "The given value is not a valid value for objects of type " + type.Name + "." );
					
				if( attributes != null ) {
					foreach( object attr in attributes ) {
						if( attr is RequiredAttribute )
							throw new ArgumentException( "A null value was used in a field with the Required attribute." );
					}
				}
				
				// Write the false used to indicate a null for a reference field
				writer.Write( false );
				return;
			}

			Type valueType = value.GetType();

			if( type == null ) {
				type = value.GetType();
			}
			else if( !type.IsAssignableFrom( valueType ) ) {
				throw new ArgumentException( "The given value is not a valid value for objects of type " + type.Name + "." );
			}
			
			if( type.IsEnum )
				type = Enum.GetUnderlyingType( type );

			BitSerializerParameterInfo attrs = new BitSerializerParameterInfo( type, "value", attributes, _options );

			if( !type.IsValueType && !attrs.IsRequired ) {
				// Write the true used to indicate a non-null reference field
				writer.Write( true );
			}

			switch( Type.GetTypeCode( type ) ) {
				case TypeCode.Boolean:
					writer.Write( (bool) value );
					break;
					
				case TypeCode.Byte:
					writer.Write( attrs.ToSerializableInteger( (ulong)(byte) value ), attrs.Precision );
					break;

				case TypeCode.Char:
					writer.Write( (char) value );
					break;
					
				case TypeCode.DateTime: {
						DateTime date = (DateTime) value;
						writer.Write( date.Ticks, 64 );
					}
					break;

				case TypeCode.Int16:
					writer.Write( attrs.ToSerializableInteger( (long) (short) value ), attrs.Precision );
					break;
					
				case TypeCode.Int32:
					writer.Write( attrs.ToSerializableInteger( (long) (int) value ), attrs.Precision );
					break;
					
				case TypeCode.Int64:
					writer.Write( attrs.ToSerializableInteger( (long) value ), attrs.Precision );
					break;
					
				case TypeCode.SByte:
					writer.Write( attrs.ToSerializableInteger( (long) (sbyte) value ), attrs.Precision );
					break;
					
				case TypeCode.String:
					writer.WriteString( (string) value, attrs.MaxLength );
					break;
					
				case TypeCode.UInt16:
					writer.Write( attrs.ToSerializableInteger( (ulong) (ushort) value ), attrs.Precision );
					break;

				case TypeCode.UInt32:
					writer.Write( attrs.ToSerializableInteger( (ulong) (uint) value ), attrs.Precision );
					break;
					
				case TypeCode.UInt64:
					writer.Write( attrs.ToSerializableInteger( (ulong) value ), attrs.Precision );
					break;
					
				case TypeCode.Single:
					writer.WriteSingle( (float) value );
					break;
					
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.DBNull:
					throw new NotImplementedException( string.Format( "Serialization is not implemented for fields of type {0}.", type.Name ) );

				case TypeCode.Object:
					if( type.IsArray ) {
						// Array
						Array array = (Array) value;
						Type elementType = type.GetElementType();

						int maxLength = attrs.MaxLength;
						
						if( array.Length > maxLength )
							throw new ArgumentException( "Array length is greater than the maximum length for this field.", "value" );

						// Prefix with the length of the array
						writer.Write( (uint) array.Length, GetPrecision( (ulong) maxLength ) );
						
						foreach( object obj in array )
							SerializeValue( writer, obj, elementType, attrs.GetElementAttributes() );
					}
					else if( type == typeof(Guid) ) {	
						Guid guid = (Guid) value;
						writer.WriteBytes( guid.ToByteArray(), 0, 16 );
					}
					else {
						if( !type.IsValueType ) {
							// Determine which type we're actually supposed to serialize
							attrs.SerializeTypeCode( writer, valueType, out type );
						}

						foreach( FieldInfo field in GetSerializableFields( type, true ) ) {
							// Members of a compound type
							SerializeValue( writer, field.GetValue( value ), field.FieldType, field.GetCustomAttributes( false ) );
						}
					}

					break;

				default:
					throw new UnexpectedException();
			}
		}
		
		/// <summary>
		/// Serializes the given value to a byte array.
		/// </summary>
		/// <param name="value">Value to serialize to an array.</param>
		/// <param name="type">Optional specific type to serialize. If <paramref name="type"/> is specified, <paramref name="value"/> must be
		///   assignable to this type. In case <paramref name="value"/> is a subclass of <paramref name="type"/>, the value is
		///   serialized as though it were of type <paramref name="type"/>.</param>
		/// <param name="attributes">Optional array of attributes used to change the encoding of the value.</param>
		/// <returns>A byte array containing the serialized value.</returns>
		/// <exception cref="ArgumentException"><paramref name="type"/> is supplied and <paramref name="value"/> is not assignable to that type.</exception>
		public byte[] SerializeMessage( object value, Type type, object[] attributes ) {
			MemoryStream stream = new MemoryStream();
			BitWriter writer = new BitWriter( stream );
			
			SerializeValue( writer, value, type, attributes );

			writer.Flush();
			return stream.ToArray();
		}

		/// <summary>
		/// Reads a value from the byte array.
		/// </summary>
		/// <param name="message">Byte array containing the serialized value.</param>
		/// <param name="type">The expected type of the value.</param>
		/// <param name="attributes">Optional array of attributes used to change the decoding of the value. These must be the
		///   same significant attributes used to encode the value.</param>
		/// <returns>The deserialized value.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='reader'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='type'/> is <see langword='null'/>.</para></exception>
		public object DeserializeMessage( byte[] message, Type type, object[] attributes ) {
			MemoryStream stream = new MemoryStream( message, false );
			BitReader reader = new BitReader( stream );
			
			return DeserializeValue( reader, type, attributes );
		}
	#endregion
	
	#region Serialization generator
		/// <summary>
		/// Gets an expression that serializes the given value expression.
		/// </summary>
		/// <param name="bitWriter"><see cref="ObjectProxy"/> for a <see cref="BitWriter"/> instance.</param>
		/// <param name="value"><see cref="Expression"/> for the value to be serialized. Because this expression will be used extensively,
		///   you should use as simple an expression for the value as possible. The base type for the serialization will be taken from this
		///   expression's <see cref="Expression.ResultType"/>.</param>
		/// <param name="paramName">Optional name of the parameter or field that the <paramref name="value"/> represents. This is used when throwing exceptions.
		///   If this is <see langword='null'/>, the name "value" is used.</param>
		/// <param name="attributes">Optional array of attributes that are used to determine how the value should be serialized.</param>
		/// <returns>An <see cref="Expression"/> for serializing the value using the same rules for serialization as used by <see cref="SerializeValue"/>.</returns>
		/// <remarks>The expression returned by this method is mostly the equivalent of placing a call to <see cref="SerializeValue"/>(
		///     <paramref name="bitWriter"/>, <paramref name="value"/>, {type of value's field or parameter}, <paramref name="attributes"/>, <paramref name="options"/> ).
		///   <para>Expressions returned by this method can be very large. You can consider capturing these expressions as methods in a dynamic
		///     type, but remember that the created expression varies on the <paramref name="attributes"/> and <paramref name="options"/> parameters.</para></remarks>
		/// <exception cref='ArgumentNullException'><paramref name='bitWriter'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='value'/> is <see langword='null'/>.</para></exception>
		/// <exception cref="ArgumentException"><paramref name="bitWriter"/> is not of a type derived from <see cref="BitWriter"/>.</exception>
		public Expression GetSerializeExpression( ObjectProxy bitWriter, Expression value, string paramName, object[] attributes ) {
			if( bitWriter == null )
				throw new ArgumentNullException( "bitWriter" );

			if( value == null )
				throw new ArgumentNullException( "value" );

			if( !typeof( BitWriter ).IsAssignableFrom( bitWriter.Type ) )
				throw new ArgumentException( "The given object proxy is not a BitWriter." );

			if( paramName == null )
				paramName = "value";

			return GetSerializeExpression( bitWriter, value, new BitSerializerParameterInfo( value.ResultType, paramName, attributes, _options ) );
		}

		private Expression GetSerializeExpression( ObjectProxy bitWriter, Expression value, BitSerializerParameterInfo attrs ) {
			if( bitWriter == null )
				throw new ArgumentNullException( "bitWriter" );

			if( value == null )
				throw new ArgumentNullException( "value" );
				
			if( !typeof(BitWriter).IsAssignableFrom( bitWriter.Type ) )
				throw new ArgumentException( "The given object proxy is not a BitWriter." );
				
			if( attrs == null )
				throw new ArgumentNullException( "attrs" );
				
			if( attrs.ParameterType != value.ResultType )
				throw new StackArgumentException( "value" );

			ListExpression topBlock = new ListExpression( true ), block = topBlock;
			Type type = attrs.ParameterType;
			
			if( attrs.IsIgnored )
				return new EmptyExpression();
			
			// Handle null values
			if( !value.ResultType.IsValueType ) {
				if( attrs.IsRequired ) {
					// Ensure it's not null
					topBlock.Add(
						If( value, null, ThrowArgNull( attrs.ParameterName ) )
					);
				}
				else {
					ListExpression ifBlock = new ListExpression( true );
					ifBlock.Add( bitWriter.Call( "Write", true ) );
					
					topBlock.Add( If( value, ifBlock, bitWriter.Call( "Write", false ) ) );
					block = ifBlock;
				}
			}
			
			if( type.IsEnum )
				type = Enum.GetUnderlyingType( type );

			switch( Type.GetTypeCode( type ) ) {
				case TypeCode.Boolean:
				case TypeCode.Char:
					block.Add( bitWriter.Call( "Write", value ) );
					break;
					
				case TypeCode.DateTime: {
					ObjectProxy date = ObjectProxy.Wrap( value );
					block.Add( bitWriter.Call( "Write", date.Prop( "Ticks" ), 64 ) );
				}
					break;

				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
					block.Add( GetSerializeIntegerExpression( bitWriter, value.ResultType.IsEnum ? Cast( value, type ) : value, attrs ) );
					break;

				case TypeCode.String:
					block.Add( bitWriter.Call( "WriteString", value, attrs.MaxLength ) );
					break;

				case TypeCode.Single:
					block.Add( bitWriter.Call( "WriteSingle", value ) );
					break;

				case TypeCode.Object:
					if( value.ResultType.IsArray ) {
						// Array
						ObjectProxy array = ObjectProxy.Wrap( value );
						Local i;
						
						// Prefix with the length of the array
						block.AddRange(
							Comment( "{0} array", attrs.ParameterName ),
							//  if( array.Length > maxLength )
							//  	throw new ArgumentException( "Array length is greater than the maximum length for this field.", "value" );
							If( GreaterThan( array.Prop( "Length" ), attrs.MaxLength ),
								Throw( New( typeof(ArgumentException), "Array length is greater than the maximum length for this field.", attrs.ParameterName ) ), null ),

							Comment( "Prefix with the length of the array" ),
							bitWriter.Call( "Write", array.Prop( "Length" ), GetPrecision( (ulong) attrs.MaxLength ) ),
							BlankLine,
							
							For( Declare( typeof(int), attrs.ParameterName + "Index", 0, out i ), LessThan( i, array.Prop( "Length" ) ), Increment( i ),
								GetSerializeExpression( bitWriter, array[i], attrs.ParameterName, attrs.GetElementAttributes() ) )
						);
					}
					else if( value.ResultType == typeof(Guid) ) {
						ObjectProxy guid = ObjectProxy.Wrap( value );
						block.Add(
							bitWriter.Call( "WriteBytes", guid.Call( "ToByteArray" ), 0, 16 )
						);
					}
					else {
						block.Add( GetSerializeTypeWithCodeExpression( bitWriter, value, attrs ) );
					}
					break;

				case TypeCode.DBNull:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Empty:
				default:
					throw new NotImplementedException( string.Format( "Serialization is not implemented for fields of type {0}.", value.ResultType.Name ) );
			}
			
			return topBlock;
		}

		private static Expression GetSerializeIntegerExpression( ObjectProxy bitWriter, Expression value, BitSerializerParameterInfo attrs ) {
			Expression preCondExpr;
			Expression expr = bitWriter.Call( "Write", attrs.GetSerializableIntegerExpression( value, out preCondExpr ),
				attrs.Precision );

			if( preCondExpr != null )
				return new ListExpression( new Expression[] { preCondExpr, expr }, true );
			else
				return expr;
		}

		private Expression GetSerializeTypeWithCodeExpression( ObjectProxy bitWriter, Expression value, BitSerializerParameterInfo attrs ) {
			if( bitWriter == null )
				throw new ArgumentNullException( "bitWriter" );

			if( value == null )
				throw new ArgumentNullException( "value" );

			if( !attrs.NeedsTypeCode ) {
				return new ListExpression( new Expression[] {
						BlankLine,
						GetSerializeCompoundTypeExpression( bitWriter, value )
					} );
			}

			ConditionalExpression lastTest = null;

			foreach( DerivedTypeCodeAttribute attr in attrs.GetTypeCodes() ) {
				if( attr.Type == null )
					continue;

				Local local;
				lastTest = If( Is( value, attr.Type ),
					List(
						// SomeType valueSomeType = (SomeType) value;
						Declare( attr.Type, attrs.ParameterName + attr.Type.Name, Cast( value, attr.Type ), out local ),
						// writer.Write( 12 );
						bitWriter.Call( "Write", attr.TypeCode, attrs.TypeCodePrecision ),
						BlankLine,
						GetSerializeCompoundTypeExpression( bitWriter, local.Get() )
					), lastTest );
			}

			return lastTest;
		}

		bool _sMethodInProduction;
		
		private Expression GetSerializeCompoundTypeExpression( ObjectProxy bitWriter, Expression value ) {
			// BJC: There's a major point to understand about this method and about caching the serialization
			// methods in general, and that's the fact that no custom attributes pass this point. We're only
			// able to do caching at this level because custom attributes on a field do not apply to the members
			// of a compound type. Custom attributes *in* the type apply, but none from its container.
			
			if( _serializeMethods != null ) {
				lock( _localLock ) {
					MethodInfo cachedMethod;
					bool topMethod = !_sMethodInProduction;
					
					if( !_serializeMethods.TryGetValue( value.ResultType, out cachedMethod ) ) {
						_sMethodInProduction = true;
						Type type = value.ResultType;
						
						if( _methodCacheType == null )
							CreateMethodCache();
						
						MethodGeneratorContext methodCxt = _methodCacheType.DefineMethod( "S_" + type.Name + "_" + type.MetadataToken.ToString(),
							MethodAttributes.Public | MethodAttributes.Static,
							typeof( void ), new Type[] { typeof( BitWriter ), type } );

						Param paramBitWriter = methodCxt.DefineParameter( 0, "bitWriter" );
						Param paramValue = methodCxt.DefineParameter( 1, "value" );
						
						_serializeMethods[type] = methodCxt.Method;
						
						methodCxt.AddExpression( If( paramBitWriter, null, ThrowArgNull( "bitWriter" ) ) );
						
						if( !type.IsValueType )
							methodCxt.AddExpression( If( paramValue, null, ThrowArgNull( "value" ) ) );
						
						methodCxt.AddExpression( GenerateSerializeCompoundTypeExpression( paramBitWriter, paramValue ) );
						methodCxt.EmitBody();
						
						cachedMethod = methodCxt.Method;
					}

					if( _methodCacheType != null && topMethod ) {
						_methodCacheType.CreateType();
						_methodCacheType = null;
					}

					_sMethodInProduction = !topMethod;
			
					// Just generate a call to the cached method
					return Call( cachedMethod, bitWriter, value );
				}
			}
			
			return GenerateSerializeCompoundTypeExpression( bitWriter, value );
		}
		
		private Expression GenerateSerializeCompoundTypeExpression( ObjectProxy bitWriter, Expression value ) {
			return new ListExpression(
				Array.ConvertAll<FieldInfo, Expression>( GetSerializableFields( value.ResultType, true ), delegate( FieldInfo field ) {
					return GetSerializeExpression( bitWriter, Field( value, field ), field.Name, field.GetCustomAttributes( false ) );
				} )
			);
		}
	#endregion
	
	#region Deserialization generator
		public Expression GetDeserializeExpression( ObjectProxy bitReader, IDataStore result, string paramName, object[] attributes ) {
			if( bitReader == null )
				throw new ArgumentNullException( "bitReader" );

			if( result == null )
				throw new ArgumentNullException( "result" );

			if( !typeof( BitReader ).IsAssignableFrom( bitReader.Type ) )
				throw new ArgumentException( "The given object proxy is not a BitReader." );

			if( paramName == null )
				paramName = "value";

			Local ulongTemp;
			return List(
				Declare<ulong>( "ulongTemp", out ulongTemp ),
				GetDeserializeExpression( bitReader, result, ulongTemp, new BitSerializerParameterInfo( result.Type, paramName, attributes, _options ) )
			);
		}

		private Expression GetDeserializeExpression( ObjectProxy bitReader, IDataStore result, Local ulongTemp, BitSerializerParameterInfo attrs ) {
			if( bitReader == null )
				throw new ArgumentNullException( "bitReader" );

			if( !typeof( BitReader ).IsAssignableFrom( bitReader.Type ) )
				throw new ArgumentException( "The given object proxy is not a BitReader." );

			if( result.Type != attrs.ParameterType )
				throw new StackArgumentException( "result" );
				
			if( ulongTemp == null )
				throw new ArgumentNullException( "ulongTemp" );
				
			if( ulongTemp.Type != typeof(ulong) )
				throw new StackArgumentException( "ulongTemp" );

			ListExpression topBlock = new ListExpression( false ), block = topBlock;
			Type type = attrs.ParameterType;

			if( attrs.IsIgnored )
				return null;

			// Handle null values
			if( !result.Type.IsValueType && !attrs.IsRequired ) {
				ListExpression ifBlock = new ListExpression( true );
				topBlock.Add( If( bitReader.Call( "ReadBoolean" ), ifBlock, result.Set( Null( type ) ) ) );
				block = ifBlock;
			}

			if( type.IsEnum )
				type = Enum.GetUnderlyingType( type );

			switch( Type.GetTypeCode( type ) ) {
				case TypeCode.Boolean:
					block.Add( result.Set( bitReader.Call( "ReadBoolean" ) ) );
					break;

				case TypeCode.Char:
					block.Add( result.Set( bitReader.Call( "ReadChar" ) ) );
					break;

				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					block.Add( GetDeserializedIntegerExpression( bitReader, result, ulongTemp, attrs ) );
					break;

				case TypeCode.String:
					block.Add( result.Set( bitReader.Call( "ReadString", attrs.MaxLength ) ) );
					break;

				case TypeCode.DateTime:
				case TypeCode.Single:
					throw new NotImplementedException( string.Format( "Serialization is not implemented for fields of type {0}.", type.Name ) );

				case TypeCode.Object:
					if( type.IsArray ) {
						int maxLength = attrs.MaxLength;
						ObjectProxy array = ObjectProxy.Wrap( result );
						
						Local i;
						
						block.AddRange(
							array.SetNewArray( bitReader.Call( "ReadInt32", GetPrecision( (ulong) maxLength ) ) ),
							For( Declare<int>( "i", 0, out i ), LessThan( i, array.Prop( "Length" ) ), Increment( i ), 
								GetDeserializeExpression( bitReader, array[i], ulongTemp, attrs.GetElementParameterInfo() ) )
						);
					}
					else if( type == typeof(Guid) ) {
						block.Add( result.Set( New( typeof(Guid), bitReader.Call( "ReadBytes", 16 ) ) ) );
					}
					else {
						block.Add( GetDeserializeTypeWithCodeExpression( bitReader, ObjectProxy.Wrap( result ), ulongTemp, attrs ) );
					}
					break;

				case TypeCode.DBNull:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Empty:
				default:
					throw new NotImplementedException( string.Format( "Serialization is not implemented for fields of type {0}.", result.Type.Name ) );
			}
			
			return topBlock;
		}

		private static Expression GetDeserializedIntegerExpression( ObjectProxy bitReader, IDataStore result, Local ulongTemp, BitSerializerParameterInfo attrs ) {
			Expression preCondExpr;
			Expression expr = result.Set( attrs.GetDeserializedIntegerExpression( ulongTemp, out preCondExpr ) );

			if( preCondExpr != null ) {
				return List( ulongTemp.Set( bitReader.Call( "ReadUInt64", attrs.Precision ) ), preCondExpr, expr );
			}
			else {
				return List( result.Set( attrs.GetDeserializedIntegerExpression( bitReader.Call( "ReadUInt64", attrs.Precision ), out preCondExpr ) ) );
			}
		}

		private Expression GetDeserializeTypeWithCodeExpression( ObjectProxy bitReader, ObjectProxy result, Local ulongTemp, BitSerializerParameterInfo attrs ) {
			if( bitReader == null )
				throw new ArgumentNullException( "bitReader" );

			if( result == null )
				throw new ArgumentNullException( "result" );

			if( !attrs.NeedsTypeCode ) {
				return new ListExpression( new Expression[] {
						GetDeserializeCompoundTypeExpression( bitReader, result, ulongTemp )
					} );
			}

			Expression[] switchExprs = new Expression[attrs.MaxTypeCode + 1];
			
			Local typeCodeLocal;
			Expression typeCodeExpr = bitReader.Call( "ReadInt32", attrs.TypeCodePrecision );
						
			foreach( DerivedTypeCodeAttribute attr in attrs.GetTypeCodes() ) {
				if( attr.Type == null )
					continue;
					
				if( switchExprs[attr.TypeCode] != null )
					throw new Exception( "The type code " + attr.TypeCode.ToString() + " was specified twice for " + attrs.ParameterName + "." );
					
				if( !attrs.ParameterType.IsAssignableFrom( attr.Type ) ) {
					throw new Exception( string.Format( "One of the type codes for {0} lists a type {1}, which is not assignable to type {2}.",
						attrs.ParameterName, attr.Type, attrs.ParameterType ) );
				}

				Local loc;
				
				switchExprs[attr.TypeCode] = List(
					Declare( attr.Type, "value" + attr.Type.Name, New( attr.Type ), out loc ),
					GetDeserializeCompoundTypeExpression( bitReader, loc, ulongTemp ),
					result.Set( loc )
				);
			}
			
			if( attrs.StoreTypeCodeField != null ) {
				return List(
					Declare( attrs.StoreTypeCodeField.FieldType, "typeCode", typeCodeExpr, out typeCodeLocal ),
					Switch( typeCodeLocal, switchExprs,
						Throw( typeof( ArgumentException ), "Unexpected type code.", attrs.ParameterName ) ),
					result.Field( attrs.StoreTypeCodeField.Name ).Set( typeCodeLocal )
				);
			}
			else {
				return Switch( typeCodeExpr, switchExprs,
					Throw( typeof(ArgumentException), "Unexpected type code.", attrs.ParameterName ) );
			}
		}

		bool _dMethodInProduction;
		
		private Expression GetDeserializeCompoundTypeExpression( ObjectProxy bitReader, ObjectProxy result, Local ulongTemp ) {
			if( _deserializeMethods != null ) {
				lock( _localLock ) {
					MethodInfo cachedMethod;
					bool topMethod = !_dMethodInProduction;

					if( !_deserializeMethods.TryGetValue( result.Type, out cachedMethod ) ) {
						_dMethodInProduction = true;
						Type type = result.Type;
						
						if( _methodCacheType == null )
							CreateMethodCache();
						
						MethodGeneratorContext methodCxt = _methodCacheType.DefineMethod( "D_" + type.Name + "_" + type.MetadataToken.ToString(),
							MethodAttributes.Public | MethodAttributes.Static,
							type, new Type[] { typeof( BitReader ) } );

						Param paramBitReader = methodCxt.DefineParameter( 0, "bitReader" );

						_deserializeMethods[type] = methodCxt.Method;

						methodCxt.AddExpression( If( paramBitReader, null, ThrowArgNull( "bitReader" ) ) );
						
						Local localResult, localULongTemp;
						
						methodCxt.AddExpressionRange(
							Declare( type, "result", type.IsValueType ? null : New( type ), out localResult ),
							Declare( typeof(ulong), "ulongTemp", out localULongTemp ),
							BlankLine,
							GenerateDeserializeCompoundTypeExpression( paramBitReader, localResult, localULongTemp ),
							BlankLine,
							Return( localResult )
						);
						
						methodCxt.EmitBody();

						cachedMethod = methodCxt.Method;
					}
					
					if( _methodCacheType != null && topMethod ) {
						_methodCacheType.CreateType();
						_methodCacheType = null;
					}
					
					_dMethodInProduction = !topMethod;

					// Just generate a call to the cached method
					return result.Set( Call( cachedMethod, bitReader ) );
				}
			}

			return GenerateDeserializeCompoundTypeExpression( bitReader, result, ulongTemp );
		}

		private Expression GenerateDeserializeCompoundTypeExpression( ObjectProxy bitReader, ObjectProxy result, Local ulongTemp ) {
			return new ListExpression(
				Array.ConvertAll<FieldInfo, Expression>( GetSerializableFields( result.Type, true ), delegate( FieldInfo field ) {
					return GetDeserializeExpression( bitReader, new Field( result.Get(), field ), ulongTemp, 
						new BitSerializerParameterInfo( field, _options ) );
				} )
			);
		}
	#endregion

		public RequestReceiverFactory GenerateRequestReceiverFactory( Type interfaceType ) {
			if( interfaceType == null )
				throw new ArgumentNullException( "interfaceType" );

			if( !interfaceType.IsInterface )
				throw new ArgumentException( "The given type is not an interface." );
			
			if( _receiverTypes == null )
				throw new InvalidOperationException( "This type can only be generated when a cache module is supplied to the constructor." );
			
			lock( _localLock ) {
				Type receiverType;
				
				if( _receiverTypes.TryGetValue( interfaceType, out receiverType ) )
					return new LocalReceiverFactory( interfaceType, receiverType );

				TypeGeneratorContext typeCxt = _cacheModule.DefineType( "__requestReceiver." + interfaceType.FullName.Replace( '+', '_' ),
					TypeAttributes.Public | TypeAttributes.Sealed );
				typeCxt.AddInterface( typeof(IRequestReceiver) );
				Field targetField = typeCxt.DefineField( "_target", interfaceType, FieldAttributes.Private );
				
				ConstructorGeneratorContext ctorCxt = typeCxt.DefineConstructor(MethodAttributes.Public, new Type[] { interfaceType } );
				Param target = ctorCxt.DefineParameter( 0, "target" );
				
				ctorCxt.AddExpressionRange(
					Call( typeof( object ).GetConstructor( Type.EmptyTypes ), ctorCxt.This ),
					If( target.IsNull, ThrowArgNull( "target" ), null ),
					targetField.Set( target )
				);
				
				MethodGeneratorContext methodCxt = typeCxt.DefineOverrideMethod( typeof(IRequestReceiver).GetMethod("ProcessRequest") );
				Param request = methodCxt.DefineParameter( 0, "request" );
				
				object[] attributes = interfaceType.GetCustomAttributes( typeof(MaxLengthAttribute), false );
				MaxLengthAttribute maxLength = (attributes.Length != 0) ? (MaxLengthAttribute) attributes[0] : null;
				int methodPrecision = (maxLength != null) ? maxLength.Precision : 32;
				Local reader, ulongTemp;
				
				methodCxt.AddExpressionRange(
					If( request.IsNull, ThrowArgNull( "request" ), null ),
					Declare<BitReader>( "reader", New( typeof(BitReader), request.Call( "GetRequestStream" ) ), out reader ),
					Declare<ulong>( "ulongTemp", out ulongTemp )
				);
					
				MethodInfo[] methods = Array.FindAll<MethodInfo>(
					interfaceType.GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy ),
					delegate( MethodInfo method ) {
						return method.GetCustomAttributes( typeof(IgnoreAttribute), true ).Length == 0;
					}
				);
				
				if( maxLength != null && methods.Length >= maxLength.MaxLength )
					throw new Exception( "There are too many methods for the given maximum length." );
					
				if( maxLength == null )
					methodPrecision = GetPrecision( (ulong) methods.Length );
					
				Expression[] cases = Array.ConvertAll<MethodInfo, Expression>( methods, delegate( MethodInfo method ) {
					ListExpression block = new ListExpression( true );
					bool isOneWay = method.GetCustomAttributes( typeof( OneWayAttribute ), false ).Length != 0;
					
					if( method.ReturnType != typeof(void) )
						throw new Exception( "Can't handle return types yet!! Ack!" );

					List<Expression> @params = new List<Expression>( Array.ConvertAll<ParameterInfo, Expression>( method.GetParameters(),
						delegate( ParameterInfo param ) {
							Local paramLocal;
							block.Add( Declare( param.ParameterType, param.Name, out paramLocal ) );
							block.Add( GetDeserializeExpression( reader, paramLocal, ulongTemp, new BitSerializerParameterInfo( param, _options ) ) );
							return paramLocal;
						}
					) );
					
					@params.Insert( 0, targetField );
					block.Add( Call( method, @params.ToArray() ) );
					block.Add( reader.Call( "Close" ) );
					
					if( !isOneWay )
						block.Add( Call( typeof( Stream ).GetMethod( "Close" ), request.Call( "GetResponseStream" ) ) );
					
					return block;
				} );
					
				methodCxt.AddExpression( Switch( reader.Call( "ReadInt32", methodPrecision ), cases, Throw( typeof(IOException), "Invalid method code for this interface." ) ) );
				
				receiverType = typeCxt.CreateType();
				_receiverTypes[interfaceType] = receiverType;
				
				return new LocalReceiverFactory( interfaceType, receiverType );
			}
		}

		public RequestSenderFactory GenerateRequestSenderFactory( Type interfaceType ) {
			if( interfaceType == null )
				throw new ArgumentNullException( "interfaceType" );

			if( !interfaceType.IsInterface )
				throw new ArgumentException( "The given type is not an interface." );

			if( _senderTypes == null )
				throw new InvalidOperationException( "This type can only be generated when a cache module is supplied to the constructor." );

			lock( _localLock ) {
				Type senderType;

				if( _senderTypes.TryGetValue( interfaceType, out senderType ) )
					return new LocalSenderFactory( senderType );

				TypeGeneratorContext typeCxt = _cacheModule.DefineType( "__requestSender." + interfaceType.FullName.Replace( '+', '_' ),
					TypeAttributes.Public | TypeAttributes.Sealed );
				typeCxt.AddInterface( interfaceType );
				Field targetField = typeCxt.DefineField( "_target", typeof(IRequestTarget), FieldAttributes.Private );

				ConstructorGeneratorContext ctorCxt = typeCxt.DefineConstructor( MethodAttributes.Public, new Type[] { typeof(IRequestTarget) } );
				Param target = ctorCxt.DefineParameter( 0, "target" );

				ctorCxt.AddExpressionRange(
					Call( typeof(object).GetConstructor( Type.EmptyTypes ), ctorCxt.This ),
					If( target.IsNull, ThrowArgNull( "target" ), null ),
					targetField.Set( target )
				);

				MethodInfo[] methods = Array.FindAll<MethodInfo>(
					interfaceType.GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy ),
					delegate( MethodInfo method ) {
						return method.GetCustomAttributes( typeof( IgnoreAttribute ), true ).Length == 0;
					}
				);

				object[] attributes = interfaceType.GetCustomAttributes( typeof( MaxLengthAttribute ), false );
				MaxLengthAttribute maxLength = (attributes.Length != 0) ? (MaxLengthAttribute) attributes[0] : null;
				int methodPrecision = (maxLength != null) ? maxLength.Precision : 32;
				int currentMethod = 0;

				if( maxLength != null && methods.Length >= maxLength.MaxLength )
					throw new Exception( "There are too many methods for the given maximum length." );

				if( maxLength == null )
					methodPrecision = GetPrecision( (ulong) methods.Length );

				foreach( MethodInfo method in methods ) {
					MethodGeneratorContext methodCxt = typeCxt.DefineOverrideMethod( method );
					Local writer, request;
					bool isOneWay = method.GetCustomAttributes( typeof(OneWayAttribute), false ).Length != 0;

					methodCxt.AddExpressionRange(
						Declare<IStreamRequest>( "request", targetField.Call( "StartRequest", isOneWay ), out request ),
						Declare<BitWriter>( "writer", New( typeof(BitWriter), request.Call( "GetRequestStream" ) ), out writer ),
						BlankLine,
						writer.Call( "Write", currentMethod, methodPrecision )
					);
					
					foreach( ParameterInfo param in method.GetParameters() ) {
						methodCxt.AddExpression( GetSerializeExpression( writer, methodCxt.Param( param.Position ), new BitSerializerParameterInfo( param, _options ) ) );
					}
					
					methodCxt.AddExpression( writer.Call( "Close" ) );
					
					if( !isOneWay )
						methodCxt.AddExpression( ObjectProxy.Wrap( request.Call( "GetResponseStream" ) ).Call( "Close" ) );
					
					currentMethod++;
				}

				senderType = typeCxt.CreateType();
				_senderTypes[interfaceType] = senderType;

				return new LocalSenderFactory( senderType );
			}
		}

		private class LocalReceiverFactory : RequestReceiverFactory {
			Type _receiverType, _interfaceType;
			
			public LocalReceiverFactory( Type interfaceType, Type receiverType ) : base( receiverType.GUID ) {
				if( interfaceType == null )
					throw new ArgumentNullException( "interfaceType" );
					
				if( receiverType == null )
					throw new ArgumentNullException( "receiverType" );

				_interfaceType = interfaceType;
				_receiverType = receiverType;
			}
		
			public override IRequestReceiver CreateRequestReceiver( object target ) {
				if( target == null )
					throw new ArgumentNullException( "target" );
					
				if( !_interfaceType.IsAssignableFrom( target.GetType() ) ) {
					IServiceProvider provider = target as IServiceProvider;
					
					if( provider == null )
						throw new ArgumentException( "The given target was not a valid target." );
						
					target = provider.GetService( _interfaceType );
					
					if( target == null )
						throw new Exception( "The requested interface was not found in the service domain." );
				}
				
				return (IRequestReceiver) Activator.CreateInstance( _receiverType, target );
			}
		}

		private class LocalSenderFactory : RequestSenderFactory {
			Type _senderType;

			public LocalSenderFactory( Type senderType ) : base( senderType.GUID ) {
				if( senderType == null )
					throw new ArgumentNullException( "senderType" );

				_senderType = senderType;
			}

			public override object CreateRequestSender( IRequestTarget target ) {
				return Activator.CreateInstance( _senderType, target );
			}
		}

		public void Dispose() {
			if( _methodCacheType != null ) {
				_methodCacheType.CreateType();
				_methodCacheType = null;
			}
		}

	#region GetSerializableFields
		/// <summary>
		/// Gets the serializable fields of the given type.
		/// </summary>
		/// <param name="type"><see cref="Type"/> from which to retrieve the serializable fields.</param>
		/// <param name="includeInherited">True to include inherited fields, false otherwise. If true, the fields
		///   of the base type appear, sorted, before the fields of the derived type.</param>
		/// <returns>The serializable members of the type.</returns>
		private static FieldInfo[] GetSerializableFields( Type type, bool includeInherited ) {
			// Get the properties and fields and sort them.
			FieldInfo[] declaredMembers = type.GetFields( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly );
			SortMembers( declaredMembers );

			// If there are no base members to return, return now
			if( type.BaseType == null || !includeInherited )
				return declaredMembers;

			// Collect the base members and add them to the beginning of our list
			FieldInfo[] baseMembers = GetSerializableFields( type.BaseType, true );
			FieldInfo[] result = new FieldInfo[baseMembers.Length + declaredMembers.Length];
			baseMembers.CopyTo( result, 0 );
			declaredMembers.CopyTo( result, baseMembers.Length );

			return result;
		}

		private static void SortMembers( MemberInfo[] members ) {
			if( members == null )
				throw new ArgumentNullException( "members" );
				
			Array.Sort<MemberInfo>( members, delegate( MemberInfo x, MemberInfo y ) {
				return x.Name.CompareTo( y.Name );
			} );
		}
	#endregion
		
		/// <summary>
		/// Gets the minimum number of bits necessary to store the given range of integers.
		/// </summary>
		/// <param name="rangeLength">Number of integers to represent.</param>
		/// <returns>The number of bits needed to store the value. To achieve this, offset the
		///	  stored value so that the result is nonnegative before encoding.</returns>
		[CLSCompliant( false )]
		public static int GetPrecision( ulong rangeLength ) {
			if( rangeLength == 0 )
				return 0;

			// Construct a mask and walk it up until we get the minimum number of bits necessary
			ulong mask = 0x1UL;
			int precision = 1;

			while( mask < rangeLength ) {
				mask <<= 1;
				mask |= 1;
				precision++;
			}

			return precision;
		}
	}
}
