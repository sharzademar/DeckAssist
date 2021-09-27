using DeckAssist.ViewModel;
using System;
using System.Collections.Concurrent;

namespace DeckAssist.Model
{
    public class ManaCurveDataPoint : ViewModelComponent
    {
        public ManaCurveDataPoint()
        {
            ConvertedManaCost = 0;
            Qty = 0;
            ColorDistribution = new ObservableConcurrentDictionary<ColorIdentity, int>
            {
                { ColorIdentity.Colorless, 0 },
                { ColorIdentity.Green, 0 },
                { ColorIdentity.Red, 0 },
                { ColorIdentity.Black, 0 },
                { ColorIdentity.Blue, 0 },
                { ColorIdentity.White, 0 },
                { ColorIdentity.Multicolored, 0 }
            };
        }

        public ObservableConcurrentDictionary<ColorIdentity, int> ColorDistribution { get; set; }
        public int ConvertedManaCost { get; set; }
        public int Qty { get; set; }
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