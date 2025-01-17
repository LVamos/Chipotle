using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

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
		// If the file exists, delete it to start fresh with initial HTML structure.
		if (File.Exists(_filePath))
		{
			File.Delete(_filePath);
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
	/// Writes a log entry to the file with a title and multiple messages, marking it as error if specified.
	/// Each newline in messages is replaced with an HTML line break.
	/// After logging all messages, the current timestamp with milliseconds is appended as the last list item.
	/// The entry starts with a level 2 heading for the message type and a level 1 heading for the title.
	/// </summary>
	/// <param name="isError">Whether the log entry represents an error.</param>
	/// <param name="title">The title of the log entry.</param>
	/// <param name="messages">An array of messages to log.</param>
	private static void Log(bool isError, string title, params string[] messages)
	{
		// Filter out the empty messages
		messages = messages.Where(m => !string.IsNullOrEmpty(m)).ToArray();

		// Choose heading based on message type
		string heading = isError ? "<h2 style='color:red;'>Error</h2>" : "<h2>Info</h2>";

		// Format time with milliseconds
		string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

		// Build HTML list items for each message, replacing newlines with <br>
		StringBuilder listItems = new();
		foreach (string message in messages)
		{
			string processed = WebUtility.HtmlEncode(message.Replace(Environment.NewLine, "<br>"));
			listItems.AppendLine($"<li>{processed}</li>");
		}
		// Append timestamp as the last list item
		listItems.AppendLine($"<li><em>{timestamp}</em></li>");

		// Create HTML fragment with level 2 heading for type and level 1 heading for title
		string htmlEntry = $@"
{heading}
<h1>{WebUtility.HtmlEncode(title)}</h1>
<ul>
{listItems}
</ul>
";
		File.AppendAllText(_filePath, htmlEntry);
	}

	/// <summary>
	/// Logs an error message or multiple error messages with a title.
	/// </summary>
	/// <param name="title">The title of the error message.</param>
	/// <param name="messages">The error messages to log.</param>
	public static void LogError(string title, params string[] messages)
	{
		Log(isError: true, title, messages);
	}

	/// <summary>
	/// Logs an informational message or multiple informational messages with a title.
	/// </summary>
	/// <param name="title">The title of the informational message.</param>
	/// <param name="messages">The informational messages to log.</param>
	public static void LogInfo(string title, params string[] messages)
	{
		Log(isError: false, title, messages);
	}
}
