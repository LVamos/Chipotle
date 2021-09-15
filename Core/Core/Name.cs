using System;
using System.Collections.Generic;

namespace Luky
{
    /// <summary>
    /// An immutable class to store unique names
    /// </summary>
    [Serializable]
    public sealed class Name : DebugSO
    {
        /// <summary>
        /// A public display name
        /// </summary>
        public readonly string Friendly;

        /// <summary>
        /// Unique identificator
        /// </summary>
        public readonly UInt16 ID;

        /// <summary>
        /// Inner name for indexing purposes
        /// </summary>
        public readonly string Indexed;

        private static UInt16 _stringCount = 0;

        private static Dictionary<string, UInt16> _stringsToIDs = new Dictionary<string, ushort>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="indexedName">Inner name</param>
        public Name(string indexedName) : this(indexedName, indexedName)
        { }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="indexedName">Inner name</param>
        /// <param name="friendlyName">Public name</param>
        public Name(string indexedName, string friendlyName)
        {
            Indexed = indexedName.PrepareForIndexing();
            Friendly = friendlyName;

            // Assign an unique ID
            if (!_stringsToIDs.TryGetValue(Indexed, out ID))
            {
                ID = ++_stringCount;
                _stringsToIDs.Add(Indexed, ID);
            }
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="name">An instance of Name</param>
        public Name(Name name) : this(name.Indexed, name.Friendly) { }

        // operator overloads

        /// <summary>
        /// </summary>
        /// <param name="s"></param>
        public static implicit operator Name(string s)
                => s == null ? null : new Name(s);

        /// <summary>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Name a, Name b)
        => (object.ReferenceEquals((object)a, (object)b) || a.Indexed != b.Indexed || a.Friendly != b.Friendly);

        /// <summary>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Name a, string b)
            => !(a == b);

        /// <summary>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Name a, Name b)
        {
            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null)
                return false;

            // Return true if the fields match:
            return a.ID == b.ID;
        }

        /// <summary>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Name a, string b)
            => (b == null) ? a == (Name)null : a == new Name(b);

        //{
        //    if (b == null)
        //        return a == (Name)null;
        //    return a == new Name(b, b);

        //}
        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
                return false;

            // If parameter cannot be cast to Name return false.
            Name n = obj as Name;
            if ((System.Object)n == null)
                return false;

            // Return true if the fields match:
            return ID == n.ID;
        }

        /// <summary>
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool Equals(Name n)
        {
            // If parameter is null return false:
            if ((object)n == null)
                return false;

            // Return true if the fields match:
            return ID == n.ID;
        }

        // overrides related to comparison operators

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        => ID.GetHashCode();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public string ToDebugString()
        => String.Format("{0} {1} {2}", ToString(), ID, Indexed);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        => Indexed;
    } // cls
}