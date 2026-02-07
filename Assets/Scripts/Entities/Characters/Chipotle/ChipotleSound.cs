using Assets.Scripts.Messaging.Events.Characters;
using Assets.Scripts.Narration.WorldDescribers;

using DavyKager;

using Game.Audio;
using Game.Entities.Characters.Components;
using Game.Entities.Items;
using Game.Messaging.Commands.Characters;
using Game.Messaging.Commands.GameInfo;
using Game.Messaging.Events;
using Game.Messaging.Events.Characters;
using Game.Messaging.Events.GameInfo;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Messaging.Events.Sound;
using Game.Models;
using Game.Narration.WorldDescribers;
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
	public class ChipotleSound : Sound
	{
		private void Update()
		{
			if (_footStep != null && _footStep.isPlaying)
				SnapFootstepToListener();
		}

		private void OnSayNavigatedObjectLocationResult(SayNavigatedObjectLocationResult message)
		{
			if (message.NoNavigatedObjects)
				Tolk.Speak("Nevybral jsi cíl.");
		}

		protected AudioSource _footStep;

		private void InitFootStepSource()
		{
			GameObject obj = new GameObject("FootstepSource");
			_footStep = obj.AddComponent<AudioSource>();
			_footStep.spatialBlend = 1f;   // 3D
			_footStep.dopplerLevel = 0f;
			_footStep.playOnAwake = false;
			_footStep.spatialize = true;
			_footStep.spatializePostEffects = false;
			_footStep.outputAudioMixerGroup = Sounds.ResonanceGroup;

			ResonanceAudioSource resonance = obj.AddComponent<ResonanceAudioSource>();
			resonance.nearFieldEffectEnabled = true;
			resonance.occlusionEnabled = true;
		}

		public override void Initialize()
		{
			base.Initialize();
			InitFootStepSource();
			_exitDescriber = new();
			_itemDescriber = new();
			_characterDescriber = new();
			_announceWalls = true;
		}

		protected new void PlayStep(Vector2 position, ObstacleType obstacle = ObstacleType.None)
		{
			string sound = GetStepSoundName(position);
			AudioClip clip = Sounds.GetClip(sound);
			SnapFootstepToListener();
			_footStep.PlayOneShot(clip, _walkVolume);

			AnnounceWall(position);
		}

		private void SnapFootstepToListener()
		{
			Vector3 position3d = Camera.main.transform.position;
			_footStep.transform.position = new Vector3(position3d.x, 1, position3d.z);
		}

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
			else
				Tolk.Speak(m.Object.Description);
		}

		/// <summary>
		/// Processes the SayZone message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnSayZoneName(SayZoneName message)
		{
			string text = Owner.Zone.Name.Friendly;
			if (Settings.SayInnerZoneNames)
				text += " " + Owner.Zone.Name.Indexed;
			Tolk.Speak(text, true);
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			base.HandleMessage(message);

			switch (message)
			{
				case NavigationStopped m:
					OnNavigationStopped(m); break;
				case SayNavigatedObjectLocationResult m:
					OnSayNavigatedObjectLocationResult(m); break;
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
				case SayZoneDescription m: OnSayZoneDescription(m); break;
				case SayZoneName m: OnSayZoneName(m); break;
				case PlaceItemResult m: OnPlaceItemResult(m); break;
				case EmptyInventory m: OnEmptyInventory(m); break;
				case PickUpItemResult m: OnPickUpItemResult(m); break;
				case SayCoordinates sc: OnSayCoordinates(sc); break;
				case SayZoneSize sl: OnSayZoneSize(sl); break;
				case SayVisitedZoneResult svl: OnSayVisitedZone(svl); break;
				case SayOrientation m: OnSayOrientation(m); break;
				case SayExitsResult ser: OnSayExitsResult(ser); break;
				case SayItemsResult sor: OnSayItemsResult(sor); break;
				case CutsceneBegan cb: OnCutsceneBegan(cb); break;
				case CharacterHitDoor m: OnCharacterHitDoor(m); break;
				case OrientationChanged ocd: OnOrientationChanged(ocd); break;
				case PositionChanged pcd: OnPositionChanged(pcd); break;
				case ObjectsCollided ocl: OnObjectsCollided(ocl); break;
				case TerrainCollided tcl: OnTerrainCollided(tcl); break;
				default: base.HandleMessage(message); break;
			}
		}


		private void OnNavigationStopped(NavigationStopped message)
		{
			if (message.TargetReached)
				Tolk.Output("Jsi u cíle.");
		}

		private void OnLeftBycar(LeftBycar m)
		{
			AudioListener.volume = 0;
			Sounds.SlideMasterVolume(1, Sounds.DefaultMasterVolume);
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="message">The message</param>
		private void OnNearbywallsDetected(NearbywallsDetected message) => throw new NotImplementedException();

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
		private void OnSayZoneDescription(SayZoneDescription m) => Tolk.Speak(Owner.Zone.Description);

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="m">Source of the message</param>
		private void OnPlaceItemResult(PlaceItemResult m)
		{
			if (m.Success)
				Tolk.Speak("Položeno");
			else
				Tolk.Speak("Sem se to nevejde");
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="m">The message to be handled</param>
		protected void OnEmptyInventory(EmptyInventory m) => Tolk.Speak("Nic u sebe nemáš");

		/// <summary>
		/// Handles the PickUpObjectResult message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnPickUpItemResult(PickUpItemResult message)
		{
			if (message.Silently)
				return;

			Dictionary<PickUpItemResult.ResultType, string> resultMessages = new()
			{
				{ PickUpItemResult.ResultType.Success, "sebráno" },
				{ PickUpItemResult.ResultType.FullInventory, "Víc toho nepobereš." },
				{ PickUpItemResult.ResultType.NothingFound, "Před tebou nic není" },
				{ PickUpItemResult.ResultType.Unpickable, "tohle nejde odnést" }
			};

			Tolk.Speak(resultMessages[message.Result]);
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
		private void OnSayZoneSize(SayZoneSize message)
		{
			Terrain.Rectangle a = Owner.Zone.Area.Value;
			Tolk.Speak($"{a.Height.ToString()} krát {a.Width.ToString()}");
		}

		private void OnSayVisitedZone(SayVisitedZoneResult message) => Tolk.Speak(message.Visited ? "jo jo" : "ne", true);

		/// <summary>
		/// Processes the SayExits message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnSayExitsResult(SayExitsResult message)
		{
			if (message.OccupiedPassage != null)
			{
				string type = message.OccupiedPassage.TypeDescription switch
				{
					"průchod" => "v průchodu",
					"dveře" => "ve dveřích",
					"vrata" => "ve vratech",
					_ => null
				};

				Zone targetZone = message.OccupiedPassage.AnotherZone(Owner.Zone);
				string to = targetZone.To;
				if (Settings.SayInnerZoneNames)
					to += " " + targetZone.Name.Indexed;
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
				string exit = _exitDescriber.GetDescription(message.Exits[0]);
				Tolk.Speak(exit, true);
				return;
			}

			string number;
			if (count is >= 2 and <= 4)
				number = (count == 2 ? "dva" : count.ToString()) + " východy: ";
			else number = count.ToString() + " východů: ";

			List<string> exits = message.Exits
				.Select(e => _exitDescriber.GetDescription(e))
				.ToList();
			string formatedList = FormatStringList(exits.ToArray(), true);
			Tolk.Speak($"{number}{formatedList}.", true);
		}

		/// <summary>
		/// Handles the SayCharactersResult message.
		/// </summary>
		/// <param name="message">The message</param>
		protected void OnSayCharactersResult(SayCharactersResult message)
		{
			if (message.Characters.IsNullOrEmpty())
			{
				Tolk.Speak("Nikdo tu není", true);
				return;
			}

			List<NavigableObjectInfo> info = message.Characters.Cast<NavigableObjectInfo>().ToList();
			List<string> descriptions = _characterDescriber.GetDescriptions(info);
			string text = FormatStringList(descriptions.ToArray());
			Tolk.Speak(text, true);
		}

		/// <summary>
		/// Handles the SayNearestObjects message.
		/// </summary>
		/// <param name="message">The message</param>
		protected void OnSayItemsResult(SayItemsResult message)
		{
			if (message.Items.IsNullOrEmpty())
			{
				Tolk.Speak("Nic tu není", true);
				return;
			}

			var objectInfo = message.Items.Cast<NavigableObjectInfo>().ToList();
			List<string> describtions = _itemDescriber.GetDescriptions(objectInfo);
			string output = FormatStringList(describtions.ToArray());
			Tolk.Speak(output, true);
		}

		/// <summary>
		/// Processes the CutsceneBegan message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnSayOrientation(SayOrientation message)
		{
			SayOrientation(Owner.Orientation);
		}

		/// <summary>
		/// Reports the current orientation of the Detective Chipotle NPC using a screen reader or
		/// voice synthesizer..
		/// </summary>
		protected void SayOrientation(Orientation2D orientation)
		{
			string description = orientation.Angle.GetCardinalDirection().GetDescription();
			Tolk.Output(description, true);
		}

		NavigableExitDescriber _exitDescriber;
		private NavigableItemDescriber _itemDescriber;
		private NavigableCharacterDescriber _characterDescriber;

		/// <summary>
		/// Processes the EntityHitDoor message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnCharacterHitDoor(CharacterHitDoor message)
		{
			string text = _exitDescriber.GetDescription(message.Exit);
			if (Settings.SayInnerPassageNames)
				text += " " + message.Exit.Exit.Name.Indexed;
			Tolk.Speak(text);
		}

		/// <summary>
		/// Processes the TerrainCollided message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnTerrainCollided(TerrainCollided message) => PlayStep(message.Position);

		/// <summary>
		/// Processes the MovementDone message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnPositionChanged(PositionChanged message)
		{
			Vector2 center = message.TargetPosition.Center;
			float height = transform.localScale.y;
			Camera.main.transform.position = center.ToVector3(height);

			if (!message.Silently)
				PlayStep(center);
		}

		/// <summary>
		/// Processes the ObjectsCollided message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnObjectsCollided(ObjectsCollided message)
		{
			if (message.Object is Item i && i.Passable)
				return;

			string text = message.Object.Name.Friendly;
			if (Settings.SayInnerItemNames && message.Object is Item)
				text += " " + message.Object.Name.Indexed;
			Tolk.Speak(text);
		}

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
				SayOrientation(message.Target);
		}

		private Vector2 _playerPosition => Owner.Area.Value.Center;

		/// <summary>
		/// Stores information for dynamic listener orientation settings.
		/// </summary>
		private (Orientation2D current, Orientation2D final, int step, int steps) _listenerOrientation;
	}
}