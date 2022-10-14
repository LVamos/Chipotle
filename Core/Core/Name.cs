using ProtoBuf;

using System;
using System.Collections.Generic;

namespace Luky
{
    /// <summary>
    /// An immutable class to store unique names
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
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

        private static readonly Dictionary<string, UInt16> _stringsToIDs = new Dictionary<string, ushort>();
        private static UInt16 _stringCount = 0;

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

        /// <summary>
        /// Converts a string to Name
        /// </summary>
        /// <param name="s">A string to convert</param>
        public static implicit operator Name(string s)
                => s == null ? null : new Name(s);

        /// <summary>
        /// Overload of != operator
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>True if operands aren't equal</returns>
        public static bool operator !=(Name a, Name b)
        => (object.ReferenceEquals((object)a, (object)b) || a.Indexed != b.Indexed || a.Friendly != b.Friendly);

        /// <summary>
        /// Overload of != operator
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>True if operands aren't equal</returns>
        public static bool operator !=(Name a, string b)
            => !(a == b);

        /// <summary>
        /// Overload of == operator
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>True if operands are equal</returns>
        public static bool operator ==(Name a, Name b)
        {
            if (Object.ReferenceEquals(a, b))
                return true;

            return a is Name && b is Name && a.ID == b.ID;
        }

        /// <summary>
        /// Overload of == operator
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>True if operands are equal</returns>
        public static bool operator ==(Name a, string b)
            => (b == null) ? a == (Name)null : a == new Name(b);

        /// <summary>
        /// Checks equality of two instances
        /// </summary>
        /// <param name="obj">An object to compare</param>
        /// <returns>True if both instances are equal</returns>
        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            // Try cast to Name
            if (!(obj is Name n))
                return false;

            // Check if the fields match
            return ID == n.ID;
        }

        /// <summary>
        /// Checks equality of two instances
        /// </summary>
        /// <param name="obj">An object to compare</param>
        /// <returns>True if both instances are equal</returns>
        public bool Equals(Name n)
        {
            if (n is null)
                return false;

            // Check if the fields match
            return ID == n.ID;
        }

        /// <summary>
        /// Returns the hash code
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        => ID.GetHashCode();

        /// <summary>
        /// Returns string representation of this object
        /// </summary>
        /// <returns>The string representation of this object</returns>
        public override string ToString()
        => Indexed;
    }
}