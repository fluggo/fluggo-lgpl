using System;
using Fluggo.Graphics;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Fluggo.Graphics {
	[AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
	abstract class VertexFieldAttribute : Attribute {
	}
	
	/// <summary>
	/// Identifies the field in a vertex type that contains position information.
	/// </summary>
	sealed class VertexPositionAttribute : VertexFieldAttribute {
	}
	
	sealed class VertexNormalAttribute : VertexFieldAttribute {
	}
	
	sealed class VertexTextureCoordAttribute : VertexFieldAttribute {
		int _set;
		
		public VertexTextureCoordAttribute( int set ) {
			_set = set;
		}
		
		public int Set {
			get {
				return _set;
			}
		}
	}
	
	class VertexTypeInfo {
		Type _type;
		FieldInfo _positionField, _normalField;
		FieldInfo[] _texCoordFields;
		int[] _texDimensions;
		
		public VertexTypeInfo( Type type ) {
			if( type == null )
				throw new ArgumentNullException( "type" );
				
			if( !type.IsValueType )
				throw new ArgumentException( "Given type is not a value type." );
				
			_type = type;
			
			List<FieldInfo> texCoordFields = new List<FieldInfo>( 16 );
			List<int> texCoordDimensions = new List<int>( 16 );
			texCoordFields.AddRange( new FieldInfo[16] );

			foreach( FieldInfo field in _type.GetFields( BindingFlags.Public ) ) {
				if( !field.FieldType.IsValueType || field.FieldType.IsPointer || field.FieldType.IsMarshalByRef )
					throw new ArgumentException( "The given type contains a reference field." );
				
				foreach( VertexFieldAttribute attr in field.GetCustomAttributes( typeof(VertexFieldAttribute), false ) ) {
					if( attr is VertexPositionAttribute ) {
						if( _positionField != null )
							throw new ArgumentException( "More than one position field was specified." );
						
						if( field.FieldType != typeof(Vector2f) &&
								field.FieldType != typeof(Vector3f) &&
								field.FieldType != typeof(Vector4f) )
							throw new ArgumentException( "The position field must have the type Vector2f, Vector3f, or Vector4f." );
							
						_positionField = field;
						break;
					}
					else if( attr is VertexNormalAttribute ) {
						if( _normalField != null )
							throw new ArgumentException( "More than one normal field was specified." );
						
						if( field.FieldType != typeof(Vector3f) &&
								field.FieldType != typeof(Vector4f) )
							throw new ArgumentException( "The normal field must have the type Vector3f or Vector4f." );
							
						_normalField = field;
						break;
					}
					else if( attr is VertexTextureCoordAttribute ) {
						VertexTextureCoordAttribute texAttr = attr as VertexTextureCoordAttribute;
						
						if( texAttr.Set < 0 )
							throw new ArgumentException( "A texture coordinate was specified with a set number below zero." );
							
						if( texAttr.Set > 15 )
							throw new ArgumentException( "A texture coordinate was specified with a set number greater than 15." );
							
						if( texCoordFields[texAttr.Set] != null )
							throw new ArgumentException( "The same texture coordinate set was specified more than once." );
							
						if( field.FieldType == typeof(float) )
							texCoordDimensions[texAttr.Set] = 1;
						else if( field.FieldType == typeof(Vector2f) )
							texCoordDimensions[texAttr.Set] = 2;
						else if( field.FieldType == typeof(Vector3f) )
							texCoordDimensions[texAttr.Set] = 3;
						else
							throw new ArgumentException( "A texture coordinate field must have the type float, Vector2f, or Vector3f." );

						texCoordFields[texAttr.Set] = field;
					}
				}
			}
			
			int firstNull = texCoordFields.IndexOf( null );
			
			if( firstNull == -1 ) {
				_texCoordFields = texCoordFields.ToArray();
				_texDimensions = texCoordDimensions.ToArray();
			}
			else {
				_texCoordFields = texCoordFields.GetRange( 0, firstNull + 1 ).ToArray();
				_texDimensions = texCoordDimensions.GetRange( 0, firstNull + 1 ).ToArray();
			}
		}

		/// <summary>
		/// Gets the type of the vertex described by this instance.
		/// </summary>
		/// <value>The <see cref='Type'/> of the vertex described by this instance.</value>
		public Type VertexType {
			get {
				return _type;
			}
		}

		/// <summary>
		/// Gets the field marked with the <see cref="VertexPositionAttribute"/>.
		/// </summary>
		/// <value>The field marked with the <see cref="VertexPositionAttribute"/>, or <see langword='null'/> if there is no such field.</value>
		public FieldInfo PositionField {
			get {
				return _positionField;
			}
		}
		
		/// <summary>
		/// Gets the field marked with the <see cref="VertexNormalAttribute"/>.
		/// </summary>
		/// <value>The field marked with the <see cref="VertexNormalAttribute"/>, or <see langword='null'/> if there is no such field.</value>
		public FieldInfo NormalField {
			get {
				return _normalField;
			}
		}
		
		public ReadOnlyCollection<FieldInfo> TextureFields {
			get {
				return Array.AsReadOnly<FieldInfo>( _texCoordFields );
			}
		}
	}
}