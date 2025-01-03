using Game.Messaging;
using Game.Messaging.Events.Sound;

using ProtoBuf;

using System;

using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Components
{
	/// <summary>
	/// Base class for all NPC components
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(AI))]
	[ProtoInclude(101, typeof(Physics))]
	[ProtoInclude(102, typeof(Sound))]
	[ProtoInclude(103, typeof(Input))]
	public abstract class CharacterComponent : MessagingObject
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
		/// Indexed name of the owner fo this component.
		/// </summary>
		protected string _ownerName;

		/// <summary>
		/// Backing field for the Onwer property.
		/// </summary>
		[ProtoIgnore]
		protected Character _owner;

		/// <summary>
		/// A reference to the parent NPC
		/// </summary>
		[ProtoIgnore]
		public Character Owner
		{
			get
			{
				if (_owner == null)
					_owner = World.GetCharacter(_ownerName)
							 ?? throw new InvalidOperationException(nameof(_ownerName));

				return _owner;
			}
		}

		public void AssignToEntity(string name)
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