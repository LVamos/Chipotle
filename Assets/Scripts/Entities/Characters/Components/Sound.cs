using Game;

using Game.Audio;
using Game.Mapping.Saves;
using Game.Serialization.Protobuf.Snapshots.Characters;
using Game.Terrain;

using System;
using System.Collections;

using UnityEngine;

namespace Game.Entities.Characters.Components
{
	/// <summary>
	/// Controls the sound output of an NPC.
	/// </summary>


	public class Sound : CharacterComponent
	{
		public override void Restore(ComponentSave save)
		{
			if (save is not SoundSave data)
				return;

			base.Restore(data);
			_announceWalls = data.AnnounceWalls;
			_walkVolume = data.WalkVolume;
		}

		public override ComponentSave Export()
		{
			var save = base.Export().ToSoundSave();
			save.AnnounceWalls = _announceWalls;
			save.WalkVolume = _walkVolume;
			return save;
		}

		/// <summary>
		/// Default voluem of sound output.
		/// </summary>
		protected const float _defaultVolume = 1;

		/// <summary>
		/// Adjusts the volume of an audio source over a specified duration to a target volume.
		/// </summary>
		/// <param name="source">An audio source to be adjusted</param>
		/// <param name="duration">Duration of the adjustment</param>
		/// <param name="targetVolume">Target volume</param>
		protected void AdjustVolume(AudioSource source, float duration, float targetVolume)
		{
			if (targetVolume == source.volume)
				return;

			StartCoroutine(Ajust());

			IEnumerator Ajust()
			{
				float startVolume = source.volume;

				for (float t = 0; t < duration; t += Time.deltaTime)
				{
					source.volume = Mathf.Lerp(startVolume, targetVolume, t / duration);
					yield return null;
				}
				source.volume = targetVolume;

				if (targetVolume <= 0)
					source.Stop();
			}
		}

		/// <summary>
		/// Volume for foot steps
		/// </summary>
		protected float _walkVolume = .5f;
		protected bool _announceWalls;

		/// <summary>
		/// Converts a string list to a CSV string.
		/// </summary>
		/// <param name="stringList">The string list to be formatted</param>
		/// <param name="addAnd">Specifies if last two items should by separated with " a " conjunction.</param>
		/// <returns>CSV string</returns>
		protected string FormatStringList(string[] stringList, bool addAnd = false)
		{
			int count = stringList.Length;
			if (count == 1)
				return stringList[0];

			if (count > 2)
			{
				for (int i = 0; i < count - 2; i++)
					stringList[i] += ", ";
			}

			stringList[count - 2] += addAnd ? " a " : ", ";
			return string.Join(string.Empty, stringList);
		}

		protected string GetStepSoundName(Vector2 position)
		{
			TerrainType terrain = World.Map[position].Terrain;
			if (terrain == TerrainType.Wall)
				return "hitwall";

			string terrainName = Enum.GetName(terrain.GetType(), terrain);
			return "movstep" + terrainName;
		}

		/// <summary>
		/// Plays a foot step sound according to current terrain.
		/// </summary>
		/// <param name="position">A 2d vector</param>
		/// <param name="obstacle">Type of an obstacle blocking the sound</param>
		protected virtual void PlayStep(Vector2 position, ObstacleType obstacle = ObstacleType.None)
		{
			if (obstacle == ObstacleType.Far)
				return;

			string sound = GetStepSoundName(position);
			AudioSource source = null;

			// Set attenuation parameters
			float volume = _walkVolume;
			float height = transform.localScale.y;
			Vector3 position3d = position.ToVector3(height);
			if (obstacle is not ObstacleType.None and not ObstacleType.IndirectPath)
			{
				volume = Sounds.GetVolumeByObstacle(obstacle, _walkVolume, _walkVolume * 2);
				source = Sounds.Play(sound, position3d, volume);
				Sounds.SetLowPass(source, obstacle);
			}
			else
				source = Sounds.Play(sound, position3d, volume);
			source.minDistance = 5;

			AnnounceWall(position);
		}

		/// <summary>
		/// How many step intervals without a step end the walk.
		/// </summary>
		private const float WalkEndFactor = 1.2f;

		/// <summary>
		/// Step duration used when the step message doesn't contain it.
		/// </summary>
		private const float DefaultStepDuration = .5f;

		private int _stepsInWalk;
		private float _lastStepTime = float.NegativeInfinity;
		private Coroutine _closingStep;

		/// <summary>
		/// Counts the steps of the current walk. If the walk ends after the first step, the other foot is brought alongside,
		/// so the closing step is played.
		/// </summary>
		/// <param name="stepDuration">Duration of the step in seconds; 0 if unknown</param>
		/// <param name="playClosingStep">Plays the closing step</param>
		protected void RegisterStep(float stepDuration, Action playClosingStep)
		{
			float interval = stepDuration > 0 ? stepDuration : DefaultStepDuration;
			float now = Time.time;
			_stepsInWalk = now - _lastStepTime > interval * WalkEndFactor ? 1 : _stepsInWalk + 1;
			_lastStepTime = now;

			if (_closingStep != null)
				StopCoroutine(_closingStep);
			_closingStep = _stepsInWalk == 1 ? StartCoroutine(PlayClosingStep(interval * WalkEndFactor, playClosingStep)) : null;
		}

		private IEnumerator PlayClosingStep(float delay, Action playClosingStep)
		{
			yield return new WaitForSeconds(delay);
			_closingStep = null;
			_stepsInWalk = 0;
			_lastStepTime = float.NegativeInfinity;
			playClosingStep();
		}

		protected void AnnounceWall(Vector2 position)
		{
			TerrainType terrain = World.Map[position].Terrain;
			if (terrain == TerrainType.Wall && _announceWalls)
				ScreenReader.Speak("zeď");
		}
	}
}