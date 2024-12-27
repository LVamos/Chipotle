using System;

using UnityEngine;
namespace Assets.Scripts.Exceptions
{
	public class LowPassFilterNotFoundException : Exception
	{
		public LowPassFilterNotFoundException(AudioSource source)
			: base($"No AudioLowPassFilter found on {source.clip?.name ?? "Unknown Clip"}.")
		{
		}
	}
}
public class LowPassFilterNotFoundException : Exception
{
	public LowPassFilterNotFoundException(AudioSource source)
		: base($"No AudioLowPassFilter found on {source.clip?.name ?? "Unknown Clip"}.")
	{
	}
}