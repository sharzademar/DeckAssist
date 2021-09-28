using DeckAssist.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace DeckAssist.Model
{
    /// <summary>
    /// Represents a data point for a particular converted mana cost value, including the quantities of the colors of each card with the converted mana cost.
    /// </summary>
    public class ManaCurveDataPoint : ViewModelComponent
    {
        /// <summary>
        /// Initialize the data point, including initializing the color distribution with the mono colors
        /// </summary>
        public ManaCurveDataPoint()
        {
            ConvertedManaCost = 0;
            Qty = 0;
            ColorDistribution = new ObservableConcurrentDictionary<ColorIdentity, int>()
            {
                { ColorIdentity.Colorless, 0 },
                { ColorIdentity.Green, 0 },
                { ColorIdentity.Red, 0 },
                { ColorIdentity.Black, 0 },
                { ColorIdentity.Blue, 0 },
                { ColorIdentity.White, 0 }
            };
        }

        /// <summary>
        /// The quantities of the cards in this data point grouped by color
        /// </summary>
        public ObservableConcurrentDictionary<ColorIdentity, int> ColorDistribution { get; set; }

        /// <summary>
        /// The converted mana cost of this data point
        /// </summary>
        public int ConvertedManaCost { get; set; }

        /// <summary>
        /// The total Qty of cards in this data point
        /// </summary>
        public int Qty { get; set; }

        /// <summary>
        /// Return a basic string representation of the data point
        /// </summary>
        /// <returns>a basic string representation of the data point</returns>
        public override string ToString()
        {
            return Qty == 0 ?
                String.Empty :
                String.Format
                (
                    "CMC: {0,2} | QTY: {1,3} | CL: {2,3} | G: {3,3} | R: {4,3} | BLA: {5,3} | BLU: {6,3} | W: {7,3} | MC: {8,3}",
                    ConvertedManaCost,
                    Qty,
                    ColorDistribution[ColorIdentity.Colorless],
                    ColorDistribution[ColorIdentity.Green],
                    ColorDistribution[ColorIdentity.Red],
                    ColorDistribution[ColorIdentity.Black],
                    ColorDistribution[ColorIdentity.Blue],
                    ColorDistribution[ColorIdentity.White],
                    ColorDistribution
                        .Where(x => EnumHelper.Flag.Count((long)x.Key) > 1)
                        .Sum(x => x.Value)
                );
        }
    }
}