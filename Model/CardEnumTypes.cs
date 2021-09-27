using Serilog;
using System;
using System.Collections.Generic;

namespace DeckAssist.Model
{
    /// <summary>
    /// Represents the name of a particular JSON token
    /// </summary>
    public enum JSONTokens
    {
        name,
        image_uris_normal,
        colors,
        type_line,
        cmc
    }

    /// <summary>
    /// Represents the various card types a card can contain. Can contain multiple.
    /// </summary>
    [Flags] public enum CardType
    {
        None = 0,
        Land = 1 << 0,
        Creature = 1 << 1,
        Artifact = 1 << 2,
        Enchantment = 1 << 3,
        Planeswalker = 1 << 4,
        Instant = 1 << 5,
        Sorcery = 1 << 6
    }

    [Flags] public enum ColorIdentity
    {
        None = 0,
        White = 1 << 0,
        Blue = 1 << 1,
        Black = 1 << 2,
        Red = 1 << 3,
        Green = 1 << 4,
        Colorless = 1 << 5
    }

    public enum Layout
    {
        normal, //implemented
        split, //implemented
        flip, //implemented
        transform, //implemented
        modal_dfc, //implemented
        meld,
        leveler,
        _class,
        saga,
        adventure,
        planar,
        scheme,
        vanguard,
        token,
        double_faced_token,
        emblem,
        augment,
        host,
        art_series,
        double_sided,
        unassigned
    }

    public static class EnumHelper
    {
        private static readonly Dictionary<Layout, LayoutProperties> layoutProperties = InitLayoutProperties();

        private static Dictionary<Layout, LayoutProperties> InitLayoutProperties()
        {
            try
            {
                return new Dictionary<Layout, LayoutProperties>
                {
                    {
                        Layout.normal,
                        new LayoutProperties
                        {
                            Naming = PropertyMode.Single,
                            Type = PropertyMode.Single,
                            ArtFaces = PropertyMode.Single,
                            ColorIdentities = PropertyMode.Single,
                            ConvertedManaCost = PropertyMode.Single
                        }
                    },
                    {
                        Layout.split,
                        new LayoutProperties
                        {
                            Naming = PropertyMode.Double,
                            Type = PropertyMode.Double,
                            ArtFaces = PropertyMode.Single,
                            ColorIdentities = PropertyMode.Single,
                            ConvertedManaCost = PropertyMode.Single
                        }
                    },
                    {
                        Layout.flip,
                        new LayoutProperties
                        {
                            Naming = PropertyMode.Double,
                            Type = PropertyMode.Double,
                            ArtFaces = PropertyMode.Single,
                            ColorIdentities = PropertyMode.Single,
                            ConvertedManaCost = PropertyMode.Single
                        }
                    },
                    {
                        Layout.transform,
                        new LayoutProperties
                        {
                            Naming = PropertyMode.Double,
                            Type = PropertyMode.Double,
                            ArtFaces = PropertyMode.Double,
                            ColorIdentities = PropertyMode.Double,
                            ConvertedManaCost = PropertyMode.Single
                        }
                    },
                    {
                        Layout.modal_dfc,
                        new LayoutProperties
                        {
                            Naming = PropertyMode.Double,
                            Type = PropertyMode.Double,
                            ArtFaces = PropertyMode.Double,
                            ColorIdentities = PropertyMode.Double,
                            ConvertedManaCost = PropertyMode.Double
                        }
                    }
                };
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Debug("{@Message}: {@ParamName} - {@ActualValue}", ex.Message, ex.ParamName, ex.ActualValue);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layout"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">A layout value was added to <c>Layout</c> that is not yet implemented</exception>
        public static LayoutProperties GetLayoutProperties(Layout layout)
        {
            return layoutProperties[layout];
        }

        public static string[] EnumStrings<T>() where T : Enum
        {
            return Enum.GetNames(typeof(T));
        }

        public static T Parse<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        public static bool TryParse<T>(string value, out T e) where T : struct
        {
            return Enum.TryParse(value, out e);
        }

        public static class Flag
        {
            public static void ToggleFlags<T>(ref T flag1, T flag2) where T : Enum
            {
                int flag1Int = (int)(object)flag1,
                    flag2Int = (int)(object)flag2;

                flag1 = (T)(object)(flag1Int ^= flag2Int);
            }

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