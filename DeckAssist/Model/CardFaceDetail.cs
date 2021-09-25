﻿using DeckAssist.ViewModel;
using System.Collections.ObjectModel;

namespace DeckAssist.Model
{
    public class CardFaceDetail : ViewModelComponent
    {
        private const string cardBack = "https://c1.scryfall.com/file/scryfall-card-backs/normal/59/597b79b3-7d77-4261-871a-60dd17403388.jpg";

        private ObservableCollection<CardType> cardTypes;
        private int cmc;
        private ObservableCollection<ColorIdentity> colorIdentities;
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
            colorIdentities = new ObservableCollection<ColorIdentity>(); //default to accept new id
            cardTypes = new ObservableCollection<CardType>();
        }

        public ObservableCollection<CardType> CardTypes { get => cardTypes; set => SetProperty(ref cardTypes, value); }
        public ObservableCollection<ColorIdentity> ColorIdentities { get => colorIdentities; set => SetProperty(ref colorIdentities, value); }
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
            ColorIdentity[] ci = new ColorIdentity[colorIdentities.Count];
            colorIdentities.CopyTo(ci, 0);

            CardType[] ct = new CardType[cardTypes.Count];
            cardTypes.CopyTo(ct, 0);

            return new CardFaceDetail
            {
                ConvertedManaCost = cmc,
                Name = name,
                ImageURI = imageURI,
                TypeLine = typeLine,
                ColorIdentities = new ObservableCollection<ColorIdentity>(ci),
                CardTypes = new ObservableCollection<CardType>(ct)
            };
        }

        private ObservableCollection<CardType> ExtractTypes(string typeLine)
        {
            var x = new ObservableCollection<CardType>();

            foreach (string name in EnumHelper.CardTypeStrings)
            {
                if (typeLine.Contains(name))
                    x.Add(EnumHelper.GetCardTypeFromString(name));
            }

            return x;
        }
    }
}