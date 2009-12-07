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
//using System.Collections;
using System.Reflection;
using System.IO;
using Fluggo.CodeGeneration.IL;
using System.Collections.Generic;

namespace Fluggo.Communications.Serialization
{
	/// <summary>
	/// Interprets the serialization options represented by a list of attributes.
	/// </summary>
	class BitSerializerParameterInfo
	{
		object[] _attributes;
		RangeAttribute _rangeAttr;
		bool _required = false, _ignored = false;
		SerializationAttribute[] _elemAttrs;
		List<DerivedTypeCodeAttribute> _typeList = new List<DerivedTypeCodeAttribute>();
		int _maxTypeCode;
		int _typeCodePrecision;
		static readonly BitSerializerOptions __defaultOptions = new BitSerializerOptions();
		BitSerializerOptions _options;
		Type _paramType;
		int _maxLength = -1;
		string _paramName;
		FieldInfo _storeTypeCodeField;

		public BitSerializerParameterInfo( FieldInfo field, BitSerializerOptions options ) :
			this( field.FieldType, field.Name, field.GetCustomAttributes( true ), options ) {
		}
		
		public BitSerializerParameterInfo( ParameterInfo param, BitSerializerOptions options ) :
			this( param.ParameterType, param.Name, param.GetCustomAttributes( true ), options ) {
		}
		
		/// <summary>
		/// Creates a new instance of the <see cref='BitSerializerParameterInfo'/> class.
		/// </summary>
		/// <param name="paramType">Declared type of the field.</param>
		/// <param name="paramName">Optional name for the field or parameter being described.</param>
		/// <param name="attributes">Array of attributes containing the serialization options for this field.
		///   Non-serialization attributes in this list are ignored.</param>
		/// <param name="options">Optional <see cref="BitSerializerOptions"/> instance with additional options.</param>
		/// <exception cref='ArgumentNullException'><paramref name='paramType'/> is <see langword='null'/>.</exception>
		public BitSerializerParameterInfo( Type paramType, string paramName, object[] attributes, BitSerializerOptions options ) {
			if( paramType == null )
				throw new ArgumentNullException( "fieldType" );

			_paramName = paramName;
			_paramType = paramType;
			_attributes = attributes;
			_options = options;

			if( _options == null )
				_options = __defaultOptions;

			if( _attributes == null )
				return;

			if( _paramName == null )
				_paramName = "(unnamed parameter)";

			// Collect the type code attributes, if any
			List<SerializationAttribute> dynElemAttrs = new List<SerializationAttribute>();
			_maxTypeCode = -1;

			foreach( object attr in _attributes ) {
				SerializationAttribute serAttr = attr as SerializationAttribute;
				DerivedTypeCodeAttribute codeAttr = attr as DerivedTypeCodeAttribute;
				StoreTypeCodeAttribute storeAttr = attr as StoreTypeCodeAttribute;
				
				if( serAttr == null )
					break;
					
				if( serAttr.ArrayElement ) {
					dynElemAttrs.Add( serAttr );
				}
				if( codeAttr != null ) {
					if( codeAttr.TypeCode > _maxTypeCode )
						_maxTypeCode = codeAttr.TypeCode;

					_typeList.Add( codeAttr );
				}
				else if( storeAttr != null ) {
					_storeTypeCodeField = paramType.GetField( storeAttr.FieldName, BindingFlags.Public | BindingFlags.Instance );
					
					if( _storeTypeCodeField == null )
						throw new Exception( "The public instance field \"" + storeAttr.FieldName + ",\" which was specified in a StoreTypeCodeAttribute, was not found." );
				}
				else if( attr is RangeAttribute ) {
					_rangeAttr = (RangeAttribute) attr;
				}
				else if( attr is MaxLengthAttribute ) {
					_maxLength = ((MaxLengthAttribute) attr).MaxLength;
				}
				else if( attr is RequiredAttribute ) {
					_required = true;
				}
				else if( attr is IgnoreAttribute ) {
					_ignored = true;
				}
			}

			// Produce the type encoding table
			if( _maxTypeCode != -1 ) {
				_typeList.Sort();
				_typeCodePrecision = BitSerializer.GetPrecision( (ulong) (_maxTypeCode) );
			}

			if( dynElemAttrs.Count != 0 )
				_elemAttrs = dynElemAttrs.ToArray();
		}

		/// <summary>
		/// Gets the name of the underlying parameter or field.
		/// </summary>
		/// <value>The name of the underlying parameter or field.</value>
		public string ParameterName {
			get {
				return _paramName;
			}
		}

		/// <summary>
		/// Gets the base type of the parameter or field.
		/// </summary>
		/// <value>The base type of the parameter or field.</value>
		public Type ParameterType {
			get {
				return _paramType;
			}
		}
		
		public FieldInfo StoreTypeCodeField {
			get {
				return _storeTypeCodeField;
			}
		}
		
		public void StoreTypeCodeFieldValue( object obj, int typeCode ) {
			if( _storeTypeCodeField == null )
				return;
				
			Type fieldType = _storeTypeCodeField.FieldType;
			
			if( fieldType.IsEnum )
				fieldType = fieldType.UnderlyingSystemType;
				
			object value;

			switch( Type.GetTypeCode( fieldType ) ) {
				case TypeCode.Byte:
					value = (byte) typeCode;
					break;

				case TypeCode.Int16:
					value = (short) typeCode;
					break;

				case TypeCode.Int32:
					value = typeCode;
					break;

				case TypeCode.Int64:
					value = (long) typeCode;
					break;

				case TypeCode.SByte:
					value = (sbyte) typeCode;
					break;

				case TypeCode.UInt16:
					value = (ushort) typeCode;
					break;

				case TypeCode.UInt32:
					value = (uint) typeCode;
					break;

				case TypeCode.UInt64:
					value = (ulong) typeCode;
					break;

				default:
					throw new Exception( "The field specified in the StoreTypeCodeAttribute was not an integral type." );
			}
			
			_storeTypeCodeField.SetValue( obj, value );
		}
		
		/// <summary>
		/// Gets a value that represents whether the field is required.
		/// </summary>
		/// <value>True if the field is required, false otherwise. This generally only applies to reference types.</value>
		public bool IsRequired { get { return _required; } }

		/// <summary>
		/// Gets a value that represents whether the field should be ignored.
		/// </summary>
		/// <value>True if the field should be ignored, false otherwise. If it is ignored, it should not be serialized.</value>
		public bool IsIgnored { get { return _ignored; } }

		public object[] GetElementAttributes() {
			// Direct reference because it shouldn't leave the caller
			return _elemAttrs;
		}
		
		public BitSerializerParameterInfo GetElementParameterInfo() {
			return new BitSerializerParameterInfo( _paramType.GetElementType(), ParameterName, _elemAttrs, _options );
		}

		/// <summary>
		/// Gets the maximum length of the field.
		/// </summary>
		/// <value>The maximum length of the field, or -1 if the maximum length isn't specified.
		///   In general, this only applies to arrays and strings.</value>
		public int MaxLength {
			get {
				if( _maxLength == -1 )
					return _paramType.IsArray ? _options.MaxArrayLength : _options.MaxStringLength;

				return _maxLength;
			}
		}

		/// <summary>
		/// Determines the serializable type of a value and writes its type code to the given stream.
		/// </summary>
		/// <param name="writer"><see cref="BitWriter"/> to which to write the type code.</param>
		/// <param name="valueType">Type of the value of the field.</param>
		/// <param name="serializedType">Reference to a <see cref="Type"/> variable. On return, this contains
		///   the type that should be serialized, which may be different from <paramref name="valueType"/>.</param>
		/// <exception cref='ArgumentNullException'><paramref name='writer'/> is <see langword='null'/>.
		///   <para>— OR —</para>
		///   <para><paramref name='valueType'/> is <see langword='null'/>.</para></exception>
		public void SerializeTypeCode( BitWriter writer, Type valueType, out Type serializedType ) {
			if( writer == null )
				throw new ArgumentNullException( "writer" );

			if( _typeList == null ) {
				serializedType = _paramType;
				return;
			}

			writer.Write( GetTypeCode( valueType, out serializedType ), _typeCodePrecision );
		}

		public bool NeedsTypeCode {
			get {
				return !_paramType.IsValueType && (_typeList != null) && (_typeList.Count != 0);
			}
		}

		/// <summary>
		/// Gets the number of bits needed to store the field itself.
		/// </summary>
		/// <value>The number of bits needed to store the field itself.</value>
		/// <exception cref="InvalidOperationException">The field or parameter is not of an integral type.</exception>
		public int Precision {
			get {
				// If there's a range set, use its precision
				if( _rangeAttr != null )
					return BitSerializer.GetPrecision( _rangeAttr.RangeLength );

				// Return the default
				switch( Type.GetTypeCode( _paramType ) ) {
					case TypeCode.Byte:
					case TypeCode.SByte:
						return 8;

					case TypeCode.Int16:
					case TypeCode.UInt16:
						return 16;

					case TypeCode.Int32:
					case TypeCode.UInt32:
						return 32;

					case TypeCode.Int64:
					case TypeCode.UInt64:
						return 64;

					default:
						throw new InvalidOperationException( "The field or parameter is not of an integral type." );
				}
			}
		}

		/// <summary>
		/// Gets the type code attributes in the order they should be evaluated for serialization.
		/// </summary>
		/// <returns>An <see cref="IEnumerable{T}"/> with <see cref="DerivedTypeCodeAttribute"/> values for the field.</returns>
		public IEnumerable<DerivedTypeCodeAttribute> GetTypeCodes() {
			foreach( DerivedTypeCodeAttribute attr in _typeList ) {
				yield return attr;
			}
		}

		/// <summary>
		/// Determines the type of value stored in the given stream.
		/// </summary>
		/// <param name="reader"><see cref="BitReader"/> from which to read the type of this field.</param>
		/// <param name="type">Reference to a <see cref="Type"/> variable. On return, this contains
		///   the type of the value stored in the stream, or <see langword='null'/> if the type code was unrecognized.</param>
		/// <returns>The type code stored in the stream.</returns>
		/// <exception cref='ArgumentNullException'><paramref name='reader'/> is <see langword='null'/>.</exception>
		/// <exception cref="Exception">The type code stored in the stream was larger than expected. This is not an unrecognized
		///   type error, but represents an error in the data stream itself.</exception>
		public int DeserializeTypeCode( BitReader reader, out Type type ) {
			if( reader == null )
				throw new ArgumentNullException( "reader" );

			if( _typeList == null ) {
				type = _paramType;
				return 0;
			}

			int typeCode = reader.ReadInt32( _typeCodePrecision );

			if( typeCode > _maxTypeCode )
				throw new Exception( "The type code was larger than expected." );

			foreach( DerivedTypeCodeAttribute attr in _typeList ) {
				if( typeCode == attr.TypeCode ) {
					type = attr.Type;
					return typeCode;
				}
			}

			throw new Exception( "Unexpected type code." );
		}

		public int MaxTypeCode {
			get {
				if( !NeedsTypeCode )
					throw new InvalidOperationException();
				
				return _maxTypeCode;
			}
		}
		
		/// <summary>
		/// Gets the number of bits needed to store the type code for this field.
		/// </summary>
		/// <value>The number of bits needed to store the type code for this field.</value>
		public int TypeCodePrecision {
			get {
				if( _typeList == null )
					return 0;
				else
					return _typeCodePrecision;
			}
		}

		/// <summary>
		/// Gets the integer code used to identify the given type.
		/// </summary>
		/// <param name="type">Type of the value stored in the field.</param>
		/// <param name="serializedType">A reference to a <see cref="Type"/> variable. On return, this contains the type
		///   that should be serialized. This may be different from <paramref name="type"/> if the exact type is not in
		///   the derived types table.</param>
		/// <returns>The ID that should be used to identify this type.</returns>
		public int GetTypeCode( Type type, out Type serializedType ) {
			foreach( DerivedTypeCodeAttribute attr in _typeList ) {
				if( attr.Type.IsAssignableFrom( type ) ) {
					serializedType = attr.Type;
					return attr.TypeCode;
				}
			}

			throw new IOException( "The type of the given value could not be represented by one of the designated derived types." );
		}

		public ulong ToSerializableInteger( long value ) {
			if( _rangeAttr == null )
				return unchecked( (ulong) value );
			else
				return _rangeAttr.ToSerializableInteger( value );
		}

		public ulong ToSerializableInteger( ulong value ) {
			if( _rangeAttr == null )
				return value;
			else
				return _rangeAttr.ToSerializableInteger( value );
		}

		public long FromSerializedInteger( ulong value ) {
			if( _rangeAttr == null )
				return unchecked( (long) value );
			else
				return _rangeAttr.FromSerializedInteger( value );
		}

		public ulong FromSerializedUnsignedInteger( ulong value ) {
			if( _rangeAttr == null )
				return value;
			else
				return _rangeAttr.FromSerializedUnsignedInteger( value );
		}

		public Expression GetSerializableIntegerExpression( Expression value, out Expression preConditionExpression ) {
			if( _rangeAttr == null ) {
				preConditionExpression = null;
				return new CastExpression( value, typeof(ulong), false );
			}
			else {
				return _rangeAttr.GetSerializableIntegerExpression( value, _paramName, out preConditionExpression );
			}
		}
		
		public Expression GetDeserializedIntegerExpression( Expression value, out Expression preConditionExpression ) {
			if( _rangeAttr == null ) {
				preConditionExpression = null;
				return new CastExpression( value, _paramType, false );
			}
			else {
				return _rangeAttr.GetDeserializedIntegerExpression( value, _paramType, out preConditionExpression );
			}
		}
	}
}