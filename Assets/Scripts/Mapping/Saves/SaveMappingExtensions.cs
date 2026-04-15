using Game.Serialization.Protobuf.Snapshots;
using Game.Serialization.Protobuf.Snapshots.Characters;
using Game.Serialization.Protobuf.Snapshots.Characters.Bartender;
using Game.Serialization.Protobuf.Snapshots.Characters.Carson;
using Game.Serialization.Protobuf.Snapshots.Characters.Chipotle;
using Game.Serialization.Protobuf.Snapshots.Characters.Tuttle;
using Game.Serialization.Protobuf.Snapshots.Entities;
using Game.Serialization.Protobuf.Snapshots.Entities.Items;
using Game.Serialization.Protobuf.Snapshots.Entities.Items.Items;
using Game.Serialization.Protobuf.Snapshots.Spatial;
using Game.Serialization.Protobuf.Snapshots.Spatial.Passages;

using System.Linq;

namespace Game.Mapping.Saves
{
	public static class SaveMappingExtensions
	{
		public static ZoneSave ToZoneSave(this MapElementSave save)
		{
			ZoneSave result = new();
			MapMapElementSave(save, result);

			return result;
		}

		public static PassageSave ToPassageSave(this MapElementSave save)
		{
			PassageSave result = new();
			MapMapElementSave(save, result);

			return result;
		}

		public static DoorSave ToDoorSave(this PassageSave save)
		{
			DoorSave result = new();
			MapMapElementSave(save, result);
			MapPassageSave(save, result);

			return result;
		}

		public static ItemSave ToItemsave(this EntitySave save)
		{
			ItemSave result = new();
			MapEntitySave(save, result);

			return result;
		}

		public static KeyHangerSave ToKeyHangerSave(this ItemSave save)
		{
			KeyHangerSave result = new();
			MapItemSave(save, result);

			return result;
		}

		public static ChipotlesCarSave ToChipotlesCarSave(this ItemSave save)
		{
			ChipotlesCarSave result = new();
			MapItemSave(save, result);

			return result;
		}

		public static EntitySave ToEntitySave(this MapElementSave save)
		{
			EntitySave result = new();
			MapMapElementSave(save, result);

			return result;
		}

		public static CharacterSave ToCharacterSave(this EntitySave save)
		{
			CharacterSave characterSave = new();
			MapEntitySave(save, characterSave);

			return characterSave;
		}

		public static SoundSave ToSoundSave(this ComponentSave save)
		{
			SoundSave soundSave = new();
			MapComponentSave(save, soundSave);

			return soundSave;
		}

		public static AISave ToAISave(this ComponentSave save)
		{
			AISave aiSave = new();
			MapComponentSave(save, aiSave);

			return aiSave;
		}

		public static PhysicsSave ToPhysicsSave(this ComponentSave save)
		{
			PhysicsSave physicsSave = new();
			MapComponentSave(save, physicsSave);

			return physicsSave;
		}

		public static CarsonAISave ToCarsonAISave(this AISave save)
		{
			CarsonAISave carsonAiSave = new();
			MapAISave(save, carsonAiSave);

			return carsonAiSave;
		}

		public static TuttleAISave ToTuttleAISave(this AISave save)
		{
			TuttleAISave tuttleAiSave = new();
			MapAISave(save, tuttleAiSave);

			return tuttleAiSave;
		}

		public static BartenderAISave ToBartenderAISave(this AISave save)
		{
			BartenderAISave bartenderAiSave = new();
			MapAISave(save, bartenderAiSave);

			return bartenderAiSave;
		}

		public static ChipotlePhysicsSave ToChipotlePhysicsSave(this PhysicsSave save)
		{
			ChipotlePhysicsSave RESULT = new();
			MapPhysicsSave(save, RESULT);

			return RESULT;
		}

		private static void MapMapElementSave(MapElementSave source, MapElementSave target)
		{
			target.Area = source.Area;
			target.Sounds = !source.Sounds.IsNullOrEmpty() ? new(source.Sounds) : null;
			target.Name = source.Name;
			target.Usable = source.Usable;
			target.UsableWith = !source.UsableWith.IsNullOrEmpty() ? new(source.UsableWith) : null;
		}

		private static void MapPassageSave(PassageSave source, PassageSave target)
		{
			MapMapElementSave(source, target);
			target.PlayersZone = source.PlayersZone;
			target.Zones = !source.Zones.IsNullOrEmpty() ? source.Zones.ToArray() : null;
			target.State = source.State;
			target.TypeDescription = source.TypeDescription;
		}

		private static void MapEntitySave(EntitySave source, EntitySave target)
		{
			MapMapElementSave(source, target);
			target.DescriptionID = source.DescriptionID;
			target.Type = source.Type;
		}

		private static void MapItemSave(ItemSave source, ItemSave target)
		{
			MapMapElementSave(source, target);
			MapEntitySave(source, target);
			target.CollisionSound = source.CollisionSound;
			target.ActionSound = source.ActionSound;
			target.LoopSound = source.LoopSound;
			target.Cutscene = source.Cutscene;
			target.PickingSound = source.PickingSound;
			target.PlacingSound = source.PlacingSound;
			target.AudibleOverWalls = source.AudibleOverWalls;
			target.LastOccludingObstacle = source.LastOccludingObstacle;
			target.LoopPositionBackup = source.LoopPositionBackup;
			target.Pickable = source.Pickable;
			target.QuickActionsAllowed = source.QuickActionsAllowed;
			target.StopWhenPlayerMoves = source.StopWhenPlayerMoves;
			target.UsableOnce = source.UsableOnce;
			target.Zones = !source.Zones.IsNullOrEmpty() ? new(source.Zones) : null;
			target.Decorative = source.Decorative;
			target.HeldBy = source.HeldBy;
			target.Passable = source.Passable;
			target.Used = source.Used;
			target.UsedOnce = source.UsedOnce;
		}

		private static void MapComponentSave(ComponentSave source, ComponentSave target)
		{
			target.Owner = source.Owner;
		}

		private static void MapAISave(AISave source, AISave target)
		{
			MapComponentSave(source, target);
			target.Hidden = source.Hidden;
			target.MaxObjectDistance = source.MaxObjectDistance;
			target.MinObjectDistance = source.MinObjectDistance;
			target.State = source.State;
		}

		private static void MapPhysicsSave(PhysicsSave source, PhysicsSave target)
		{
			MapComponentSave(source, target);
			target.State = source.State;
			target.Area = source.Area;
			target.Goal = source.Goal;
			target.Inventory = !source.Inventory.IsNullOrEmpty() ? new(source.Inventory) : null;
			target.MaxObjectDistance = source.MaxObjectDistance;
			target.MinObjectDistance = source.MinObjectDistance;
			target.NavigableObjectsRadius = source.NavigableObjectsRadius;
			target.NearbyWalls = source.NearbyWalls;
			target.ObjectManipulationHelpRadius = source.ObjectManipulationHelpRadius;
			target.Orientation = source.Orientation;
			target.Path = source.Path;
			target.RestartApproaching = source.RestartApproaching;
			target.Speed = source.Speed;
			target.StepLength = source.StepLength;
			target.TargetPlayerDistance = source.TargetPlayerDistance;
			target.WallDistanceThreshold = source.WallDistanceThreshold;
			target.Height = source.Height;
			target.Width = source.Width;
		}
	}
}