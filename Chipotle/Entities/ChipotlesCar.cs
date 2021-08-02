using Game.Terrain;
using System.Collections.Generic;
using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    public class ChipotlesCar: DumpObject
    {
        protected HashSet<Locality> _visitedLocalities = new HashSet<Locality>();
        public IReadOnlyCollection<Locality> VisitedLocalities
        {
            get => _visitedLocalities;
        }

        public ChipotlesCar(Name name, Plane area) : base(name, area, "detektivovo auto", null, null, null, null)
        { }

        protected override void OnUseObject(UseObject message)
        {
            base.OnUseObject(message);

            if(_visitedLocalities.IsNullOrEmpty())
                _actionSoundID = World.Sound.Play(stream: World.Sound.GetRandomSoundStream("snd14"), null, false, PositionType.Absolute, message.Tile.Position.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);

        }
    }
}
