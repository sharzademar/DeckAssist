using DeckAssist.ViewModel;
using System;
using System.Collections.ObjectModel;

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

        public CardType CardTypes { get => cardTypes; set => SetProperty(ref cardTypes, value); }
        public ColorIdentity ColorIdentities { get => colorIdentities; set => SetProperty(ref colorIdentities, value); }
        public int ConvertedManaCost { get => cmc; set => SetProperty(ref cmc, value); }
        public string ImageURI { get => imageURI; set => SetProperty(ref imageURI, value); }
        public string Name { get => name; set => SetProperty(ref name, value); }
        public string TypeLine
        {
            get => typeLine;
            set
            {
                CardTypes = ExtractTypes(value);
                SetProperty(ref typeLine, value);
            }
        }
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