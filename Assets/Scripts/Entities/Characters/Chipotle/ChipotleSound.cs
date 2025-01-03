﻿using Assets.Scripts.Messaging.Events.Characters;

using DavyKager;

using Game.Audio;
using Game.Messaging.Commands.GameInfo;
using Game.Messaging.Events;
using Game.Messaging.Events.Characters;
using Game.Messaging.Events.GameInfo;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Messaging.Events.Sound;
using Game.Terrain;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Chipotle
{
	/// <summary>
	/// Controls the sound output of the detective Chipotle NPC
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class ChipotleSound : Components.Sound
	{
		public void OnSaySize(SaySize message)
		{
			string text = $"{message.Area.Height} krát {message.Area.Width}";
			Tolk.Speak(text);
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="m">The message to be handled</param>
		protected void OnSayObjectDescription(SayObjectDescription m)
		{
			if (m.Object == null)
				Tolk.Speak("Před tebou nic není");
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
		private Dictionary<string, (string name, float gain)> _reverbPresets = new()
		{
			["chata c1"] = ("drivingincarsports", .1f),
			["obývák s1"] = ("livingroom", .9f),
			["asfaltka c1"] = ("plain", .1f),
			["cesta c1"] = ("pipesmall", .08f),
			["zahrada c1"] = ("outdoorsbackyard", .1f),
			["dvorek s1"] = ("outdoorsbackyard", .1f),
			["garáž s1"] = ("parkinglot", .1f),
			["hala s1"] = ("castlecupboard", .13f),
			["chodbička w1"] = ("castlecupboard", .13f),
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
			["hala w1"] = ("woodengalleoncourtyard", .05f),
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
		protected override void HandleMessage(Message message)
		{
			base.HandleMessage(message);

			switch (message)
			{
				case LeftBycar m: OnLeftBycar(m); break;
				case NoWallsNearby m:
					OnNoWallsNearby(m); break;
				case NearbywallsDetected m:
					OnNearbywallsDetected(m); break;
				case InteractResult m:
					OnInteractResult(m); break;
				case SayCharactersResult m: OnSayCharactersResult(m); break;
				case SaySize m: OnSaySize(m); break;
				case SayObjectDescription m: OnSayObjectDescription(m); break;
				case SayLocalityDescription m: OnSayLocalityDescription(m); break;
				case SayLocalityName m: OnSayLocalityName(m); break;
				case PlaceObjectResult m: OnPutObjectResult(m); break;
				case EmptyInventory m: OnEmptyInventory(m); break;
				case PickUpObjectResult m: OnPickUpObjectResult(m); break;
				case SayCoordinates sc: OnSayCoordinates(sc); break;
				case SayLocalitySize sl: OnSayLocalitySize(sl); break;
				case SayVisitedLocalityResult svl: OnSayVisitedLocality(svl); break;
				case SayOrientation m: OnSayOrientation(m); break;
				case SayExitsResult ser: OnSayExitsResult(ser); break;
				case SayObjectsResult sor: OnSayObjectsResult(sor); break;
				case CutsceneBegan cb: OnCutsceneBegan(cb); break;
				case DoorHit dh: OnEntityHitDoor(dh); break;
				case OrientationChanged ocd: OnOrientationChanged(ocd); break;
				case PositionChanged pcd: OnPositionChanged(pcd); break;
				case ObjectsCollided ocl: OnObjectsCollided(ocl); break;
				case TerrainCollided tcl: OnTerrainCollided(tcl); break;
				default: base.HandleMessage(message); break;
			}
		}

		private void OnLeftBycar(LeftBycar m)
		{
			AudioListener.volume = 0;
			Sounds.AdjustMasterVolume(1, Sounds.DefaultMasterVolume);
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="message">The message</param>
		private void OnNearbywallsDetected(NearbywallsDetected message)
		{
			throw new NotImplementedException();
		}

		private void OnNoWallsNearby(NoWallsNearby message)
		{
			//todo dodělat
		}

		/// <summary>
		/// Event handler for the result of an interaction.
		/// </summary>
		/// <param name="message">The event to be handled</param>
		private void OnInteractResult(InteractResult message)
		{
			Dictionary<InteractResult.ResultType, string> answers = new()
			{
				{ InteractResult.ResultType.NoObjects, "Není tu nic co by se dalo použít" },
				{ InteractResult.ResultType.NoUsableObjects, "Tohle se použít nedá" },
				{ InteractResult.ResultType.Far, "Musíš jít blíž" }
			};
			if (answers.TryGetValue(message.Result, out string answer))
				Tolk.Speak(answer);
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
			var resultMessages = new Dictionary<PickUpObjectResult.ResultType, string>
			{
				{ PickUpObjectResult.ResultType.Success, "sebráno" },
				{ PickUpObjectResult.ResultType.FullInventory, "Víc toho nepobereš." },
				{ PickUpObjectResult.ResultType.NothingFound, "Před tebou nic není" },
				{ PickUpObjectResult.ResultType.Unreachable, "Musíš jít blíž" },
				{ PickUpObjectResult.ResultType.Unpickable, "tohle nejde odnést" }
			};

			Tolk.Speak(resultMessages[m.Result]);
		}

		/// <summary>
		/// Initializes the component and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			base.Activate();
			_listenerOrientation.steps = -1;
		}

		/// <summary>
		/// Handles the SayCoordinates message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnSayCoordinates(SayCoordinates message)
		{
			Vector2 coords = message.Relative ? Owner.Area.Value.ToRelative().Center : Owner.Area.Value.Center;
			int intX = (int)coords.x;
			string x = coords.x == intX ? intX.ToString() : coords.x.ToString("0.0");
			int intY = (int)coords.y;
			string y = coords.y == intY ? intY.ToString() : coords.y.ToString("0.0");
			string result = x + (message.Relative ? " " : ", ") + y;
			Tolk.Speak(result, true);
		}


		/// <summary>
		/// Processes the CutsceneBegan message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnSayLocalitySize(SayLocalitySize message)
		{
			Terrain.Rectangle a = Owner.Locality.Area.Value;
			Tolk.Speak($"{a.Height.ToString()} krát {a.Width.ToString()}");
		}

		private void OnSayVisitedLocality(SayVisitedLocalityResult message)
			=> Tolk.Speak(message.Visited ? "jo jo" : "ne", true);

		/// <summary>
		/// Processes the SayExits message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnSayExitsResult(SayExitsResult message)
		{
			if (message.OccupiedPassage != null)
			{
				string type = message.OccupiedPassage.ToString() switch
				{
					"průchod" => "v průchodu",
					"dveře" => "ve dveřích",
					"vrata" => "ve vratech",
					_ => null
				};

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

			string[] exits = message.Exits.Select(e => string.Join(" ", e)).ToArray();
			Tolk.Speak(number + FormatStringList(exits, true) + ".", true);
		}

		/// <summary>
		/// Handles the SayCharactersResult message.
		/// </summary>
		/// <param name="message">The message</param>
		protected void OnSayCharactersResult(SayCharactersResult message)
		{
			if (message.Characters.IsNullOrEmpty())
				Tolk.Speak("Nikdo tu není", true);
			else
			{
				string text = FormatStringList(message.Characters);
				Tolk.Speak(text, true);
			}
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
			=> Tolk.Output(Owner.Orientation.Angle.GetCardinalDirection().GetDescription(), true);

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
		private void OnTerrainCollided(TerrainCollided message)
			=> PlayTerrain(message.Position);

		/// <summary>
		/// Processes the MovementDone message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnPositionChanged(PositionChanged message)
		{
			Vector2 center = message.TargetPosition.Center;
			Camera.main.transform.position = new Vector3(center.x, 2, center.y);

			if (!message.Silently)
				PlayTerrain(message.TargetPosition.Center);
		}

		/// <summary>
		/// Processes the ObjectsCollided message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnObjectsCollided(ObjectsCollided message)
			=> Tolk.Speak(message.Object.Name.Friendly);

		/// <summary>
		/// Processes the TurnoverDone message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnOrientationChanged(OrientationChanged message)
		{
			float source = (float)message.Source.Angle.CartesianDegrees;
			float target = (float)message.Target.Angle.CartesianDegrees;
			Camera.main.transform.Rotate(0, (float)(source - target), 0);

			if (message.Announce)
				SayOrientation();
		}

		private Vector2 _playerPosition => Owner.Area.Value.Center;

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
			Vector3 position3d = new Vector3(position.x, 1, position.y);
			TerrainType terrain = World.Map[position].Terrain;

			if (terrain == TerrainType.Wall)
			{
				Sounds.Play("hitwall", position3d, _defaultVolume);
				Tolk.Speak("zeď");
			}
			else
			{
				string sound = GetStepSoundName(terrain);
				Sounds.Play(sound, position3d, _walkVolume);
			}
		}

		private static string GetStepSoundName(TerrainType terrain)
		{
			string terrainName = Enum.GetName(terrain.GetType(), terrain);
			return "movstep" + terrainName;
		}
	}
}