using Game.Messaging;
using Game.Messaging.Events;

using Luky;

using ProtoBuf;

using System;

namespace Game.Entities
{
    /// <summary>
    /// Base class for all NPC components
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(AIComponent))]
    [ProtoInclude(101, typeof(PhysicsComponent))]
    [ProtoInclude(102, typeof(SoundComponent))]
    [ProtoInclude(103, typeof(InputComponent))]
    public abstract class CharacterComponent : MessagingObject
    {
        /// <summary>
        /// Sends an inner message to the containing NPC.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        protected void InnerMessage(GameMessage message)
            => Owner.TakeMessage(message);

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
        protected override void HandleMessage(GameMessage message)
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
            => _cutsceneInProgress = true;

        /// <summary>
        /// Processes the CutsceneEnded message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnCutsceneEnded(CutsceneEnded message)
=> _cutsceneInProgress = false;
    }
}