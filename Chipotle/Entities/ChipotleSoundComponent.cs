using DavyKager;

using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Game.Entities
{
    public class ChipotleSoundComponent : SoundComponent
    {
        private Dictionary<string, string> _reverbPresets = new Dictionary<string, string>
        {
            ["asfaltka c1"] = "plain",
            ["cesta c1"] = "pipesmall",
            ["zahrada c1"] = "outdoorsbackyard",
            ["dvorek s1"] = "outdoorsbackyard",
            ["garáž s1"] = "parkinglot",
            ["hala s1"] = "castlecupboard",
            ["koupelna s1"] = "prefabcaravan",
            ["kuchyň s1"] = "carpettedhallway",
            ["ložnice w1"] = "carpettedhallway",
            ["pokoj s1"] = "carpettedhallway",
            ["ulice s1"] = "prefabouthouse",
            ["chodba h1"] = "woodenshortpassage",
            ["ulice h1"] = "prefabouthouse",
            ["výčep h1"] = "castlesmallroom",
            ["záchod h2"] = "icepalacecupboard",
            ["záchod h3"] = "drivingincarracer",
            ["záchod h4"] = "drivingincarsports",
            ["balkon p1"] = "prefabouthouse",
            ["garáž p1"] = "parkinglot",
            ["hala p1"] = "carpettedhallway",
            ["jídelna p1"] = "woodencourtyard",
            ["koupelna p1"] = "prefabcaravan",
            ["kuchyň p1"] = "prefabpractiseroom",
            ["ložnice p1"] = "carpettedhallway",
            ["obývák p1"] = "livingroom",
            ["ulice p1"] = "prefabouthouse",
            ["záchod p1"] = "drivingincarsports",
            ["garáž w1"] = "parkinglot",
            ["hala w1"] = "woodencourtyard",
            ["chodba w1"] = "woodenshortpassage",
            ["jídelna w1"] = "woodencourtyard",
            ["koupelna w1"] = "bathroom",
            ["kuchyň w1"] = "room",
            ["ložnice w1"] = "paddedcell",
            ["obývák w1"] = "livingroom",
            ["pokoj pro hosty w1"] = "paddedcell",
            ["příjezdová cesta w1"] = "sewerpipe",
            ["salón w1"] = "castlecupboard",
            ["sklep w1"] = "dustyroom",
            ["terasa w1"] = "icepalacecourtyard",
            ["garáž v1"] = "spacestationlargeroom",
            ["hala v1"] = "prefabworkshop",
            ["chodba v1"] = "prefabschoolroom",
            ["kancelář v1"] = "paddedcell",
            ["ulice v1"] = "prefabouthouse"
        };

        public override void Update()
        {
            base.Update();
            UpdateListener();
        }



        public ChipotleSoundComponent() : base()
        {


        }

        protected void SayOrientation()
            => Tolk.Speak(Owner.Orientation.Angle.GetCardinalDirection().GetDescription());

        private void OnTurnoverDone(TurnEntityResult message) => SayOrientation();



        public override void Start()
        {
            _sound.ListenerOrientationUp = new Vector3(0, -1, 0);
            base.Start();

            RegisterMessages(
            new Dictionary<Type, Action<GameMessage>>()
            {
                [typeof(LocalityChanged)] = (m) => OnLocalityChanged((LocalityChanged)m),
                [typeof(DoorHit)] = (m) => OnEntityHitDoor((DoorHit)m),
                [typeof(TurnEntityResult)] = (m) => OnTurnoverDone((TurnEntityResult)m),
                [typeof(EntityMoved)] = (m) => OnMovementDone((EntityMoved)m),
                [typeof(ObjectsCollided)] = (m) => OnObjectsCollided((ObjectsCollided)m),
                [typeof(TerrainCollided)] = (m) => OnInpermeableTerrainCollision((TerrainCollided)m)
            }
            );

        }


        private void OnLocalityChanged(LocalityChanged message)
        {
            _sound.ApplyEaxReverbPreset(_reverbPresets[message.Target.Name.Indexed.ToLower()]);
        }

        private void OnEntityHitDoor(DoorHit m)
=> SayDelegate("dveře");
        private void OnInpermeableTerrainCollision(GameMessage message) => Tolk.Speak("penetráces");

        private void OnObjectsCollided(ObjectsCollided message)
        {
            GameObject collidingObject = message.Tile.Object;

            // Announce
            Timer t = new Timer();
            t.Interval = 500;
            t.Tick += (object s, EventArgs e) => { Say(collidingObject.Name.Friendly); t.Stop(); };
            t.Start();
            _sound.Play(_sound.GetRandomSoundStream("movhitwall"), null, looping: false, PositionType.Absolute, message.Tile.Position.AsOpenALVector(), true);
        }

        protected void UpdateListener()
        {
            Vector2 position = Owner.Area.Center;
            Vector2 orientation = Owner.Orientation.UnitVector;

            if (_sound.ListenerPosition.X != position.X || _sound.ListenerPosition.Z != position.Y)
            {
                _sound.ListenerPosition = position.AsOpenALVector();
            }

            if (_sound.ListenerOrientationFacing.X != orientation.X || _sound.ListenerOrientationFacing.Z != orientation.Y)
            {
                _sound.ListenerOrientationFacing = orientation.AsOpenALVector();
            }
        }

        private void OnMovementDone(EntityMoved message) => PlayTerrain(message.Target);

        private void PlayTerrain(Tile tile)
        {
            string soundName = "movstep" + Enum.GetName(tile.Terrain.GetType(), tile.Terrain);
            _sound.Play(stream: _sound.GetRandomSoundStream(soundName), role: null, looping: false, PositionType.Relative, new Vector3(0, -1.7f, 0), true, 1f, null, 1f, 0, Playback.OpenAL);
        }
    }
}
