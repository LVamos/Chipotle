using System.IO;

public static class FileLogger
{
	static FileLogger()
	{
		if (File.Exists(logFilePath))
		{
			try
			{
				File.Delete(logFilePath);
			}
			catch (System.Exception)
			{

			}
		}
	}

	private static string logFilePath = "Assets/Editor/log.txt";

	/// <summary>
	/// Static method that writes a message to a log file.
	/// </summary>
	/// <param name="message">The message to be logged.</param>
	public static void LogMessage(string message)
	{
		// Ensure the directory exists
		string directory = Path.GetDirectoryName(logFilePath);
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);

		// Write the message to the file
		using (StreamWriter writer = new StreamWriter(logFilePath, true))
			writer.WriteLine($"{System.DateTime.Now}: {message}");
	}
}
