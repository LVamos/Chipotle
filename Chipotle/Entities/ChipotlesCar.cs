using System.Collections.Generic;
using System.Linq;

using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;
using Game.UI;

using Luky;

namespace Game.Entities
{
    public class ChipotlesCar : DumpObject
    {
        protected override void Move(Plane targetArea)
        {
            base.Move(targetArea);
            ChipotlesCarMoved message = new ChipotlesCarMoved(this, targetArea);
            Player.ReceiveMessage(message);
            _tuttle.ReceiveMessage(message);
        }


        private void Move(Plane targetLocation, string cutscene)
        {
            World.PlayCutscene(this, cutscene);
            Move(targetLocation);
            _moved = true;
        }


        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<System.Type, System.Action<Messaging.GameMessage>>
                {
                    [typeof(MoveChipotlesCar)] = (message) => OnMoveChipotlesCar((MoveChipotlesCar)message),
                    [typeof(UnblockLocality)] = (message) => OnUnblockLocality((UnblockLocality)message)
                }
                );
        }

        private void OnMoveChipotlesCar(MoveChipotlesCar message)
            => Move(message.Destination);

        private void OnUnblockLocality(UnblockLocality message)
        {
            if (!_allowedDestinations.Contains(message.Locality))
                _allowedDestinations.Add(message.Locality);
        }

        private Dictionary<string, string> _destinations = new Dictionary<string, string>() // locality inner name/rectangle coordinates
        {
            ["ulice p1"] = "1810, 1123, 1812, 1119", // at Christine's
            ["ulice h1"] = "1539, 1000, 1543, 998", // At the pub
            ["ulice s1"] = "1324, 1036, 1328, 1034", // In safe distance from Sweeney's house
            ["příjezdová cesta w1"] = "1025, 1036, 1027, 1032", // At a fence
            ["ulice v1"] = "2006, 1087, 2008, 1083", // On park place next to Vanilla crunch
            ["asfaltka c1"] = "1207, 984, 1209, 980" // At the edge of "cesta c1"
        };

        private Entity _tuttle => World.GetEntity("tuttle");
        private Entity Player => World.Player;

        private bool IsChipotleAlone()
            => !_area.GetLocality().IsItHere(_tuttle);

        private void DestinationMenu(string preferredCutscene = null)
        {
            Assert(!_allowedDestinations.IsNullOrEmpty(), "No allowed destinations.");

            string cutscene;
            if (string.IsNullOrEmpty(preferredCutscene))
                cutscene = IsChipotleAlone() ? "cs37" : "cs36";
            else cutscene = preferredCutscene;
            string[] innerNames = _allowedDestinations.Select(d => d.Name.Indexed).ToArray<string>();

            Dictionary<string, Locality> destinations = new Dictionary<string, Locality>();
            _allowedDestinations.Where(d => d != _area.GetLocality())
            .Foreach(d => destinations.Add(d.Name.Friendly, d));
            if (destinations.Count == 1)
            {
                Move(destinations.First().Value, cutscene);
                return;
            }

            string[] items = destinations.Keys.ToArray<string>();//_allowedDestinations.Select(d => d.Name.Friendly).ToArray<string>();
            int item = WindowHandler.Menu(items, "Kam chceš jet?");

            if (item >= 0)
                Move(destinations[items[item]], cutscene);
        }

        protected HashSet<Locality> _allowedDestinations = new HashSet<Locality>();

        protected HashSet<Locality> _visitedLocalities = new HashSet<Locality>();
        private bool _moved;

        public IReadOnlyCollection<Locality> VisitedLocalities => _visitedLocalities;

        public ChipotlesCar(Name name, Plane area) : base(name, area, "detektivovo auto", null, null, null, null)
        { }

        protected override void OnUseObject(UseObject message)
        {
            base.OnUseObject(message);

            // When it's not allowed to use the car, play a knocking souund.
            if (
                            (_area.GetLocality().Name.Indexed == "příjezdová cesta w1" && !_moved && !(WalshAreaObjectsUsed() && WalshAreaExplored()))
            || (_area.GetLocality().Name.Indexed == "asfaltka c1" && !CarsonsBenchesUsed()))
            {
                _actionSoundID = World.Sound.Play(World.Sound.GetRandomSoundStream("snd14"), null, false, PositionType.Absolute, message.Tile.Position.AsOpenALVector(), true, 1, null, 1, 0, Playback.OpenAL);
            }

            // If player didn't leave Walsh area but used required objects and went through all area
            else if (!_moved && WalshAreaObjectsUsed() && WalshAreaExplored())
            {
                AllowDestination(World.GetLocality("ulice p1"));
                DestinationMenu("cs20");
                AllowDestination(World.GetLocality("příjezdová cesta w1"));
            }
            else DestinationMenu(); // Let player seldct destination.
        }

        private bool WalshAreaExplored()
            => true;// Player.VisitedLocalities.Count == 14
                    //&& Player.VisitedLocalities.All(l => l.Name.Indexed.ToLower().Contains("w1"));

        private IEnumerable<DumpObject> WalshAreaObjects =>
            (new string[]
{"tělo w1", "hadice w1", "popelnice w1", "prkno w1", "lavička w1"})
            .Select(o => World.GetObject(o) as DumpObject);

        private bool WalshAreaObjectsUsed()
            => true;// WalshAreaObjects.All(o => o.Used);

        private void Move(Locality locality) => Move(new Plane(_destinations[locality.Name.Indexed]));

        private void Move(Locality locality, string cutscene)
            => Move(new Plane(_destinations[locality.Name.Indexed]), cutscene);


        private bool CarsonsBenchesUsed()
                => World.GetObjectsByType("lavice u carsona")
            .Any(o => o.Used);

        private void AllowDestination(Locality destination)
        {
            if (!_allowedDestinations.Contains(destination))
                _allowedDestinations.Add(destination);
        }
    }

}
