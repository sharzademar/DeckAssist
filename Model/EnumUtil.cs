using Serilog;
using System;
using System.Collections.Generic;

namespace DeckAssist.Model
{
    /// <summary>
    /// Utility class to assist in Enum operations
    /// </summary>
    public static class EnumUtil
    {
        private static readonly Dictionary<Layout, PropertySettings> layoutProperties = InitLayoutProperties();

        private static Dictionary<Layout, PropertySettings> InitLayoutProperties()
        {
            try
            {
                PropertySettings normal,
                                 splitFace,
                                 doubleOneCMC,
                                 doubleAll;

                
                //define "normal" property
                normal = new PropertySettings();

                //define properties for cards with one split face
                splitFace = new PropertySettings();
                splitFace.Name.PropertyMode = PropertyMode.Double;
                splitFace.Type.PropertyMode = PropertyMode.Double;

                //define cards with one cast that then transform
                doubleOneCMC = new PropertySettings(PropertyMode.Double);
                doubleOneCMC.ConvertedManaCost.PropertyMode = PropertyMode.Single;

                //define double sided card
                doubleAll = new PropertySettings(PropertyMode.Double);
                //double sided cmc derived from mana_cost entry
                doubleAll.ConvertedManaCost.JSONToken = JSONToken.mana_cost;

                return new Dictionary<Layout, PropertySettings>
                {
                    { Layout.normal, normal },
                    { Layout.split, splitFace },
                    { Layout.flip, splitFace },
                    { Layout.transform, doubleOneCMC },
                    { Layout.modal_dfc, doubleAll },
                    { Layout.meld, normal }
                };
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Debug("{@Message}: {@ParamName} - {@ActualValue}", ex.Message, ex.ParamName, ex.ActualValue);
                throw;
            }
        }

        /// <summary>
        /// Returns the property settings for the provided layout
        /// </summary>
        /// <param name="layout">The layout for w hich to retrieve the settings</param>
        /// <returns>The property settings for the layout</returns>
        /// <exception cref="KeyNotFoundException">A layout value was added to <c>Layout</c> that is not yet implemented</exception>
        public static PropertySettings GetPropertySettings(Layout layout)
        {
            return layoutProperties[layout];
        }

        /// <summary>
        /// Returns an array of strings that is the names of the provided enumerated type
        /// </summary>
        /// <typeparam name="T">The provided enumerated type</typeparam>
        /// <returns>The names of the enum as an array of strings</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static string[] EnumStrings<T>() where T : Enum
        {
            return Enum.GetNames(typeof(T));
        }

        /// <summary>
        /// Generic wrapper for Enum.TryParse
        /// </summary>
        /// <typeparam name="T">The type of the enum parameter</typeparam>
        /// <param name="value">The string representation of the name of an enum constant</param>
        /// <param name="e">The enum to store the conversion in</param>
        /// <returns>Whether or not the conversion succeeded</returns>
        public static bool TryParse<T>(string value, out T e) where T : struct
        {
            return Enum.TryParse(value, out e);
        }
        /// <summary>
        /// Collection of utility functions related to flags
        /// </summary>
        public static class Flag
        {
            /// <summary>
            /// Toggle the flag bit of a flag
            /// </summary>
            /// <typeparam name="T">The type of the flag</typeparam>
            /// <param name="flag1">The flag to modify</param>
            /// <param name="flag2">The flag bit(s) to toggle</param>
            public static void ToggleFlags<T>(ref T flag1, T flag2) where T : Enum
            {
                int flag1Int = (int)(object)flag1,
                    flag2Int = (int)(object)flag2;

                flag1 = (T)(object)(flag1Int ^= flag2Int);
            }
            /// <summary>
            /// Counts the set bits of a flag
            /// </summary>
            /// <param name="lValue">The flag represented as a long</param>
            /// <returns>The number of set bits in lValue</returns>
            public static int Count(long lValue)
            {
                int setCount = 0;

                // while there are still set bits
                while(lValue != 0)
                {
                    //unsets the largest bit (e.g 0101 & 0100 = 0100, 0100 & 0000 = 0000)
                    lValue &= (lValue - 1);

                    //increase count of bits found
                    setCount++;
                }

                return setCount;
            }
        }
    }
}