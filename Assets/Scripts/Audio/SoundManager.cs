using Assets.Scripts.Models;

using Game.Audio;
using Game.Terrain;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Audio;

using Material = Assets.Scripts.Terrain.Material;

namespace Assets.Scripts.Audio
{
	public class SoundManager : MonoBehaviour
	{
		public void Mute(float duration = .5f)
		{
			if (_muted)
				return;

			_muted = true;
			AdjustMasterVolume(duration, 0);
		}

		private const float _fullMasterVolume = 1;
		private bool _muted;
		public void Unmute(float duration = .5f)
		{
			if (!_muted)
				return;

			_muted = false;
			AdjustMasterVolume(duration, _fullMasterVolume);
		}

		public AudioSource ConvertTo3d(AudioSource source, Vector3 position, int? lowPassFrequency = null)
		{
			source.spatialize = true;
			source.outputAudioMixerGroup = MixerGroup;
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

		public void SlideVolume(AudioSource sound, float duration, float targetVolume)
		{
			if (targetVolume == sound.volume)
				return;

			StartCoroutine(Adjust());

			IEnumerator Adjust()
			{
				float startVolume = sound.volume;

				for (float t = 0; t < duration; t += Time.deltaTime)
				{
					sound.volume = Mathf.Lerp(startVolume, targetVolume, t / duration);
					yield return null;
				}
				sound.volume = targetVolume;

				if (targetVolume <= 0)
					sound.Stop();
			}
		}

		private void Update()
		{
			return;
		}

		public AudioSource Play2d(string soundName, float volume = 1, bool loop = false, bool fadeIn = false, float fadingDuration = .5f)
		{
			AudioSource source = _soundPool.GetSource();
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

			return source;
		}

		public void SetRoomParameters(Locality locality)
		{
			_roomObject.transform.position = locality.transform.position;
			Vector3 size = locality.transform.localScale;
			_resonanceRoom.size = size;
			LocalityMaterialsDefinitionModel localityMaterials = locality.Materials;
			_resonanceRoom.leftWall = _materials[localityMaterials.LeftWall];
			_resonanceRoom.frontWall = _materials[localityMaterials.FrontWall];
			_resonanceRoom.rightWall = _materials[localityMaterials.RightWall];
			_resonanceRoom.backWall = _materials[localityMaterials.BackWall];
			_resonanceRoom.floor = _materials[localityMaterials.Floor];
			_resonanceRoom.ceiling = _materials[localityMaterials.Ceiling];

			if (locality.Type == Locality.LocalityType.Outdoor)
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

		public AudioSource DisableLowPass(AudioSource source)
		{
			_soundPool.DisableLowPass(source);
			source.outputAudioMixerGroup = MixerGroup;
			return source;
		}

		public void SetLowPass(AudioSource source, int cutOffFrequency)
		{
			AudioLowPassFilter lowPass = source.GetComponent<AudioLowPassFilter>();
			if (lowPass == null)
				lowPass = _soundPool.EnableLowPass(source);

			lowPass.cutoffFrequency = cutOffFrequency;
			source.outputAudioMixerGroup = null;
		}

		public AudioSource PlayMuffled(string soundName, Vector3 position, float volume = 1, bool loop = false, int cutOffFrequency = 22000)
		{
			AudioSource source = _soundPool.GetMuffledSource();
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

			return source;
		}

		public AudioSource Play(string soundName, Vector3 position, float volume = 1, bool loop = false, bool fadeIn = false, float fadingDuration = .5f)
		{
			AudioSource source = _soundPool.GetSource();
			source.transform.position = position;
			source.clip = Sounds.GetClip(soundName);
			source.spatialBlend = 1; // Full surround sound
			source.spatialize = true;
			source.spatializePostEffects = true;
			source.outputAudioMixerGroup = Sounds.MixerGroup;
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
			return source;
		}

		private void Awake()
		{
			// Get mixer
			AudioMixer mixer = Resources.Load<AudioMixer>("ResonanceAudioMixer") ?? throw new InvalidOperationException("Mixer not loaded");
			MixerGroup = mixer.FindMatchingGroups("Master").FirstOrDefault() ?? throw new InvalidOperationException("Mixer group not loaded");

			_soundPool = (new GameObject("Sound pool")).AddComponent<SoundPool>();
			_roomObject = new("Resonance Audio room");
			_resonanceRoom = _roomObject.AddComponent<ResonanceAudioRoom>();
		}

		private GameObject _roomObject;
		private ResonanceAudioRoom _resonanceRoom;

		public AudioMixerGroup MixerGroup { get; private set; }

		public float MasterVolume
		{
			get => AudioListener.volume;
			set => AudioListener.volume = value;
		}

		/// <summary>
		/// Adjusts the volume of an audio source over a specified duration to a target volume.
		/// </summary>
		/// <param name="source">An audio source to be adjusted</param>
		/// <param name="duration">Duration of the adjustment</param>
		/// <param name="targetVolume">Target volume</param>
		public void AdjustMasterVolume(float duration, float targetVolume)
		{
			if (targetVolume == AudioListener.volume)
				return;

			StartCoroutine(Adjust());

			IEnumerator Adjust()
			{
				float startVolume = AudioListener.volume;
				float elapsedTime = 0f;

				while (elapsedTime < duration)
				{
					elapsedTime += Time.deltaTime;
					AudioListener.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
					yield return null;
				}

				AudioListener.volume = targetVolume;
			}
		}

		public void StopAllSounds()
		{
			AudioSource[] sources = FindObjectsOfType<AudioSource>();
			foreach (AudioSource soruce in sources)
				soruce.Stop();
		}
	}
}

