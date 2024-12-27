using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

using ProtoBuf;

namespace Game.Entities
{
    /// <summary>
    /// Represents the killer's car object in the garage of Vanilla crunch (garáž v1) locality.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class KillersCar : Item
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        public KillersCar(Name name, Rectangle area, bool decorative, bool pickable) : base(name, area, "vražedné auto", decorative, pickable) { }

        /// <summary>
        /// Indicates if the Detective Chipotle NPC had icecream from the icecream machine (automat
        /// v1) object.
        /// </summary>
        private bool ChipotleHadIcecream
            => (World.GetObject("automat v1") as IcecreamMachine).Used;

        /// <summary>
        /// Reference to the Detective's car (detektivovo auto) object.
        /// </summary>
        private ChipotlesCar ChipotlesCar
            => World.GetObject("detektivovo auto") as ChipotlesCar;

        /// <summary>
        /// Indicates if the keys are on the hanger.
        /// </summary>
        private bool KeysOnHanger
            => (World.GetObject("věšák v1") as KeyHanger).KeysHanging;

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnUseObjects(UseObjects message)
        {
            if (KeysOnHanger)
                _sounds.action = "snd14";
            else if (ChipotleHadIcecream)
                _cutscene = "cs7";
            else
            {
                _cutscene = "cs8";
                ChipotlesCar.TakeMessage(new MoveChipotlesCar(this, World.GetLocality("ullice h1")));
            }

            base.OnUseObjects(message);
        }
    }
}