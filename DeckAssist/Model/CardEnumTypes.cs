using System;

namespace DeckAssist.Model
{
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
        modal_dfc,
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