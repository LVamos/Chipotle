using System;
using System.Collections.Generic;

using DavyKager;

using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using OpenTK;

namespace Game.Entities
{
    /// <summary>
    /// Controls the sound output of the detective Chipotle NPC
    /// </summary>
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
        private void OnMovementDone(EntityMoved message) => PlayTerrain(message.Target);

        /// <summary>
        /// Processes the ObjectsCollided message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnObjectsCollided(ObjectsCollided message)
        {
            _sound.Play(_sound.GetRandomSoundStream("movhitwall"), null, looping: false, PositionType.Absolute, message.Tile.Position.AsOpenALVector(), true);
            Tolk.Speak(message.Tile.Object.Name.Friendly);
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