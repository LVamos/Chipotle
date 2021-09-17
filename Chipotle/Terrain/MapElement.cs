using Game.Messaging;
using Game.Messaging.Commands;

using Luky;

using System;
using System.Collections.Generic;

namespace Game.Terrain
{
    /// <summary>
    /// Base class for all objects that can be displayed on the game map.
    /// </summary>
    public abstract class MapElement : MessagingObject
    {

        /// <summary>
        /// Destroys the element.
        /// </summary>
        protected virtual void Destroy()
        {
            _messagingEnabled = false;
            Disappear();
        }

        /// <summary>
        /// Displays the element in the game World.
        /// </summary>
        protected virtual void Appear() { }


        /// <summary>
        /// Erases the element from the game World.
        /// </summary>
        protected virtual void Disappear()
        {        }



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner and public name of the element</param>
        /// <param name="area">Coordinates of the area the element occupies</param>
        public MapElement(Name name, Plane area) : base()
        {
            Name = name ?? throw new ArgumentException(nameof(name));
            _area = area;// ?? throw new ArgumentException(nameof(area));
        }

        /// <summary>
        /// Inner and public name of the element
        /// </summary>
        public readonly Name Name;

        /// <summary>
        /// The area occupied by the element
        /// </summary>
        protected Plane _area;

        /// <summary>
        /// Returns copy of the area occupied by the element
        /// </summary>
        public Plane Area
            => new Plane(_area);

        /// <summary>
        /// Returns the public name of the element.
        /// </summary>
        /// <returns>Public name of the element</returns>
        public override string ToString()
=> Name.Friendly;


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
        /// Processes the Destroy message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnDestroy(Destroy message)
            => Destroy();
    }
}
