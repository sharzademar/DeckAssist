using DeckAssist.Http;
using DeckAssist.ViewModel;
using System;

namespace DeckAssist.Model
{
    /// <summary>
    /// Represents the various card types a card can contain. Can contain multiple.
    /// </summary>
    [Flags]
    public enum CardType
    {
        None = 0,
        Land = 1 << 0,
        Creature = 1 << 1,
        Artifact = 1 << 2,
        Enchantment = 1 << 3,
        Planeswalker = 1 << 4,
        Instant = 1 << 5,
        Sorcery = 1 << 6,
        Legendary = 1 << 7,
        Tribal = 1 << 8,
        Aura = 1 << 9
    }

    /// <summary>
    /// Represents the collection of color identities a card can have.
    /// </summary>
    [Flags]
    public enum ColorIdentity
    {
        None = 0,
        White = 1 << 0,
        Blue = 1 << 1,
        Black = 1 << 2,
        Red = 1 << 3,
        Green = 1 << 4,
        Colorless = 1 << 5
    }

    /// <summary>
    /// Represent the face of a card along with its relevant data
    /// </summary>
    public class CardFaceDetail : ViewModelComponent
    {
        private CardType cardTypes;
        private int cmc;
        private ColorIdentity colorIdentities;
        private string imageURI;
        private string name;
        private string typeLine;

        /// <summary>
        /// Initialize a CardFaceDetail object with default values and a name of "Unassigned"
        /// </summary>
        public CardFaceDetail()
        {
            //initialize with unusable values to indicate value not relevant unless manually assigned
            cmc = -1;
            name = "Unassigned";
            typeLine = "Unassigned";
            imageURI = ScryfallBridge.CardBackURI; //default to card back
            colorIdentities = ColorIdentity.None;
            cardTypes = CardType.None;
        }

        /// <summary>
        /// The set of flags that represent the various card types of the card face. Can only be read;
        /// </summary>
        public CardType CardTypes { get => cardTypes; private set => SetProperty(ref cardTypes, value); }

        /// <summary>
        /// The set of flags that represent the various color identities of the card face
        /// </summary>

        public ColorIdentity ColorIdentities { get => colorIdentities; set => SetProperty(ref colorIdentities, value); }

        /// <summary>
        /// The set of flags that represent the various color identities of the card face
        /// </summary>
        public int ConvertedManaCost { get => cmc; set => SetProperty(ref cmc, value); }

        /// <summary>
        /// The artwork for the card face
        /// </summary>
        public string ImageURI { get => imageURI; set => SetProperty(ref imageURI, value); }

        /// <summary>
        /// The name of the card face
        /// </summary>
        public string Name { get => name; set => SetProperty(ref name, value); }

        /// <summary>
        /// The Type line text of the card. This also sets CardFaceDetail.CardTypes.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public string TypeLine
        {
            get => typeLine;
            set
            {
                CardTypes = ExtractTypes(value);
                SetProperty(ref typeLine, value);
            }
        }

        /// <summary>
        /// Create a memberwise copy of a CardFaceDetail.
        /// </summary>
        /// <returns>A new CardFaceDetail with the same member values as this object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public CardFaceDetail Copy()
        {
            return new CardFaceDetail
            {
                ConvertedManaCost = cmc,
                Name = name,
                ImageURI = imageURI,
                TypeLine = typeLine,
                ColorIdentities = colorIdentities,
                CardTypes = cardTypes
            };
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private CardType ExtractTypes(string typeLine)
        {
            CardType x = CardType.None;

            foreach (string name in EnumUtil.EnumStrings<CardType>())
            {
                if (typeLine.Contains(name))
                {
                    if (!EnumUtil.TryParse(name, out CardType ct))
                    {
                        throw new ArgumentException(String.Format("Scryfall returned an unimplemented card type: {0}", typeLine));
                    }
                    x |= ct;
                }
            }

            return x;
        }
    }
}