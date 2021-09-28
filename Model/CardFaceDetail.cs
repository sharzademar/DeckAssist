using DeckAssist.ViewModel;
using System;

namespace DeckAssist.Model
{
    public class CardFaceDetail : ViewModelComponent
    {
        private const string cardBack = "https://c1.scryfall.com/file/scryfall-card-backs/normal/59/597b79b3-7d77-4261-871a-60dd17403388.jpg";

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
            imageURI = cardBack; //default to card back
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

            foreach (string name in EnumHelper.EnumStrings<CardType>())
            {
                if (typeLine.Contains(name))
                {
                    if (!EnumHelper.TryParse(name, out CardType ct))
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