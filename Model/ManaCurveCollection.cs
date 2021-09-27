using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DeckAssist.Model
{
    public class ManaCurveCollection : ObservableConcurrentDictionary<int, ManaCurveDataPoint>
    {
        private readonly IEnumerable<Card> cards;

        public ManaCurveCollection(IEnumerable<Card> c)
        {
            this.cards = c;
            NumLands = 0;
            Sum = 0;
        }

        public int NumLands { get; private set; }
        public int Sum { get; private set; }
        public void AnalyzeCurve()
        {
            Console.WriteLine("AnalyzeCurve()");
            Clear();
            NumLands = cards.Where(x => x.SelectedCardFaceDetail.CardTypes.Contains(CardType.Land)).Sum(x => x.Qty);
            Sum = cards.Sum(x => x.Qty);

            var cardGrouping = cards
                .Where(x => !x.SelectedCardFaceDetail.CardTypes.Contains(CardType.Land))
                .GroupBy(x => x.CardLayout == Layout.modal_dfc ?
                                x.SelectedCardFaceDetail.ConvertedManaCost : x.ConvertedManaCost);

            foreach (var group in cardGrouping)
            {
                ManaCurveDataPoint mcdp = new ManaCurveDataPoint { ConvertedManaCost = group.Key };
                Add(group.Key, mcdp);

                foreach (var card in group)
                {
                    mcdp.Qty += card.Qty;

                    if (card.SelectedCardFaceDetail.ColorIdentities.Count == 1)
                        mcdp.ColorDistribution[card.SelectedCardFaceDetail.ColorIdentities.First()] += card.Qty;
                    else if (card.SelectedCardFaceDetail.ColorIdentities.Count > 1)
                        mcdp.ColorDistribution[ColorIdentity.Multicolored] += card.Qty;
                    else
                        throw new ArgumentException(String.Format("Card contains no color identity: {0}", card));
                }
            }
        }

        public void Clear()
        {
            this.Keys.ToList().ForEach(x => this.Remove(x));
        }
    }
}