using Game.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using UnityEngine;

namespace Game.Terrain
{
    public static class PassageFactory
    {
        private static string GetAttribute(XElement element, string attribute, bool prepareForIndexing = true) => prepareForIndexing ? element.Attribute(attribute)?.Value.Sanitize() : element?.Attribute(attribute)?.Value;

        public static Passage Create(XElement passageNode, bool createGameObject = false)
        {
            Name name = new(
                Extract("indexedname"),
                Extract("friendlyname", false)
                );
            bool isDoor = Extract("door").ToBool();
            bool closed = Extract("closed").ToBool();
            bool openable = Extract("openable").ToBool();

            PassageState state = PassageState.Open;
            if (!openable)
                state = PassageState.Locked;
            else if (openable && closed)
                state = PassageState.Closed;

            Rectangle area = new(Extract("coordinates"));
            List<string> zones = new()
            { Extract("from"),
                    Extract("to")
            };
            DoorType doorType = Extract("type") == "door" ? DoorType.Door : DoorType.Gate;

            return Create(
                createGameObject,
                name,
                area,
                zones,
                isDoor,
                state,
                doorType
                );

            string Extract(string attributeName, bool sanitize = true)
                        => GetAttribute(passageNode, attributeName, sanitize);
        }

        public static Passage AddComponent(GameObject obj, string name, bool isDoor)
        {
            if (_types.TryGetValue(name, out Type itemType))
                return obj.AddComponent(itemType) as Passage;

            if (isDoor)
                return obj.AddComponent<Door>() as Door;

            return obj.AddComponent<Passage>() as Passage;
        }

        private static Dictionary<string, Type> _types;

        /// <summary>
        /// Loads items from a YAML file.
        /// </summary>
        public static void Init()
        {
            Dictionary<string, string> types = null;
            YamlHelper.LoadFromResources(MainScript.PassagesPath, out types);
            if (types.Any(p => string.IsNullOrWhiteSpace(p.Key) || string.IsNullOrWhiteSpace(p.Value)))
                throw new ArgumentException("Invalid record.");

            _types = types.ToDictionary(p => p.Key, p => Type.GetType($"Game.Terrain.{p.Value}"));
        }

        private static GameObject GetHostObject(Name name, bool create)
        {
            return !create
                ? SceneObjects.GetPassage(name.Inner)
                : SceneObjects.GetOrCreatePassage(name.Inner);
        }

        /// <summary>
        /// Creates new instance of a passage according to the given parameters.
        /// </summary>
        /// <param name="name">Inner name of the passage</param>
        /// <param name="area">Coordinates of the are occupied by the passage</param>
        /// <param name="zones">Zones connectedd by the passage</param>
        /// <param name="isDoor">Specifies if the passage is a door.</param>
        /// <param name="state">State of a door</param>
        /// <param name="doorType">Type of a door</param>
        /// <returns>A new instance of the passage</returns>
        public static Passage Create(
            bool createGameObject = false,
            Name name = null,
            Rectangle area = default,
            List<string> zones = null,
            bool isDoor = false,
            PassageState state = default,
            DoorType doorType = default)
        {
            if (!createGameObject)
            {
                if (name == null || string.IsNullOrWhiteSpace(name.Inner))
                    throw new ArgumentNullException(nameof(name));
                if (zones.IsNullOrEmpty() || zones.Count() != 2)
                    throw new ArgumentException("Invalid zones.");
            }
            GameObject obj = GetHostObject(name, createGameObject);
            Passage passage = null;
            if (_types.TryGetValue(name.Inner, out Type passageType))
            {
                passage = obj.GetComponent(passageType) as Passage;
                if (passage is Door door)
                    door.Initialize(name, state, area, zones, doorType);
                else passage.Initialize(name, area, zones);
                return passage;
            }

            passage = obj.GetComponent<Passage>();

            if (passage is not Passage && passage is not Door)
                passage.Initialize(name, area, zones);
            else if (passage is Door)
                (passage as Door).Initialize(name, state, area, zones, doorType);
            else passage.Initialize(name, area, zones);

            return passage;
        }
    }
}
