using Assets.Scripts.Messaging.Events.Characters;
using Assets.Scripts.Narration.WorldDescribers;

using Game.Audio;
using Game.Entities.Characters.Components;
using Game.Entities.Items;
using Game.Messaging.Commands.Characters;
using Game.Messaging.Commands.GameInfo;
using Game.Messaging.Events;
using Game.Messaging.Events.Characters;
using Game.Messaging.Events.Characters.Movement;
using Game.Messaging.Events.GameInfo;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Messaging.Events.Sound;
using Game.Models;
using Game.Narration.WorldDescribers;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Chipotle.SoundComponent
{
	/// <summary>
	/// Controls the sound output of the detective Chipotle NPC
	/// </summary>
	public class ChipotleSound : Sound
	{
		private Sonar _sonar;

		private void OnDestroy()
		{
			foreach (AudioSource footStep in _footSteps)
			{
				if (footStep != null)
					Destroy(footStep.gameObject);
			}
		}

		private void LateUpdate()
		{
			UpdateClockOffset();
			FinishWarmUp();
		}

		/// <summary>
		/// The audio clock advances in whole buffers, so a single reading is imprecise. Averaging the difference
		/// against the real time over many frames gives a stable mapping between the two clocks.
		/// </summary>
		private void UpdateClockOffset()
		{
			double offset = AudioSettings.dspTime - Time.realtimeSinceStartupAsDouble;
			if (!_clockOffsetKnown)
			{
				_clockOffset = offset;
				_clockOffsetKnown = true;
			}
			else _clockOffset += ClockOffsetSmoothing * (offset - _clockOffset);
		}

		private void OnSayNavigatedObjectLocationResult(SayNavigatedObjectLocationResult message)
		{
			if (message.NoNavigatedObjects)
				ScreenReader.Speak("Nevybral jsi cíl.");
		}

		/// <summary>Moves the source slightly ahead of the listener. Straight below it the direction of the sound is not defined and it can jump to the side.</summary>
		private const float FootstepForwardOffset = 0.3f;

		/// <summary>Delay between a step and its sound. The sound is scheduled on the audio clock at the exact time instead of starting at the next audio buffer.</summary>
		private const double StepSoundLead = 0.025;

		private const double StepSoundMinLead = 0.01;

		/// <summary>Weight of one frame in the smoothed difference between the audio clock and the real time.</summary>
		private const double ClockOffsetSmoothing = 0.02;

		private const int WarmUpFrameCount = 15;

		/// <summary>
		/// One source per foot. Consecutive steps alternate, so a step doesn't cut off the previous one.
		/// </summary>
		private readonly AudioSource[] _footSteps = new AudioSource[2];
		private int _nextFootStep;
		private double _clockOffset;
		private bool _clockOffsetKnown;
		private AudioClip _warmUpClip;
		private int _warmUpFrames;

		private void InitFootStepSource()
		{
			for (int i = 0; i < _footSteps.Length; i++)
				_footSteps[i] = CreateFootStepSource($"FootstepSource{i + 1}");

			WarmUpFootStep();
		}

		private AudioSource CreateFootStepSource(string name)
		{
			GameObject obj = new GameObject(name);
			AudioSource source = obj.AddComponent<AudioSource>();
			source.spatialBlend = 1f;   // 3D
			source.dopplerLevel = 0f;
			source.playOnAwake = false;
			source.spatialize = true;
			source.spatializePostEffects = false;
			source.outputAudioMixerGroup = Sounds.ResonanceGroup;

			ResonanceAudioSource resonance = obj.AddComponent<ResonanceAudioSource>();
			resonance.nearFieldEffectEnabled = true;
			resonance.occlusionEnabled = true;
			return source;
		}

		/// <summary>
		/// A source that plays for the first time can start with an undefined direction and sound at the side.
		/// Playing a silent clip first lets the spatializer get the right position before the first step.
		/// </summary>
		private void WarmUpFootStep()
		{
			AttachFootStepToCamera();
			_warmUpClip = AudioClip.Create("footstep warm-up", 4410, 1, 44100, false);
			foreach (AudioSource footStep in _footSteps)
			{
				footStep.clip = _warmUpClip;
				footStep.volume = 0;
				footStep.loop = true;
				footStep.Play();
			}
			_warmUpFrames = WarmUpFrameCount;
		}

		private void FinishWarmUp()
		{
			if (_warmUpFrames <= 0 || --_warmUpFrames > 0)
				return;

			foreach (AudioSource footStep in _footSteps)
			{
				if (footStep.clip != _warmUpClip)
					continue;

				footStep.Stop();
				footStep.loop = false;
			}
		}

		public override void Initialize()
		{
			base.Initialize();
			InitFootStepSource();
			_exitDescriber = new();
			_itemDescriber = new();
			_characterDescriber = new();
			_announceWalls = true;
			_sonar = new();
		}

		protected new void PlayStep(Vector2 position, ObstacleType obstacle = ObstacleType.None, bool terrainCollided = false)
		{
			string sound = GetStepSoundName(position);
			AudioClip clip = Sounds.GetClip(sound);
			if (!terrainCollided)
				PlayFootStep(clip);
			else
			{
				float size = 2 * TileMap.TileSize;
				Rectangle temp = Rectangle.FromCenter(position, size, size);
				Vector2 tempPoint = temp.GetClosestPoint(Owner.Center);
				Vector3 finalPosition = tempPoint.ToVector3(1);
				Sounds.Play(sound, finalPosition, _walkVolume);
			}
			AnnounceWall(position);
		}

		/// <summary>
		/// The source is a child of the camera (listener), so it follows it no matter in which order the scripts
		/// move the camera. It sits below and slightly ahead of the listener, because the direction of a sound at zero distance
		/// or exactly below the listener is not defined and it can jump to the side.
		/// </summary>
		private void AttachFootStepToCamera()
		{
			Transform camera = Camera.main != null ? Camera.main.transform : null;
			if (camera == null)
				return;

			foreach (AudioSource footStep in _footSteps)
			{
				if (footStep.transform.parent == camera)
					continue;

				footStep.transform.SetParent(camera, false);
				footStep.transform.localPosition = new Vector3(0, -Mathf.Max(0.3f, camera.position.y - 1f), FootstepForwardOffset);
			}
		}

		/// <summary>
		/// Plays a step sound at an exact time on the audio clock so the intervals between steps stay even.
		/// </summary>
		private void PlayFootStep(AudioClip clip)
		{
			AttachFootStepToCamera();
			if (!_clockOffsetKnown)
				UpdateClockOffset();

			double start = Time.realtimeSinceStartupAsDouble + _clockOffset + StepSoundLead;
			start = Math.Max(start, AudioSettings.dspTime + StepSoundMinLead);

			AudioSource footStep = _footSteps[_nextFootStep];
			_nextFootStep = (_nextFootStep + 1) % _footSteps.Length;

			footStep.loop = false;
			footStep.clip = clip;
			footStep.volume = _walkVolume;
			footStep.PlayScheduled(start);
		}

		public void OnSaySize(SaySize message)
		{
			string text = $"{message.Area.Height} krát {message.Area.Width}";
			ScreenReader.Speak(text);
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="m">The message to be handled</param>
		protected void OnSayObjectDescription(SayObjectDescription m)
		{
			if (m.Object == null)
				ScreenReader.Speak("Před tebou nic není");
			else
				ScreenReader.Speak(m.Object.Description);
		}

		/// <summary>
		/// Processes the SayZone message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnSayZoneName(SayZoneName message)
		{
			string text = Owner.Zone.Name.Friendly;
			if (Settings.SayInnerZoneNames)
				text += " " + Owner.Zone.Name.Inner;
			ScreenReader.Speak(text, true);
		}

		private void OnSonarToggled(SonarToggled message)
		{
			string state = message.SonarEnabled ? "zapnut" : "vypnut";
			ScreenReader.Speak($"Sonar {state}");

			if (!message.SonarEnabled)
				_sonar.SetState();
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="m">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			base.HandleMessage(message);

			switch (message)
			{
				case SonarToggled m: OnSonarToggled(m); break;
				case SonarDetectedNoObstacles m:
					OnSonarDetectedNoObstacles(m); break;
				case SonarDetectedObstacles m:
					OnSonarDetectedObstacles(m); break;
				case NoAcousticObstacleDetected m: OnNoAcousticObstacleDetected(m); break;
				case AcousticObstacleDetected m: OnAcousticObstacleDetected(m); break;
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

		private void OnSonarDetectedNoObstacles(SonarDetectedNoObstacles message)
			=> _sonar.SetState(false, false);

		private void OnSonarDetectedObstacles(SonarDetectedObstacles message)
			=> _sonar.SetState(message.ObstaclesOnLeft, message.ObstaclesOnRight, message.ObstaclesInFront);

		private void OnNoAcousticObstacleDetected(Message message)
			=> Sounds.RoomManager.StopSimulatingObstacle();

		private void OnAcousticObstacleDetected(AcousticObstacleDetected message)
			=> Sounds.RoomManager.SimulateObstacle(message.Direction);

		private void OnNavigationStopped(NavigationStopped message)
		{
			if (message.TargetReached)
				ScreenReader.Output("Jsi u cíle.");
		}

		private void OnLeftBycar(LeftBycar m)
		{
			AudioListener.volume = 0;
			Sounds.SlideMasterVolume(1, Settings.DefaultMasterVolume);
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
			};
			if (answers.TryGetValue(message.Result, out string answer))
				ScreenReader.Speak(answer);
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="m">The message to be handled</param>
		private void OnSayZoneDescription(SayZoneDescription m)
			=> ScreenReader.Speak(Owner.Zone.Description);

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="m">Source of the message</param>
		private void OnPlaceItemResult(PlaceItemResult m)
		{
			if (m.Success)
				ScreenReader.Speak("Položeno");
			else
				ScreenReader.Speak("Sem se to nevejde");
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="m">The message to be handled</param>
		protected void OnEmptyInventory(EmptyInventory m)
			=> ScreenReader.Speak("Nic u sebe nemáš");

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

			ScreenReader.Speak(resultMessages[message.Result]);
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
			ScreenReader.Speak(result, true);
			GUIUtility.systemCopyBuffer = coords.GetString();
		}

		/// <summary>
		/// Processes the CutsceneBegan message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnSayZoneSize(SayZoneSize message)
		{
			Terrain.Rectangle a = Owner.Zone.Area.Value;
			ScreenReader.Speak($"{a.Height.ToString()} krát {a.Width.ToString()}");
		}

		private void OnSayVisitedZone(SayVisitedZoneResult message)
			=> ScreenReader.Speak(message.Visited ? "jo jo" : "ne", true);

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
					to += " " + targetZone.Name.Inner;
				ScreenReader.Speak($"Stojíš {type}{to}", true);
				return;
			}

			if (message.Exits.IsNullOrEmpty())
			{
				ScreenReader.Speak("žádné východy nevidíš", true);
				return;
			}

			int count = message.Exits.Count;
			if (count == 1)
			{
				string exit = _exitDescriber.GetDescription(message.Exits[0]);
				ScreenReader.Speak(exit, true);
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
			ScreenReader.Speak($"{number}{formatedList}.", true);
		}

		/// <summary>
		/// Handles the SayCharactersResult message.
		/// </summary>
		/// <param name="message">The message</param>
		protected void OnSayCharactersResult(SayCharactersResult message)
		{
			if (message.Characters.IsNullOrEmpty())
			{
				ScreenReader.Speak("Nikdo tu není", true);
				return;
			}

			List<NavigableObjectInfo> info = message.Characters.Cast<NavigableObjectInfo>().ToList();
			List<string> descriptions = _characterDescriber.GetDescriptions(info);
			string text = FormatStringList(descriptions.ToArray());
			ScreenReader.Speak(text, true);
		}

		/// <summary>
		/// Handles the SayNearestObjects message.
		/// </summary>
		/// <param name="message">The message</param>
		protected void OnSayItemsResult(SayItemsResult message)
		{
			if (message.Items.IsNullOrEmpty())
			{
				ScreenReader.Speak("Nic tu není", true);
				return;
			}

			var objectInfo = message.Items.Cast<NavigableObjectInfo>().ToList();
			List<string> describtions = _itemDescriber.GetDescriptions(objectInfo);
			string output = FormatStringList(describtions.ToArray());
			ScreenReader.Speak(output, true);
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
			ScreenReader.Output(description, true);
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
			string text = _exitDescriber.GetShortDescription(message.Exit);
			if (Settings.SayInnerPassageNames)
				text += " " + message.Exit.Exit.Name.Inner;
			ScreenReader.Speak(text);
		}

		/// <summary>
		/// Processes the TerrainCollided message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnTerrainCollided(TerrainCollided message)
			=> PlayStep(message.Position, default, true);

		/// <summary>
		/// Processes the MovementDone message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnPositionChanged(PositionChanged message)
		{
			if (message.Silently)
				return;

			Vector2 position = message.TargetPosition.Center;
			PlayStep(position);
			RegisterStep(message.StepDuration, () => PlayStep(position));
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
				text += " " + message.Object.Name.Inner;
			ScreenReader.Speak(text);
		}

		/// <summary>
		/// Processes the TurnoverDone message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnOrientationChanged(OrientationChanged message)
		{
			if (message.Announce)
				SayOrientation(message.Target);
		}

		private Vector2 _playerPosition => Owner.Area.Value.Center;
	}
}