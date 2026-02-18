using Game.Messaging;
using Game.Messaging.Events.Sound;



using System;

using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Components
{
	/// <summary>
	/// Base class for all NPC components
	/// </summary>

	public abstract class CharacterComponent : GameComponent<Character>
	{
		public virtual void Initialize()
		{
		}

		/// <summary>
		/// Sends an inner message to the containing NPC.
		/// </summary>
		/// <param name="message">The message to be sent</param>
		protected void InnerMessage(Message message)
		{
			Owner.TakeMessage(message);
		}

		/// <summary>
		/// Backing field for the Onwer property.
		/// </summary>

		protected Character _owner;

		/// <summary>
		/// A reference to the parent NPC
		/// </summary>

		public override Character Owner
		{
			get
			{
				if (_owner == null)
					_owner = World.GetCharacter(_ownerName)
							 ?? throw new InvalidOperationException(nameof(_ownerName));

				return _owner;
			}
		}

		public void SetParent(string name)
		{
			_ownerName = name
						 ?? throw new ArgumentException(nameof(name));
		}

		/// <summary>
		/// Indicates if a cutscene is played in the moment.
		/// </summary>
		protected bool _cutsceneInProgress;

		/// <summary>
		/// Name of the parent NPC
		/// </summary>
		public Name Name => Owner?.Name;

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case CutsceneEnded ce: OnCutsceneEnded(ce); break;
				case CutsceneBegan cb: OnCutsceneBegan(cb); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Processes the CutsceneBegan message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected virtual void OnCutsceneBegan(CutsceneBegan message)
		{
			_cutsceneInProgress = true;
		}

		/// <summary>
		/// Processes the CutsceneEnded message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected virtual void OnCutsceneEnded(CutsceneEnded message)
		{
			_cutsceneInProgress = false;
		}
	}
}