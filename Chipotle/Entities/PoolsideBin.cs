using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    public class PoolsideBin : DumpObject
    {
        public PoolsideBin(Name name, Plane area) : base(name, area, "popelnice u bazénu", null, null, null, "cs3", true)
        { }

        protected override void OnUseObject(UseObject message)
        {
            base.OnUseObject(message);

            if (UsedOnce)
            {
                Move(new Plane(new Vector2(911, 1042)));
            }
        }
    }
}
