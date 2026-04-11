using Game.Serialization.Protobuf.Saves;
using Game.Terrain;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Game.Mapping.Saves
{
	public static class RuntimeSaveMappingExtensions
	{
		public static RectangleSave ToRectangleSave(this Rectangle value)
	=> new()
	{
		UpperLeft = value.UpperLeftCorner.ToVector2Save(),
		LowerRight = value.LowerRightCorner.ToVector2Save()
	};

		public static Rectangle? ToRectangle(this RectangleSave value)
		{
			if (value == null)
				return null;

			return new(
						value.UpperLeft.ToVector2(),
						value.LowerRight.ToVector2()
						);
		}

		public static HashSet<Vector2Save> ToVector2SaveHashSet(this HashSet<Vector2> value)
		{
			if (value == null)
				return null;

			return
				value.Select(p => p.ToVector2Save())
				.ToHashSet();
		}

		public static HashSet<Vector2> ToVector2HashSet(this HashSet<Vector2Save> value)
		{
			if (value == null)
				return null;

			return
				value.Select(p => p.ToVector2())
				.ToHashSet();
		}

		public static Queue<Vector2Save> ToVector2SaveQueue(this Queue<Vector2> value)
		{
			if (value == null)
				return null;

			IEnumerable<Vector2Save> temp = value
				.Select(p => p.ToVector2Save());
			return new(temp);
		}

		public static Queue<Vector2> ToVector2Queue(this Queue<Vector2Save> value)
		{
			if (value == null)
				return null;

			IEnumerable<Vector2> temp = value
				.Select(p => p.ToVector2());
			return new(temp);
		}

		public static Vector3 ToVector3(this Vector3Save value)
	=> new(value.X, value.Y, value.Z);

		public static Vector3Save ToVector3Save(this Vector3 value)
			=> new()
			{
				X = value.x,
				Y = value.y,
				Z = value.z
			};

		public static Vector2 ToVector2(this Vector2Save value)
	=> new(value.X, value.Y);

		public static Vector2Save ToVector2Save(this Vector2 value)
			=> new()
			{
				X = value.x,
				Y = value.y
			};

		public static NameSave ToNameSave(this Name value)
		{
			if (value == null)
				return null;

			return new()
			{
				Indexed = value.Inner,
				Friendly = value.Friendly
			};
		}

		public static Name ToName(this NameSave value)
		{
			if (value == null)
				return null;

			return new(value.Indexed, value.Friendly);
		}
	}
}
