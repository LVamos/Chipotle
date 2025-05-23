namespace Game.Models
{
	public class AttenuationModel
	{
		public int? LowPassFrequency;
		public float Volume;
		public float SpatialBlend;

		public AttenuationModel(int? lowPassFrequency, float volume, float spatialBlend = 1)
		{
			LowPassFrequency = lowPassFrequency;
			Volume = volume;
			SpatialBlend = spatialBlend;
		}
	}
}
