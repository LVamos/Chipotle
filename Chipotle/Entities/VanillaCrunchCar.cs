using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

using ProtoBuf;

namespace Game.Entities
{
    /// <summary>
    /// Represents the car of the Vanilla crunch company (auto v1) object standing in the garage of
    /// the company (garáž v1) locality.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class VanillaCrunchCar : Item
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        public VanillaCrunchCar(Name name, Rectangle area, bool decorative, bool pickable, string type) : base(name, area, type, decorative, pickable) { }

        /// <summary>
        /// A reference to the key hanger (věšák v1) object in the garage of the Vanilla crunch
        /// company (garáž v1) locality.
        /// </summary>
        private KeyHanger KeyHanger
            => World.GetObject("věšák v1") as KeyHanger;

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnUseObjects(UseObjects message)
        {
            _sounds.action = KeyHanger.KeysHanging ? "snd14" : "snd4";
            base.OnUseObjects(message);
        }
    }
}