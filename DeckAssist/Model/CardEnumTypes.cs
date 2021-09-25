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
        public static string[] CardTypeStrings = Enum.GetNames(typeof(CardType));

        public static CardType GetCardTypeFromString(string strCardType)
        {
            return (CardType)Enum.Parse(typeof(CardType), strCardType);
        }
    }
}