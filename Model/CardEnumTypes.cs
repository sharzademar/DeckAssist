using Serilog;
using System;
using System.Collections.Generic;

namespace DeckAssist.Model
{

    public enum JSONTokens
    {
        name,
        image_uris_normal,
        colors,
        type_line,
        cmc
    }
    public enum CardType
    {
        Land,
        Creature,
        Artifact,
        Enchantment,
        Planeswalker,
        Instant,
        Sorcery
    }

    public enum ColorIdentity
    {
        White,
        Blue,
        Black,
        Red,
        Green,
        Colorless,
        Multicolored
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
    }
}