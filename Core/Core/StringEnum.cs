namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class StringEnum
    {
        public int Index;
        public string[] Values;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="values"></param>
        protected StringEnum(params string[] values)
        => Values = values;

        public string Name
        {
            get
            {
                if (Values.Length == 0)
                {
                    return "";
                }

                if (!Index.IsValidIndexFor(Values))
                {
                    Index = 0;
                }

                return Values[Index];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="se"></param>
        public static implicit operator int(StringEnum se)
            => se.Index;

        /// <summary>
        /// 
        /// </summary>
        public virtual void MoveNext()
        {
            Index++;
            if (!Index.IsValidIndexFor(Values))
            {
                Index = 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void MovePrevious()
        {
            Index--;
            if (!Index.IsValidIndexFor(Values))
            {
                Index = Values.Length - 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;

        // just a note, I didn't overload assignment operator because you can't in C#.
        // I could have changed this from a class to a struct and then I could make constructors that basically work like the assignment operator overloading, but I chose to leave it as is.
    } // end StringEnum class
}