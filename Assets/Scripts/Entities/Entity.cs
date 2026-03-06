using Game.Serialization.Protobuf.Snapshots;
using Game.Serialization.Protobuf.Snapshots.Entities;
using Game.Terrain;


using System;

namespace Game.Entities
{
	/// <summary>
	/// A base class for all simple objects and NPCS
	/// </summary>
	public class Entity : MapElement
	{
		public void Restore(EntitySave data)
		{
			base.Restore((MapElementSave)data);
			_descriptionID = data.DescriptionID;
			Type = data.Type;
		}

		/// <summary>
		/// Type of the object; it allows grouping objects with tha same behavior.
		/// </summary>
		public string Type { get; protected set; }

		/// <summary>
		/// Stores ID of the current description of the object.
		/// </summary>
		protected int _descriptionID;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="type">Type of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		public void Initialize(Name name, string type, Rectangle? area)
		{
			base.Initialize(name, area);
			_sounds["navigation"] = "SonarLoop";
			Type = type.PrepareForIndexing();
		}

		/// <summary>
		/// Returns current description of the object.
		/// </summary>
		public string Description { get => World.GetObjectDescription(this, _descriptionID); }

		/// <summary>
		/// Returns the hash code for this object.
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode() => unchecked(3000 * (2000 + Area.GetHashCode()) * (3000 + Type.GetHashCode()) * 4000);

		/// <summary>
		/// Returns public name of the object.
		/// </summary>
		/// <returns>public name of the object</returns>
		public override string ToString() => Name.Friendly;

		/// <summary>
		/// Destroys the object or NPC.
		/// </summary>
		public override void Destroy()
		{
			World.Remove(this);
			_messagingEnabled = false;
			Destroy(gameObject);
		}

		/// <summary>
		/// Moves the object to the specified coordinates.
		/// </summary>
		/// <param name="targetArea">Target coordinates</param>
		protected virtual void Move(Game.Terrain.Rectangle targetArea) => Area = (Rectangle?)targetArea ?? throw new ArgumentNullException(nameof(targetArea));

	}
}