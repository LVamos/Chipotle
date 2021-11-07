using System;
using System.Collections.Generic;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// Controls the sound output of an NPC.
    /// </summary>
    [Serializable]
    public class SoundComponent : EntityComponent
    {
        /// <summary>
        /// Reference to the sound player
        /// </summary>
        protected SoundThread _sound => World.Sound;

        /// <summary>
        /// Converts a string list to a CSV string.
        /// </summary>
        /// <param name="stringList">The string list to be formatted</param>
        /// <param name="addAnd">Specifies if last two items should by separated with " a " conjunction.</param>
        /// <returns>CSV string</returns>
        protected string FormatStringList(List<string> stringList, bool addAnd = false)
        {
            int count = stringList.Count;
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