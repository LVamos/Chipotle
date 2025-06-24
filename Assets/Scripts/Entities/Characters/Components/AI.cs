using Game.Entities.Characters.Bartender;
using Game.Entities.Characters.Carson;
using Game.Entities.Characters.Christine;
using Game.Entities.Characters.Mariotti;
using Game.Entities.Characters.Sweeney;
using Game.Entities.Characters.Tuttle;
using Game.Messaging.Commands.Characters;
using Game.Messaging.Commands.Movement;
using Game.Messaging.Events.Characters;
using Game.Messaging.Events.Movement;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Message = Game.Messaging.Message;
using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Characters.Components
{
	/// <summary>
	/// Controls the behavior of an NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(BartenderAI))]
	[ProtoInclude(101, typeof(CarsonAI))]
	[ProtoInclude(102, typeof(ChristineAI))]
	[ProtoInclude(103, typeof(MariottiAI))]
	[ProtoInclude(104, typeof(SweeneyAI))]
	[ProtoInclude(105, typeof(TuttleAI))]
	public class AI : CharacterComponent
	{
		protected virtual void Reveal(Vector2 target)
		{
			_hidden = false;
			InnerMessage(new Reveal(this, new(target)));
		}
		/// <summary>
		/// Area occupied by the NPC
		/// </summary>
		protected Rectangle _area;

		/// <summary>
		/// Indicates if the NPC is invisible for other NPCs and objects.
		/// </summary>
		protected bool _hidden;

		/// <summary>
		/// Indicates what the NPC is doing in the moment.
		/// </summary>
		protected CharacterState _state = CharacterState.Waiting;
		protected float _minObjectDistance = 2;
		protected float _maxObjectDistance = 4;

		/// <summary>
		/// Reference to the Detective Chipotle NPC
		/// </summary>
		[ProtoIgnore]
		protected Character _player => World.Player;

		protected Vector2[] FindFreePlacementsAroundArea(Rectangle area, float minDistance, float maxDistance, bool sameZone = true)
		{
			float height = transform.localScale.z;
			float width = transform.localScale.x;

			Vector2[] points = World.GetFreePlacementsNear(new() { Owner }, area, height, width, minDistance, maxDistance, sameZone)
				.ToArray();

			return points;
		}

		/// <summary>
		/// Sends Tuttle on the specified path.
		/// </summary>
		/// <param name="path">The path to be folloewd</param>
		protected void FollowPath(Queue<Vector2> path) => InnerMessage(new FollowPath(this, path));

		/// <summary>
		/// Tuttle makes few random steps in the current zone.
		/// </summary>
		protected void GoNear(Rectangle target, float minDistance, float maxDistance, bool watchPlayer = false)
		{
			List<Vector2> Targets = GetPointsAround(target, minDistance, maxDistance).Take(10).ToList();

			// Tuttle tries each point from the array.
			if (Targets.Count > 0)
				TryGoTo(Targets, watchPlayer);
		}

		private List<Vector2> GetPointsAround(Rectangle target, float minDistance, float maxDistance)
		{
			return Rectangle.GetPointsAround(target, minDistance, maxDistance, World.ValidplacementsResolution)
.Where(p => !_area.Contains(p))
.ToList();
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case PositionChanged pc: OnPositionChanged(pc); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Jumps to a specific position.
		/// </summary>
		/// <param name="position">The position to jump to considered a center of the character.</param>
		/// <param name="silently">Whether to jump silently or not. Default is true.</param>
		protected void JumpTo(Vector2 position, bool silently = true)
		{
			SetPosition message = new(this, position, silently);
			InnerMessage(message);
		}

		/// <summary>
		/// Handles the TuttleStateChanged message.
		/// </summary>
		/// <param name="message">The message</param>
		protected void OnCharacterStateChanged(CharacterStateChanged message) => _state = message.State;

		/// <summary>
		/// Processes the PositionChanged message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnPositionChanged(PositionChanged message) => _area = message.TargetPosition;

		/// <summary>
		/// sets state of the NPC and announces the change to other components.
		/// </summary>
		protected void SetState(CharacterState state)
		{
			_state = state;
			InnerMessage(new CharacterStateChanged(this, state));
		}

		/// <summary>
		/// Starts following the player.
		/// </summary>
		protected void StartFollowingPlayer() => InnerMessage(new StartFollowingPlayer(this));

		protected void TryGoTo(List<Vector2> points, bool watchPlayer = false)
		{
			TryGoTo message = new(this, points, watchPlayer);
			InnerMessage(message);
		}

		/// <summary>
		/// Makes the NPC invisible for the other NPCs and objects.
		/// </summary>
		protected void Hide()
		{
			_hidden = true;
			InnerMessage(new Hide(this));
		}

		/// <summary>
		/// Sends Tuttle to the specified point.
		/// </summary>
		/// <param name="point">The target point</param>
		/// <param name="watchPlayer">Specifies if Tuttle should stop following the player while leading to the target</param>
		protected void GoToPoint(Vector2 point, bool watchPlayer = false) => InnerMessage(new GotoPoint(this, point, watchPlayer));

		protected void JumpNear(Rectangle area)
		{
			Vector2? target = FindFreePlacementsAroundArea(area, _minObjectDistance, _maxObjectDistance)
				.FirstOrDefault();
			if (target == null)
				throw new InvalidOperationException("No free placement found.");
			JumpTo(target.Value);
		}
	}
}