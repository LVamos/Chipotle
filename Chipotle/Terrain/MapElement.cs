using System;
using System.Collections.Generic;

using Game.Messaging;
using Game.Messaging.Commands;

using Luky;

namespace Game.Terrain
{
    /// <summary>
    /// Base class for all objects that can be displayed on the game map.
    /// </summary>
    [Serializable]
    public abstract class MapElement : MessagingObject
    {
        /// <summary>     
        /// <summary>
        /// Lowpass setting for simulation of sounds obstructed by an object.
        /// </summary>
        protected (float gain, float gainHF) _overWallLowpass = (.9f, .03f);

        /// <summary>
        /// Lowpass setting for simulation of sounds over wall.
        /// </summary>
        protected (float gain, float gainHF) _overObjectLowpass = (.7f, .3f);

        /// <summary>
        /// Lowpass setting for simulation of sounds obstructed by a door.
        /// </summary>
        protected (float gain, float gainHF) _overDoorLowpass = (.3f, .5f);

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
        protected float OverObjectVolume => _defaultVolume * .9f;

        /// Inner and public name of the element
        /// </summary>
        public readonly Name Name;

        /// <summary>
        /// The area occupied by the element
        /// </summary>
        protected Plane _area;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner and public name of the element</param>
        /// <param name="area">Coordinates of the area the element occupies</param>
        public MapElement(Name name, Plane area) : base()
        {
            Name = name ?? throw new ArgumentException(nameof(name));
            _area = area;
        }

        /// <summary>
        /// Returns copy of the area occupied by the element
        /// </summary>
        public Plane Area
            =>_area == null ? null : new Plane(_area);

        /// <summary>
        /// Initializes the element and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>
                {
                    [typeof(Destroy)] = (m) => OnDestroy((Destroy)m)
                }
                );
        }

        /// <summary>
        /// Returns the public name of the element.
        /// </summary>
        /// <returns>Public name of the element</returns>
        public override string ToString()
=> Name.Friendly;

        /// <summary>
        /// Displays the element in the game World.
        /// </summary>
        protected virtual void Appear() { }

        /// <summary>
        /// Destroys the element.
        /// </summary>
        protected virtual void Destroy()
        {
            _messagingEnabled = false;
            Disappear();
        }

        /// <summary>
        /// Erases the element from the game World.
        /// </summary>
        protected virtual void Disappear()
        { }

        /// <summary>
        /// Processes the Destroy message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnDestroy(Destroy message)
            => Destroy();
    }
}