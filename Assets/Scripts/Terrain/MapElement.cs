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

using Unity.VisualScripting;

using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Terrain
{
	/// <summary>
	/// Base class for all objects that can be displayed on the game map.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(Entity))]
	[ProtoInclude(101, typeof(Locality))]
	[ProtoInclude(102, typeof(Passage))]
	public abstract class MapElement : MessagingObject
	{

		[ProtoIgnore]
		protected AudioSource _navigationAudio;

		/// <summary>
		/// Returns pooint that belongs to this object and is tho most close to tthe player.
		/// </summary>
		protected Vector2 GetClosestPointToPlayer()
		{
			return _area.Value.GetClosestPoint(World.Player.Area.Value.Center);
		}

		/// <summary>
		/// Dimensions of the map element.
		/// </summary>
		public (float width, float height) Dimensions { get; protected set; }

		/// <summary>
		/// Default volume for the sound loop if there's any.
		/// </summary>
		protected float _defaultVolume = 1;

		/// <summary>
		/// Volume used with sound attenuation.
		/// </summary>
		protected float _overWallVolume => _defaultVolume * .4f;

		/// <summary>
		/// Volume used with sound attenuation.
		/// </summary>
		protected float OverDoorVolume => _defaultVolume * .5f;

		/// <summary>
		/// Volume used with sound attenuation.
		/// </summary>
		protected float OverObjectVolume => _defaultVolume * .95f;

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
		public override string ToString()
		{
			return Name.Friendly;
		}

		/// <summary>
		/// Destroys the element.
		/// </summary>
		protected virtual void DestroyObject()
		{
			_messagingEnabled = false;
		}

		/// <summary>
		/// Processes the Destroy message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected virtual void OnDestroyObject(DestroyObject message)
		{
			DestroyObject();
		}

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
			UpdateNavigatingSound();
		}

		/// <summary>
		/// Plays a navigation sound on position of the object.
		/// </summary>
		/// <param name="loop">Specifies if the navigating soudn should be played in loop</param>
		protected virtual void ReportPosition(bool loop = false)
		{
			Vector2 position2d = GetClosestPointToPlayer();
			Vector3 position3d = new(position2d.x, 2, position2d.y);
			_navigationAudio = Sounds.Play(_sounds["navigation"], position3d, _defaultVolume, loop);
		}

		/// <summary>
		/// Processes the StopExitNavigation message.
		/// </summary>
		/// <param name="message">Source of the message</param>
		protected void OnStopNavigation(StopNavigation message)
		{
			StopNavigation();
		}

		/// <summary>
		/// Processes the StartNavigation message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnStartNavigation(StartNavigation message)
		{
			StartNavigation();
		}

		protected bool ShouldNavigationContinue()
		{
			float distance = GetDistanceToPlayer();
			bool playerInHere = SameLocality(World.Player);
			return distance > 1 && distance <= 50 && playerInHere;
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
		protected float GetDistanceToPlayer()
		{
			return World.GetDistance(this, World.Player);
		}

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
		/// Checks if the specified element and this element are at least partially in the same locality.
		/// </summary>
		/// <param name="element">The element to be checked</param>
		/// <returns>True if the specified element and this element are at least partially in the same locality.</returns>
		public bool SameLocality(Entity element)
		{
			IEnumerable<Locality> mine = _area.Value.GetLocalities();
			IEnumerable<Locality> its = element.Area.Value.GetLocalities();

			return mine.Any(l => its.Contains(l));
		}
		/// <summary>
		/// Updates position and attenuation of navigating sound if the navigation is in progress.
		/// </summary>
		protected virtual void UpdateNavigatingSound()
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
			ObstacleType obstacle = World.DetectAcousticObstacles(new((Vector2)opposite));
			bool muffled = obstacle is ObstacleType.Wall or ObstacleType.Object;

			if (obstacle == ObstacleType.Wall)
			{
				Sounds.SetLowPass(_navigationAudio, Sounds.OverWallLowpass);
				Sounds.SlideVolume(_navigationAudio, .5f, _overWallVolume);
			}
			else if (obstacle == ObstacleType.Object)
			{
				Sounds.SetLowPass(_navigationAudio, Sounds.OverObjectLowpass);
				Sounds.SlideVolume(_navigationAudio, .5f, OverObjectVolume);
			}
			else if (_muffled)
			{
				// Turn off attenuation
				Sounds.DisableLowpass(_navigationAudio);
				Sounds.SlideVolume(_navigationAudio, .5f, _defaultVolume);
			}

			_muffled = muffled;
		}

		public override bool Equals(object obj)
		{
			return obj is MapElement element &&
				   base.Equals(obj) &&
				   Name.Indexed == element.Name.Indexed;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), Name.Indexed);
		}
	}
}