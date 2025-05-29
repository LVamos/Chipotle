using Game.Terrain;

using System;

using UnityEngine;

namespace Assets.Scripts.Models
{
	public class ReadyPortalModel
	{
		public Passage Passage;
		public Vector3 Position;
		public bool DoubleAttenuation;

		public ReadyPortalModel(Passage passage, Vector3 position, bool doubleAttenuation)
		{
			Passage = passage ?? throw new ArgumentNullException(nameof(passage));
			Position = position;
			DoubleAttenuation = doubleAttenuation;
		}
	}
}
