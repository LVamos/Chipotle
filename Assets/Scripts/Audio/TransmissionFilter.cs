using UnityEngine;

namespace Game.Audio
{
	/// <summary>
	/// Three-band equalizer with the bands of Steam Audio materials (400 Hz, 2.5 kHz, 15 kHz): a gain for the middle band
	/// and a low and a high shelf, so a sound passing a door keeps its bass and loses its treble.
	/// </summary>
	public class TransmissionFilter : MonoBehaviour
	{
		/// <summary>
		/// Amplitude gains of the low, middle and high band.
		/// </summary>
		public void SetGains(float low, float mid, float high)
		{
			_low = Mathf.Max(low, _minGain);
			_mid = Mathf.Max(mid, _minGain);
			_high = Mathf.Max(high, _minGain);
		}

		private const float _minGain = 1e-5f;

		/// <summary>
		/// Crossovers between the bands: geometric means of the band centers.
		/// </summary>
		private const float _lowCrossover = 1000, _highCrossover = 6100;

		private volatile float _low = 1, _mid = 1, _high = 1;
		private float _appliedLow = -1, _appliedMid = -1, _appliedHigh = -1;
		private int _sampleRate;
		private readonly Biquad _lowShelf = new(), _highShelf = new();

		private void Awake() => _sampleRate = AudioSettings.outputSampleRate;

		private void OnEnable()
		{
			_lowShelf.Clear();
			_highShelf.Clear();
			_active = true;
		}

		/// <summary>
		/// Unity keeps calling the filter of a disabled script, so a pool source reused for another sound would stay muffled.
		/// </summary>
		private void OnDisable()
		{
			_active = false;
			SetGains(1, 1, 1);
		}

		private volatile bool _active;

		private void OnAudioFilterRead(float[] data, int channels)
		{
			if (!_active)
				return;

			float low = _low, mid = _mid, high = _high;
			if (low != _appliedLow || mid != _appliedMid || high != _appliedHigh)
			{
				_lowShelf.SetShelf(false, _lowCrossover, low / mid, _sampleRate);
				_highShelf.SetShelf(true, _highCrossover, high / mid, _sampleRate);
				(_appliedLow, _appliedMid, _appliedHigh) = (low, mid, high);
			}

			for (int i = 0; i < data.Length; i++)
			{
				int channel = i % channels;
				data[i] = mid * _highShelf.Process(_lowShelf.Process(data[i], channel), channel);
			}
		}

		/// <summary>
		/// Shelving filter after the Audio EQ Cookbook (R. Bristow-Johnson), slope 1, up to 8 channels.
		/// </summary>
		private class Biquad
		{
			private double _b0 = 1, _b1, _b2, _a1, _a2;
			private readonly double[] _x1 = new double[8], _x2 = new double[8], _y1 = new double[8], _y2 = new double[8];

			public void Clear()
			{
				System.Array.Clear(_x1, 0, 8);
				System.Array.Clear(_x2, 0, 8);
				System.Array.Clear(_y1, 0, 8);
				System.Array.Clear(_y2, 0, 8);
			}

			/// <param name="high">A high shelf instead of a low one</param>
			/// <param name="frequency">Corner frequency in Hz</param>
			/// <param name="gain">Amplitude gain of the shelf</param>
			/// <param name="sampleRate">Sample rate in Hz</param>
			public void SetShelf(bool high, float frequency, float gain, int sampleRate)
			{
				double a = System.Math.Sqrt(gain);
				double w0 = 2 * System.Math.PI * frequency / sampleRate;
				double cos = System.Math.Cos(w0);
				double alpha = System.Math.Sin(w0) / 2 * System.Math.Sqrt(2);
				double root = 2 * System.Math.Sqrt(a) * alpha;
				double sign = high ? -1 : 1;

				double b0 = a * (a + 1 - sign * (a - 1) * cos + root);
				double b1 = sign * 2 * a * (a - 1 - sign * (a + 1) * cos);
				double b2 = a * (a + 1 - sign * (a - 1) * cos - root);
				double a0 = a + 1 + sign * (a - 1) * cos + root;
				double a1 = -sign * 2 * (a - 1 + sign * (a + 1) * cos);
				double a2 = a + 1 + sign * (a - 1) * cos - root;

				_b0 = b0 / a0;
				_b1 = b1 / a0;
				_b2 = b2 / a0;
				_a1 = a1 / a0;
				_a2 = a2 / a0;
			}

			public float Process(float x, int channel)
			{
				if (channel >= 8)
					return x;

				double y = _b0 * x + _b1 * _x1[channel] + _b2 * _x2[channel] - _a1 * _y1[channel] - _a2 * _y2[channel];
				_x2[channel] = _x1[channel];
				_x1[channel] = x;
				_y2[channel] = _y1[channel];
				_y1[channel] = y;
				return (float)y;
			}
		}
	}
}
