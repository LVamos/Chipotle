using Game.Messaging.Commands.Physics;

using System.Collections.Generic;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
    /// <summary>
    /// Represents the murder car (vražedné auto v1) in the garage of the Vanilla crunch company (garáž v1) zone.
    /// </summary>
    public class MurderCar : Item
    {
        public override void Initialize(
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
            base.Initialize(
                name,
                area,
                type,
                decorative,
                pickable,
                usable,
                passable,
                acousticObstacle: acousticObstacle);
        }

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnUseObjects(UseObjects message)
        {
            // The Vanilla keys (klíče v1) play the ending cutscene themselves.
            bool unlockedByKeys = message.Target == this && message.UsedObject?.Name.Inner == "klíče v1";
            _cutscene = unlockedByKeys ? null : "DrumOnCar";

            base.OnUseObjects(message);
        }
    }
}
