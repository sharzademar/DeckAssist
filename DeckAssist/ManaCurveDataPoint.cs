using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeckAssist
{
    public class ManaCurveDataPoint : ViewModelBase
    {
        public int ConvertedManaCost { get; set; }
        public ObservableConcurrentDictionary<ColorIdentity, int> ColorDistribution { get; set; }
        public int Qty { get; set; }

        public ManaCurveDataPoint()
        {
            ConvertedManaCost = 0;
            Qty = 0;
            ColorDistribution = new ObservableConcurrentDictionary<ColorIdentity, int>();
            ColorDistribution.Add(ColorIdentity.Colorless, 0);
            ColorDistribution.Add(ColorIdentity.Green, 0);
            ColorDistribution.Add(ColorIdentity.Red, 0);
            ColorDistribution.Add(ColorIdentity.Black, 0);
            ColorDistribution.Add(ColorIdentity.Blue, 0);
            ColorDistribution.Add(ColorIdentity.White, 0);
            ColorDistribution.Add(ColorIdentity.Multicolored, 0);
        }

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
                    ColorDistribution[ColorIdentity.Multicolored]
                );
        }
    }
}
