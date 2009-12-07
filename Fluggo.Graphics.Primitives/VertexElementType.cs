namespace Fluggo.Graphics {
	/// <summary>
	/// Describes the data type of a vertex element.
	/// </summary>
	public enum VertexElementType {
		/// <summary>
		/// One single-precision floating point value (four bytes).
		/// </summary>
		Float,

		/// <summary>
		/// Two single-precision floating point values (eight bytes).
		/// </summary>
		Float2,

		/// <summary>
		/// Three single-precision floating point values (twelve bytes).
		/// </summary>
		Float3,

		/// <summary>
		/// Four single-precision floating point values (sixteen bytes).
		/// </summary>
		Float4,
		
		/// <summary>
		/// One unsigned eight-bit value.
		/// </summary>
		Byte,

		/// <summary>
		/// Two unsigned eight-bit values.
		/// </summary>
		Byte2,

		/// <summary>
		/// Three unsigned eight-bit values.
		/// </summary>
		Byte3,

		/// <summary>
		/// Four unsigned eight-bit values.
		/// </summary>
		Byte4
	}
}