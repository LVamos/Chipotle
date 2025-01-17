using System;
using System.Globalization;
using System.IO;
using System.Net;

/// <summary>
/// Provides static methods for logging HTML-formatted messages to a file.
/// </summary>
public static class Logger
{
	/// <summary>
	/// The file path to which logs will be written.
	/// </summary>
	private static string _filePath;

	/// <summary>
	/// Initializes the logger with the specified file path and creates the log file if it doesn't exist.
	/// </summary>
	/// <param name="filePath">The path of the log file.</param>
	public static void Initialize(string filePath)
	{
		_filePath = filePath;
		InitializeLogFile();
	}

	/// <summary>
	/// Creates the log file with initial HTML structure if it does not exist.
	/// </summary>
	private static void InitializeLogFile()
	{
		// If the file does not exist, create it with initial HTML structure.
		if (!File.Exists(_filePath))
		{
			string header = "<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Log</title></head><body>\n";
			File.WriteAllText(_filePath, header);
		}
	}

	/// <summary>
	/// Optionally adds footer and closes HTML tags when closing the log.
	/// </summary>
	private static void WriteFooter()
	{
		// Optionally we can add a footer and close HTML tags when closing the log.
		string footer = "</body></html>";
		File.AppendAllText(_filePath, footer);
	}

	/// <summary>
	/// Writes a log entry to the file with a message and marks it as error if specified.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="isError">Whether the log entry represents an error.</param>
	private static void Log(string message, bool isError)
	{
		// Choose heading based on message type
		string heading = isError ? "<h2 style='color:red;'>Error</h2>" : "<h2>Info</h2>";

		// Format time with milliseconds
		string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

		// Create HTML fragment
		string htmlEntry = $@"
{heading}
<ul>
    <li>{WebUtility.HtmlEncode(message)}</li>
    <li><em>{timestamp}</em></li>
</ul>
";
		File.AppendAllText(_filePath, htmlEntry);
	}

	/// <summary>
	/// Logs an error message.
	/// </summary>
	/// <param name="message">The error message to log.</param>
	public static void LogError(string message)
	{
		Log(message, isError: true);
	}

	/// <summary>
	/// Logs an informational message.
	/// </summary>
	/// <param name="message">The informational message to log.</param>
	public static void LogInfo(string message)
	{
		Log(message, isError: false);
	}
}
