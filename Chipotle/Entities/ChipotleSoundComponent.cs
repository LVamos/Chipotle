using System;
using System.Collections.Generic;
using System.Linq;

using DavyKager;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using OpenTK;

namespace Game.Entities
{
    /// <summary>
    /// Controls the sound output of the detective Chipotle NPC
    /// </summary>
    [Serializable]
    public class ChipotleSoundComponent : SoundComponent
    {
        /// <summary>
        /// Reverb presets for individual localities
        /// </summary>
        private readonly Dictionary<string, (string name, float gain)> _reverbPresets = new Dictionary<string, (string name, float gain)>
        {
            ["obývák s1"] = ("livingroom", .9f),
            ["asfaltka c1"] = ("plain", .1f),
            ["cesta c1"] = ("pipesmall", .08f),
            ["zahrada c1"] = ("outdoorsbackyard", .1f),
            ["dvorek s1"] = ("outdoorsbackyard", .1f),
            ["garáž s1"] = ("parkinglot", .1f),
            ["hala s1"] = ("castlecupboard", .13f),
            ["koupelna s1"] = ("prefabcaravan", .1f),
            ["kuchyň s1"] = ("carpettedhallway", .1f),
            ["ložnice w1"] = ("carpettedhallway", .1f),
            ["pokoj s1"] = ("carpettedhallway", .1f),
            ["ulice s1"] = ("prefabouthouse", .2f),
            ["chodba h1"] = ("woodenshortpassage", .1f),
            ["ulice h1"] = ("prefabouthouse", .2f),
            ["výčep h1"] = ("castlesmallroom", .1f),
            ["záchod h2"] = ("icepalacecupboard", .05f),
            ["záchod h3"] = ("drivingincarracer", .25f),
            ["záchod h4"] = ("drivingincarsports", .1f),
            ["balkon p1"] = ("prefabouthouse", .1f),
            ["garáž p1"] = ("parkinglot", .2f),
            ["hala p1"] = ("carpettedhallway", .9f),
            ["jídelna p1"] = ("castlecupboard", .2f),
            ["koupelna p1"] = ("prefabcaravan", .1f),
            ["kuchyň p1"] = ("prefabpractiseroom", .1f),
            ["ložnice p1"] = ("carpettedhallway", .8f),
            ["obývák p1"] = ("livingroom", .9f),
            ["ulice p1"] = ("prefabouthouse", .1f),
            ["záchod p1"] = ("drivingincarsports", .7f),
            ["bazén w1"] = ("SportFullStadium", .1f),
            ["garáž w1"] = ("parkinglot", .2f),
            ["hala w1"] = ("woodencourtyard", .05f),
            ["chodba w1"] = ("castlelongpassage", .03f),
            ["jídelna w1"] = ("castlesmallroom", .01f),
            ["koupelna w1"] = ("sportsmallswimmingpool", .05f),
            ["kuchyň w1"] = ("room", .2f),
            ["ložnice w1"] = ("paddedcell", .1f),
            ["obývák w1"] = ("livingroom", .6f),
            ["pokoj pro hosty w1"] = ("paddedcell", .1f),
            ["příjezdová cesta w1"] = ("sewerpipe", .1f),
            ["salón w1"] = ("castlecupboard", .1f),
            ["sklep w1"] = ("dustyroom", .1f),
            ["terasa w1"] = ("icepalacecourtyard", .01f),
            ["garáž v1"] = ("parkinglot", .2f),
            ["hala v1"] = ("prefabworkshop", .2f),
            ["chodba v1"] = ("castleshortpassage", .1f),
            ["kancelář v1"] = ("paddedcell", .1f),
            ["ulice v1"] = ("prefabouthouse", .3f)
        };

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            _sound.ListenerOrientationUp = new Vector3(0, -1, 0);
            base.Start();

            RegisterMessages(
            new Dictionary<Type, Action<GameMessage>>()
            {
                [typeof(SayExits)] = (message) => OnSayExits((SayExits)message),
                [typeof(SaySurroundingObjects)] = (message) => OnSayNearestObjects((SaySurroundingObjects)message),
                [typeof(CutsceneBegan)] = (message) => OnCutsceneBegan((CutsceneBegan)message),
                [typeof(LocalityChanged)] = (m) => OnLocalityChanged((LocalityChanged)m),
                [typeof(DoorHit)] = (m) => OnEntityHitDoor((DoorHit)m),
                [typeof(TurnEntityResult)] = (m) => OnTurnoverDone((TurnEntityResult)m),
                [typeof(EntityMoved)] = (m) => OnMovementDone((EntityMoved)m),
                [typeof(ObjectsCollided)] = (m) => OnObjectsCollided((ObjectsCollided)m),
                [typeof(TerrainCollided)] = (message) => OnInpermeableTerrainCollision((TerrainCollided)message)
            }
            );
        }

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();
            UpdateListener();
        }

        /// <summary>
        /// Returns word description of the specified angle.
        /// </summary>
        /// <param name="compassDegrees">The angle in compass degrees to be described</param>
        /// <returns>A word description in a string</returns>
        protected string GetAngleDescription(double compassDegrees, bool to = false)
        {
            if (compassDegrees >= 315 || compassDegrees < 45)
                return "před tebou";
            if (compassDegrees >= 45 && compassDegrees < 135)
                return " napravo";
            if (compassDegrees >= 135 && compassDegrees < 225)
                return " za tebou";

            return " nalevo";
        }

        /// <summary>
        /// Processes the CutsceneBegan message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnCutsceneBegan(CutsceneBegan message)
        {
            base.OnCutsceneBegan(message);
            switch (message.CutsceneName)
            {
                case "cs7": case "cs8": case "cs10": _sound.ApplyEaxReverbPreset("carpettedhallway", 0); break;
            }
        }

        /// <summary>
        /// Processes the SayExits message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnSayExits(SayExits message)
        {
            if (message.ExitInfo == null && !message.NothingFound)
                return;

            if (message.NothingFound)
                Tolk.Speak("žádné východy");
            else
            {
                List<string> info = message.ExitInfo
                    .Select(e => GetAngleDescription(e.compassDegrees) + " " + e.description)
                    .ToList<string>();

                if (info.Count == 1)
                {
                    (string description, double compassDegrees) record = message.ExitInfo.First();
                    Tolk.Speak($"{GetAngleDescription(record.compassDegrees)} je východ {record.description}");
                    return;
                }

                string number;
                if (info.Count == 2 || info.Count == 3)
                    number = "Jsou tady " + (info.Count == 2 ? "dva" : "3") + " východy: ";
                else number = "Je tady " + info.Count.ToString() + " východů: ";
                Tolk.Speak(number + FormatStringList(info, true));
            }
        }

        /// <summary>
        /// Handles the SayNearestObjects message.
        /// </summary>
        /// <param name="message">The message</param>
        protected void OnSayNearestObjects(SaySurroundingObjects message)
        {
            if (message.ObjectInfo == null && !message.NothingFound)
                return;

            if (message.NothingFound)
                Tolk.Speak("Nic tu není");
            else
            {
                List<string> objectInfo = message.ObjectInfo
                    .Select(o => o.friendlyName + " " + GetAngleDescription(o.compassDegrees)).ToList<string>();
                Tolk.Speak(FormatStringList(objectInfo));
            }
        }

        /// <summary>
        /// Reports the current orientation of the Detective Chipotle NPC using a screen reader or
        /// voice synthesizer..
        /// </summary>
        protected void SayOrientation()
                    => Tolk.Speak(Owner.Orientation.Angle.GetCardinalDirection().GetDescription());

        /// <summary>
        /// sets listener's orientation according to the current orientation of the Detective
        /// Chipotle NPC.
        /// </summary>
        protected void UpdateListener()
        {
            Vector2 position = Owner.Area.Center;
            Vector2 orientation = Owner.Orientation.UnitVector;

            if (_sound.ListenerPosition.X != position.X || _sound.ListenerPosition.Z != position.Y)
                _sound.ListenerPosition = position.AsOpenALVector();

            if (_sound.ListenerOrientationFacing.X != orientation.X || _sound.ListenerOrientationFacing.Z != orientation.Y)
                _sound.ListenerOrientationFacing = orientation.AsOpenALVector();
        }

        /// <summary>
        /// Processes the EntityHitDoor message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnEntityHitDoor(DoorHit message)
=> Tolk.Speak("dveře");

        /// <summary>
        /// Processes the TerrainCollided message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnInpermeableTerrainCollision(TerrainCollided message) => PlayTerrain(message.Tile);

        /// <summary>
        /// Processes the LocalityChanged message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnLocalityChanged(LocalityChanged message)
        {
            string targetLocality = message.Target.Name.Indexed.ToLower();
            (string name, float gain) = _reverbPresets[targetLocality];
            _sound.ApplyEaxReverbPreset(name, gain);
        }

        /// <summary>
        /// Processes the MovementDone message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnMovementDone(EntityMoved message) => PlayTerrain(World.Map[message.Target]);

        /// <summary>
        /// Processes the ObjectsCollided message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnObjectsCollided(ObjectsCollided message)
        {
            _sound.Play(_sound.GetRandomSoundStream("movhitwall"), null, looping: false, PositionType.Absolute, message.Position.AsOpenALVector(), true);
            Tolk.Speak(message.Object.Name.Friendly);
        }

        /// <summary>
        /// Processes the TurnoverDone message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnTurnoverDone(TurnEntityResult message) => SayOrientation();

        /// <summary>
        /// Plays a sound representation of a tile.
        /// </summary>
        /// <param name="tile">A tile to be announced</param>
        private void PlayTerrain(Tile tile)
        {
            string soundName = "movstep" + Enum.GetName(tile.Terrain.GetType(), tile.Terrain);
            _sound.Play(stream: _sound.GetRandomSoundStream(soundName), role: null, looping: false, PositionType.Relative, new Vector3(0, -1.7f, 0), true, 1f, null, 1f, 0, Playback.OpenAL);
        }
    }
}