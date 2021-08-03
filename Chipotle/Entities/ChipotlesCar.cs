using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

using System.Collections.Generic;
using System.Linq;

namespace Game.Entities
{
    public class ChipotlesCar : DumpObject
    {
        protected HashSet<Locality> _destinations = new HashSet<Locality>();

        protected HashSet<Locality> _visitedLocalities = new HashSet<Locality>();
        public IReadOnlyCollection<Locality> VisitedLocalities => _visitedLocalities;

        public ChipotlesCar(Name name, Plane area) : base(name, area, "detektivovo auto", null, null, null, null)
        { }

        protected override void OnUseObject(UseObject message)
        {
            base.OnUseObject(message);

            // Local function to check if an object from Walsh area was used
            bool Used(string name)
                => World.GetObject(name).Used;

            // Check if all localities of Walsh area were visited.
            bool walshAreaExplored =
            World.Player.VisitedLocalities.Count == 14
                    && World.Player.VisitedLocalities.All(l => l.Name.Indexed.ToLower().Contains("w1"));

            // Check if some important objects were used
            bool ObjectsUsed =
                 (new string[]
                 {"tělo w1", "hadice w1", "popelnice w1", "prkno w1", "lavička w1"})
            .All(o => Used(o));

            bool leftArea = !_visitedLocalities.IsNullOrEmpty(); // Checks if player left Walsh area with the car

            if (!leftArea && !(ObjectsUsed && walshAreaExplored))
            {
                _actionSoundID = World.Sound.Play(
                    World.Sound.GetRandomSoundStream("snd14"),
                    null,
                    false,
                    PositionType.Absolute,
                    message.Tile.Position.AsOpenALVector(),
                    true,
                    1f,
                    null,
                    1f,
                    0,
                    Playback.OpenAL
                    );
                return;
            }

            // If player didn't leave Walsh area but used required objects and went through all area
            if (!leftArea && ObjectsUsed && walshAreaExplored)
            {
                _destinations.Add(World.GetLocality("ulice p1"));
                World.PlayCutscene(this, "cs20");
            }
        }
    }
}
