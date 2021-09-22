using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace DeckAssist
{
    public class Deck : ViewModelBase
    {
        private ObservableCollection<Card> cards;
        private ManaCurveCollection manaCurve;
        private Card selectedCard;

        public ObservableCollection<Card> Cards { get => cards; set => SetProperty(ref cards, value); }
        public ManaCurveCollection ManaCurve { get => manaCurve; set => SetProperty(ref manaCurve, value); }
        public int NumLands { get => ManaCurve.NumLands; set => ManaCurve.NumLands = value; }
        public Card SelectedCard { get => selectedCard; set => SetProperty(ref selectedCard, value); }

        public Deck()
        {
            cards = new ObservableCollection<Card>();
            manaCurve = new ManaCurveCollection();
            ClearDeck();
        }

        public void ClearDeck()
        {
            Cards.Clear();
            ManaCurve.Keys.ToList().ForEach(x => ManaCurve.Remove(x));
            ManaCurve.Sum = 0;
            NumLands = 0;
            selectedCard = new Card();
        }

        public void UpdateManaCurve()
        {
            RaisePropertyChanged("ManaCurve");
        }

        public void AddCard(Card card, int index = -1)
        {
            ManaCurve.Sum += card.Qty;

            int cmc = card.CardLayout == Layout.modal_dfc ? card.SelectedCardFaceDetail.ConvertedManaCost : card.ConvertedManaCost;

            card.ParentDeck = this;

            if (index == -1)
                Cards.Add(card);
            else
                Cards.Insert(index, card);

            if (card.SelectedCardFaceDetail.TypeLine.Contains("Land"))
                NumLands += card.Qty;
            else
            {
                ManaCurveDataPoint mcdp = null;
                if (!ManaCurve.ContainsKey(cmc))
                //if (!ManaCurve.Select(x => x.ConvertedManaCost).Contains(cmc))
                {
                    mcdp = new ManaCurveDataPoint { ConvertedManaCost = cmc };
                    ManaCurve.Add(cmc, mcdp);
                }
                else
                {
                    mcdp = ManaCurve[cmc];
                }

                
                mcdp.Qty += card.Qty;

                if (card.SelectedCardFaceDetail.ColorIdentities.Count == 1)
                    mcdp.ColorDistribution[card.SelectedCardFaceDetail.ColorIdentities.First()] += card.Qty;
                else if (card.SelectedCardFaceDetail.ColorIdentities.Count > 1)
                    mcdp.ColorDistribution[ColorIdentity.Multicolored] += card.Qty;
                else
                    throw new ArgumentException(String.Format("Card contains no color identity: {0}", card));
            }
            UpdateManaCurve();
        }

        public void RemoveCard(Card card)
        {
            ManaCurve.Sum -= card.Qty;

            int cmc = card.CardLayout == Layout.modal_dfc ? card.SelectedCardFaceDetail.ConvertedManaCost : card.ConvertedManaCost;

            Cards.Remove(card);

            if (card.SelectedCardFaceDetail.TypeLine.Contains("Land"))
                NumLands -= card.Qty;
            else
            {
                ManaCurveDataPoint mcdp = ManaCurve[cmc];
                mcdp.Qty -= card.Qty;

                if (mcdp.Qty == 0)
                {
                    ManaCurve.Remove(cmc);
                }
                else
                {
                    if (card.SelectedCardFaceDetail.ColorIdentities.Count == 1)
                        mcdp.ColorDistribution[card.SelectedCardFaceDetail.ColorIdentities.First()] -= card.Qty;
                    else if (card.SelectedCardFaceDetail.ColorIdentities.Count > 1)
                        mcdp.ColorDistribution[ColorIdentity.Multicolored] -= card.Qty;
                    else
                        throw new ArgumentException(String.Format("Card contains no color identity: {0}", card));
                }
            }
            UpdateManaCurve();
        }

        public int GetIndexOf(Card card)
        {
            return Cards.IndexOf(card);
        }
    }
}
