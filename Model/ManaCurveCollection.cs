using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DeckAssist.Model
{
    public class ManaCurveCollection : ObservableConcurrentDictionary<int, ManaCurveDataPoint>
    {
        private readonly IEnumerable<Card> cards;

        /// <summary>
        /// Initialize the mana curve with a collection of cards
        /// </summary>
        /// <param name="c">the collection of cards the mana curve represents</param>
        public ManaCurveCollection(IEnumerable<Card> c)
        {
            cards = c;
            NumLands = 0;
            Sum = 0;
        }

        /// <summary>
        /// The number of land cards in the deck. They are not counted in the mana curve.
        /// </summary>
        public int NumLands { get; private set; }

        /// <summary>
        /// The total number of cards
        /// </summary>
        public int Sum { get; private set; }

        /// <summary>
        /// Generate data points for each CMC value and quantities of each color of cards in a data point
        /// </summary>
        public void AnalyzeCurve()
        {
            ManaCurveDataPoint mcdp;
            ColorIdentity ci;

            // clear the curve
            Clear();
            // count the number of lands
            NumLands = cards.Where(x => x.SelectedCardFaceDetail.CardTypes.HasFlag(CardType.Land)).Sum(x => x.Qty);
            // count the number of cards
            Sum = cards.Sum(x => x.Qty);

            //Group non-lands by their converted mana costs, counting modal cards as their selected face
            var cardGrouping = cards
                .Where(x => !x.SelectedCardFaceDetail.CardTypes.HasFlag(CardType.Land))
                .GroupBy(x => x.CardLayout == Layout.modal_dfc ?
                                x.SelectedCardFaceDetail.ConvertedManaCost : x.ConvertedManaCost);

            //for each mana cost
            foreach (var group in cardGrouping)
            {
                //generate a new data point
                mcdp = new ManaCurveDataPoint { ConvertedManaCost = group.Key };

                //add an entry to the underlying dictionary
                Add(group.Key, mcdp);

                //for each card in the current mana cost
                foreach (var card in group)
                {
                    //get the card's color identities
                    ci = card.SelectedCardFaceDetail.ColorIdentities;
                    //increase the Qty of the data point by the Qty of the card
                    mcdp.Qty += card.Qty;
                    //if the data point doesnt contain a key for the card's color id's, add one
                    if (!mcdp.ColorDistribution.ContainsKey(ci))
                        mcdp.ColorDistribution.Add(ci, 0);
                    //increase's the data point's color Qty by the Qty of the card
                    mcdp.ColorDistribution[ci] += card.Qty;
                }
            }
        }

        /// <summary>
        /// Remove all data from the internal dictionary
        /// </summary>
        public void Clear()
        {
            Keys.ToList().ForEach(x => this.Remove(x));
        }
    }
}