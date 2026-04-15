using Assets.Scripts.Spatial;

using Game.Mapping.Saves;
using Game.Messaging.Commands.Characters;
using Game.Messaging.Commands.Movement;
using Game.Messaging.Events.Characters;
using Game.Serialization.Protobuf.Snapshots.Characters;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Characters.Components
{
	/// <summary>
	/// Controls the behavior of an NPC.
	/// </summary>

	public class AI : CharacterComponent
	{
		public override void Restore(ComponentSave save)
		{
			if (save is not AISave data)
				return;

			base.Restore(data);
			_hidden = data.Hidden;
			_maxObjectDistance = data.MaxObjectDistance;
			_minObjectDistance = data.MinObjectDistance;
			_state = data.State;
		}

		public override ComponentSave Export()
		{
			var save = base.Export().ToAISave();

			save.Hidden = _hidden;
			save.MaxObjectDistance = _maxObjectDistance;
			save.MinObjectDistance = _minObjectDistance;
			save.State = _state;

			return save;
		}

		protected virtual void Reveal(Vector2 target)
		{
			_hidden = false;
			InnerMessage(new Reveal(this, new(target)));
		}

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

		protected Character _player => World.Player;

		protected Vector2[] FindFreePlacementsAroundArea(Rectangle area, float minDistance, float maxDistance, bool sameZone = true)
		{
			float height = transform.localScale.z;
			float width = transform.localScale.x;

			Vector2[] points = World.Placements.GetFreePlacementsNear(new() { Owner }, area, height, width, minDistance, maxDistance, sameZone)
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
		protected void GoNear(Rectangle target, float minDistance, float maxDistance, bool watchPlayer)
		{
			List<Vector2> Targets = GetPointsAround(target, minDistance, maxDistance)?.ToList();
			if (Targets.Count > 0)
				TryGoTo(Targets, watchPlayer);
		}

		private List<Vector2> GetPointsAround(Rectangle target, float minDistance, float maxDistance)
		{
			return Rectangle.GetPointsAround(target, minDistance, maxDistance, PlacementFinder.ValidplacementsResolution)?
.Where(p => !Owner.Area.Value.Contains(p))?
.ToList();
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
		protected void OnCharacterStateChanged(StateChanged message) => _state = message.State;

		/// <summary>
		/// sets state of the NPC and announces the change to other components.
		/// </summary>
		protected void SetState(CharacterState state)
		{
			_state = state;
			InnerMessage(new StateChanged(this, state));
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