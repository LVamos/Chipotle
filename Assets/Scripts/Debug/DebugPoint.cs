using System;
using UnityEngine;

namespace Game.Debug
{
	/// <summary>
	/// Serializable wrapper for Vector2 that only exposes X and Y properties.
	/// </summary>
	[Serializable]
	public class SerializableVector2
	{
		/// <summary>
		/// Gets or sets the X coordinate.
		/// </summary>
		public float X { get; set; }

		/// <summary>
		/// Gets or sets the Y coordinate.
		/// </summary>
		public float Y { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializableVector2"/> class.
		/// </summary>
		public SerializableVector2() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializableVector2"/> class from a Vector2.
		/// </summary>
		/// <param name="vector">The vector to copy from.</param>
		public SerializableVector2(Vector2 vector)
		{
			X = vector.x;
			Y = vector.y;
		}

		/// <summary>
		/// Converts this serializable vector to a Unity Vector2.
		/// </summary>
		/// <returns>A Unity Vector2 with the same coordinates.</returns>
		public Vector2 ToVector2() => new Vector2(X, Y);

		/// <summary>
		/// Implicit conversion from SerializableVector2 to Vector2.
		/// </summary>
		public static implicit operator Vector2(SerializableVector2 sv) => sv?.ToVector2() ?? Vector2.zero;

		/// <summary>
		/// Implicit conversion from Vector2 to SerializableVector2.
		/// </summary>
		public static implicit operator SerializableVector2(Vector2 v) => new SerializableVector2(v);
	}

	/// <summary>
	/// Represents a single debug point with a name and a 2D position.
	/// </summary>
	[Serializable]
	public class DebugPoint
	{
		/// <summary>
		/// Gets or sets the position of the point in world coordinates.
		/// </summary>
		public SerializableVector2 Position { get; set; }

		/// <summary>
		/// Gets or sets the human-readable name of the point.
		/// </summary>
		public string Name { get; set; }
	}
}