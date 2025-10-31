using System.Text;

using UnityEditor;

using UnityEngine;

/// <summary>
/// Registers encoding providers before project generation to support CP1250 and other encodings
/// </summary>
[InitializeOnLoad]
public static class EncodingProviderInitializer
{
	static EncodingProviderInitializer()
	{
		try
		{
			// Register code page provider to support encodings like CP1250
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			Debug.Log("Encoding providers registered successfully in Editor.");
		}
		catch (System.Exception ex)
		{
			Debug.LogError($"Failed to register encoding providers: {ex.Message}");
		}
	}
}