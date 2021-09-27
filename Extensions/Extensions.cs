using System;
using DeckAssist.Model;

namespace DeckAssist.Extensions
{
    /// <summary>
    /// Class <c>Extensions</c> provides extensions to various common classes and data types
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Method <c>EqualsIgnoreCase</c> extends string and determines whether this string matches another string, ignoring case.
        /// </summary>
        /// <param name="str">The string instance this method extends</param>
        /// <param name="value">The string value this instance is compared to</param>
        public static bool EqualsIgnoreCase(this String str, string value)
        {
            return str.ToLower().Equals(value.ToLower());
        }
    }
}