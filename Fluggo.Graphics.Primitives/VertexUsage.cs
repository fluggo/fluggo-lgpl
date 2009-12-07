namespace Fluggo.Graphics {
	/// <summary>
	/// Describes the meaning of a vertex element value.
	/// </summary>
	public enum VertexUsage {
		/// <summary>
		/// The element contains a non-transformed position as a <see cref="VertexElementType.Float3">VertexElementType.Float3</see> value.
		/// </summary>
		Position,

		/// <summary>
		/// The element contains a vertex normal as a <see cref="VertexElementType.Float3">VertexElementType.Float3</see> value.
		/// </summary>
		Normal,
		Color,
		TextureCoordinate
	}
}