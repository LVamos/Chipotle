using Game.Messaging;
using Game.Messaging.Commands.Physics;
using Game.Terrain;

using System.Collections.Generic;

namespace Game.Entities.Items
{
    public class VanillaKeys : Item
    {
        /// <summary>
        /// Indicates if the Detective Chipotle NPC had icecream from the icecream machine (automat
        /// v1) object.
        /// </summary>
        private bool ChipotleHadIcecream
        {
            get
            {
                var icecreamMachine = World.GetItem("automat v1") as IcecreamMachine;
                return icecreamMachine.Used;
            }
        }

        private List<string> _unlockableCars = new() {
                    "auto v1",
                    "auto v2",
                    "auto v3",
                    "auto v4",
                    "auto v5",
                    "auto v6",
                    "auto v7",
                    "auto v8",
                    "auto v9"
                };

        public override void Initialize
            (
            Name name,
            Rectangle? area,
            string type,
            bool decorative,
            bool pickable,
            bool usable,
            bool passable = false,
            string collisionSound = null,
            string actionSound = null,
            string loopSound = null,
            string cutscene = null,
            bool usableOnce = false,
            bool audibleOverWalls = true,
            float volume = 1,
            bool stopWhenPlayerMoves = false,
            bool quickActionsAllowed = false,
            string pickingSound = null,
            string placingSound = null,
            List<string> usableWith = null,
            bool acousticObstacle = false
            )
        {
            List<string> usableList = new() {
                    "věšák v1",
                    "auto v1",
                    "auto v2",
                    "auto v3",
                    "auto v4",
                    "auto v5",
                    "auto v6",
                    "auto v7",
                    "auto v8",
                    "auto v9",
                    "vražedné auto v1"
                };

            base.Initialize(
                name,
                area,
                type,
                decorative,
                true,
                false,
                true,
                collisionSound,
                actionSound,
                loopSound,
                cutscene,
                false,
                audibleOverWalls,
                volume,
                stopWhenPlayerMoves,
                quickActionsAllowed,
                pickingSound,
                placingSound,
                usableList,
                acousticObstacle: acousticObstacle
                );
        }

        protected override void HandleMessage(Message message)
        {
            switch (message)
            {
                case UseObjects m: OnUseObjects(m); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Reference to the Detective's car (detektivovo auto) object.
        /// </summary>
        private ChipotlesCar ChipotlesCar
            => World.GetItem("detektivovo auto") as ChipotlesCar;

        private void OnUseObjects(UseObjects message)
        {
            string target = message.Target?.Name.Inner;
            if (target == null)
                _cutscene = "DrumOnCar";
            else if (target == "věšák v1")
                _cutscene = "HangKeys";
            else if (_unlockableCars.Contains(target))
                _cutscene = "CarUnlockAttempt";
            else if (target == "vražedné auto v1")
            {
                GameManager.FinishGame();
                if (ChipotleHadIcecream)
                    _cutscene = "cs7";
                else
                {
                    _cutscene = "cs8";
                    Zone destination = World.GetZone("ulice h1");
                    MoveChipotlesCar newMessage = new(this, destination);
                    ChipotlesCar.TakeMessage(newMessage);
                }
            }
            base.OnUseObjects(message);
        }
    }
}
