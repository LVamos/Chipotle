using Game.Entities;
using Game.Messaging;
using Game.Messaging.Commands;

using Luky;

using OpenTK;

using ProtoBuf;

using System;

namespace Game.Terrain
{
    /// <summary>
    /// Base class for all objects that can be displayed on the game map.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(GameObject))]
    [ProtoInclude(101, typeof(Locality))]
    [ProtoInclude(102, typeof(Passage))]
    public abstract class MapElement : MessagingObject
    {
        /// <summary>
        /// Plays a sound at the specified position.
        /// </summary>
        /// <param name="soundName">Name of the sound to be played</param>
        /// <param name="position">Position at which the sound should be played</param>
		protected int Play(string soundName, Vector2 position) => string.IsNullOrEmpty(soundName) ? 0 : World.Sound.Play(World.Sound.GetRandomSoundStream(soundName), null, false, PositionType.Absolute, position.AsOpenALVector(), true, _defaultVolume);

        /// <summary>
        /// Plays a sound at the specified position.
        /// </summary>
        /// <param name="soundName">Name of the sound to be played</param>
        /// <param name="position">Position of the sound</param>
        /// <param name="loop">determines if the soudn should be played in loop.</param>
        /// <returns>Handle of the sound</returns>
		protected int Play(string soundName, Vector2 position, bool loop) => World.Sound.Play(World.Sound.GetRandomSoundStream(soundName), null, loop, PositionType.Absolute, position.AsOpenALVector(), true, _defaultVolume);

        /// <summary>
        /// Returns pooint that belongs to this object and is tho most close to tthe player.
        /// </summary>
        protected Vector2 GetClosestPointToPlayer()
            => _area.GetClosestPoint(World.Player.Area.Center);

        /// <summary>
        /// Plays a sound at the position closest to the player.
        /// </summary>
        /// <param name="soundName">Name of the sound to be played</param>
        /// <param name="loop">Determines if the soudn should be played in loop</param>
		protected int Play(string soundName, bool loop = false) => string.IsNullOrEmpty(soundName) ? 0 : Play(soundName, GetClosestPointToPlayer(), loop);

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
        protected float OverWallVolume => _defaultVolume * .4f;

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
        public readonly Name Name;

        /// <summary>
        /// A backing field for Area.
        /// </summary>
        protected Rectangle _area;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner and public name of the element</param>
        /// <param name="area">Coordinates of the area the element occupies</param>
        public MapElement(Name name, Rectangle area) : base()
        {
            Name = name ?? throw new ArgumentException(nameof(name));
            Area = area;
        }

        /// <summary>
        /// Returns copy of the area occupied by the element
        /// </summary>
        public Rectangle Area
        {
            get => _area == null ? null : new Rectangle(_area);
            protected set => SetArea(value);
        }

        /// <summary>
        /// Sets value of the Area property.
        /// </summary>
        /// <param name="value">A value assigned to the property</param>
        protected virtual void SetArea(Rectangle value)
        {
            _area = value;
            if (value != null)
                Dimensions = (_area.Width, _area.Height);
        }

        /// <summary>
        /// Returns the public name of the element.
        /// </summary>
        /// <returns>Public name of the element</returns>
        public override string ToString()
=> Name.Friendly;

        /// <summary>
        /// Destroys the element.
        /// </summary>
        protected virtual void Destroy() => _messagingEnabled = false;

        /// <summary>
        /// Processes the Destroy message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnDestroy(Destroy message)
            => Destroy();

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case Destroy d: OnDestroy(d); break;
                default: base.HandleMessage(message); break;
            }
        }
    }
}