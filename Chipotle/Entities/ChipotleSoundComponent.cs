using DavyKager;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using OpenTK;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Entities
{
    /// <summary>
    /// Controls the sound output of the detective Chipotle NPC
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class ChipotleSoundComponent : SoundComponent
    {
        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="m">The message to be handled</param>
        protected void OnSayObjectDescription(SayObjectDescription m)
        {
            if (m.Object == null)
                Tolk.Speak("Před tebou nci není");
            else Tolk.Speak(m.Object.Description);
        }

        /// <summary>
        /// Processes the SayLocality message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnSayLocalityName(SayLocalityName message)
=> Tolk.Speak(Owner.Locality.Name.Friendly, true);


        /// <summary>
        /// Reverb presets for individual localities
        /// </summary>
        private Dictionary<string, (string name, float gain)> _reverbPresets = new Dictionary<string, (string name, float gain)>
        {
			["chata c1"] = ("drivingincarsports", .1f),
			["obývák s1"] = ("livingroom", .9f),
            ["asfaltka c1"] = ("plain", .1f),
            ["cesta c1"] = ("pipesmall", .08f),
            ["zahrada c1"] = ("outdoorsbackyard", .1f),
            ["dvorek s1"] = ("outdoorsbackyard", .1f),
            ["garáž s1"] = ("parkinglot", .1f),
            ["hala s1"] = ("castlecupboard", .13f),
            ["koupelna s1"] = ("prefabcaravan", .1f),
            ["kuchyň s1"] = ("carpettedhallway", .1f),
            ["pokoj s1"] = ("carpettedhallway", .1f),
            ["ulice s1"] = ("prefabouthouse", .2f),
            ["chodba h1"] = ("woodenshortpassage", .15f),
            ["ulice h1"] = ("prefabouthouse", .2f),
            ["výčep h1"] = ("castlesmallroom", .15f),
            ["záchod h2"] = ("icepalacecupboard", .05f),
            ["záchod h3"] = ("drivingincarracer", .17f),
            ["záchod h4"] = ("drivingincarsports", .1f),
            ["balkon p1"] = ("prefabouthouse", .1f),
            ["garáž p1"] = ("parkinglot", .09f),
            ["hala p1"] = ("carpettedhallway", .5f),
            ["jídelna p1"] = ("castlecupboard", .13f),
            ["koupelna p1"] = ("prefabcaravan", .1f),
            ["kuchyň p1"] = ("prefabpractiseroom", .1f),
            ["ložnice p1"] = ("carpettedhallway", .6f),
            ["obývák p1"] = ("livingroom", .9f),
            ["ulice p1"] = ("prefabouthouse", .13f),
            ["záchod p1"] = ("drivingincarsports", .7f),
            ["bazén w1"] = ("SportFullStadium", .02f),
            ["garáž w1"] = ("parkinglot", .13f),
            ["hala w1"] = ("woodencourtyard", .05f),
            ["chodba w1"] = ("castlelongpassage", .02f),
            ["jídelna w1"] = ("castlesmallroom", .01f),
            ["koupelna w1"] = ("sportsmallswimmingpool", .02f),
            ["kuchyň w1"] = ("room", .2f),
            ["ložnice w1"] = ("paddedcell", .1f),
            ["obývák w1"] = ("livingroom", .6f),
            ["pokoj pro hosty w1"] = ("paddedcell", .1f),
            ["příjezdová cesta w1"] = ("sewerpipe", .1f),
            ["salón w1"] = ("castlecupboard", .05f),
            ["sklep w1"] = ("dustyroom", .1f),
            ["terasa w1"] = ("icepalacecourtyard", .02f),
            ["garáž v1"] = ("parkinglot", .2f),
            ["hala v1"] = ("prefabworkshop", .11f),
            ["chodba v1"] = ("castleshortpassage", .06f),
            ["kancelář v1"] = ("paddedcell", .2f),
            ["ulice v1"] = ("prefabouthouse", .3f)
        };

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case SayObjectDescription m: OnSayObjectDescription(m); break;
                case SayLocalityDescription m: OnSayLocalityDescription(m); break;
                case SayLocalityName m: OnSayLocalityName(m); break;
                case PlaceObjectResult m: OnPutObjectResult(m); break;
                case EmptyInventory m: OnEmptyInventory(m); break;
                case PickUpObjectResult m: OnPickUpObjectResult(m); break;
                case SayCoordinates sc: OnSayCoordinates(sc); break;
                case SayLocalitySize sl: OnSayLocalitySize(sl); break;
                case SayVisitedLocalityResult svl: OnSayVisitedLocality(svl); break;
                case SayOrientation sor: OnSayOrientation(sor); break;
                case SayExitsResult ser: OnSayExitsResult(ser); break;
                case SayObjectsResult sor: OnSayObjectsResult(sor); break;
                case CutsceneBegan cb: OnCutsceneBegan(cb); break;
                case LocalityChanged lcd: OnLocalityChanged(lcd); break;
                case DoorHit dh: OnEntityHitDoor(dh); break;
                case OrientationChanged ocd: OnOrientationChanged(ocd); break;
                case PositionChanged pcd: OnPositionChanged(pcd); break;
                case ObjectsCollided ocl: OnObjectsCollided(ocl); break;
                case TerrainCollided tcl: OnTerrainCollided(tcl); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="m">The message to be handled</param>
        private void OnSayLocalityDescription(SayLocalityDescription m)
        {
            Tolk.Speak(Owner.Locality.Description);
        }

        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="m">Source of the message</param>
        private void OnPutObjectResult(PlaceObjectResult m)
        {
            if (m.Success)
                Tolk.Speak("Položeno");
            else Tolk.Speak("Sem se to nevejde");
        }

        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="m">The message to be handled</param>
        protected void OnEmptyInventory(EmptyInventory m)
=> Tolk.Speak("Nic u sebe nemáš");

        /// <summary>
        /// Handles the PickUpObjectResult message.
        /// </summary>
        /// <param name="m">The message to be processed</param>
        protected void OnPickUpObjectResult(PickUpObjectResult m)
        {
            switch (m.Result)
            {
                case PickUpObjectResult.ResultType.Success:
                    Tolk.Speak("sebráno");
                    break;
                case PickUpObjectResult.ResultType.FullInventory:
                    Tolk.Speak("Víc toho nepobereš.");
                    break;
                case PickUpObjectResult.ResultType.NoObject:
                    Tolk.Speak("Před tebou nic není");
                    break;
                default:
                    Tolk.Speak("tohle nejde odnést");
                    break;
            }
        }

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            _listenerOrientation.steps = -1;
            base.Start();
        }

        /// <summary>
        /// Handles the SayCoordinates message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        private void OnSayCoordinates(SayCoordinates message)
        {
            Vector2 coords = message.Relative ? Owner.Area.ToRelative().Center : Owner.Area.Center;
            string result = Math.Round(coords.X).ToString() + (message.Relative ? " " : ", ") + Math.Round(coords.Y).ToString();
            Tolk.Speak(result, true);
        }


        /// <summary>
        /// Processes the CutsceneBegan message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnSayLocalitySize(SayLocalitySize message)
        {
            Rectangle a = Owner.Locality.Area;
            Tolk.Speak($"{a.Height.ToString()} krát {a.Width.ToString()}");
        }

        private void OnSayVisitedLocality(SayVisitedLocalityResult message)
            => Tolk.Speak(message.Visited ? "jo jo" : "ne", true);

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();
            UpdateListenerOrientation();
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
                case "cs7": case "cs8": case "cs10": World.Sound.ApplyEaxReverbPreset("carpettedhallway", 0); break;
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
                string type = null;
                switch (message.OccupiedPassage.ToString())
                {
                    case "průchod": type = "v průchodu"; break;
                    case "dveře": type = "ve dveřích"; break;
                    case "vrata": type = "ve vratech"; break;
                }

                string to = message.OccupiedPassage.AnotherLocality(Owner.Locality).To;
                Tolk.Speak($"Stojíš {type}{to}", true);
                return;
            }

            if (message.Exits.IsNullOrEmpty())
            {
                Tolk.Speak("žádné východy nevidíš", true);
                return;
            }

            int count = message.Exits.Count;
            if (count == 1)
            {
                Tolk.Speak(message.Exits[0][0], true);
                return;
            }

            string number;
            if (count >= 2 && count <= 4)
                number = "Jsou tu " + (count == 2 ? "dva" : count.ToString()) + " východy: ";
            else number = "Je tu " + count.ToString() + " východů: ";

            string[] exits = message.Exits.Select(e => string.Join(", ", e)).ToArray();
            Tolk.Speak(number + FormatStringList(exits, true) + ".", true);
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
        protected void OnSayOrientation(SayOrientation message)
            => SayOrientation();

        /// <summary>
        /// Reports the current orientation of the Detective Chipotle NPC using a screen reader or
        /// voice synthesizer..
        /// </summary>
        protected void SayOrientation()
                    => Tolk.Speak(Owner.Orientation.Angle.GetCardinalDirection().GetDescription(), true);

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

            World.Sound.ListenerOrientationFacing = _listenerOrientation.current.UnitVector.AsOpenALVector(); // Apply changes
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
        private void OnTerrainCollided(TerrainCollided message)
             => PlayTerrain(message.Position);

        /// <summary>
        /// Processes the LocalityChanged message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnLocalityChanged(LocalityChanged message)
        {
            string targetLocality = message.Target.Name.Indexed.ToLower();
            (string name, float gain) = _reverbPresets[targetLocality];
            World.Sound.ApplyEaxReverbPreset(name, gain);
        }

        /// <summary>
        /// Processes the MovementDone message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnPositionChanged(PositionChanged message)
        {
            World.Sound.ListenerPosition = message.TargetPosition.Center.AsOpenALVector();

            if (!message.Silently)
                PlayTerrain(message.TargetPosition.Center);
        }

        /// <summary>
        /// Processes the ObjectsCollided message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnObjectsCollided(ObjectsCollided message)
            => Tolk.Speak(message.Object.Name.Friendly, true);

        /// <summary>
        /// Processes the TurnoverDone message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnOrientationChanged(OrientationChanged message)
        {
            // Immediate change
            if (message.Immediately)
            {
                World.Sound.ListenerOrientationFacing = message.Target.UnitVector.AsOpenALVector();
                return;
            }

            SayOrientation();

            // Fluent change
            _listenerOrientation.step = 2 * (message.Degrees / Math.Abs(message.Degrees));
            _listenerOrientation.steps = message.Degrees / _listenerOrientation.step;
            _listenerOrientation.current = message.Source;
            _listenerOrientation.final = message.Target;
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
        private void PlayTerrain(Vector2 position)
        {
            TerrainType terrain = World.Map[position].Terrain;

            if (terrain == TerrainType.Wall)
            {
                World.Sound.Play(stream: World.Sound.GetRandomSoundStream("hitwall"), role: null, looping: false, PositionType.Absolute, position.AsOpenALVector(), true, 1);
                Tolk.Speak("zeď");
            }
            else
            {
                string soundName = "movstep" + Enum.GetName(terrain.GetType(), terrain);
                World.Sound.Play(stream: World.Sound.GetRandomSoundStream(soundName), role: null, looping: false, PositionType.Relative, Vector3.Zero, true, _walkingVolume, null, 1f, 0, Playback.OpenAL);
            }
        }
    }
}