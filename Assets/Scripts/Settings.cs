using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

using UnityEngine;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Game
{
    public static class Settings
    {
        public static bool UseClockDirections;

        /// <summary>
        /// Saves the settings into a YAML file specified in <see cref="ConfigurationFileName"/>.
        /// </summary>
        public static void SaveSettings()
        {
            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            Dictionary<string, object> settings = new();

            // Using reflection to get static properties
            FieldInfo[] fields = typeof(Settings).GetFields();
            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(null);
                settings.Add(field.Name, value);
            }

            string content = serializer.Serialize(settings);
            File.WriteAllText(ConfigurationFileName, content);
        }

        /// <summary>
        /// Loads a YAML configuration file and applies settings.
        /// </summary>
        /// <param name="configurationName">Name of a YAML file without extension</param>
        public static void LoadSettings()
        {
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            string path = Path.Combine(MainScript.ConfigPath, "config.dat");
            string configName = File.ReadAllText(path);
            path = Path.Combine(MainScript.ConfigPath, configName) + ".yaml";
            string yamlContent = File.ReadAllText(path);
            Dictionary<string, object> settingsDictionary = deserializer.Deserialize<Dictionary<string, object>>(yamlContent);

            // Using reflection to set static properties
            FieldInfo[] fields = typeof(Settings).GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                if (!settingsDictionary.ContainsKey(field.Name))
                    continue;

                object value = settingsDictionary[field.Name];

                // Special case for Vector2? because YamlDotNet may not correctly deserialize complex types
                if (field.FieldType != typeof(Vector2?))
                {
                    field.SetValue(null, Convert.ChangeType(value, field.FieldType));
                    continue;
                }

                if (value is not Dictionary<object, object> vectorDict)
                    field.SetValue(null, null);
                else
                {
                    float x = float.Parse(vectorDict["X"].ToString(), CultureInfo.InvariantCulture);
                    float y = float.Parse(vectorDict["Y"].ToString(), CultureInfo.InvariantCulture);
                    field.SetValue(null, new Vector2?(new(x, y)));
                }
            }

            Configuration = configName;
            ConfigurationFileName = path;
        }

        /// <summary>
        /// Name of currently loaded configuration file.
        /// </summary>
        public static string Configuration { get; private set; }

        /// <summary>
        /// Enables or disables logging of currently played sounds into console.
        /// </summary>
        public static bool LogPlayingSounds { get; set; }

        /// <summary>
        /// Gets or sets the name of the configuration file.
        /// </summary>
        private static string ConfigurationFileName;

        /// <summary>
        /// Name of a XML map file without extension to be loaded
        /// </summary>
        public static string MapName;

        /// <summary>
        /// Specifies if the JAWS key hook should be disabled.
        /// </summary>
        public static bool DisableJawsKeyHook;

        /// <summary>
        /// Enables some test methods.
        /// </summary>
        public static bool TestCommandsEnabled;

        /// <summary>
        /// Enables or disables cutscenes.
        /// </summary>
        public static bool PlayCutscenes;

        /// <summary>
        /// Custom initial position for Tuttle
        /// </summary>
        public static Vector2? TuttleTestStart;

        /// <summary>
        /// Enables or disables a custom initial posiiton for Tuttle.
        /// </summary>
        public static bool AllowTuttlesCustomPosition;

        /// <summary>
        /// Temporary start position of Chipotle character
        /// </summary>
        public static Vector2? TestChipotleStartPosition;

        /// <summary>
        /// Enables or disables commands for loading and creating predefined saves.
        /// </summary>
        public static bool AllowPredefinedSaves;

        /// <summary>
        /// Enables or disables error reporting.
        /// </summary>
        public static bool ReportErrors;

        /// <summary>
        /// Enables or disables raising unhandled exceptions.
        /// </summary>
        public static bool ThrowExceptions;

        /// <summary>
        /// Enables or disables music in main menu.
        /// </summary>
        public static bool PlayMenuLoop;

        /// <summary>
        /// Enables or disables main menu at startup.
        /// </summary>
        public static bool MainMenuAtStartup;

        /// <summary>
        /// Enables or disables sending Tuttle to the pool zone at startup.
        /// </summary>
        public static bool SendTuttleToPool;

        /// <summary>
        /// Enables or disables Tuttle's Chipotle following.
        /// </summary>
        public static bool LetTuttleFollowChipotle;
    }
}