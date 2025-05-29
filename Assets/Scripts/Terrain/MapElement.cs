using Game.Audio;
using Game.Entities;
using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Commands.GameInfo;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.Characters;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Terrain
{
	/// <summary>
	/// Base class for all objects that can be displayed on the game map.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(Entity))]
	[ProtoInclude(101, typeof(Zone))]
	[ProtoInclude(102, typeof(Passage))]
	public abstract class MapElement : MessagingObject
	{
		private const int _navigationMaxDistance = 50;
		private const float _navigationVolume = .5f;
		private const float _navigationMinDistance = .6f;

		[ProtoIgnore]
		protected AudioSource _navigationAudio;

		/// <summary>
		/// Returns pooint that belongs to this object and is tho most close to tthe player.
		/// </summary>
		public Vector2 GetClosestPointToPlayer() => _area.Value.GetClosestPoint(World.Player.Area.Value.Center);

		/// <summary>
		/// Dimensions of the map element.
		/// </summary>
		public (float width, float height) Dimensions { get; protected set; }

		/// <summary>
		/// Default volume for the sound loop if there's any.
		/// </summary>
		protected float _defaultVolume = 1;




		/// Inner and public name of the element
		/// </summary>
		public Name Name { get; protected set; }

		/// <summary>
		/// A backing field for Area.
		/// </summary>
		protected Rectangle? _area;

		/// <summary>
		/// Indicates if the sound navigation is enabled.
		/// </summary>
		[ProtoIgnore]
		protected bool _navigating;

		/// <summary>
		/// Names of sound effect files used by the object
		/// </summary>
		protected Dictionary<string, string> _sounds = new(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Enables or disables sound attenuation.
		/// </summary>
		protected bool _muffled;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner and public name of the element</param>
		/// <param name="area">Coordinates of the area the element occupies</param>
		public virtual void Initialize(Name name, Rectangle? area)
		{
			Name = name ?? throw new ArgumentException(nameof(name));
			Area = area;
		}

		/// <summary>
		/// Returns copy of the area occupied by the element
		/// </summary>
		public Rectangle? Area
		{
			get => _area == null ? (Rectangle?)null : new Rectangle(_area.Value);
			protected set => SetArea(value);
		}

		/// <summary>
		/// Sets value of the Area property.
		/// </summary>
		/// <param name="value">A value assigned to the property</param>
		protected virtual void SetArea(Rectangle? value)
		{
			_area = value;
			if (value != null)
			{
				Dimensions = (_area.Value.Width, _area.Value.Height);
				transform.position = _area.Value.Center;
			}
		}

		/// <summary>
		/// Returns the public name of the element.
		/// </summary>
		/// <returns>Public name of the element</returns>
		public override string ToString() => Name.Friendly;

		/// <summary>
		/// Destroys the element.
		/// </summary>
		protected virtual void DestroyObject() => _messagingEnabled = false;

		/// <summary>
		/// Processes the Destroy message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected virtual void OnDestroyObject(DestroyObject message) => DestroyObject();

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			base.HandleMessage(message);
			switch (message)
			{
				case StartNavigation m: OnStartNavigation(m); break;
				case StopNavigation m: OnStopNavigation(m); break;
				case DestroyObject d: OnDestroyObject(d); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Starts sound navigation.
		/// </summary>
		protected void StartNavigation()
		{
			bool keepNavigating = ShouldNavigationContinue();
			ReportPosition(keepNavigating);
			_navigating = keepNavigating;
			UpdateNavigatingSoundPosition();
		}

		/// <summary>
		/// Plays a navigation sound on position of the object.
		/// </summary>
		/// <param name="loop">Specifies if the navigating soudn should be played in loop</param>
		protected virtual void ReportPosition(bool loop = false)
		{
			Vector2 position2d = GetClosestPointToPlayer();
			Vector3 position3d = new(position2d.x, 0, position2d.y);
			_navigationAudio = Sounds.Play(_sounds["navigation"], position3d, _navigationVolume, loop);
			_navigationAudio.maxDistance = _navigationMaxDistance;
			_navigationAudio.minDistance = _navigationMinDistance;
			_navigationAudio.rolloffMode = AudioRolloffMode.Linear;
		}

		/// <summary>
		/// Processes the StopExitNavigation message.
		/// </summary>
		/// <param name="message">Source of the message</param>
		protected void OnStopNavigation(StopNavigation message) => StopNavigation();

		/// <summary>
		/// Processes the StartNavigation message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnStartNavigation(StartNavigation message) => StartNavigation();

		protected bool ShouldNavigationContinue()
		{
			float distance = GetDistanceToPlayer();
			bool playerInHere = SameZone(World.Player);
			return distance > 1 && distance <= _navigationMaxDistance && playerInHere;
		}

		/// <summary>
		/// Watches distance of tthe player and turns navigation off if he approaches the object.
		/// </summary>
		protected void WatchNavigation()
		{
			if (!_navigating)
				return;

			if (!ShouldNavigationContinue())
				StopNavigation();
		}

		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public override void GameUpdate()
		{
			base.GameUpdate();
			WatchNavigation();
		}

		/// <summary>
		/// Returns distance from this passage to the player.
		/// </summary>
		/// <returns>Distance in meters</returns>
		public float GetDistanceToPlayer() => World.GetDistance(this, World.Player);

		/// <summary>
		/// Stops the sound navigation.
		/// </summary>
		protected void StopNavigation()
		{
			_navigationAudio.loop = false;
			_navigationAudio = null;
			_navigating = false;
			World.Player.TakeMessage(new NavigationStopped(this));
		}

		/// <summary>
		/// Checks if the specified element and this element are at least partially in the same zone.
		/// </summary>
		/// <param name="element">The element to be checked</param>
		/// <returns>True if the specified element and this element are at least partially in the same zone.</returns>
		public virtual bool SameZone(Entity element)
		{
			List<Zone> mine = _area.Value.GetZones().ToList();
			List<Zone> its = element.Area.Value.GetZones().ToList();

			bool result = mine.Any(l => its.Contains(l));
			return result;
		}
		/// <summary>
		/// Updates position and attenuation of navigating sound if the navigation is in progress.
		/// </summary>
		protected virtual void UpdateNavigatingSoundPosition()
		{
			if (!_navigating)
				return;

			/* 
             * To give the player the impression that the navigation sound is heard over the entire object its position is set to the coordinates of the object point closest to the player. 
             */
			Vector2 position2d = GetClosestPointToPlayer();
			Vector3 position3d = new(position2d.x, 2, position2d.y);
			_navigationAudio.transform.position = position3d;

			// Find opposite point
			Vector2 player = World.Player.Area.Value.Center;
			Vector2? opposite = _area.Value.GetAlignedPoint(player);
			if (opposite == null)
				return; // Sound blocked, play it normally.

			// Detect potentional acoustic obstacles and set up attenuate parameters
			ObstacleType obstacle = World.DetectOcclusion(this);
			bool muffled = obstacle is ObstacleType.Wall or ObstacleType.ItemOrCharacter;

			if (obstacle == ObstacleType.Wall)
			{
				Sounds.SetLowPass(_navigationAudio, Sounds.OverWallLowpass);
				Sounds.SlideVolume(_navigationAudio, .5f, Sounds.GetOverWallVolume(_navigationAudio.volume));
			}
			else if (obstacle == ObstacleType.ItemOrCharacter)
			{
				Sounds.SetLowPass(_navigationAudio, Sounds.OverObjectLowpass);
				Sounds.SlideVolume(_navigationAudio, .5f, Sounds.GetOverObjectVolume(_navigationAudio.volume));
			}
			else if (_muffled)
			{
				// Turn off attenuation
				Sounds.DisableLowpass(_navigationAudio);
				Sounds.SlideVolume(_navigationAudio, .5f, _defaultVolume);
			}

			_muffled = muffled;
		}

		/// <summary>
		/// Returns true if the player is looking towards the given object (within a specified angle threshold).
		/// </summary>
		/// <param name="playerPosition">2D position of the player</param>
		/// <param name="playerDirection">2D unit vector of player's orientation</param>
		/// <param name="objectPosition">2D position of the target object</param>
		/// <param name="maxAngleDegrees">Allowed view cone (in degrees), default is 60°</param>
		public bool IsPlayerLookingAtMe(float maxAngleDegrees = 60f)
		{
			Vector2 playerPosition = World.Player.Area.Value.Center;
			Vector2 myPosition = _area.Value.Center;
			Vector2 toMe = myPosition - playerPosition;
			if (toMe == Vector2.zero)
				return true; // Player is exactly at the object's position

			toMe.Normalize();

			Vector2 playerDirection = World.Player.Orientation.UnitVector;
			float dot = Vector2.Dot(playerDirection, toMe);
			float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

			return angle <= maxAngleDegrees * 0.5f;
		}

		public override bool Equals(object obj)
		{
			return obj is MapElement element &&
				   base.Equals(obj) &&
				   Name.Indexed == element.Name.Indexed;
		}

		public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name.Indexed);

		public Zone GetZoneNearPlayer()
		{
			Vector2 closestPoint = GetClosestPointToPlayer();
			Zone myZone = World.GetZone(closestPoint);
			return myZone;
		}
	}
}