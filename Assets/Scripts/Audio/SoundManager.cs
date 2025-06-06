using Assets.Scripts.Audio;
using Assets.Scripts.Models;

using DavyKager;

using Game.Terrain;

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Audio;

using Material = Assets.Scripts.Terrain.Material;

namespace Game.Audio
{
	public class SoundManager : MonoBehaviour
	{
		private MixerManager _mixer;

		public float GetLinearRolloffAttenuation(AudioSource source, float defaultVolume) => GetLinearRolloffAttenuation(source.transform.position, source.minDistance, source.maxDistance, defaultVolume);
		public float GetLinearRolloffAttenuation(Vector3 position, float minDistance, float maxDistance, float defaultVolume)
		{
			float distance = Vector3.Distance(position, World.Player.transform.position);

			float t = (distance - minDistance) / (maxDistance - minDistance);
			float attenuation = 1f - Mathf.Clamp01(t);
			return defaultVolume * attenuation;
		}


		private Dictionary<AudioSource, Coroutine> _lowPassCoroutines = new();
		public void SlideLowPass(AudioSource source, float duration, float targetFrequency, bool disableLowPassAfterwards = false)
		{
			//test
			if (source.clip.name.ToLower().Contains("fire"))
				Debug.Log("");
			if (source.outputAudioMixerGroup == null)
				throw new LowPassFilterNotFoundException(source);

			_soundPool.EnableLowPass(source);

			if (targetFrequency == GetLowPass(source))
				return;

			Coroutine coroutine;
			if (_lowPassCoroutines.TryGetValue(source, out coroutine))
			{
				if (coroutine != null)
					StopCoroutine(_lowPassCoroutines[source]);
				_lowPassCoroutines.Remove(source);
			}

			Coroutine newCoroutine = StartCoroutine(SlideLowPassStep(source, duration, targetFrequency, disableLowPassAfterwards));
			_lowPassCoroutines[source] = newCoroutine;
		}

		public float GetLowPass(AudioSource source)
		{
			AudioLowPassFilter lowPass = _soundPool.GetLowPass(source);
			return lowPass.cutoffFrequency;
		}

		private IEnumerator SlideLowPassStep(AudioSource source, float duration, float targetFrequency, bool disableLowPassAfterwards = false)
		{
			float startFrequency = GetLowPass(source);

			for (float t = 0; t < duration; t += Time.deltaTime)
			{
				float value = (int)Mathf.Lerp(startFrequency, targetFrequency, t / duration);
				SetLowPass(source, value);
				yield return null;
			}
			SetLowPass(source, targetFrequency);

			if (disableLowPassAfterwards)
				_soundPool.DisableLowPass(source);

			// Remove the coroutine
			if (_lowPassCoroutines.ContainsKey(source))
				_lowPassCoroutines.Remove(source);
		}

		public void Mute(float duration = _fadingDuration)
		{
			if (_muted)
				return;

			_muted = true;
			SlideMasterVolume(duration, 0.2f);
		}

		private const float _fullMasterVolume = 1;
		private const int _fadingDuration = 2;
		private bool _muted;
		public void Unmute(float duration = _fadingDuration)
		{
			if (!_muted)
				return;

			_muted = false;
			SlideMasterVolume(duration, _fullMasterVolume);
		}

		public AudioSource ConvertTo3d(AudioSource source, Vector3 position, int? lowPassFrequency = null)
		{
			source.spatialize = true;
			source.outputAudioMixerGroup = ResonanceGroup;
			source.spatialBlend = 1;
			source.transform.position = position;
			if (lowPassFrequency != null)
				SetLowPass(source, lowPassFrequency.Value);
			return source;
		}

		public AudioSource ConvertTo2d(AudioSource source, bool disableLowPass = false)
		{
			if (disableLowPass)
				_soundPool.DisableLowPass(source);
			source.spatialize = false;
			source.outputAudioMixerGroup = null;
			source.spatialBlend = 0;
			return source;
		}

		public void SlideVolume(AudioSource sound, float duration, float targetVolume, bool stopWhenDone = true)
		{
			//test
			//if (sound.name.Contains("hodiny w1"))
			//System.Diagnostics.Debugger.Break();

			if (targetVolume == sound.volume)
				return;

			StartCoroutine(SlideVolumeStep(sound, duration, targetVolume, stopWhenDone));
		}

		private IEnumerator SlideVolumeStep(AudioSource sound, float duration, float targetVolume, bool stopWhenDone = true)
		{
			float startVolume = sound.volume;

			for (float t = 0; t < duration; t += Time.deltaTime)
			{
				sound.volume = Mathf.Lerp(startVolume, targetVolume, t / duration);
				yield return null;
			}
			sound.volume = targetVolume;

			if (stopWhenDone && targetVolume <= 0)
				sound.Stop();
		}

		private void Update()
		{
			return;
		}

		public AudioSource Play2d(string soundName, float volume = 1, bool loop = false, bool fadeIn = false, float fadingDuration = .5f, string description = null)
		{
			AudioSource source = _soundPool.GetSource();
			source.name = description ?? "sound";
			source.clip = Sounds.GetClip(soundName);
			source.spatialize = false;
			source.spatialBlend = 0;
			source.loop = loop;
			source.outputAudioMixerGroup = null;

			if (fadeIn)
			{
				source.volume = 0;
				source.Play();
				SlideVolume(source, fadingDuration, volume);
			}
			else
			{
				source.volume = volume;
				source.Play();
			}

			_soundPool.SoundStartedPlaying(source);
			return source;
		}

		public void SetRoomParameters(Zone zone)
		{
			_roomObject.transform.position = zone.transform.position;
			Vector3 size = zone.transform.localScale;
			_resonanceRoom.size = size;
			ZoneMaterialsDefinitionModel zoneMaterials = zone.Materials;
			_resonanceRoom.leftWall = _materials[zoneMaterials.LeftWall];
			_resonanceRoom.frontWall = _materials[zoneMaterials.FrontWall];
			_resonanceRoom.rightWall = _materials[zoneMaterials.RightWall];
			_resonanceRoom.backWall = _materials[zoneMaterials.BackWall];
			_resonanceRoom.floor = _materials[zoneMaterials.Floor];
			_resonanceRoom.ceiling = _materials[zoneMaterials.Ceiling];

			if (zone.Type == Zone.ZoneType.Outdoor)
				_resonanceRoom.reverbTime = .4f;
		}

		private Dictionary<Material, ResonanceAudioRoomManager.SurfaceMaterial> _materials = new()
		{
	{ Material.Transparent, ResonanceAudioRoomManager.SurfaceMaterial.Transparent },
	{ Material.AcousticCeilingTiles, ResonanceAudioRoomManager.SurfaceMaterial.AcousticCeilingTiles },
	{ Material.BrickBare, ResonanceAudioRoomManager.SurfaceMaterial.BrickBare },
	{ Material.BrickPainted, ResonanceAudioRoomManager.SurfaceMaterial.BrickPainted },
	{ Material.ConcreteBlockCoarse, ResonanceAudioRoomManager.SurfaceMaterial.ConcreteBlockCoarse },
	{ Material.ConcreteBlockPainted, ResonanceAudioRoomManager.SurfaceMaterial.ConcreteBlockPainted },
	{ Material.CurtainHeavy, ResonanceAudioRoomManager.SurfaceMaterial.CurtainHeavy },
	{ Material.FiberglassInsulation, ResonanceAudioRoomManager.SurfaceMaterial.FiberglassInsulation },
	{ Material.GlassThin, ResonanceAudioRoomManager.SurfaceMaterial.GlassThin },
	{ Material.GlassThick, ResonanceAudioRoomManager.SurfaceMaterial.GlassThick },
	{ Material.Grass, ResonanceAudioRoomManager.SurfaceMaterial.Grass },
	{ Material.LinoleumOnConcrete, ResonanceAudioRoomManager.SurfaceMaterial.LinoleumOnConcrete },
	{ Material.Marble, ResonanceAudioRoomManager.SurfaceMaterial.Marble },
	{ Material.Metal, ResonanceAudioRoomManager.SurfaceMaterial.Metal },
	{ Material.ParquetOnConcrete, ResonanceAudioRoomManager.SurfaceMaterial.ParquetOnConcrete },
	{ Material.PlasterRough, ResonanceAudioRoomManager.SurfaceMaterial.PlasterRough },
	{ Material.PlasterSmooth, ResonanceAudioRoomManager.SurfaceMaterial.PlasterSmooth },
	{ Material.PlywoodPanel, ResonanceAudioRoomManager.SurfaceMaterial.PlywoodPanel },
	{ Material.PolishedConcreteOrTile, ResonanceAudioRoomManager.SurfaceMaterial.PolishedConcreteOrTile },
	{ Material.Sheetrock, ResonanceAudioRoomManager.SurfaceMaterial.Sheetrock },
	{ Material.WaterOrIceSurface, ResonanceAudioRoomManager.SurfaceMaterial.WaterOrIceSurface },
	{ Material.WoodCeiling, ResonanceAudioRoomManager.SurfaceMaterial.WoodCeiling },
	{ Material.WoodPanel, ResonanceAudioRoomManager.SurfaceMaterial.WoodPanel }
};

		private SoundPool _soundPool;

		public void DisableLowPass(AudioSource source) => _soundPool.DisableLowPass(source);


		public void SetLowPass(AudioSource source, float cutOffFrequency)
		{
			AudioLowPassFilter lowPass = _soundPool.GetLowPass(source);
			lowPass.cutoffFrequency = cutOffFrequency;
		}

		public AudioSource PlayMuffled(string soundName, Vector3 position, float volume = 1, bool loop = false, int cutOffFrequency = 22000, string description = null)
		{
			AudioSource source = _soundPool.GetSource();
			source.name = description ?? "sound";
			AudioLowPassFilter lowPass = source.gameObject.GetComponent<AudioLowPassFilter>();
			lowPass.cutoffFrequency = cutOffFrequency;
			source.transform.position = position;
			source.clip = Sounds.GetClip(soundName);
			source.volume = volume;
			source.spatialBlend = 1; // Full surround sound
			source.spatialize = true;
			source.loop = loop;
			source.dopplerLevel = 0;
			source.Play();
			_soundPool.SoundStartedPlaying(source);

			return source;
		}

		public AudioSource Play(string soundName, Vector3 position, float volume = 1, bool loop = false, bool fadeIn = false, float fadingDuration = .5f, string description = null)
		{
			AudioSource source = _soundPool.GetSource();
			source.name = description ?? "sound";
			source.transform.position = position;
			source.clip = Sounds.GetClip(soundName);
			source.spatialBlend = 1; // Full surround sound
			source.spatialize = true;
			source.spatializePostEffects = false;
			source.outputAudioMixerGroup = ResonanceGroup;
			source.loop = loop;
			source.dopplerLevel = 0;

			ResonanceAudioSource resonance = source.GetComponent<ResonanceAudioSource>();
			resonance.bypassRoomEffects = false;
			resonance.nearFieldEffectEnabled = true;
			resonance.occlusionEnabled = true;

			if (fadeIn)
			{
				source.volume = 0;
				source.Play();
				SlideVolume(source, fadingDuration, volume);
				return source;
			}

			source.volume = volume;
			source.Play();
			_soundPool.SoundStartedPlaying(source);
			return source;
		}

		private async Task Awake()
		{
			LoadMixer();
			CreateSoundPool();
			CreateResonanceRoom();
		}

		private void CreateResonanceRoom()
		{
			_roomObject = new("Resonance Audio room");
			_resonanceRoom = _roomObject.AddComponent<ResonanceAudioRoom>();
		}

		private void CreateSoundPool()
		{
			_soundPool = (new GameObject("Sound pool")).AddComponent<SoundPool>();
		}

		private void LoadMixer()
		{
			GameObject obj = new("Audio mixer");
			_mixer = obj.AddComponent<MixerManager>();
		}

		private GameObject _roomObject;
		private ResonanceAudioRoom _resonanceRoom;

		public AudioMixerGroup ResonanceGroup { get => _mixer.ResonanceGroup; }
		public float MasterVolume
		{
			get => AudioListener.volume;
			set => AudioListener.volume = value;
		}

		private float _masterVolumeStepSize;
		private float _masterVolumeTargetVolume;

		/// <summary>
		/// Adjusts the volume of an audio source over a specified duration to a target volume.
		/// </summary>
		/// <param name="source">An audio source to be adjusted</param>
		/// <param name="duration">Duration of the adjustment</param>
		/// <param name="targetVolume">Target volume</param>
		public void SlideMasterVolume(float duration, float newTargetVolume)
		{
			if (newTargetVolume == AudioListener.volume)
				return;

			_masterVolumeTargetVolume = newTargetVolume;
			float startVolume = AudioListener.volume;
			int steps = Mathf.RoundToInt(duration * 100); // 100 kroků za sekundu
			_masterVolumeStepSize = (_masterVolumeTargetVolume - startVolume) / steps;
			float stepInterval = duration / steps;

			CancelInvoke(nameof(MasterVolumeSlideStep));
			InvokeRepeating(nameof(MasterVolumeSlideStep), 0, stepInterval);

		}

		private void MasterVolumeSlideStep()
		{
			if (Mathf.Abs(AudioListener.volume - _masterVolumeTargetVolume) < Mathf.Abs(_masterVolumeStepSize))
			{
				AudioListener.volume = _masterVolumeTargetVolume;
				CancelInvoke(nameof(MasterVolumeSlideStep));
				return;
			}

			AudioListener.volume += _masterVolumeStepSize;
		}

		public void StopAllSounds()
		{
			AudioSource[] sources = FindObjectsOfType<AudioSource>();
			foreach (AudioSource soruce in sources)
				soruce.Stop();
		}

		public void MuteSpeech()
		{
			Tolk.Silence();
		}
	}
}

