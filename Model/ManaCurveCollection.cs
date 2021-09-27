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
            NumLands = cards.Where(x => x.SelectedCardFaceDetail.CardTypes.HasFlag(CardType.Land)).Sum(x => x.Qty);
            Sum = cards.Sum(x => x.Qty);

            var cardGrouping = cards
                .Where(x => !x.SelectedCardFaceDetail.CardTypes.HasFlag(CardType.Land))
                .GroupBy(x => x.CardLayout == Layout.modal_dfc ?
                                x.SelectedCardFaceDetail.ConvertedManaCost : x.ConvertedManaCost);

            foreach (var group in cardGrouping)
            {
                ManaCurveDataPoint mcdp = new ManaCurveDataPoint { ConvertedManaCost = group.Key };
                
                Add(group.Key, mcdp);

                foreach (var card in group)
                {
                    ColorIdentity ci = card.SelectedCardFaceDetail.ColorIdentities;
                    mcdp.Qty += card.Qty;
                    if (!mcdp.ColorDistribution.ContainsKey(ci))
                        mcdp.ColorDistribution.Add(ci, 0);
                    mcdp.ColorDistribution[ci] += card.Qty;
                }
            }
        }

        public void Clear()
        {
            this.Keys.ToList().ForEach(x => this.Remove(x));
        }
    }
}