using ProtoBuf;

using System;

namespace Game.Entities
{
    /// <summary>
    /// Controls the sound output of an NPC.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(ChipotleSoundComponent))]
    [ProtoInclude(101, typeof(TuttleSoundComponent))]
    public class SoundComponent : CharacterComponent
    {
        /// <summary>
        /// Volume for foot steps
        /// </summary>
        protected float _walkingVolume = 1;

        /// <summary>
        /// Default volume for sound output
        /// </summary>
        protected float _defaultVolume = 1;

        /// <summary>
        /// Converts a string list to a CSV string.
        /// </summary>
        /// <param name="stringList">The string list to be formatted</param>
        /// <param name="addAnd">Specifies if last two items should by separated with " a " conjunction.</param>
        /// <returns>CSV string</returns>
        protected string FormatStringList(string[] stringList, bool addAnd = false)
        {
            int count = stringList.Length;
            if (count == 1)
                return stringList[0];

            if (count > 2)
            {
                for (int i = 0; i < count - 2; i++)
                    stringList[i] += ", ";
            }

            stringList[count - 2] += addAnd ? " a " : ", ";
            return String.Join(string.Empty, stringList);
        }
    }
}