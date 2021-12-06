using System;

using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// Represents the key hanger object in the garage in Vanilla crunch company (garáž v1) locality.
    /// </summary>
    [Serializable]
    public class KeyHanger : DumpObject
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        public KeyHanger(Name name, Plane area, bool decorative) : base(name, area, "věšák na klíče", decorative) { }

        /// <summary>
        /// Indicates if the keys are on the hanger.
        /// </summary>
        public bool KeysHanging { get; private set; } = true;

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnUseObject(UseObject message)
        {
            _sounds.action = KeysHanging ? "snd6" : "snd5";
            KeysHanging = !KeysHanging;
            base.OnUseObject(message);
        }
    }
}