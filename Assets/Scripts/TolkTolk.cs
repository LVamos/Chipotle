using DavyKager;

namespace Assets.Scripts
{
	public static class TolkTolk
	{
		public static void Load() => Tolk.Load();
		public static void Speak(string prompt) => Tolk.Speak(prompt);
	}
}
