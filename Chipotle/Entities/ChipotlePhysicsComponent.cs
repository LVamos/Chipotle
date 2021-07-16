using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;
using DavyKager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Luky;

using System.Windows.Forms;
using Game.Entities;

namespace Game
{
	public class ChipotlePhysicsComponent : PhysicsComponent
	{
		public override void Update()
		{
			base.Update();
			UpdateRotation();
		}

		private void UpdateRotation()
		{
			if (_plannedRotations > 0)
			{
				_orientation.Rotate(_rotationStep);
				_plannedRotations--;

				if (_plannedRotations == 0)
					Owner.ReceiveMessage(new TurnEntityResult (this, _orientation));
			}
		}

		private int _rotationStep;
		private int _plannedRotations;


		public override void Start()
		{
			// set initial position.
			SetPosition(new Plane(new Vector2(1027, 1005)));
			_orientation = new Orientation2D(0, 1);
			_area.GetLocality().ReceiveMessage(new LocalityEntered(this, Owner));

			base.Start();

			RegisterMessageHandlers(
				new Dictionary<Type, Action<GameMessage>>()
				{
					// Test messages
					[typeof(SayTerrain )] = (message) => OnTerrainInfo((SayTerrain )message),

					// Other messages
					[typeof(SayNearestObject)] = (m) => OnNearestObjectAnnouncement((SayNearestObject)m),
					[typeof(SayLocality)] = (m) => OnLocalityAnnouncement((SayLocality)m),
					[typeof(MoveEntity )] = (m) => OnMovement((MoveEntity )m),
					[typeof(TurnEntity )] = (message) => OnTurnover((TurnEntity )message),
					[typeof(UseObject )] = (message) => OnInteractionStart(message)
				}
				);

		}


		private void OnNearestObjectAnnouncement(SayNearestObject m)
		{
			GameObject o = World.GetNearestObjects(_area.UpperLeftCorner).Where(obj => obj.Locality == _area.GetLocality()).FirstOrDefault();
			if (o == null)
			{
				Say("Nic tu není");
				return;
			}
			string msg = o.Name.Friendly;

			if (o.Area.LowerRightCorner.Y > _area.Center.Y)
				msg += " před tebou";
			else if (o.Area.UpperRightCorner.Y < _area.Center.Y)
				msg += " za tebou";
			else if (o.Area.LowerRightCorner.Y <= _area.Center.Y && o.Area.UpperRightCorner.Y >= _area.Center.Y)
			{
				if (o.Area.UpperRightCorner.X < _area.Center.X)
					msg += " vlevo";
				else
					msg += " vpravo";
			}

			Say(msg);
		}

		private void OnLocalityAnnouncement(SayLocality m)
=> SayDelegate(World.Map[Area.UpperLeftCorner].Locality.Name.Friendly);

		private void OnTerrainInfo(SayTerrain  message)
		{
			SayDelegate(World.Map[Area.UpperLeftCorner].Terrain.GetDescription());
		}

		private void OnInteractionStart(GameMessage message)
		{
			Tolk.Speak("OnUseObject neimplementováno.");
		}

		private void OnTurnover(TurnEntity  message)
		{
			_rotationStep = message.Degrees >= 0 ? 1 : -1;
			_plannedRotations = Math.Abs(message.Degrees);
		}

		private void OnMovement(MoveEntity  message)
		{
			// Get target coordinates
			var finalOrientation = _orientation;

			if (message.Direction != TurnType.None)
				finalOrientation.Rotate(message.Direction);

			var target = Area;
			target.Move(finalOrientation, 1);

			// Is the terrain occupable?
			Assert(target.IsInMapBoundaries(), "Columbo off the map!"); // Verify map boundaries.
			Tile targetTile = World.Map[target.UpperLeftCorner] ?? throw new InvalidOperationException($"{nameof(OnMovement)}: empty tile."); // Null test

			if (!targetTile.Permeable)
			{
				Owner.ReceiveMessage(new TerrainCollided (this, targetTile));
				return;
			}

			// Isn't an entity or object over there?
			if (targetTile.IsOccupied && targetTile.Object != Owner)
			{
				Owner.ReceiveMessage(new ObjectsCollided(this, targetTile));
				return;
			}

			// The road is clear! Move!
			Locality sourceLocality = _area.GetLocality();
			Locality targetLocality = World.Map[target.Center].Locality;

			if (targetLocality != sourceLocality)
			{
				sourceLocality.ReceiveMessage(new LocalityLeft(this, Owner));
				targetLocality.ReceiveMessage(new LocalityEntered(this, Owner));
			}
			SetPosition(target);
			Owner.ReceiveMessage(new EntityMoved (this, targetTile));

		}





	}

	
}
