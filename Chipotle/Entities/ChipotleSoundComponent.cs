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
        /// Stores a position the player was located on when the last cutscene started.
        /// </summary>
        private Vector2? _cutsceneStartPosition;

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            _sound.ListenerOrientationUp = new Vector3(0, -1, 0);
            _listenerOrientation.steps = -1;
            _listenerPosition.steps = 30;
            _listenerPosition.currentStep = -1;
            base.Start();

            RegisterMessages(
            new Dictionary<Type, Action<GameMessage>>()
            {
                [typeof(SayVisitedLocalityResult)] = (message) => OnSayVisitedLocality((SayVisitedLocalityResult)message),
                [typeof(SayOrientation)] = (message) => OnSayOrientation((SayOrientation)message),
                [typeof(SayExitsResult)] = (message) => OnSayExitsResult((SayExitsResult)message),
                [typeof(SayObjectsResult)] = (message) => OnSayObjectsResult((SayObjectsResult)message),
                [typeof(CutsceneBegan)] = (message) => OnCutsceneBegan((CutsceneBegan)message),
                [typeof(LocalityChanged)] = (m) => OnLocalityChanged((LocalityChanged)m),
                [typeof(DoorHit)] = (m) => OnEntityHitDoor((DoorHit)m),
                [typeof(OrientationChanged)] = (m) => OnOrientationChanged((OrientationChanged)m),
                [typeof(PositionChanged)] = (message) => OnPositionChanged((PositionChanged)message),
                [typeof(ObjectsCollided)] = (m) => OnObjectsCollided((ObjectsCollided)m),
                [typeof(TerrainCollided)] = (message) => OnInpermeableTerrainCollision((TerrainCollided)message)
            }
            );
        }

        private void OnSayVisitedLocality(SayVisitedLocalityResult message)
            => Tolk.Speak(message.Visited ? "jo jo" : "ne", true);

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
            _cutsceneStartPosition = Owner.Area.Center;

            switch (message.CutsceneName)
            {
                case "cs7": case "cs8": case "cs10": _sound.ApplyEaxReverbPreset("carpettedhallway", 0); break;
            }
        }

        /// <summary>
        /// Processes the SayExits message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnSayExitsResult(SayExitsResult message)
        {
            if (message.OccupiedPassage != null)
            {
                string type = message.OccupiedPassage is Door ? " ve dveřích " : " v průchodu ";
                string to = message.OccupiedPassage.AnotherLocality(Owner.Locality).To;
                Tolk.Speak($"Stojíš {type}{to}", true);
                return;
            }

            if (message.Exits.IsNullOrEmpty())
            {
                Tolk.Speak("žádné východy nevidíš", true);
                return;
            }

            int count = message.Exits.Length;
                if (count == 1)
                {
                    Tolk.Speak(message.Exits[0], true);
                    return;
                }

                string number;
                if (count >= 2 && count <= 4)
                    number = "Jsou tu " + (count == 2 ? "dva" : "3") + " východy: ";
                else number = "Je tu " + count.ToString() + " východů: ";

                Tolk.Speak(number + FormatStringList(message.Exits, true) +".", true);
        }

        /// <summary>
        /// Handles the SayNearestObjects message.
        /// </summary>
        /// <param name="message">The message</param>
        protected void OnSayObjectsResult(SayObjectsResult message)
        {
            if (message.Objects.IsNullOrEmpty())
                Tolk.Speak("Nic tu není", true);
                else Tolk.Speak(FormatStringList(message.Objects), true);
        }

        /// <summary>
        /// Processes the CutsceneBegan message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnSayOrientation(SayOrientation message) => SayOrientation();

        /// <summary>
        /// Reports the current orientation of the Detective Chipotle NPC using a screen reader or
        /// voice synthesizer..
        /// </summary>
        protected void SayOrientation()
                    => Tolk.Speak(Owner.Orientation.Angle.GetCardinalDirection().GetDescription(), true);

        /// <summary>
        /// Settings for listener position updates
        /// </summary>
        private (Vector3 final, Vector3 step, int steps, int currentStep) _listenerPosition;

        /// <summary>
        /// sets listener's orientation according to the current orientation of the Detective
        /// Chipotle NPC.
        /// </summary>
        protected void UpdateListener()
        {
            UpdateListenerOrientation();
            UpdateListenerPosition();
        }

        private void UpdateListenerPosition()
        {
            if (_listenerPosition.currentStep == -1)
                return;

            if (_listenerPosition.currentStep == _listenerPosition.steps)
            {
                _sound.ListenerPosition = _listenerPosition.final;
                _listenerPosition.currentStep = -1;
                return;
            }

            _sound.ListenerPosition += _listenerPosition.step;
            _listenerPosition.currentStep++;
        }

        /// <summary>
        /// Updates orientation of the listener.
        /// </summary>
        private void UpdateListenerOrientation()
        {
            if (_listenerOrientation.steps < 0)
                return;

            if (_listenerOrientation.steps > 0) // Partial rotation
            {
                _listenerOrientation.current = _listenerOrientation.current.Rotate(_listenerOrientation.step); // Partial rotation
                _listenerOrientation.steps--;
            }
            else if (_listenerOrientation.steps == 0)  // Final step
            {
                _listenerOrientation.current = _listenerOrientation.final;
                _listenerOrientation.steps = -1;
            }

            _sound.ListenerOrientationFacing = _listenerOrientation.current.UnitVector.AsOpenALVector(); // Apply changes
        }

        /// <summary>
        /// Processes the EntityHitDoor message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnEntityHitDoor(DoorHit message)
=> Tolk.Speak("dveře", true);

        /// <summary>
        /// Processes the TerrainCollided message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnInpermeableTerrainCollision(TerrainCollided message)
             => PlayTerrain(World.Map[message.Position].Terrain);

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
        private void OnPositionChanged(PositionChanged message)
        {
            Vector3 current = _sound.ListenerPosition;
            Vector3 final = message.TargetPosition.Center.AsOpenALVector();
            final.Y = current.Y;

            // Skip it if the distance is too big
            if (World.GetDistance(current, final) > 5)
            {
                _listenerPosition.currentStep = -1;
                _sound.ListenerPosition = final;
                return;
            }

            // Plan update of listener's position
            _listenerPosition.final = final;

            Vector3 step = new Vector3(
            (Math.Abs(final.X - current.X) * (final.X < current.X ? -1 : 1)) / _listenerPosition.steps,
            0,
            (Math.Abs(final.Z - current.Z) * (final.Z < current.Z ? -1 : 1)) / _listenerPosition.steps
            );
            _listenerPosition.step = step;
            _listenerPosition.currentStep = 0;

            if(!message.Silently)
            PlayTerrain(World.Map[message.TargetPosition.Center].Terrain);

            WatchCutscene();
        }

        /// <summary>
        /// Stops an ongoing cutscene if the player moved 10 steps away from the initial position he was locayted on when the cutscene started.
        /// </summary>
        private void WatchCutscene()
        {
            if (_cutsceneStartPosition.HasValue && World.GetDistance((Vector2)_cutsceneStartPosition, Owner.Area.Center) > 5)
            {
                _cutsceneStartPosition = null;
                World.StopCutscene(Owner);
            }
        }

        /// <summary>
        /// Processes the ObjectsCollided message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnObjectsCollided(ObjectsCollided message)
        {
            Vector2 position = message.Position;
            Vector2 myOrientation = Owner.Orientation.UnitVector;
            Vector2 myPosition = Owner.Area.Center;
            Vector2 opposite = myOrientation.PerpendicularRight.PerpendicularRight +myPosition;
            PositionType positionType = PositionType.Absolute;
            if (position == opposite)
            {
                positionType = PositionType.Relative;
                position = new Vector2(0, -3);
            }

            _sound.Play(_sound.GetRandomSoundStream("movhitwall"), null, looping: false, positionType, position.AsOpenALVector(), true);
            Tolk.Speak(message.Object.Name.Friendly, true);
        }

        /// <summary>
        /// Processes the TurnoverDone message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnOrientationChanged(OrientationChanged message)
        {
             SayOrientation();

            // Immediate change
            if (message.Immediately)
            {
                _sound.ListenerOrientationFacing = message.Target.UnitVector.AsOpenALVector();
                return;
            }

            // Fluent change
            _listenerOrientation.steps = Math.Abs(message.Degrees);
                _listenerOrientation.step = message.Degrees / _listenerOrientation.steps;
            _listenerOrientation.current = message.Source;
            _listenerOrientation.final= message.Target;
        }

        /// <summary>
        /// Stores information for dynamic listener orientation settings.
        /// </summary>
        private (Orientation2D current, Orientation2D final, int step, int steps) _listenerOrientation;
        private const int _orientationSteps = 70;

        /// <summary>
        /// Plays a sound representation of a tile.
        /// </summary>
        /// <param name="tile">A tile to be announced</param>
        private void PlayTerrain(TerrainType terrain)
        {
            string soundName = "movstep" + Enum.GetName(terrain.GetType(), terrain);
            _sound.Play(stream: _sound.GetRandomSoundStream(soundName), role: null, looping: false, PositionType.Relative, Vector3.Zero, true, _walkingVolume, null, 1f, 0, Playback.OpenAL);

            if (terrain == TerrainType.Wall)
                Tolk.Speak("zeď", true);
        }
    }
}